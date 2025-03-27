using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace Script.Controller.SaveLoad {
    public class SaveManager {
        public ConcurrentDictionary<string, string> SaveData { get; private set; } = new();
        
        public string Path { get; init; }
        public string FileName { get; init; }
        public string FilePath => System.IO.Path.Combine(Path, FileName);

        public SaveManager() : this(Application.persistentDataPath) { }

        public SaveManager(string path, string fileName = "data.json") {
            Path = path;
            FileName = fileName;
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

        public async Task SaveToLocal() {
            try {
                Debug.Log($"Saving data to: {FilePath}");
                var str = Serialize(SaveData);
                Debug.Log($"Serialized data: {str}");

                await using (StreamWriter sw = new StreamWriter(FilePath, false)) {
                    await sw.WriteAsync(str);
                    await sw.FlushAsync();
                    Debug.Log("Data saved successfully.");
                }
            }
            catch (System.Exception e) {
                Debug.LogError($"Error saving data: {e}");
                Debug.LogError(e);
            }
        }

        public async Task LoadFromLocal() {
            using StreamReader sr = new StreamReader(FilePath);
            var str = await sr.ReadToEndAsync();
            var saveData = Deserialize<ConcurrentDictionary<string, string>>(Decrypt(str));

            foreach (var data in saveData.Keys) {
                if (!SaveData.TryGetValue(data, out var value)) SaveData.TryAdd(data, saveData[data]);
                else if (value != saveData[data]) SaveData[data] = saveData[data];
            }
        }

#warning load save from cloud
        public async Task SaveToCloud() { }
        public async Task LoadFromCloud() { }
    }
}