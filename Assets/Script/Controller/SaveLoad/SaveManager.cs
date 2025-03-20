using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace Script.Controller.SaveLoad {
    public class SaveManager {
        public Dictionary<string, string> SaveData { get; private set; } = new();
        public string Path => Application.persistentDataPath;
        public string FileName { get; set; } = "data";
        public string FilePath => System.IO.Path.Combine(Path, FileName);

        public static string Serialize(Dictionary<string, string> data) { return JsonConvert.SerializeObject(data); }

        public static Dictionary<string, string> Deserialize(string data) {
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(data);
        }

        #warning implement encryption
        private static string Encrypt(string data) => data;
        private static string Decrypt(string coded) => coded;

        public async Awaitable SaveToLocal() {
            var str = Serialize(SaveData);

            using (StreamWriter sw = new StreamWriter(FilePath, false)) {
                await sw.WriteAsync(Encrypt(str));
            }
        }

        public async Awaitable LoadFromLocal() {
            using StreamReader sr = new StreamReader(FilePath);
            var str = await sr.ReadToEndAsync();
            var saveData = Deserialize(Decrypt(str));

            foreach (var data in saveData.Keys) {
                if (!SaveData.TryGetValue(data, out var value)) SaveData.Add(data, saveData[data]);
                else if (value != saveData[data]) SaveData[data] = saveData[data];
            }
        }

        #warning load save from cloud
        public async Awaitable SaveToCloud() { }
        public async Awaitable LoadFromCloud() { }
    }
}