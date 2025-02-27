using System;
using Script.Controller;
using Script.Machine.Products;
using Script.Resources;
using UnityEngine;

namespace Script.Machine.ResourceManager {
    [Serializable]
    public abstract class ResourceUse {
        public Resource Resource { get => _resource; }
        [SerializeField]private Resource _resource;
        public int Amount { get => _amount; }
        [SerializeField]private int _amount;
        protected MachineBase _machine;

        public virtual void Start(MachineBase machine) {
            _machine = machine;
        }

        public virtual void Stop() { }

        protected virtual void UseResource() {
            var controller = GameController.Instance.ResourceController;
            if (!controller.TryGetAmount(_resource, out var amount)) {
                Debug.LogError($"Cannot get resource amount {_resource}");
                return;
            }

            if (!controller.TrySetAmount(_resource, amount - Amount)) {
                Debug.LogError($"Cannot set resource {_resource} to new amount: {amount - Amount}");
            }
        }
    }

    [Serializable]
    public class ResourceUseOnProductCreated : ResourceUse {
        public override void Start(MachineBase machine) {
            base.Start(machine);

            _machine.onCreateProduct += OnCreateProduct;
        }
        
        private void OnCreateProduct(ProductBase product) => UseResource();

        public override void Stop() {
            base.Stop();
            
            _machine.onCreateProduct -= OnCreateProduct;
        }
    }

    [Serializable]
    public class ResourceUseOvertime : ResourceUse {
        public float TimeInterval { get => _timeInterval; }
        [SerializeField]private float _timeInterval;

        private Timer _timer;
        
        public override void Start(MachineBase machine) {
            base.Start(machine);

            if (_timer is null) {
                _timer = new CountdownTimer(TimeInterval);
                _timer.OnTimerStop += OnTimerStop;
            }
            _timer.Start();
        }

        private void OnTimerStop() {
            UseResource(); 
            _timer.Start();
        }

        public override void Stop() {
            base.Stop();
            
            _timer.Pause();
        }
    }
}