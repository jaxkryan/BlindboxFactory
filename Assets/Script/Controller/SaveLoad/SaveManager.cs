using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Firebase;
using Firebase.Database;
using Newtonsoft.Json;
using Script.Alert;
using UnityEngine;
using UnityEngine.Android;

namespace Script.Controller.SaveLoad {
    public class SaveManager {
        public ConcurrentDictionary<string, string> SaveData { get; private set; } = new();

        private DatabaseReference dbRef;

        public string Path { get; init; }
        public string FileName { get; init; }
        public string FilePath => System.IO.Path.Combine(Path, FileName);

        public SaveManager() : this(Application.persistentDataPath) { }

        public SaveManager(string path, string fileName = "data.json") {
            Path = path;
            FileName = fileName;
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

        public async Task SaveToLocal() {
            try {
                if (Application.platform == RuntimePlatform.Android) {
                    if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite)
                        || !Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead)) return;
                }

                using (var file = System.IO.File.Open(FilePath, FileMode.OpenOrCreate)) { }

                Debug.Log($"Saving data to: {FilePath}");
                var str = Serialize(SaveData);
                Debug.Log($"Serialized data: {str}");

                await using (StreamWriter sw = new StreamWriter(FilePath, false)) {
                    await sw.WriteAsync(Encrypt(str));
                    await sw.FlushAsync();
                    Debug.Log("Data saved successfully.");
                }
            }
            catch (System.Exception e) {
                Debug.LogException(new System.Exception($"Error saving data from local file", e));
            }
        }

        public async Task LoadFromLocal() {
            try {
                if (Application.platform == RuntimePlatform.Android) {
                    if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite)
                        || !Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead)) return;
                }

                using (var file = System.IO.File.Open(FilePath, FileMode.OpenOrCreate)) { }

                using StreamReader sr = new StreamReader(FilePath);
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
                Debug.LogException(new System.Exception($"Error loading data from local file", e));
            }
        }

#warning load save from cloud
        public async Task SaveToCloud() { }
        public async Task LoadFromCloud() { }
    }
}