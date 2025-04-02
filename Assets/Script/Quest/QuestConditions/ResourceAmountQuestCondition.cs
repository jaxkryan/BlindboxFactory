using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using Script.Controller;
using Script.Resources;
using UnityEngine;

namespace Script.Quest {
    [CreateAssetMenu(menuName = "Quest/Condition/Resource Amount Condition", fileName = "Resource Amount Condition")]
    public class ResourceAmountQuestCondition : QuestCondition {
        [SerializeField] public SerializedDictionary<Resource, long> Resources;

        protected override string OnProgressCheck(Quest quest) {
            List<string> list = new List<string>();

            foreach (var pair in Resources) {
                if (!GameController.Instance.ResourceController.TryGetAmount(pair.Key, out var value)) value = 0;

                if (value > pair.Value) value = pair.Value;
                var str = $"{ResourceController.FormatNumber(value)}/{ResourceController.FormatNumber(pair.Value)}";
                str = $"{pair.Key} {str}";
                list.Add(str);
                Debug.LogWarning(str);
            }
            
            return string.Join("\n", list.ToArray());
        }

        public override bool Evaluate(Quest quest) {
            var controller = GameController.Instance.ResourceController;
            return Resources.All(r => controller.TryGetAmount(r.Key, out var amount) && amount >= r.Value);
        }
    }
}