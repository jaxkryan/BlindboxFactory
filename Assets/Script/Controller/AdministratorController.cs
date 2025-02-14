#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Script.HumanResource.Administrator {
    public class AdministratorController {
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

        private void AssignAdministrator(Administrator? value, ref Administrator? admin, AdministratorPosition position) {
            if (value == admin) return;
            OnAdminChanged?.Invoke(position, value);
            admin?.OnDismissManager();
            admin = value;
            admin?.OnAssignManager();
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
        }

        public AdministratorController() : this(new HashSet<Administrator>()) { }


        public event Action<AdministratorPosition, Administrator?> OnAdminChanged = (position, administrator) => { };

    }
}