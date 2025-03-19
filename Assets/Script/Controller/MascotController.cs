#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Newtonsoft.Json;
using Script.Controller;
using Script.Gacha.Base;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Script.HumanResource.Administrator {
    [Serializable]
    public class MascotController : ControllerBase {
        [SerializeField] public List<Sprite> Portraits;
        public Mascot? GeneratorMascot {
            get => _generatorMascot;
            set => AssignMascot(value, ref _generatorMascot, MascotType.Generator);
        }

        private Mascot? _generatorMascot;

        public Mascot? CanteenMascot {
            get => _canteenMascot;
            set => AssignMascot(value, ref _canteenMascot, MascotType.Canteen);
        }

        private Mascot? _canteenMascot;

        public Mascot? RestroomMascot {
            get => _restroomMascot;
            set => AssignMascot(value, ref _restroomMascot, MascotType.Restroom);
        }

        private Mascot? _restroomMascot;

        public Mascot? MiningMascot {
            get => _miningMascot;
            set => AssignMascot(value, ref _miningMascot, MascotType.MiningMachine);
        }

        private Mascot? _miningMascot;

        public Mascot? FactoryMascot {
            get => _factoryMascot;
            set => AssignMascot(value, ref _factoryMascot, MascotType.ProductFactory);
        }

        private Mascot? _factoryMascot;

        public Mascot? StorageMascot {
            get => _storageMascot;
            set => AssignMascot(value, ref _storageMascot, MascotType.Storage);
        }

        private Mascot? _storageMascot;

        private List<Mascot> _assignedMascots =>
            typeof(MascotController).GetProperties()
                .Where(p => p.PropertyType == typeof(Mascot))
                .Where(p => ((Mascot?)p.GetValue(this)) != null)
                .Select(info => (Mascot)info.GetValue(this))
                .ToList();

        private void AssignMascot(Mascot? value, ref Mascot? admin, MascotType position) {
            if (value == admin) return;
            OnMascotChanged?.Invoke(position, value);
            admin?.OnDismiss();
            admin = value;
            admin?.OnAssign();
        }

        public ReadOnlyCollection<Mascot> MascotsList {
            get => _mascotsList.ToList().AsReadOnly();
        }

        private List<Mascot> _mascotsList = new();

        public bool TryAddMascot(Mascot mascot) {
            if (_mascotsList.Contains(mascot)) return false;

            _mascotsList.Add(mascot);
            return true;
        }

        public bool TryRemoveMascot(Mascot mascot) {
            if (!_mascotsList.Contains(mascot)) return false;

            return _mascotsList.Remove(mascot);
        }

        public event Action<MascotType, Mascot?> OnMascotChanged = delegate { };

        public void AddMascot(Mascot mascot) {
            if (mascot == null) {
                Debug.LogError("❌ AddMascot FAILED: Mascot is NULL!");
                return;
            }

            Debug.Log($"✅ Adding Mascot: {mascot.name} | Type: {mascot.Policies} | Rarity: {mascot.Grade}");

            _mascotsList ??= new List<Mascot>(); // Ensure the list is not null
            _mascotsList.Add(mascot);
        }

        public void RemoveMascot(Mascot mascot) {
            if (!_mascotsList.Remove(mascot)) return;
            // If the mascot was assigned, unassign it
            if (GeneratorMascot == mascot) {
                GeneratorMascot = null;
                mascot.OnDismiss(); // Call dismiss if it was assigned
            }

            if (CanteenMascot == mascot) {
                CanteenMascot = null;
                mascot.OnDismiss(); // Call dismiss if it was assigned
            }

            if (RestroomMascot == mascot) {
                RestroomMascot = null;
                mascot.OnDismiss(); // Call dismiss if it was assigned
            }

            if (MiningMascot == mascot) {
                MiningMascot = null;
                mascot.OnDismiss(); // Call dismiss if it was assigned
            }

            if (FactoryMascot == mascot) {
                FactoryMascot = null;
                mascot.OnDismiss(); // Call dismiss if it was assigned
            }

            if (StorageMascot == mascot) {
                StorageMascot = null;
                mascot.OnDismiss(); // Call dismiss if it was assigned
            }


            Debug.Log($"Removed mascot: {mascot.Name} from collection.");
        }

        public override void OnDestroy() {
            base.OnDestroy();
            _assignedMascots.ForEach(admin => admin.OnDismiss());
        }

        public override void OnEnable() {
            base.OnEnable();
            _assignedMascots.ForEach(admin => admin.OnAssign());
        }

        public override void OnDisable() {
            base.OnDisable();

            _assignedMascots.ForEach(admin => admin.OnDismiss());
        }

        public override void OnStart() {
            base.OnStart();

            _assignedMascots.ForEach(admin => admin.OnAssign());
        }

        public override void OnUpdate(float deltaTime) {
            base.OnUpdate(deltaTime);

            _assignedMascots.ForEach(mascot => mascot.OnUpdate(Time.deltaTime));
        }

        public override void Load() {
            if (!GameController.Instance.SaveManager.SaveData.TryGetValue(this.GetType().Name, out var saveData)
                || JsonConvert.DeserializeObject<SaveData>(saveData) is not SaveData data) return;
            ClearData();

            foreach (var mascotData in data.MascotsList) {
                var mascot = ScriptableObject.CreateInstance<Mascot>();
                mascot.Name = mascotData.Name;
                mascot.name = mascotData.Name;
                mascot.SetGrade(mascotData.Grade);
                mascot.Portrait = Portraits.Count > mascotData.PortraitIndex ? Portraits[mascotData.PortraitIndex] : null;

                foreach (var policyData in mascotData.Policies) {
                    var policy = (Policy)ScriptableObject.CreateInstance(policyData.Type);
                    if (!policy) {
                        Debug.LogError("Cannot load policy of type: " + policyData.Type);
                        continue;
                    }
                    
                    policy.Load(policyData);
                }
                _mascotsList.Add(mascot);
            }
            
            if (data.GeneratorMascotIndex > -1 && data.GeneratorMascotIndex < _mascotsList.Count ) GeneratorMascot = _mascotsList[data.GeneratorMascotIndex];
            if (data.FactoryMascotIndex > -1 && data.FactoryMascotIndex < _mascotsList.Count ) FactoryMascot = _mascotsList[data.FactoryMascotIndex];
            if (data.CanteenMascotIndex > -1 && data.CanteenMascotIndex < _mascotsList.Count ) CanteenMascot = _mascotsList[data.CanteenMascotIndex];
            if (data.MiningMascotIndex > -1 && data.MiningMascotIndex < _mascotsList.Count ) MiningMascot = _mascotsList[data.MiningMascotIndex];
            if (data.RestroomMascotIndex > -1 && data.RestroomMascotIndex < _mascotsList.Count ) RestroomMascot = _mascotsList[data.RestroomMascotIndex];
            if (data.StorageMascotIndex > -1 && data.StorageMascotIndex < _mascotsList.Count ) StorageMascot = _mascotsList[data.StorageMascotIndex];

            void ClearData() {
                _generatorMascot = null;
                _factoryMascot = null;
                _canteenMascot = null;
                _miningMascot = null;
                _restroomMascot = null;
                _storageMascot = null;
                _mascotsList.Clear();
            }
        }
        public override void Save() {
            var newSave = new SaveData(){MascotsList =  new()};
            foreach (var mascot in _mascotsList) {
                if (GeneratorMascot == mascot) newSave.GeneratorMascotIndex = _mascotsList.IndexOf(mascot);
                if (CanteenMascot == mascot) newSave.CanteenMascotIndex = _mascotsList.IndexOf(mascot);
                if (RestroomMascot == mascot) newSave.RestroomMascotIndex = _mascotsList.IndexOf(mascot);
                if (MiningMascot == mascot) newSave.MiningMascotIndex = _mascotsList.IndexOf(mascot);
                if (FactoryMascot == mascot) newSave.FactoryMascotIndex = _mascotsList.IndexOf(mascot);
                if (StorageMascot == mascot) newSave.StorageMascotIndex = _mascotsList.IndexOf(mascot);

                var mData = new MascotData() {
                    Name = mascot.Name,
                    Grade = mascot.Grade,
                    Policies = mascot.Policies.Select(p => p.Save()).ToList(),
                    PortraitIndex =  Portraits.IndexOf(mascot.Portrait)
                };
                newSave.MascotsList.Add(mData);
            }
            
            if (!GameController.Instance.SaveManager.SaveData.TryGetValue(this.GetType().Name, out var saveData)
                || JsonConvert.DeserializeObject<SaveData>(saveData) is SaveData data) 
                GameController.Instance.SaveManager.SaveData.Add(this.GetType().Name, JsonConvert.SerializeObject(newSave));
            else GameController.Instance.SaveManager.SaveData[this.GetType().Name] = JsonConvert.SerializeObject(newSave);
        }

        public class SaveData {
            public int GeneratorMascotIndex = -1;
            public int CanteenMascotIndex = -1;
            public int RestroomMascotIndex = -1;
            public int MiningMascotIndex = -1;
            public int FactoryMascotIndex = -1;
            public int StorageMascotIndex = -1;
            public List<MascotData> MascotsList;
        }
        public class MascotData {
            public EmployeeName Name;
            public int PortraitIndex;
            public List<Policy.SaveData> Policies;
            public Grade Grade;
        }
    }
}