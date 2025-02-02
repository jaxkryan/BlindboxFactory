using System;
using System.Collections.Generic;
using Script.HumanResource.Administrator.Positions;
using UnityEngine;

namespace Script.HumanResource.Administrator {
    public class AdministratorController : MonoBehaviour {
        public HRAdministrator HRAdministrator;
        public FacilityAdministrator FacilityAdministrator;
        public SupplyAdministrator SupplyAdministrator;
        public FinanceAdministrator FinanceAdministrator;
        
        public event Action<Administrator> OnAdminChanged;

        public void AssignHRAdministrator(HRAdministrator administrator) => AssignAdministrator(administrator);

        public void AssignFacilityAdministrator(FacilityAdministrator administrator)  => AssignAdministrator(administrator);

        public void AssignSupplyAdministrator(SupplyAdministrator administrator) => AssignAdministrator(administrator);

        public void AssignFinanceAdministrator(FinanceAdministrator administrator) => AssignAdministrator(administrator);

        private void AssignAdministrator<TAdministrator>(TAdministrator administrator) where TAdministrator : Administrator {
            List<Administrator> positions = new();
            switch (administrator.GetType()) {
                case Type Hr when Hr == typeof(HRAdministrator):
                    positions.Add(HRAdministrator);
                    break;
                case Type Facility when Facility == typeof(FacilityAdministrator):
                    positions.Add(FacilityAdministrator);
                    break;
                case Type Finance when Finance == typeof(FinanceAdministrator):
                    positions.Add(FinanceAdministrator);
                    break;
                case Type Supply when Supply == typeof(SupplyAdministrator):
                    positions.Add(SupplyAdministrator);
                    break;
                default:
                    throw new NotImplementedException();
            }

            try {
                OnAdminChanged?.Invoke(administrator);
                positions.ForEach(position => {
                    if (positions.GetType() == typeof(TAdministrator)) {
                        position?.OnDismissManager();
                        position = administrator;
                        position.OnAssignManager();
                    }
                });
            }
            catch (Exception e) {
                Debug.LogWarning($"Unable to assign administrator {administrator.GetType()}: {e.Message}");
            }
        }
    }
}