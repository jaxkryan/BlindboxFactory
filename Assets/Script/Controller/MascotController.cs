﻿
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using ZLinq;
using JetBrains.Annotations;
using NUnit.Framework;
using Script.Controller;
using Script.Controller.SaveLoad;
using Script.Gacha.Base;
using Script.Resources;
using Script.Utils;
using UnityEngine;

namespace Script.HumanResource.Administrator {
    [Serializable]
    public class MascotController : ControllerBase {
        [SerializeField] public PortraitRandomizer Portraits;

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
                .AsValueEnumerable()
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
        public MascotType? GetAssignedDepartment([CanBeNull] Mascot mascot) {
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

        [SerializeField]private List<Mascot> _mascotsList = new();

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
        public event Action<Mascot> OnMascotAdded = delegate { };
        public event Action<Mascot> OnMascotRemoved = delegate { };
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
            OnMascotAdded?.Invoke(mascot);
        }

        public void RemoveMascot(Mascot mascot)
        {

            // If the mascot was assigned, unassign it
            if (GeneratorMascot == mascot)
            {
                GeneratorMascot = null;
                mascot.OnDismiss(); // Call dismiss if it was assigned
            }

            if (CanteenMascot == mascot)
            {
                CanteenMascot = null;
                mascot.OnDismiss(); // Call dismiss if it was assigned
            }

            if (RestroomMascot == mascot)
            {
                RestroomMascot = null;
                mascot.OnDismiss(); // Call dismiss if it was assigned
            }

            if (MiningMascot == mascot)
            {
                MiningMascot = null;
                mascot.OnDismiss(); // Call dismiss if it was assigned
            }

            if (FactoryMascot == mascot)
            {
                FactoryMascot = null;
                mascot.OnDismiss(); // Call dismiss if it was assigned
            }

            if (StorageMascot == mascot)
            {
                StorageMascot = null;
                mascot.OnDismiss(); // Call dismiss if it was assigned
            }

            // Add 500 Gold to the player's resources when remove a mascot
            var resourceController = GameController.Instance?.ResourceController;
            if (resourceController != null)
            {
                if (resourceController.TryGetAmount(Resource.Gold, out long currentGold))
                {
                    long newGold = currentGold + 500;
                    if (resourceController.TrySetAmount(Resource.Gold, newGold))
                    {
                        //Debug.Log($"Added 500 Gold for removing mascot: {mascot.Name}. New Gold amount: {newGold}");
                    }
                    else
                    {
                        //Debug.LogWarning($"Failed to set new Gold amount ({newGold}) after removing mascot: {mascot.Name}");
                    }
                }
                else
                {
                    //Debug.LogWarning($"Failed to get current Gold amount for adding 500 Gold after removing mascot: {mascot.Name}");
                }
            }
            else
            {
                Debug.LogError("ResourceController is null in MascotController.RemoveMascot. Cannot add 500 Gold.");
            }
            OnMascotRemoved?.Invoke(mascot);
            if (!_mascotsList.Remove(mascot)) return;

            if (GameController.Instance?.Log ?? false) Debug.Log($"Removed mascot: {mascot.Name} from collection.");
        }

        public override void OnApplicationQuit() {
            assignedMascots.ForEach(admin => admin.OnDismiss());
            base.OnApplicationQuit();
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
                if (!saveManager.TryGetValue(this.GetType().Name, out var saveData)
                      || SaveManager.Deserialize<SaveData>(saveData) is not SaveData data) return;
                ClearData();

                foreach (var mascotData in data.MascotsList) {
                    var mascot = ScriptableObject.CreateInstance<CommonMascot>();
                    mascot.Name = mascotData.Name;
                    mascot.name = mascotData.Name;
                    mascot.SetGrade(mascotData.Grade);
                    if (Portraits != null && mascotData.PortraitIndex >= 0) {
                        mascot.Portrait = Portraits.ItemPool.Count() > mascotData.PortraitIndex
                            ? Portraits.ItemPool.ToList().ElementAtOrDefault(mascotData.PortraitIndex)
                            : null;
                    }

                    var policies = new List<Policy>();
                    foreach (var policyData in mascotData.Policies) {
                        var policy = (Policy)ScriptableObject.CreateInstance(policyData.Type);
                        if (!policy) {
                            Debug.LogError("Cannot load policy of type: " + policyData.Type);
                            continue;
                        }

                        policy.Load(policyData);
                        policies.Add(policy);
                    }

                    mascot.Policies = policies;
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
                ex.RaiseException();

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
                    Policies = mascot.Policies.Select(p => {
                        try {
                            return p.Save();
                        }
                        catch {
                            Debug.LogWarning("Cannot save mascot");
                        }

                        return null;
                    }).Where(m => m != null).ToList(),
                };
                if (Portraits != null) {
                    mData.PortraitIndex = Portraits?.ItemPool.ToList().IndexOf(mascot.Portrait) ?? -1;
                }
                newSave.MascotsList.Add(mData);
            }

            try {
                var serialized = SaveManager.Serialize(newSave);
                saveManager.AddOrUpdate(this.GetType().Name, serialized);
                // if (!saveManager.TryGetValue(this.GetType().Name, out var saveData)
                //     || SaveManager.Deserialize<SaveData>(saveData) is SaveData data)
                //     saveManager.SaveData.TryAdd(this.GetType().Name,
                //         SaveManager.Serialize(newSave));
                // else
                //     saveManager.SaveData[this.GetType().Name]
                //         = SaveManager.Serialize(newSave);
            }
            catch (System.Exception ex) {
                Debug.LogError($"Cannot save {GetType()}");
                Debug.LogException(ex);
                ex.RaiseException();

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