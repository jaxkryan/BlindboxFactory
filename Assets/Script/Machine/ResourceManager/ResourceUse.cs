using System;
using Script.Controller;
using Script.Machine.Products;
using Script.Resources;
using UnityEngine;
using UnityEngine.Serialization;

namespace Script.Machine.ResourceManager {
    [Serializable]
    public abstract class ResourceUse {
        public Resource Resource { get => _resource; }
        [SerializeField]private Resource _resource;
        public int Amount { get => _amount; }
        [SerializeField]private int _amount;
        [FormerlySerializedAs("_machine")] public MachineBase Machine;
        protected ResourceManager _resourceManager;
        private bool _started = false;

        public void Start(MachineBase machine, ResourceManager resourceManager) {

            if (_started) return;
            Machine = machine;
            _resourceManager = resourceManager;
            _started = true;
            OnStart();
        }

        protected abstract void OnStart();

        public void Stop() {
            if (!_started) return;
            _started = false;
            OnStop();   
        }
        protected abstract void OnStop();

        protected virtual void UseResource() {
            if (!_resourceManager.TryConsumeResources(this, 1, out _)) {
                Debug.LogError($"Cannot consume resource: {_resource}!");
            }
        }
    }

    [Serializable]
    public class ResourceUseOnProductCreated : ResourceUse {
        protected override void OnStart() {
            Machine.onCreateProduct += OnCreateProduct;
        }
        
        private void OnCreateProduct(ProductBase product) => UseResource();

        protected override void OnStop() {
            Machine.onCreateProduct -= OnCreateProduct;
        }
    }

    [Serializable]
    public class ResourceUseOvertime : ResourceUse {
        public float TimeInterval { get => _timeInterval; }
        [SerializeField]private float _timeInterval;

        private Timer _timer;
        
        protected override void OnStart() {
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

        protected override void OnStop() {
            _timer.Pause();
        }
    }
}