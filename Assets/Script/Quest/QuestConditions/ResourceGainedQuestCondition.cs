using System;
using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using Script.Controller;
using Script.Resources;
using UnityEngine;

namespace Script.Quest {
    [CreateAssetMenu(menuName = "Quest/Condition/Resource Consumed Condition", fileName = "Resource Consumed Condition")]
    public class ResourceGainedQuestCondition : QuestCondition {
        Func<Resource, string> keyName = (resource) => $"Gained{Enum.GetName(typeof(Resource), resource)}";
        
        [SerializeReference]
        public SerializedDictionary<Resource, long> Resources;

        protected override string OnProgressCheck(Quest quest) {
            List<string> list = new List<string>();

            foreach (var pair in Resources) {
                long value;
                if (!quest.TryGetQuestData(keyName(pair.Key)+"Remaining", out value)) {
                    value = 0;
                }
                
                if (value > pair.Value) value = pair.Value;
                var str = $"{value}/{pair.Value}";
                if (Resources.Count > 1) str = $"{pair.Key} {str}";
                list.Add(str);
            }
            
            return string.Join("\n", list.ToArray());
        }

        public override bool Evaluate(Quest quest) {
            var controller = GameController.Instance.ResourceController;
            Debug.LogWarning($"Evaluating quest {quest.Name}");
            bool passed = true;

            foreach (var resource in Resources.Keys) {
                Debug.LogWarning($"Quest: {quest.Name}. Resource: {resource}");
                var resourceKey = keyName(resource);
                var remKey = keyName(resource)+"Remaining";
                if (!controller.TryGetAmount(resource, out long current)) {
                    continue;
                }

                if (!quest.TryGetQuestData(resourceKey, out long resValue)) {
                    passed = false;
                    quest.AddData(resourceKey, current);
                    resValue = current;
                }

                if (!quest.TryGetQuestData(remKey, out long remValue)) {
                    passed = false;
                    quest.AddData(remKey, Resources[resource]);
                    remValue = Resources[resource];
                }

                if (current == resValue) continue;
                if (resValue > current) {
                    resValue = current;
                    quest.SetData(resourceKey, resValue);
                }
                else {
                    var newRemValue = remValue - (current - resValue);
                    if (remValue > 0) quest.SetData(remKey, newRemValue);
                    remValue = newRemValue;
                }
                if (remValue > 0) passed = false;
            }

            Debug.LogWarning($"Quest: {quest.Name}. Pass: {passed}");
            
            return passed;
        }
    }
}