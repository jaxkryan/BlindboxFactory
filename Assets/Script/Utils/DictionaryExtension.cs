using System.Collections.Generic;
using UnityEngine;

namespace Script.Utils {
    public static class DictionaryExtension {
        public static Dictionary<TKey, TValue> WithoutKeys<TKey, TValue>(this Dictionary<TKey, TValue> dict,
            params TKey[] keys) {
            var ret = new Dictionary<TKey, TValue>(dict);

            foreach (var key in keys) {
                if (dict.ContainsKey(key)) ret.Remove(key);
            }
            
            return ret;
        }
    }
}