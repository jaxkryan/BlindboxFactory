using System;
using System.Collections.Generic;
using System.Linq;
using Script.Machine.ResourceManager;
using Script.Resources;
using UnityEngine;

namespace Script.Machine.Products {
    [Serializable]
    public abstract class ProductBase : IProduct {
        public abstract float MaxProgress { get; }
        public abstract List<ResourceUse> ResourceUse { get; }
        public virtual bool CanCreateProduct { get => true; }
        protected MachineBase _machine;
        public void SetParent(MachineBase machine) => _machine = machine;

        public abstract void OnProductCreated();

        public virtual IProduct.SaveData Save()  =>
            new IProduct.SaveData() {
                Type = this.GetType().FullName,
                MaxProgress = MaxProgress,
                CanCreateProduct = CanCreateProduct,
                ResourceUse = ResourceUse.Select(r => new IProduct.SaveData.ResourceUseData() {
                    Amount = r.Amount,
                    Resource = r.Resource,
                    IsResourceUseOnProductCreated = r.GetType().IsSubclassOf(typeof(ResourceUseOnProductCreated)),
                    CurrentTime = r is ResourceUseOvertime rOvertime1 ? rOvertime1.Timer.Time : 0f,
                    TimeInterval = r is ResourceUseOvertime rOvertime2 ? rOvertime2.TimeInterval : 0f,
                }).ToList()
            };

        public abstract void Load(IProduct.SaveData data);
    }

    [Serializable]
    public abstract class SingleProductBase : ProductBase {
        public override float MaxProgress { get => _maxProgress; }
        [SerializeField] protected float _maxProgress = 100f;
        public override List<ResourceUse> ResourceUse { get => _resourceUse; }
        [SerializeReference, SubclassSelector] protected List<ResourceUse> _resourceUse = new();

        protected void BaseLoad(IProduct.SaveData data) {
            _maxProgress = data.MaxProgress;
            _resourceUse = data.ResourceUse.Select(r => {
                    var timer = new CountdownTimer(r.TimeInterval);
                    timer.Time = r.CurrentTime;
                    return r.IsResourceUseOnProductCreated
                        ? (ResourceUse)new ResourceUseOnProductCreated() { Resource = r.Resource, Amount = r.Amount }
                        : (ResourceUse)new ResourceUseOvertime()
                            { Resource = r.Resource, Amount = r.Amount, TimeInterval = r.TimeInterval, Timer = timer};
                })
                .ToList();
        }
    }
}