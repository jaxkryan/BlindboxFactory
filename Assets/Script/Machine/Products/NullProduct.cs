using System;
using System.Collections.Generic;
using Script.Machine.ResourceManager;

namespace Script.Machine.Products {
    [Serializable]
    public class NullProduct : ProductBase {
        public override float MaxProgress => 0f;
        public override void OnProductCreated() { }
        public override List<ResourceUse> ResourceUse => new List<ResourceUse>();
        public override void Load(IProduct.SaveData data) {  }
    }
}