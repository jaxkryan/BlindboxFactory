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
    [SerializeField] public MascotType Position;
    [SerializeField] private AdministratorManagerUI _manager;
    [SerializeField] private TextMeshProUGUI _ability;

    private void Awake() {
        _manager = GetComponentInParent<AdministratorManagerUI>();
    }
    
    

    public Mascot Mascot {
        get => _mascot;
        set {
            if (value == null) return;
            _mascot = value;
            SetUpAdmin();
        }
    }
    Mascot _mascot;

    void Start() { SetUpAdmin(); }

    void SetUpAdmin() {
        _ability.color = Mascot ? Color.grey : Color.green;
        
        _position.text = Enum.GetName(typeof(MascotType), Position);
        _portrait.sprite = Mascot?.Portrait ?? _defaultPortrait;
        _name.text = Mascot?.Name.ToString() ?? string.Empty;
        var policies = "";
        Mascot?.Policies.ForEach(p => policies += p.Description + "\n");
        _policies.text = policies;
        
    }

    public void OnClickInfo() {
        //Get list of admins from the same department
        var adminController = GameController.Instance.MascotController;
        var list = adminController?.MascotsList?.ToList() ?? new List<Mascot>(); 
        //Instantiate new selection window
        if (_manager.SelectionUI is null) return;
        var selectionUI = Instantiate(_manager.SelectionUI, _manager.gameObject.transform.parent);
        //Add data to that window
        selectionUI.List = list;
        selectionUI.Clear();
        selectionUI.Spawn();
        selectionUI.Current = Mascot;
        _manager.gameObject.SetActive(false);
        //Subscribe to the selected event
        selectionUI.onAdminSelected += (administrator) => {
            _manager.gameObject.SetActive(true);
            Destroy(selectionUI.gameObject);
            Mascot = administrator;
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
        var layout = text.GetComponent<LayoutElement>();
        if (!layout) return;
        layout.preferredWidth = d.x;
        layout.preferredHeight = d.y;
    }
    
    private void OnGUI() { SetUpAdmin(); 

        SetDimensionsToEmpty(_ability);
        SetDimensionsToEmpty(_policies);
        
    }
}