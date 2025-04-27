using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using ZLinq;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Script.Controller.SaveLoad.SaveIgnore {
    [Obsolete("WIP, Low priority")]
    [CreateAssetMenu(menuName = "SaveLoad/Ignorable Settings", fileName = "Ignorable Settings")]
    public class IgnorableSettings : ScriptableObject {
        [SerializeField] public List<IgnoreEntry> IgnoreEntries;

        [Serializable]
        public class IgnoreEntry {
            [SerializeReference, SubclassSelector] public ControllerBase Controller;
            [Tooltip("Each steps is separated by a dot.")]
            [SerializeField] public List<string> Paths;
        }
    }

    [Obsolete("WIP, Low priority")]
    public static class SaveManagerExtension {
        public static ConcurrentDictionary<string, string>  RemoveIgnoredPaths(this ConcurrentDictionary<string, string> dictionary, params string[] keys) {
            var dict = new ConcurrentDictionary<string, string>(dictionary);
            foreach (var key in keys) {
                var parts = key.Trim().Split(".");
                var rootKey = parts.AsValueEnumerable().FirstOrDefault();
                if (string.IsNullOrEmpty(rootKey)) continue;
                
                if (!dictionary.TryGetValue(rootKey, out var data)) continue;
                if (parts.Length > 1) {
                    JObject root = JObject.Parse(data);
                    
                }
                else dictionary.TryRemove(rootKey, out data);
            }
            return dict;
        }

        private static void Iterate(ref ConcurrentDictionary<string, string> dictionary, JObject obj, string key) {
            
        }
    } 
}