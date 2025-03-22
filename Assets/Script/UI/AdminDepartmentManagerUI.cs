#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Script.Controller;
using Script.HumanResource.Administrator;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AdminDepartmentManagerUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _position;
    [SerializeField] private Image _portrait;
    [SerializeField] private Sprite _defaultPortrait;
    [SerializeField] private TextMeshProUGUI _name;
    [SerializeField] private TextMeshProUGUI _policies;
    [SerializeField] public MascotType Position;
    [SerializeField] private AdministratorManagerUI _manager;
    [SerializeField] private TextMeshProUGUI _ability;

    private MascotController _adminController;
    private Mascot? _mascot;

    private void Awake()
    {
        _manager = GetComponentInParent<AdministratorManagerUI>();
        _adminController = GameController.Instance.MascotController; // Get the controller
    }

    private void OnEnable()
    {
        _adminController.OnMascotChanged += OnMascotChangedHandler;
    }

    private void OnDisable()
    {
        _adminController.OnMascotChanged -= OnMascotChangedHandler;
    }

    private void OnMascotChangedHandler(MascotType department, Mascot? mascot)
    {
        if (department == Position)
        {
            Mascot = mascot; // Update local mascot when the controller notifies us
        }
    }

    public Mascot? Mascot
    {
        get => _mascot;
        set
        {
            if (value == _mascot) return;
            _mascot = value;
            SetUpAdmin();
        }
    }

    private void Start()
    {
        SetUpAdmin();
    }

    private void SetUpAdmin()
    {
        _ability.color = Mascot != null ? Color.grey : Color.green;
        _position.text = Enum.GetName(typeof(MascotType), Position);
        _portrait.sprite = Mascot?.Portrait ?? _defaultPortrait;
        _name.text = Mascot?.Name.ToString() ?? string.Empty;
        var policies = "";
        if (Mascot != null)
        {
            Mascot.Policies.ForEach(p => policies += p.Description + "\n");
        }
        _policies.text = policies;
    }

    public void OnClickInfo()
    {
        var list = _adminController?.MascotsList?.ToList() ?? new List<Mascot>();
        if (_manager.SelectionUI == null) return;
        var selectionUI = Instantiate(_manager.SelectionUI, _manager.gameObject.transform.parent);
        selectionUI.List = list;
        selectionUI.Clear();
        selectionUI.Spawn();
        selectionUI.Current = Mascot;
        _manager.gameObject.SetActive(false);
        selectionUI.onAdminSelected += (administrator) =>
        {
            _manager.gameObject.SetActive(true);
            Destroy(selectionUI.gameObject);
            // Update the controller immediately
            SetControllerMascot(administrator);
        };
    }

    // New method to sync with the controller
    private void SetControllerMascot(Mascot? mascot)
    {
        switch (Position)
        {
            case MascotType.Generator:
                _adminController.GeneratorMascot = mascot;
                break;
            case MascotType.Canteen:
                _adminController.CanteenMascot = mascot;
                break;
            case MascotType.Restroom:
                _adminController.RestroomMascot = mascot;
                break;
            case MascotType.MiningMachine:
                _adminController.MiningMascot = mascot;
                break;
            case MascotType.ProductFactory:
                _adminController.FactoryMascot = mascot;
                break;
            case MascotType.Storage:
                _adminController.StorageMascot = mascot;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(Position), Position, null);
        }
        // Local UI will update via OnMascotChangedHandler
    }

    private void OnValidate()
    {
        SetUpAdmin();
        SetDimensionsToEmpty(_ability);
        SetDimensionsToEmpty(_policies);
    }

    private Vector3 GetEmptyDimension(TextMeshProUGUI text)
    {
        var txt = text.text;
        text.text = string.Empty;
        var d = text.transform.localScale;
        text.text = txt;
        return d;
    }

    private void SetDimensionsToEmpty(TextMeshProUGUI text)
    {
        var d = GetEmptyDimension(text);
        var layout = text.GetComponent<LayoutElement>();
        if (layout != null)
        {
            layout.preferredWidth = d.x;
            layout.preferredHeight = d.y;
        }
    }

    private void OnGUI()
    {
        SetUpAdmin();
        SetDimensionsToEmpty(_ability);
        SetDimensionsToEmpty(_policies);
    }
}