using System.Collections.Generic;
using System.Linq;
using MyBox;
using Script.Controller;
using Script.Machine;
using Script.Machine.Machines.Generator;
using Script.Utils;
using UnityEngine;

namespace Script.HumanResource.Administrator.Policies
{
    [CreateAssetMenu(menuName = "HumanResource/Policies/IncreaseGeneratorPowerPolicy")]
    public class IncreaseGeneratorPowerPolicy : Policy
    {
        [SerializeField] public Vector2 Additives;
        [SerializeField] public Vector2 Multiplier;
        [SerializeField] private bool _forAllGenerators;

        [ConditionalField(nameof(_forAllGenerators), true)]
        [SerializeField] private CollectionWrapperList<Generator> _generators;

        // Track original Power values to revert changes
        private Dictionary<Generator, int> _originalPowerValues = new();

        public override void OnAssign()
        {
            var controller = GameController.Instance.MachineController;
            var list = controller.Machines
                .Where(m => m is Generator)
                .Cast<Generator>();

            if (!_forAllGenerators)
            {
                list = list.Where(l => _generators.Value.Any(s => s.GetType() == l.GetType()));
            }

            list.ForEach(ApplyBonus);

            controller.onMachineAdded += ApplyBonus;
        }

        private List<Generator> _appliedMachines = new();

        public void ApplyBonus(IMachine machine)
        {
            if (machine is not Generator generator) return;

            // Store the original Power value before applying the bonus
            if (!_originalPowerValues.ContainsKey(generator))
            {
                _originalPowerValues[generator] = generator.Power;
            }

            _appliedMachines.Add(generator);
            UpdatePower(generator, true);
        }

        public void UnapplyBonus(IMachine machine)
        {
            if (machine is not Generator generator) return;

            _appliedMachines.Remove(generator);

            // Revert to the original Power value
            if (_originalPowerValues.ContainsKey(generator))
            {
                generator.Power = _originalPowerValues[generator];
                _originalPowerValues.Remove(generator); // Clean up
            }
        }

        private void UpdatePower(Generator generator, bool apply)
        {
            if (apply)
            {
                // Apply the multiplicative bonus
                float multiplier = UnityEngine.Random.Range(Multiplier.x, Multiplier.y);
                int newPower = Mathf.RoundToInt(generator.Power * multiplier);

                // Apply the additive bonus
                float additive = UnityEngine.Random.Range(Additives.x, Additives.y);
                newPower += Mathf.RoundToInt(additive);


                generator.Power = newPower;
            }
        }

        protected override void ResetValues()
        {
            _appliedMachines.ToList().ForEach(UnapplyBonus);
            _originalPowerValues.Clear();

            var controller = GameController.Instance.MachineController;
        }

        public override SaveData Save() {
            var data = base.Save().CastToSubclass<IncreaseGeneratorPowerSaveData, SaveData>();
            if (data is null) return base.Save();

            data.Additives = new(Additives);
            data.Multiplier = new (Multiplier);
            return data;
        }

        public override void Load(SaveData data) {
            base.Load(data);
            
            if (data is not IncreaseGeneratorPowerSaveData coreData) return;

            Additives = coreData.Additives;
            Multiplier = coreData.Multiplier;
        }

        public class IncreaseGeneratorPowerSaveData : SaveData {
            public V2 Additives;
            public V2 Multiplier;
        }
    }
}