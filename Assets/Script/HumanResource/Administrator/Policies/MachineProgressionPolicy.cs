using System;
using System.Collections.Generic;
using System.Linq;
using AYellowpaper.SerializedCollections;
using MyBox;
using Script.Controller;
using Script.HumanResource.Worker;
using Script.Machine;
using Script.Utils;
using UnityEngine;

namespace Script.HumanResource.Administrator.Policies {
    [CreateAssetMenu(menuName = "HumanResource/Policies/MachineProgressionPolicy")]
    public class MachineProgressionPolicy : Policy {
        [SerializeField] private bool _forAllMachines;

        [ConditionalField(nameof(_forAllMachines), true)] [SerializeField]
        private CollectionWrapperList<MachineBase> _machineType;

        [SerializeField] public Vector2 Additives;
        [SerializeField] public Vector2 Multiplier;

        private Unity.Mathematics.Random randomGen; // Single Random instance for the policy
        private List<IMachine> _appliedMachines = new();

        public override void OnAssign() {
            // Initialize the Random instance with a seed (using instance ID for uniqueness)
            randomGen = Unity.Mathematics.Random.CreateFromIndex((uint)GetInstanceID());

            var controller = GameController.Instance.MachineController;
            List<MachineBase> machines = _forAllMachines
                ? controller.Machines.ToList()
                : controller.Machines.Where(m => _machineType.Value.Any(type => type.GetType() == m.GetType()))
                    .ToList();
            machines.ForEach(ApplyBonus);

            controller.onMachineAdded += ApplyBonus;
            controller.onMachineRemoved += UnapplyBonus;
        }

        protected override void ResetValues() {
            var controller = GameController.Instance.MachineController;
            _appliedMachines.Clone().ForEach(UnapplyBonus);
        }

        private void ApplyBonus(IMachine machine) {
            _appliedMachines.Add(machine);
            machine.onProgress += Evt;

            void Evt(float progress) {
                if (_appliedMachines.Contains(machine)) OnProgress(machine, progress);
                else machine.onProgress -= Evt;
            }
        }

        private void UnapplyBonus(IMachine machine) => _appliedMachines.Remove(machine);

        private void OnProgress(IMachine machine, float progress) {
            if (progress <= 0) return;

            var changeAmount = 0f;
            changeAmount += PickRandom(Additives);
            changeAmount += PickRandom(Multiplier) * (progress + changeAmount);

            machine.CurrentProgress += changeAmount;
        }

        private float PickRandom(Vector2 range) => randomGen.NextFloat(range.x, range.y);

        public override SaveData Save() {
            var data = base.Save().CastToSubclass<ProgressionData, SaveData>();
            if (data is null) return base.Save();

            data.Additives = new V2(Additives);
            data.Multiplier = new V2(Multiplier);
            data.ForAllMachines = _forAllMachines;
            var buildables = GameController.Instance.MachineController.Buildables;
            data.PrefabNames = new();
            if (_machineType?.Value != null) {
                foreach (var machine in _machineType.Value.Where(m => m != null)) {
                    var b = buildables.FirstOrDefault(b => Equals(b.gameObject, machine.gameObject));
                    if (b) data.PrefabNames.Add(b.Name);
                }
            }

            return data;
        }

        public override void Load(SaveData data) {
            if (data is ProgressionData progressionData) {
                Additives = progressionData.Additives;
                Multiplier = progressionData.Multiplier;
                _forAllMachines = progressionData.ForAllMachines;
                var buildables = GameController.Instance.MachineController.Buildables;
                var list = new List<MachineBase>();
                foreach (var build in buildables.Where(b => progressionData.PrefabNames.Contains(b.Name))) {
                    var machine = build.gameObject.GetComponent<MachineBase>();
                    if (machine is not null) list.Add(machine);
                }

                _machineType = new CollectionWrapperList<MachineBase> { Value = list };
            }

            base.Load(data);
        }

        public class ProgressionData : SaveData {
            public bool ForAllMachines;
            public List<string> PrefabNames;
            public V2 Additives;
            public V2 Multiplier;
        }
    }
}