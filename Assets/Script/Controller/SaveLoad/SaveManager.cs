using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using ZLinq;
using System.Threading.Tasks;
using Firebase;
using Firebase.Database;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Script.Utils;
using UnityEngine;
using UnityEngine.Android;

namespace Script.Controller.SaveLoad
{
    public class SaveManager
    {
        private ConcurrentDictionary<string, string> _saveData = new();
        private string _playerId;
        private DatabaseReference dbRef;

        public string Path { get; init; }
        public string FileName => IncludeTimeStamp();
        private string _fileName;
        public string FilePath => System.IO.Path.Combine(Path, FileName);
        public int MaxSaves { get; init; }
        private bool _log => GameController.Instance.Log;

        public string CurrentSavePath
        {
            get => _currentSavePath ?? FilePath;
            private set => _currentSavePath = value;
        }

        [CanBeNull] public string _currentSavePath;

        public string CurrentLoadPath
        {
            get => _currentLoadPath ?? FilePath;
            private set => _currentLoadPath = value;
        }

        [CanBeNull] public string _currentLoadPath;

        public SaveManager(int maxSaves = 10) : this(Application.persistentDataPath, maxSaves: maxSaves) { }

        public SaveManager(string path, string fileName = "data.json", int maxSaves = 10)
        {
            Path = path;
            _fileName = fileName;
            MaxSaves = maxSaves;
            _playerId = "local_user";
            InitializeFirebase();
        }

        public SaveManager(string playerId)
        {
            _playerId = playerId;
            InitializeFirebase();
        }

        private bool _isFirebaseInitialized = false;
        private Task _firebaseInitTask;

        private void InitializeFirebase()
        {
            FirebaseDatabase.DefaultInstance.SetPersistenceEnabled(false);
            _firebaseInitTask = FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
                var dependencyStatus = task.Result;
                if (dependencyStatus == DependencyStatus.Available)
                {
                    FirebaseApp.LogLevel = Firebase.LogLevel.Debug;
                    dbRef = FirebaseDatabase.DefaultInstance.RootReference;
                    _isFirebaseInitialized = true;
                    Debug.Log("Firebase initialized successfully.");
                }
                else
                {
                    Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
                    dbRef = null;
                    _isFirebaseInitialized = false;
                }
            });
        }

        private async Task EnsureFirebaseInitialized()
        {
            if (!_isFirebaseInitialized)
            {
                await _firebaseInitTask;
            }

            if (!_isFirebaseInitialized)
            {
                throw new InvalidOperationException("Firebase initialization failed.");
            }

            dbRef.Child("users").KeepSynced(true);
        }

        private static JsonSerializerSettings Settings
        {
            get => new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.All };
        }

        public static string Serialize<TSource>(TSource data)
        {
            var x = JsonConvert.SerializeObject(data, Settings);
            return x;
        }

        public static TSource Deserialize<TSource>(string data)
        {
            return JsonConvert.DeserializeObject<TSource>(data, Settings);
        }

#warning implement encryption
        private static string Encrypt(string data) => data;
        private static string Decrypt(string coded) => coded;

        public void AddOrUpdate(string key, string value)
            => _saveData.AddOrUpdate(key, value, (_, _) => value);

        public bool TryGetValue(string key, out string value)
            => _saveData.TryGetValue(key, out value);

        public async Task SaveToFirebase()
        {
            try
            {
                await EnsureFirebaseInitialized();
                string json = JsonConvert.SerializeObject(_saveData.AsValueEnumerable().ToDictionary(p => p.Key, p => p.Value));
                if (_log) Debug.Log("Json fr: " + json);
                await dbRef.Child("users").Child(_playerId).SetRawJsonValueAsync(json);
                if (_log) Debug.Log($"Data saved to Firebase for player: {_playerId}");
            }
            catch (System.Exception e)
            {
                Debug.LogException(new System.Exception($"[Cannot save] Error saving to Firebase", e));
            }
        }

        public async Task LoadFromFirebase()
        {
            try
            {
                await EnsureFirebaseInitialized();
                var snapshot = await dbRef.Child("users").Child(_playerId).GetValueAsync();
                if (!snapshot.Exists)
                {
                    if (_log) Debug.Log($"No save data found in Firebase for player: {_playerId}");
                    return;
                }


                string json = snapshot.GetRawJsonValue();
                if (string.IsNullOrEmpty(json))
                {
                    Debug.LogWarning("Firebase data is empty or invalid.");
                    return;
                }

                var saveData = Deserialize<Dictionary<string, string>>(json);
                if (saveData == null)
                {
                    Debug.LogError("Failed to deserialize Firebase data.");
                    return;
                }

                foreach (var key in saveData.Keys)
                {
                    if (!saveData.TryGetValue(key, out var value) || value is null) continue;
                    AddOrUpdate(key, value);
                }

                if (_log) Debug.Log($"Data loaded successfully from Firebase for player: {_playerId}");
            }
            catch (System.Exception e)
            {
                Debug.LogException(new System.Exception($"[Cannot load] Error loading from Firebase", e));
            }
        }

        private string IncludeTimeStamp()
        {
            var startTime = GameController.Instance.SessionStartTime;
            if (startTime == null || startTime == DateTime.MinValue) return _fileName;
            var parts = _fileName.Split('.');
            if (parts.Length < 1) return _fileName;
            parts[0] = parts[0] + "-" + startTime.Ticks;
            return string.Join(".", parts);
        }

        public async Task SaveToLocal()
        {
            try
            {
                if (Application.platform == RuntimePlatform.Android)
                {
                    if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite)
                        || !Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead)) return;
                }

                if (GameController.Instance.SessionStartTime == DateTime.MinValue)
                {
                    Debug.LogWarning("[Cannot save] Session not started!");
                    return;
                }

                RemoveOldSaves();

                CurrentSavePath = FilePath;

                using (var file = System.IO.File.Open(CurrentSavePath, FileMode.OpenOrCreate))
                {
                    file.Close();
                }

                if (_log) Debug.Log($"Saving data to: {CurrentSavePath}");
                var str = Serialize(_saveData.AsValueEnumerable().ToDictionary(p => p.Key, p => p.Value));
                if (_log) Debug.Log($"Serialized data: {str}");

                await using (StreamWriter sw = new StreamWriter(FilePath, false))
                {
                    await sw.WriteAsync(Encrypt(str));
                    await sw.FlushAsync();
                    sw.Close();
                    if (_log) Debug.Log("Data saved successfully.");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogException(new System.Exception($"[Cannot save] Error saving data from local file", e));
            }
        }

        private void RemoveOldSaves()
        {
            var filePaths = GetAllSavePaths();

            while (filePaths.Count >= MaxSaves && filePaths.Count > 0)
            {
                var removingFilePath = filePaths.ElementAtOrDefault(filePaths.Count - 1);
                if (string.IsNullOrEmpty(removingFilePath))
                {

                    Debug.LogError($"{nameof(removingFilePath)} is empty");
                    break;
                }

                RemoveSave(removingFilePath);
                filePaths.Remove(removingFilePath);
            }
        }

        private void RemoveSave(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            else Debug.LogError($"Cannot remove file because it doesn't exist: {path}");
        }


        private List<string> GetAllSavePaths() {
            var parts = _fileName.Split('.').AsValueEnumerable().ToList();
            var name = parts.AsValueEnumerable().ElementAtOrDefault(0) ?? "";

            name += "-";
            if (parts.Count > 0) parts.RemoveAt(0);

            return Directory.GetFiles(Path, $"{name}*{string.Join(".", parts)}").AsValueEnumerable().OrderByDescending(file => file)
                .ToList();
        }

        public async Task LoadFromLocal()
        {
            try
            {
                if (Application.platform == RuntimePlatform.Android)
                {
                    if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite)
                        || !Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead)) return;
                }

                CurrentLoadPath = GetAllSavePaths().Count > 0 ? GetAllSavePaths().AsValueEnumerable().First() : FilePath;
                if (_log) Debug.Log($"Current load path: {CurrentLoadPath}");
                if (!File.Exists(CurrentLoadPath))
                {
                    if (_log) Debug.Log($"Save file doesn't exist: {CurrentLoadPath}. Cancelling...");
                    return;
                }

                using (var file = System.IO.File.Open(CurrentLoadPath, FileMode.OpenOrCreate))
                {
                    file.Close();
                }

                using StreamReader sr = new StreamReader(CurrentLoadPath);
                var str = await sr.ReadToEndAsync();
                var saveData = Deserialize<Dictionary<string, string>>(Decrypt(str));


                if (saveData is not null) {
                    foreach (var data in saveData.Keys) {
                        if (!saveData.TryGetValue(data, out var value) || value is null) break;

                        AddOrUpdate(data, value);
                    }
                }

                sr.Close();
            }
            catch (System.Exception e)
            {
                Debug.LogException(new System.Exception($"[Cannot load] Error loading data from local file", e));
            }
        }

#warning load save from cloud
        public async Task SaveToCloud() { }
        public async Task LoadFromCloud() { }
    }
}