using System;
using Script.Controller;
using Script.HumanResource.Administrator;
using UnityEngine;

public class AdministratorManagerUI : MonoBehaviour
{
    [SerializeField] private GameObject _managerPanel;             // Main panel for the UI
    [SerializeField] private AdminDepartmentManagerUI Generator;   // For Generator position
    [SerializeField] private AdminDepartmentManagerUI Canteen;     // For Canteen position
    [SerializeField] private AdminDepartmentManagerUI Restroom;    // For Restroom position
    [SerializeField] private AdminDepartmentManagerUI Mining;      // For MiningMachine position
    [SerializeField] private AdminDepartmentManagerUI Factory;     // For ProductFactory position
    [SerializeField] private AdminDepartmentManagerUI Storage;     // For Storage position
    [SerializeField] public SelectAdminUI SelectionUI;

    private MascotController _adminController;

    private void Awake()
    {
        _adminController = GameController.Instance.MascotController;

        // Ensure the panel is hidden by default
        if (_managerPanel != null)
        {
            _managerPanel.SetActive(false);
        }
        else
        {
           // Debug.LogWarning("ManagerPanel is not assigned in the Inspector!");
        }

        // Ensure each department UI has the correct position assigned
        Generator.Position = MascotType.Generator;
        Canteen.Position = MascotType.Canteen;
        Restroom.Position = MascotType.Restroom;
        Mining.Position = MascotType.MiningMachine;
        Factory.Position = MascotType.ProductFactory;
        Storage.Position = MascotType.Storage;

        // Load initial mascots from controller
        Generator.Mascot = _adminController.GeneratorMascot;
        Canteen.Mascot = _adminController.CanteenMascot;
        Restroom.Mascot = _adminController.RestroomMascot;
        Mining.Mascot = _adminController.MiningMascot;
        Factory.Mascot = _adminController.FactoryMascot;
        Storage.Mascot = _adminController.StorageMascot;
    }

    public void Open()
    {
        if (_managerPanel == null)
        {
            //Debug.LogWarning("Cannot open AdministratorManagerUI: ManagerPanel is null!");
            return;
        }

        _managerPanel.SetActive(true);
    }

    public void Close()
    {
        if (_managerPanel != null)
        {
            _managerPanel.SetActive(false);
        }
    }

    public void Save()
    {
        Close();
    }

    
}