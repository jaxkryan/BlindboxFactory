
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Script.Controller;
using Script.Controller.SaveLoad;
using Script.Gacha.Base;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Script.HumanResource.Administrator {
    [Serializable]
    public class MascotController : ControllerBase {
        [SerializeField] public List<Sprite> Portraits;

        [CanBeNull]
        public Mascot GeneratorMascot {
            get => _generatorMascot;
            set => AssignMascot(value, ref _generatorMascot, MascotType.Generator);
        }

        [CanBeNull] private Mascot _generatorMascot;

        [CanBeNull]
        public Mascot CanteenMascot {
            get => _canteenMascot;
            set => AssignMascot(value, ref _canteenMascot, MascotType.Canteen);
        }

        [CanBeNull] private Mascot _canteenMascot;

        [CanBeNull]
        public Mascot RestroomMascot {
            get => _restroomMascot;
            set => AssignMascot(value, ref _restroomMascot, MascotType.Restroom);
        }

        [CanBeNull] private Mascot _restroomMascot;

        [CanBeNull]
        public Mascot MiningMascot {
            get => _miningMascot;
            set => AssignMascot(value, ref _miningMascot, MascotType.MiningMachine);
        }

        [CanBeNull] private Mascot _miningMascot;

        [CanBeNull]
        public Mascot FactoryMascot {
            get => _factoryMascot;
            set => AssignMascot(value, ref _factoryMascot, MascotType.ProductFactory);
        }

        [CanBeNull] private Mascot _factoryMascot;

        [CanBeNull]
        public Mascot StorageMascot {
            get => _storageMascot;
            set => AssignMascot(value, ref _storageMascot, MascotType.Storage);
        }

        [CanBeNull] private Mascot _storageMascot;
#nullable enable
        private List<Mascot> assignedMascots =>
            typeof(MascotController).GetProperties()
                .Where(p => p.PropertyType == typeof(Mascot))
                .Where(p => ((Mascot?)p.GetValue(this)) != null)
                .Select(info => (Mascot)info.GetValue(this))
                .ToList();
#nullable restore

        private void AssignMascot([CanBeNull] Mascot newMascot, [CanBeNull] ref Mascot mascot, MascotType position) {
            if (newMascot == mascot) return;

            // If assigning a new mascot, ensure it’s not assigned elsewhere
            if (newMascot != null) {
                var currentDepartment = GetAssignedDepartment(newMascot);
                if (currentDepartment.HasValue && currentDepartment.Value != position) {
                    SetMascotForDepartment(currentDepartment.Value, null); // Unassign from previous department
                }
            }

            OnMascotChanged?.Invoke(position, newMascot);
            mascot?.OnDismiss();
            mascot = newMascot;
            mascot?.OnAssign();
        }

        // Helper method to find where a mascot is currently assigned
        private MascotType? GetAssignedDepartment([CanBeNull] Mascot mascot) {
            if (mascot == null) return null;
            if (GeneratorMascot == mascot) return MascotType.Generator;
            if (CanteenMascot == mascot) return MascotType.Canteen;
            if (RestroomMascot == mascot) return MascotType.Restroom;
            if (MiningMascot == mascot) return MascotType.MiningMachine;
            if (FactoryMascot == mascot) return MascotType.ProductFactory;
            if (StorageMascot == mascot) return MascotType.Storage;
            return null;
        }

        // Helper method to set a department’s mascot (used for unassigning)
        private void SetMascotForDepartment(MascotType department, [CanBeNull] Mascot mascot) {
            switch (department) {
                case MascotType.Generator:
                    GeneratorMascot = mascot;
                    break;
                case MascotType.Canteen:
                    CanteenMascot = mascot;
                    break;
                case MascotType.Restroom:
                    RestroomMascot = mascot;
                    break;
                case MascotType.MiningMachine:
                    MiningMascot = mascot;
                    break;
                case MascotType.ProductFactory:
                    FactoryMascot = mascot;
                    break;
                case MascotType.Storage:
                    StorageMascot = mascot;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(department), department, null);
            }
        }

        public ReadOnlyCollection<Mascot> MascotsList {
            get => _mascotsList.AsReadOnly();
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
#nullable enable
        public event Action<MascotType, Mascot?> OnMascotChanged = delegate { };
#nullable restore

        public void AddMascot(Mascot mascot) {
            if (mascot == null) {
                Debug.Log("❌ AddMascot FAILED: Mascot is NULL!");
                return;
            }

            //Debugging 
            //string policiesDetails = mascot.Policies != null && mascot.Policies.Any()
            //    ? string.Join(", ", mascot.Policies.Select(p => p.Description ?? p.ToString()))
            //    : "No Policies";

            //Debug.Log($"✅ Adding Mascot: {mascot.Name} | Type: [{policiesDetails}] | Rarity: {mascot.Grade}");

            _mascotsList ??= new List<Mascot>();
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
            assignedMascots.ForEach(admin => admin.OnDismiss());
        }

        public override void OnEnable() {
            base.OnEnable();
            assignedMascots.ForEach(admin => admin.OnAssign());
        }

        public override void OnDisable() {
            base.OnDisable();

            assignedMascots.ForEach(admin => admin.OnDismiss());
        }

        public override void OnStart() {
            base.OnStart();

            assignedMascots.ForEach(admin => admin.OnAssign());
        }

        public override void OnUpdate(float deltaTime) {
            base.OnUpdate(deltaTime);

            assignedMascots.ForEach(mascot => mascot.OnUpdate(Time.deltaTime));
        }

        public override void Load(SaveManager saveManager) {
            try {
                if (!saveManager.SaveData.TryGetValue(this.GetType().Name, out var saveData)
                      || JsonConvert.DeserializeObject<SaveData>(saveData) is not SaveData data) return;
                ClearData();

                foreach (var mascotData in data.MascotsList) {
                    var mascot = ScriptableObject.CreateInstance<Mascot>();
                    mascot.Name = mascotData.Name;
                    mascot.name = mascotData.Name;
                    mascot.SetGrade(mascotData.Grade);
                    mascot.Portrait = Portraits.Count > mascotData.PortraitIndex
                        ? Portraits[mascotData.PortraitIndex]
                        : null;

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

                if (data.GeneratorMascotIndex > -1 && data.GeneratorMascotIndex < _mascotsList.Count)
                    GeneratorMascot = _mascotsList[data.GeneratorMascotIndex];
                if (data.FactoryMascotIndex > -1 && data.FactoryMascotIndex < _mascotsList.Count)
                    FactoryMascot = _mascotsList[data.FactoryMascotIndex];
                if (data.CanteenMascotIndex > -1 && data.CanteenMascotIndex < _mascotsList.Count)
                    CanteenMascot = _mascotsList[data.CanteenMascotIndex];
                if (data.MiningMascotIndex > -1 && data.MiningMascotIndex < _mascotsList.Count)
                    MiningMascot = _mascotsList[data.MiningMascotIndex];
                if (data.RestroomMascotIndex > -1 && data.RestroomMascotIndex < _mascotsList.Count)
                    RestroomMascot = _mascotsList[data.RestroomMascotIndex];
                if (data.StorageMascotIndex > -1 && data.StorageMascotIndex < _mascotsList.Count)
                    StorageMascot = _mascotsList[data.StorageMascotIndex];

            }
            catch (System.Exception ex) {
                Debug.LogError($"Cannot load {GetType()}");
                Debug.LogException(ex);
                return;
            }
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

        public override void Save(SaveManager saveManager) {
            var newSave = new SaveData() { MascotsList = new() };
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
                    PortraitIndex = Portraits.IndexOf(mascot.Portrait)
                };
                newSave.MascotsList.Add(mData);
            }

            try {
                if (!saveManager.SaveData.TryGetValue(this.GetType().Name, out var saveData)
                    || JsonConvert.DeserializeObject<SaveData>(saveData) is SaveData data)
                    saveManager.SaveData.TryAdd(this.GetType().Name,
                        JsonConvert.SerializeObject(newSave));
                else
                    saveManager.SaveData[this.GetType().Name]
                        = JsonConvert.SerializeObject(newSave);
            }
            catch (System.Exception ex) {
                Debug.LogError($"Cannot save {GetType()}");
                Debug.LogException(ex);
            }
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