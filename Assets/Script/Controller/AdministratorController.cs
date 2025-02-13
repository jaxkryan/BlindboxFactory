#nullable enable
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Script.HumanResource.Administrator.Positions;
using UnityEngine;

namespace Script.HumanResource.Administrator {
    public class AdministratorController {
        public HRAdministrator? HRAdministrator {
            get => _hrAdministrator;
            private set => AssignAdministrator(value, ref _hrAdministrator);
        }

        private HRAdministrator? _hrAdministrator;

        public FacilityAdministrator? FacilityAdministrator {
            get => _facilityAdministrator;
            private set => AssignAdministrator(value, ref _facilityAdministrator);
        }

        private FacilityAdministrator? _facilityAdministrator;

        public SupplyAdministrator? SupplyAdministrator {
            get => _supplyAdministrator;
            private set => AssignAdministrator(value, ref _supplyAdministrator);
        }

        private SupplyAdministrator? _supplyAdministrator;

        public FinanceAdministrator? FinanceAdministrator {
            get => _financeAdministrator;
            private set => AssignAdministrator(value, ref _financeAdministrator);
        }

        private FinanceAdministrator? _financeAdministrator;

        private void AssignAdministrator<TAdministrator>(TAdministrator value, ref TAdministrator admin)
            where TAdministrator : Administrator? {
            if (value == admin) return;
            OnAdminChanged?.Invoke(value);
            admin?.OnDismissManager();
            admin = value;
            admin?.OnAssignManager();
        }

        public ReadOnlyDictionary<AdministratorPosition, HashSet<Administrator>> AdministratorList {
            get => new ReadOnlyDictionary<AdministratorPosition, HashSet<Administrator>>(_administratorList);
        }

        private Dictionary<AdministratorPosition, HashSet<Administrator>> _administratorList;

        public AdministratorController(Dictionary<AdministratorPosition, HashSet<Administrator>> administratorList,
            HRAdministrator hrAdministrator = null, FacilityAdministrator facilityAdministrator = null,
            SupplyAdministrator supplyAdministrator = null, FinanceAdministrator financeAdministrator = null) {
            HRAdministrator = hrAdministrator;
            FacilityAdministrator = facilityAdministrator;
            SupplyAdministrator = supplyAdministrator;
            FinanceAdministrator = financeAdministrator;
            _administratorList = administratorList;
        }

        public AdministratorController() : this(new Dictionary<AdministratorPosition, HashSet<Administrator>>()) { }

        public void AddAdministrator<TAdministrator>(TAdministrator administrator)
            where TAdministrator : Administrator {
            switch (typeof(TAdministrator)) {
                case Type hr when hr == typeof(HRAdministrator):
                    if (!TryAddWorker(AdministratorPosition.HR, administrator))
                        throw new Exception("Cannot add administrator");
                    break;
                case Type facility when facility == typeof(FacilityAdministrator):
                    if (!TryAddWorker(AdministratorPosition.Facility, administrator))
                        throw new Exception("Cannot add administrator");
                    break;
                case Type supply when supply == typeof(SupplyAdministrator):
                    if (!TryAddWorker(AdministratorPosition.Supply, administrator))
                        throw new Exception("Cannot add administrator");
                    break;
                case Type finance when finance == typeof(FinanceAdministrator):
                    if (!TryAddWorker(AdministratorPosition.Finance, administrator))
                        throw new Exception("Cannot add administrator");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            bool TryAddWorker(AdministratorPosition type, Administrator admin) {
                HashSet<Administrator> administratorList;
                if (!_administratorList.TryGetValue(type, out administratorList))
                    administratorList = new HashSet<Administrator>();
                if (!administratorList.Add(admin)) return false;
                _administratorList[type] = administratorList;
                return true;
            }
        }

        public void RemoveAdministrator<TAdministrator>(TAdministrator administrator) where TAdministrator : Administrator {
            switch (typeof(TAdministrator)) {
                case Type hr when hr == typeof(HRAdministrator):
                    if (!TryRemoveWorker(AdministratorPosition.HR, administrator))
                        throw new Exception("Cannot remove administrator");
                    break;
                case Type facility when facility == typeof(FacilityAdministrator):
                    if (!TryRemoveWorker(AdministratorPosition.Facility, administrator))
                        throw new Exception("Cannot remove administrator");
                    break;
                case Type supply when supply == typeof(SupplyAdministrator):
                    if (!TryRemoveWorker(AdministratorPosition.Supply, administrator))
                        throw new Exception("Cannot remove administrator");
                    break;
                case Type finance when finance == typeof(FinanceAdministrator):
                    if (!TryRemoveWorker(AdministratorPosition.Finance, administrator))
                        throw new Exception("Cannot remove administrator");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            bool TryRemoveWorker(AdministratorPosition type, Administrator admin) {
                HashSet<Administrator> administratorList;
                if (!_administratorList.TryGetValue(type, out administratorList))
                    administratorList = new HashSet<Administrator>();
                if (!administratorList.Contains(admin)) return false;
                administratorList.Remove(admin);
                _administratorList[type] = administratorList;
                return true;
            } }

        public event Action<Administrator?> OnAdminChanged = administrator => { };

        public void AssignHRAdministrator(HRAdministrator administrator) => HRAdministrator = administrator;

        public void AssignFacilityAdministrator(FacilityAdministrator administrator) =>
            FacilityAdministrator = administrator;

        public void AssignSupplyAdministrator(SupplyAdministrator administrator) => SupplyAdministrator = administrator;

        public void AssignFinanceAdministrator(FinanceAdministrator administrator) =>
            FinanceAdministrator = administrator;
    }
}