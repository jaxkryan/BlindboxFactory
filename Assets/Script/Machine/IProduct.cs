using System.Collections.Generic;
using Script.Machine.ResourceManager;
using Script.Resources;
using UnityEngine;

namespace Script.Machine {
    public interface IProduct {
        float MaxProgress { get; }
        List<ResourceUse> ResourceUse { get; }
        void OnProductCreated();
        bool CanCreateProduct { get; }

        SaveData Save();
        void Load(SaveData data);
        
        public class SaveData {
            public string Type;
            public float MaxProgress;
            public bool CanCreateProduct;
            public List<ResourceUseData> ResourceUse;
            public class ResourceUseData {
                public Resource Resource;
                public int Amount;
                public bool IsResourceUseOnProductCreated;
                public float TimeInterval;
                public float CurrentTime;
            }
        }
    }
}