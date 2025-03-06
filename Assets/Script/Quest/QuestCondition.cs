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
        public virtual string Description { get => _description; }
        [SerializeField] protected string _description;

        public string Progress(Quest quest) {
            Evaluate(quest);
            return OnProgressCheck(quest);
        }

        protected abstract string OnProgressCheck(Quest quest);
        public abstract bool Evaluate(Quest quest);
    }

    [CreateAssetMenu(menuName = "Quest/Condition", fileName = "Machine Built Condition")]
    public class MachineBuiltQuestCondition : QuestCondition {
        Func<MachineBase, string> keyName = (machine) => $"{machine.name}";
        
        [Tooltip("For prefabs comparing only")] [SerializeReference]
        public SerializedDictionary<MachineBase, int> BuildMachines;

        protected override string OnProgressCheck(Quest quest) {
            List<string> list = new List<string>();

            foreach (var pair in BuildMachines) {
                int value;
                var machineCount = GameController.Instance.MachineController.Machines
                    .Count(m => m.name == pair.Key.name);
                if (!quest.TryGetQuestData(keyName(pair.Key), out value)) {
                    value = machineCount;
                }

                var amount = machineCount - value;
                if (amount < 0) amount = 0;
                if (amount > pair.Value) amount = pair.Value;

                var str = $"{amount}/{pair.Value}";
                if (BuildMachines.Count > 1) str = $"{pair.Key.name} {str}";
                list.Add(str);
            }
            
            return string.Join("\n", list.ToArray());
        }

        public override bool Evaluate(Quest quest) {
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
        [SerializeReference] public SerializedDictionary<Resource, long> Resources;

        protected override string OnProgressCheck(Quest quest) {
            List<string> list = new List<string>();

            foreach (var pair in Resources) {
                if (!GameController.Instance.ResourceController.TryGetAmount(pair.Key, out var value)) value = 0;

                if (value > pair.Value) value = pair.Value;
                var str = $"{value}/{pair.Value}";
                if (Resources.Count > 1) str = $"{pair.Key} {str}";
                list.Add(str);
            }
            
            return string.Join("\n", list.ToArray());
        }

        public override bool Evaluate(Quest quest) {
            var controller = GameController.Instance.ResourceController;
            return Resources.All(r => controller.TryGetAmount(r.Key, out var amount) && amount >= r.Value);
        }
    }

    [CreateAssetMenu(menuName = "Quest/Condition", fileName = "Previous Quest Condition")]
    public class PreviousQuestCondition : QuestCondition {
        [SerializeReference] public List<Quest> Quests;

        protected override string OnProgressCheck(Quest quest) {
            var list = new List<string>();

            foreach (var q in Quests) {
                list.Add($"{q.Name} {q.State}");
            }

            return string.Join("\n", list);
        }
        public override bool Evaluate(Quest quest) => Quests.All(q => q.State == QuestState.Complete);
    }
}