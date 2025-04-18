using System;
using System.Linq;
using Script.Machine.Machines;
using Script.Machine.Machines.Canteen;
using Script.Machine.Machines.Generator;
using Unity.VisualScripting;
using UnityEngine;

namespace Script.Machine.MachineDataGetter {
    public abstract class MachineData {
        public InformationPanel _panel;
        protected MachineBase _machine;
        protected MachineData(InformationPanel panel, MachineBase machine) {
            _panel = panel;
            _machine = machine;
        }


        public virtual void Draw() {
            _panel.Name.text = _machine.name.Replace("(Clone)", "").Trim(); ;
            _panel.Icon.sprite = _machine.GetComponent<SpriteRenderer>().sprite;
            _panel.Description.text = GetDescriptionText();
        }

        protected abstract string GetDescriptionText();
        
        public static MachineData Create(InformationPanel panel, MachineBase machine) {
            return machine switch {
                BlindBoxMachine blindBoxMachine => new BlindBoxMachineData(panel, machine),
                RestRoom restRoom => new RestroomData(panel, machine),
                ResourceExtractor resourceExtractor => new ResourceExtractorData(panel, machine),
                StoreHouse storeHouse => new StoreHouseData(panel, machine),
                Canteen canteen => new CanteenData(panel, machine),
                CanteenKitchen canteenKitchen => new CanteenKitchenData(panel, machine),
                Generator generator => new GeneratorData(panel, machine),
                StorageMachine storageMachine => new StorageMachineData(panel, machine),
                TestMachine testMachine => new TestMachineData(panel, machine),
                _ => throw new ArgumentOutOfRangeException(nameof(machine))
            };
        }
    }
    
    public class BlindBoxMachineData : MachineData {
        public BlindBoxMachineData(InformationPanel panel, MachineBase machine) : base(panel, machine) { }
        protected override string GetDescriptionText() 
        {
            if (_machine is not BlindBoxMachine blindBoxMachine)
                return "Invalid Machine Data\n";

            if (blindBoxMachine.Product is not BlindBox bbProduct) return "aa";

            string recipeInfo = blindBoxMachine.recipes.Count > 0
                ? string.Join("\n", blindBoxMachine.recipes.Select(r => $"Recipe: {r.BoxTypeName}"))
                : "No recipes available\n";

            return $"Blind Box Machine\n" +
                   $"Amount: {blindBoxMachine.amount}\n" +
                   $"Max Amount: {blindBoxMachine.maxAmount}\n" +
                   $"Producing: {bbProduct.BoxTypeName}" + $" x {blindBoxMachine.amount}\n" +
                   $"Current Progress: {blindBoxMachine.CurrentProgress}/{blindBoxMachine.MaxProgress}\n" +
                   $"{recipeInfo}";
        }
    }
    
    public class ResourceExtractorData : MachineData {
        public ResourceExtractorData(InformationPanel panel, MachineBase machine) : base(panel, machine) { }
        protected override string GetDescriptionText() 
        {
            return $"Extractor" +
                   $"\nDiamond : 5%" +
                   $"\nCloud : 15%" +
                   $"\nRainbow : 15%" +
                   $"\nRuby : 15%" +
                   $"\nGummy : 20%" +
                   $"\nStar : 20%";
        }
    }

    public class CanteenData : MachineData
    {
        private Canteen _canteen;

        public CanteenData(InformationPanel panel, MachineBase machine) : base(panel, machine)
        {
            _canteen = machine as Canteen;
        }

        protected override string GetDescriptionText()
        {
            if (_canteen == null) return "No data available.";


            if (_canteen.GetCanteenData() is not Canteen.CanteenData saveData) return "Failed to retrieve data.";

            var availableMeals = saveData.Storage.AvailableMeals;
            var maxCapacity = saveData.Storage.MaxCapacity;
            float percentage = (maxCapacity > 0) ? (availableMeals / (float)maxCapacity) * 100 : 0;

            return $"Canteen Information:\n" +
                   $"Available Meals: {availableMeals}\n" +
                   $"Max Capacity: {maxCapacity}\n" +
                   $"Meal Storage: {percentage:F1}% full";
        }
    }


    public class CanteenKitchenData : MachineData {
        public CanteenKitchenData(InformationPanel panel, MachineBase machine) : base(panel, machine) { }
        protected override string GetDescriptionText() { return "aaa"; }
    }
    
    public class GeneratorData : MachineData {
        private Generator _generator;

        public GeneratorData(InformationPanel panel, MachineBase machine) : base(panel, machine)
        {
            _generator = machine as Generator;
        }

        protected override string GetDescriptionText()
        {
            if (_generator == null) return "No data available.";

            if (_generator.GetGeneratorData() is not Generator.GeneratorData saveData) return "Failed to retrieve data.";

            int powerOutput = saveData.Power;

            return $"Generator Information:\n" +
                   $"Power Output: {powerOutput} kW";
        }
    }
    
    public class StorageMachineData : MachineData {
        public StorageMachineData(InformationPanel panel, MachineBase machine) : base(panel, machine) { }
        protected override string GetDescriptionText() { return "aaa"; }
    }
    
    public class TestMachineData : MachineData {
        public TestMachineData(InformationPanel panel, MachineBase machine) : base(panel, machine) { }
        protected override string GetDescriptionText() { return ""; }
    }

    public class RestroomData : MachineData
    {
        public RestroomData(InformationPanel panel, MachineBase machine) : base(panel, machine) { }

        protected override string GetDescriptionText()
        {
            if (_machine is not null && _machine.Slots is not null)
            {
                return $"Slots Available: {_machine.Slots.Count()}";
            }
            return "No slots available.";
        }
    }


    public class StoreHouseData : MachineData
    {
        public StoreHouseData(InformationPanel panel, MachineBase machine) : base(panel, machine) { }

        protected override string GetDescriptionText()
        {
            if (_machine is StoreHouse storeHouse)
            {
                return $"Stored Resources: {storeHouse.resorceamount}\nStored Boxes: {storeHouse.boxamount}";
            }
            return "No storage data available.";
        }
    }
}