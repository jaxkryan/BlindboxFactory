using System;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using Script.Controller;
using Script.Machine;
using Script.Resources;
using UnityEngine;

namespace Script.Quest {
    public abstract class QuestCondition : ScriptableObject {
        public abstract bool Condition(Quest quest);
    }

    [CreateAssetMenu(menuName = "Quest/Condition", fileName = "Machine Built Condition")]
    public class MachineBuiltQuestCondition : QuestCondition {
        Func<MachineBase, string> keyName = (machine) => $"{machine.name}";
        
        [Tooltip("For prefabs comparing only")] [SerializeReference]
        public SerializedDictionary<MachineBase, int> BuildMachines;

        public override bool Condition(Quest quest) {
            var machineController = GameController.Instance.MachineController;
            bool passed = true;
            foreach (var machine in BuildMachines) {
                var key = keyName(machine.Key);
                var machineCount = machineController.Machines
                    .Count(m => m.name == machine.Key.name);

                if (!quest.TryGetQuestData(key, out int value)) {
                    quest.AddData(key, machineCount);
                    passed = false;
                }
                else if (machineCount - value < machine.Value) passed = false;
            }

            return passed;
        }
    }

    [CreateAssetMenu(menuName = "Quest/Condition", fileName = "Resource Consumed Condition")]
    public class ResourceConsumedQuestCondition : QuestCondition {
        Func<Resource, string> keyName = (resource) => $"Consumed{Enum.GetName(typeof(Resource), resource)}";
        
        [SerializeReference]
        public SerializedDictionary<Resource, long> Resources;
        public override bool Condition(Quest quest) {
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

                if (current == resValue) continue;
                if (resValue < current) {
                    resValue = current;
                    quest.SetData(resourceKey, resValue);
                }
                else {
                    var newRemValue = remValue - (resValue - current);
                    if (remValue > 0) quest.SetData(remKey, newRemValue);
                    remValue = newRemValue;
                }
                if (remValue > 0) passed = false;
            }

            return passed;
        }
    }

    [CreateAssetMenu(menuName = "Quest/Condition", fileName = "Resource Consumed Condition")]
    public class ResourceGainedQuestCondition : QuestCondition {
        Func<Resource, string> keyName = (resource) => $"Gained{Enum.GetName(typeof(Resource), resource)}";
        
        [SerializeReference]
        public SerializedDictionary<Resource, long> Resources;
        public override bool Condition(Quest quest) {
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

            return passed;
        }
    }

    [CreateAssetMenu(menuName = "Quest/Condition", fileName = "Resource Amount Condition")]
    public class ResourceAmountQuestCondition : QuestCondition {
        [SerializeReference] public SerializedDictionary<Resource, int> Resources;
        public override bool Condition(Quest quest) {
            var controller = GameController.Instance.ResourceController;
            return Resources.All(r => controller.TryGetAmount(r.Key, out long amount) && amount >= r.Value);
        }
    }

    public class PreviousQuestCondition : QuestCondition {
        [SerializeReference] public List<Quest> Quests;
        public override bool Condition(Quest quest) => Quests.All(q => q.State == QuestState.Complete);
    }
}