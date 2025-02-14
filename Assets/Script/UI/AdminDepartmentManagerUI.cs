using System;
using System.Collections.Generic;
using System.Linq;
using Script.Controller;
using Script.HumanResource.Administrator;
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
    [SerializeField] private TextMeshProUGUI _ability;

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
        if (Administrator) {
            _ability.color = Color.grey;
        }
        else {
            _ability.color = Color.green;
            
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
        var list = adminController?.AdministratorList?.ToList() ?? new List<Administrator>(); 
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

    private void OnValidate() {
        SetUpAdmin();

        SetDimensionsToEmpty(_ability);
        SetDimensionsToEmpty(_policies);
    }
    private Vector3 GetEmptyDimension(TextMeshProUGUI text){
        var txt = text.text;
        text.text = string.Empty;
        var d = text.transform.localScale;
        text.text = txt;
        return d;
    }

    private void SetDimensionsToEmpty(TextMeshProUGUI text) {
        var d = GetEmptyDimension(text);
        text.GetComponent<LayoutElement>().preferredWidth = d.x;
        text.GetComponent<LayoutElement>().preferredHeight = d.y;
    }
    
    private void OnGUI() { SetUpAdmin(); 

        SetDimensionsToEmpty(_ability);
        SetDimensionsToEmpty(_policies);
        
    }
}