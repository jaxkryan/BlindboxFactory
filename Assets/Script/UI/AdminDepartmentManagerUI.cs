using System;
using System.Collections.Generic;
using System.Linq;
using MyBox;
using Script.Controller;
using Script.Gacha.Base;
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
    [SerializeField] private Button _viewDetailsButton;
    [SerializeField] private MascotDetailUI _mascotDetailUI;

    private MascotController _adminController;
    private Mascot? _mascot;

    private readonly Dictionary<Grade, Color> _gradeColors = new()
    {
        { Grade.Common, Color.green },
        { Grade.Rare, Color.blue },
        { Grade.Special, new Color(0.5f, 0f, 1f) },
        { Grade.Epic, new Color(1f, 0.5f, 0f) },
        { Grade.Legendary, Color.yellow }
    };

    private void Awake()
    {
        _manager = GetComponentInParent<AdministratorManagerUI>();
        _adminController = GameController.Instance.MascotController;
        if (_viewDetailsButton != null)
        {
            _viewDetailsButton.onClick.AddListener(OnClickViewDetails);
        }
        if (_mascotDetailUI != null)
        {
            _mascotDetailUI.gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        _adminController.OnMascotChanged += OnMascotChangedHandler;
    }

    private void OnDisable()
    {
        _adminController.OnMascotChanged -= OnMascotChangedHandler;
    }

    private void OnDestroy()
    {
        if (_viewDetailsButton != null)
        {
            _viewDetailsButton.onClick.RemoveAllListeners();
        }
    }

    private void OnMascotChangedHandler(MascotType department, Mascot? mascot)
    {
        if (department == Position)
        {
            Mascot = mascot;
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
        _portrait.sprite = Mascot?.Portrait ?? _defaultPortrait;
        _name.text = Mascot?.Name.ToString() ?? string.Empty;

        if (Mascot != null)
        {
            string policies = "";
            foreach (var p in Mascot.Policies)
            {
                policies += $"• {p.Description}\n";
            }
            policies = policies.TrimEnd('\n');
            _policies.text = policies;
            _policies.color = _gradeColors[Mascot.Grade];
            _name.color = _gradeColors[Mascot.Grade];
        }
        else
        {
            _policies.text = "";
            _policies.color = Color.white;
        }

        if (_viewDetailsButton != null)
        {
            _viewDetailsButton.interactable = Mascot != null;
            _viewDetailsButton.gameObject.SetActive(Mascot != null);
        }
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
        selectionUI.SetTargetPosition(Position); // Set the target position
        _manager.gameObject.SetActive(false);
        selectionUI.onAdminSelected += (administrator) =>
        {
            _manager.gameObject.SetActive(true);
            Destroy(selectionUI.gameObject);
            SetControllerMascot(administrator);
        };
    }

    private void OnClickViewDetails()
    {
        if (Mascot == null)
        {
            //Debug.LogWarning("Cannot view details: No mascot assigned to this department.");
            return;
        }
        if (_mascotDetailUI == null)
        {
            //Debug.LogError("MascotDetailUI is not assigned in AdminDepartmentManagerUI!");
            return;
        }
        _mascotDetailUI.gameObject.SetActive(true);
        _mascotDetailUI.DisplayDetails(Mascot);
    }

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
                throw new ArgumentOutOfRangeException();
        }
    }
}