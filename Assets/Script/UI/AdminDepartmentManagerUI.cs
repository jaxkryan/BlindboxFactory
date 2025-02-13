using System;
using System.Collections.Generic;
using System.Linq;
using Script.Controller;
using Script.HumanResource.Administrator;
using Script.HumanResource.Administrator.Positions;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class AdminDepartmentManagerUI : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI _position;
    [SerializeField] private Image _portrait;
    [SerializeField] private Sprite _defaultPortrait;
    [SerializeField] private TextMeshProUGUI _name;
    [SerializeField] private TextMeshProUGUI _policies;
    [SerializeField] public AdministratorPosition Position;
    [SerializeField] private AdministratorManagerUI _manager;

    private void Awake() {
        _manager = GetComponentInParent<AdministratorManagerUI>();
    }

    public Administrator Administrator {
        get => _administrator;
        set {
            if (value == null) return;
            _administrator = value;
            SetUpAdmin();
        }
    }
    Administrator _administrator;

    void Start() { SetUpAdmin(); }

    void SetUpAdmin() {
        if (Administrator != null) switch (Position) {
            case AdministratorPosition.HR:
                if (Administrator is not HRAdministrator) {
                    Administrator = null;
                    return;
                }

                break;
            case AdministratorPosition.Facility:
                if (Administrator is not FacilityAdministrator) {
                    Administrator = null;
                    return;
                }

                break;
            case AdministratorPosition.Supply:
                if (Administrator is not SupplyAdministrator) {
                    Administrator = null;
                    return;
                }

                break;
            case AdministratorPosition.Finance:
                if (Administrator is not FinanceAdministrator) {
                    Administrator = null;
                    return;
                }

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        _position.text = Enum.GetName(typeof(AdministratorPosition), Position);
        _portrait.sprite = Administrator?.Portrait ?? _defaultPortrait;
        _name.text = Administrator?.Name.ToString() ?? string.Empty;
        var policies = "";
        Administrator?.Policies.ForEach(p => policies += p.Description + "\n");
        _policies.text = policies;
    }

    public void OnClickInfo() {
        //Get list of admins from the same department
        var adminController = GameController.Instance.AdministratorController;
        var list = adminController?.AdministratorList?.GetValueOrDefault(Position)?.ToList() ?? new List<Administrator>(); 
        //Instantiate new selection window
        if (_manager.SelectionUI is null) return;
        var selectionUI = Instantiate(_manager.SelectionUI, _manager.gameObject.transform.parent);
        //Add data to that window
        selectionUI.List = list;
        selectionUI.Clear();
        selectionUI.Spawn();
        selectionUI.Current = Administrator;
        _manager.gameObject.SetActive(false);
        //Subscribe to the selected event
        selectionUI.onAdminSelected += (administrator) => {
            _manager.gameObject.SetActive(true);
            Destroy(selectionUI.gameObject);
            Administrator = administrator;
            SetUpAdmin();
        };
    }

    private void OnValidate() { SetUpAdmin(); }
    private void OnGUI() { SetUpAdmin(); }
}