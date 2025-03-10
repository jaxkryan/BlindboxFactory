#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Script.Controller;
using Unity.VisualScripting;
using UnityEngine;

namespace Script.HumanResource.Administrator {
    public class MascotController : ControllerBase {
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

        private List<Mascot> _assignedMascots => typeof(MascotController).GetProperties()
            .Where(p => p.PropertyType == typeof(Mascot))
            .Where(p => ((Mascot?)p.GetValue(this)) != null)
            .Select(info => (Mascot)info.GetValue(this))
            .ToList();
        
        private void AssignMascot(Mascot? value, ref Mascot? admin, MascotType position) {
            if (value == admin) return;
            OnAdminChanged?.Invoke(position, value);
            admin?.OnDismiss();
            admin = value;
            admin?.OnAssign();
        }

        public ReadOnlyCollection<Mascot> MascotsList {
            get => _mascotsList.ToList().AsReadOnly();
        }

        private HashSet<Mascot> _mascotsList = new();

        public event Action<MascotType, Mascot?> OnAdminChanged = delegate { };

        public void AddMascot(Mascot mascot)
        {
            if (mascot == null)
            {
                Debug.LogError("❌ AddMascot FAILED: Mascot is NULL!");
                return;
            }

            Debug.Log($"✅ Adding Mascot: {mascot.name} | Type: {mascot.Policies.ToString()} | Rarity: {mascot.Grade}");

            _mascotsList ??= new HashSet<Mascot>(); // Ensure the list is not null
            _mascotsList.Add(mascot);
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

        public override void Load() { throw new NotImplementedException(); }
        #warning saved mascot list, each assigned mascot
        public override void Save() { throw new NotImplementedException(); }

        public override void OnStart(){
            base.OnStart();
            
            _assignedMascots.ForEach(admin => admin.OnAssign());
        }
    }
}
