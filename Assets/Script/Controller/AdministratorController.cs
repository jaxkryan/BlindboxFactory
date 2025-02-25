#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Script.Controller;
using Unity.VisualScripting;

namespace Script.HumanResource.Administrator {
    public class AdministratorController : ControllerBase {
        public Administrator? HRAdministrator {
            get => _hrAdministrator;
            set => AssignAdministrator(value, ref _hrAdministrator, AdministratorPosition.HR);
        }

        private Administrator? _hrAdministrator;

        public Administrator? FacilityAdministrator {
            get => _facilityAdministrator;
            set => AssignAdministrator(value, ref _facilityAdministrator, AdministratorPosition.Facility);
        }

        private Administrator? _facilityAdministrator;

        public Administrator? SupplyAdministrator {
            get => _supplyAdministrator;
            set => AssignAdministrator(value, ref _supplyAdministrator, AdministratorPosition.Supply);
        }

        private Administrator? _supplyAdministrator;

        public Administrator? FinanceAdministrator {
            get => _financeAdministrator;
            set => AssignAdministrator(value, ref _financeAdministrator, AdministratorPosition.Finance);
        }

        private Administrator? _financeAdministrator;

        private List<Administrator> _assignedAdministrators => typeof(AdministratorController).GetProperties()
            .Where(p => p.PropertyType == typeof(Administrator))
            .Where(p => ((Administrator?)p.GetValue(this)) != null)
            .Select(info => (Administrator)info.GetValue(this))
            .ToList();
        
        private void AssignAdministrator(Administrator? value, ref Administrator? admin, AdministratorPosition position) {
            if (value == admin) return;
            OnAdminChanged?.Invoke(position, value);
            admin?.OnDismiss();
            admin = value;
            admin?.OnAssign();
        }

        public ReadOnlyCollection<Administrator> AdministratorList {
            get => _administratorList.ToList().AsReadOnly();
        }

        private HashSet<Administrator> _administratorList;

        public AdministratorController(HashSet<Administrator> administratorList,
            Administrator? hrAdministrator = null, Administrator? facilityAdministrator = null,
            Administrator? supplyAdministrator = null, Administrator? financeAdministrator = null) {
            HRAdministrator = hrAdministrator;
            FacilityAdministrator = facilityAdministrator;
            SupplyAdministrator = supplyAdministrator;
            FinanceAdministrator = financeAdministrator;
            _administratorList = administratorList;
            _assignedAdministrators.ForEach(admin => admin.OnAssign());
        }

        public AdministratorController() : this(new HashSet<Administrator>()) { }
        public event Action<AdministratorPosition, Administrator?> OnAdminChanged = delegate { };

        
        public override void OnDestroy() {
            base.OnDestroy();
            _assignedAdministrators.ForEach(admin => admin.OnDismiss());
        }

        public override void OnEnable() {
            base.OnEnable();
            _assignedAdministrators.ForEach(admin => admin.OnAssign());
        }

        public override void OnDisable() {
            base.OnDisable();
            
            _assignedAdministrators.ForEach(admin => admin.OnDismiss());
        }

        public override void Load() { throw new NotImplementedException(); }
        public override void Save() { throw new NotImplementedException(); }

        public override void OnStart() {
            base.OnStart();
            
            _assignedAdministrators.ForEach(admin => admin.OnAssign());
        }
    }
}