using System;
using Script.Machine.Machines;
using Script.Machine.Machines.Canteen;
using Script.Machine.Machines.Generator;
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
            _panel.Name.text = _machine.name;
            _panel.Icon.image = _machine.GetComponent<SpriteRenderer>().sprite.texture;
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
        protected override string GetDescriptionText() { return ""; }
    }
    
    public class ResourceExtractorData : MachineData {
        public ResourceExtractorData(InformationPanel panel, MachineBase machine) : base(panel, machine) { }
        protected override string GetDescriptionText() { return ""; }
    }
    
    public class CanteenData : MachineData {
        public CanteenData(InformationPanel panel, MachineBase machine) : base(panel, machine) { }
        protected override string GetDescriptionText() { return ""; }
    }
    
    public class CanteenKitchenData : MachineData {
        public CanteenKitchenData(InformationPanel panel, MachineBase machine) : base(panel, machine) { }
        protected override string GetDescriptionText() { return ""; }
    }
    
    public class GeneratorData : MachineData {
        public GeneratorData(InformationPanel panel, MachineBase machine) : base(panel, machine) { }
        protected override string GetDescriptionText() { return ""; }
    }
    
    public class StorageMachineData : MachineData {
        public StorageMachineData(InformationPanel panel, MachineBase machine) : base(panel, machine) { }
        protected override string GetDescriptionText() { return ""; }
    }
    
    public class TestMachineData : MachineData {
        public TestMachineData(InformationPanel panel, MachineBase machine) : base(panel, machine) { }
        protected override string GetDescriptionText() { return ""; }
    }
    
    public class RestroomData : MachineData {
        public RestroomData(InformationPanel panel, MachineBase machine) : base(panel, machine) { }
        protected override string GetDescriptionText() { return ""; }
    }
    
    public class StoreHouseData : MachineData {
        public StoreHouseData(InformationPanel panel, MachineBase machine) : base(panel, machine) { }
        protected override string GetDescriptionText() { return ""; }
    }
    }