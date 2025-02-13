using System;
using Script.Controller;
using Script.HumanResource.Administrator;
using Script.HumanResource.Administrator.Positions;
using UnityEngine;

public class AdministratorManagerUI : MonoBehaviour {
    [SerializeField] private AdminDepartmentManagerUI Hr;
    [SerializeField] private AdminDepartmentManagerUI Facility;
    [SerializeField] private AdminDepartmentManagerUI Supply;
    [SerializeField] private AdminDepartmentManagerUI Finance;
    [SerializeField] public SelectAdminUI SelectionUI;

    private AdministratorController _adminController;

    private void Awake() {
        _adminController = GameController.Instance.AdministratorController;
    }

    public void Save() {
        try {
            _adminController.AssignHRAdministrator(Hr.Administrator as HRAdministrator);
            _adminController.AssignFacilityAdministrator(Facility.Administrator as FacilityAdministrator);
            _adminController.AssignSupplyAdministrator(Supply.Administrator as SupplyAdministrator);
            _adminController.AssignFinanceAdministrator(Finance.Administrator as FinanceAdministrator);

            this.gameObject.SetActive(false);
        }
        catch (Exception e) {
            Debug.Log(e);
        }
    }
}
