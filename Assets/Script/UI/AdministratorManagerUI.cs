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

    private MascotController _adminController;

    private void Awake() {
        _adminController = GameController.Instance.MascotController;
    }

    public void Save() {
        try {
            _adminController.GeneratorMascot = Hr.Mascot;
            _adminController.CanteenMascot = Facility.Mascot;
            _adminController.RestroomMascot = Supply.Mascot;
            _adminController.MiningMascot = Finance.Mascot;

            this.gameObject.SetActive(false);
        }
        catch (System.Exception e) {
            Debug.Log(e);
        }
    }
}
