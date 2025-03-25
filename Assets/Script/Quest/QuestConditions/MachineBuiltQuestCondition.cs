using System;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using Script.Controller;
using Script.Machine;
using UnityEngine;

namespace Script.Quest {
    [CreateAssetMenu(menuName = "Quest/Condition/Machine Built Condition", fileName = "Machine Built Condition")]
    public class MachineBuiltQuestCondition : QuestCondition {
        Func<MachineBase, string> keyName = (machine) => $"{machine.name}";
        
        [Tooltip("For prefabs comparison only")]
        [SerializeReference] public SerializedDictionary<MachineBase, int> BuildMachines;

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
}