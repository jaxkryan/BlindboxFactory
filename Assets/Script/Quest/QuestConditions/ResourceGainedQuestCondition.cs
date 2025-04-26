using System;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using Script.Controller;
using Script.Resources;
using UnityEngine;

namespace Script.Quest {
    [CreateAssetMenu(menuName = "Quest/Condition/Resource Gained Condition", fileName = "Resource Gained Condition")]
    public class ResourceGainedQuestCondition : QuestCondition {
        Func<Resource, string> keyName = (resource) => $"Gained{Enum.GetName(typeof(Resource), resource)}";
        
        [SerializeField]
        public SerializedDictionary<Resource, long> Resources;

        protected override string OnProgressCheck(Quest quest) {
            List<string> list = new List<string>();

            foreach (var pair in Resources) {
                long value;
                if (!quest.TryGetQuestData(keyName(pair.Key)+"Remaining", out value)) {
                    value = pair.Value;
                }

                value = pair.Value - value;
                
                if (value > pair.Value) value = pair.Value;
                var str = $"{value}/{pair.Value}";
                if (Resources.Count > 1) str = $"{pair.Key} {str}";
                list.Add(str);
            }
            
            return string.Join("\n", list.ToArray());
        }

        public override bool Evaluate(Quest quest) {
            var controller = GameController.Instance.ResourceController;
            bool passed = true;

            foreach (var resource in Resources.Keys) {
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

                var newRemValue = new[]{remValue - (current - resValue), remValue}.Min();
                if (resValue > current) quest.SetData(resourceKey, current);
                quest.SetData(remKey, newRemValue >= 0 ? newRemValue : 0);
                
                if (newRemValue > 0) passed = false;
            }
            
            return passed;
        }
    }
}