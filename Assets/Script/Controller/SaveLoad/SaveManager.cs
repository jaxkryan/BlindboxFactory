using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Firebase;
using Firebase.Database;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Script.Alert;
using UnityEngine;
using UnityEngine.Android;

namespace Script.Controller.SaveLoad {
    public class SaveManager {
        public ConcurrentDictionary<string, string> SaveData { get; private set; } = new();

        private DatabaseReference dbRef;

        public string Path { get; init; }
        public string FileName => IncludeTimeStamp();
        private string _fileName;
        public string FilePath => System.IO.Path.Combine(Path, FileName);
        public int MaxSaves { get; init; }
        private bool _log => GameController.Instance.Log;

        public string CurrentSavePath {
            get => _currentSavePath ?? FilePath;
            private set => _currentSavePath = value;
        }

        [CanBeNull] public string _currentSavePath; 
        public string CurrentLoadPath { 
            get => _currentLoadPath ?? FilePath;
            private set => _currentLoadPath = value;
        }
        [CanBeNull] public string _currentLoadPath; 

        public SaveManager(int maxSaves = 10) : this(Application.persistentDataPath, maxSaves: maxSaves) { }

        public SaveManager(string path, string fileName = "data.json", int maxSaves = 10) {
            Path = path;
            _fileName = fileName;
            MaxSaves = maxSaves;
            // InitializeFirebase();
        }

        private void InitializeFirebase() {
            FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
                var dependencyStatus = task.Result;
                if (dependencyStatus == DependencyStatus.Available) {
                    FirebaseApp.LogLevel = Firebase.LogLevel.Debug;

                    dbRef = FirebaseDatabase.DefaultInstance.RootReference;
                    Debug.Log("Firebase initialized successfully.");
                }
                else {
                    Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
                    dbRef = null;
                }
            });
        }

        private static JsonSerializerSettings Settings {
            get => new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All };
        }


        public static string Serialize<TSource>(TSource data) {
            var x = JsonConvert.SerializeObject(data, Settings);
            return x;
        }

        public static TSource Deserialize<TSource>(string data) {
            return JsonConvert.DeserializeObject<TSource>(data, Settings);
        }

#warning implement encryption
        private static string Encrypt(string data) => data;
        private static string Decrypt(string coded) => coded;

        public async Task SaveToFirebase() {
            if (dbRef == null) {
                Debug.LogError("Firebase is not initialized. Cannot save data.");
                return;
            }

            string json = JsonConvert.SerializeObject(SaveData);
            Debug.Log("Json fr: " + json);
            var saveTask = dbRef.Child("users").Child("1").SetRawJsonValueAsync(json).ContinueWith(task => {
                if (task.IsFaulted) { Debug.Log("Error: Failed to save data to Firebase."); }
                else if (task.IsCompleted) { Debug.Log("Data saved to Firebase."); }
            });
            await saveTask;
        }

        private string IncludeTimeStamp() {
            var startTime = GameController.Instance.SessionStartTime;
            if (startTime == null || startTime == DateTime.MinValue) return _fileName;
            var parts = _fileName.Split('.');
            if (parts.Length < 1) return _fileName;
            parts[0] = parts[0] + "-" + startTime.Ticks;
            return string.Join(".", parts);
        } 
        
        public async Task SaveToLocal() {
            try {
                if (Application.platform == RuntimePlatform.Android) {
                    if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite)
                        || !Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead)) return;
                }

                if (GameController.Instance.SessionStartTime == DateTime.MinValue) {
                    Debug.LogWarning("[Cannot save] Session not started!");
                    return;
                }

                RemoveOldSaves();

                CurrentSavePath = FilePath;
                
                using (var file = System.IO.File.Open(CurrentSavePath, FileMode.OpenOrCreate)) { }

                if (_log) Debug.Log($"Saving data to: {CurrentSavePath}");
                var str = Serialize(SaveData);
                if (_log) Debug.Log($"Serialized data: {str}");

                await using (StreamWriter sw = new StreamWriter(FilePath, false)) {
                    await sw.WriteAsync(Encrypt(str));
                    await sw.FlushAsync();
                    if (_log) Debug.Log("Data saved successfully.");
                }
            }
            catch (System.Exception e) {
                Debug.LogException(new System.Exception($"[Cannot save] Error saving data from local file", e));
            }
        }

        private void RemoveOldSaves() {
            var filePaths = GetAllSavePaths();
            while (filePaths.Count >= MaxSaves && filePaths.Count > 0) {
                var removingFilePath = filePaths.ElementAtOrDefault(filePaths.Count - 1);
                if (string.IsNullOrEmpty(removingFilePath)) {
                    Debug.LogError($"{nameof(removingFilePath)} is empty");
                    break;
                }
                RemoveSave(removingFilePath);
                filePaths.Remove(removingFilePath);
            }
        }

        private void RemoveSave(string path) {
            if (File.Exists(path)) { File.Delete(path); }
            else Debug.LogError($"Cannot remove file because it doesn't exist: {path}");
        }

        private List<string> GetAllSavePaths() {
            var parts = _fileName.Split('.').ToList();
            var name = parts.ElementAtOrDefault(0) ?? "";
            name += "-";
            if (parts.Count > 0) parts.RemoveAt(0);

            return Directory.GetFiles(Path, $"{name}*{string.Join(".", parts)}").OrderByDescending(file => file).ToList();
        }

        public async Task LoadFromLocal() {
            try {
                if (Application.platform == RuntimePlatform.Android) {
                    if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite)
                        || !Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead)) return;
                }
                
                CurrentLoadPath = GetAllSavePaths().Count > 0 ? GetAllSavePaths().First() : FilePath;
                if (_log) Debug.Log($"Current load path: {CurrentLoadPath}");

                using (var file = System.IO.File.Open(CurrentLoadPath, FileMode.OpenOrCreate)) { }

                using StreamReader sr = new StreamReader(CurrentLoadPath);
                var str = await sr.ReadToEndAsync();
                var saveData = Deserialize<ConcurrentDictionary<string, string>>(Decrypt(str));

                foreach (var data in saveData?.Keys ?? new List<string>()) {
                    if (saveData is null) break;
                    if (!SaveData.TryGetValue(data, out var value)) SaveData.TryAdd(data, saveData[data]);
                    else if (value != saveData[data]) SaveData[data] = saveData[data];
                }
                // AlertManager.Instance.Raise(new GameAlert.Builder(AlertType.Notification)
                //     .WithHeader("Loading data")
                //     .WithMessage("Loading data successfully")
                //     .WithCloseButton()
                //     .Build());
            }
            catch (System.Exception e) {
                Debug.LogException(new System.Exception($"[Cannot load] Error loading data from local file", e));
            }
        }

#warning load save from cloud
        public async Task SaveToCloud() { }
        public async Task LoadFromCloud() { }
    }
}