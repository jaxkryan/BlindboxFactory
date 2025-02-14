using System;
using Script.Controller;
using Script.HumanResource.Administrator;
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
            _adminController.HRAdministrator = Hr.Administrator;
            _adminController.FacilityAdministrator = Facility.Administrator;
            _adminController.SupplyAdministrator = Supply.Administrator;
            _adminController.FinanceAdministrator = Finance.Administrator;

            this.gameObject.SetActive(false);
        }
        catch (Exception e) {
            Debug.Log(e);
        }
    }
}
