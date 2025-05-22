using System;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using BuildingSystem.Models;
using Script.Controller;
using Script.Machine;
using UnityEngine;

namespace Script.Quest {
    [CreateAssetMenu(menuName = "Quest/Condition/Machine Built Condition", fileName = "Machine Built Condition")]
    public class MachineBuiltQuestCondition : QuestCondition {
        Func<BuildableItem, string> keyName = (machine) => $"{machine.Name}";

        [Tooltip("For prefabs comparison only")] [SerializeField]
        public SerializedDictionary<BuildableItem, int> BuildMachines;

        protected override string OnProgressCheck(Quest quest) {
            List<string> list = new List<string>();

            foreach (var pair in BuildMachines) {
                int value;
                var machineCount = GameController.Instance.MachineController.Machines
                    .Count(m => m.PrefabName == pair.Key.Name);
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
                    .Count(m => m.PrefabName == machine.Key.Name);
                if (!quest.TryGetQuestData(key, out int value)) {
                    if (machineCount < machine.Value) {
                        quest.AddData(key, machineCount);
                        passed = false;
                    }
                }
                else if (machineCount - value < machine.Value) passed = false;
            }

            return passed;
        }
    }
}