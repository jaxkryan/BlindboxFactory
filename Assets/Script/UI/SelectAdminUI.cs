using System;
using System.Collections.Generic;
using System.Linq;
using Script.Controller;
using Script.HumanResource.Administrator;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class UIScrollable : MonoBehaviour
{
    [SerializeField] protected ScrollRect _scrollRect;
    [SerializeField] protected GameObject _container;
    [SerializeField] protected TextMeshProUGUI _emptyListText;
    [SerializeField] protected GameObject _itemPrefab;

    public abstract void Clear();

    public abstract void Spawn();
    protected virtual void Awake()
    {
        _scrollRect = GetComponentInChildren<ScrollRect>();
        _container = _scrollRect.content.gameObject;
    }

    protected virtual void OnValidate()
    {
        _scrollRect = GetComponentInChildren<ScrollRect>();
        _container = _scrollRect.content.gameObject;
    }

    public virtual void Save()
    {
        Destroy(this.gameObject);
    }
}

public abstract class UIList<TItem> : UIScrollable where TItem : class
{

    public List<TItem> List { get; set; }

    public TItem Current
    {
        get => _current;
        set
        {
            if (_current == value) return;
            OnSelectedChanged(value);
            _current = value;
        }
    }

    protected TItem _current;
    protected abstract void OnSelectedChanged(TItem value);

}
public class SelectAdminUI : UIList<Mascot>
{
    private List<AdminSelectionUI> _selectionList = new();
    private MascotController _mascotController;
    [SerializeField] private GameObject _alertPrefab;
    [SerializeField] private TextMeshProUGUI _alertMessageText;
    [SerializeField] private Button _alertYesButton;
    [SerializeField] private Button _alertNoButton;
    private MascotType _targetPosition;

    protected override void Awake()
    {
        base.Awake();
        _mascotController = GameController.Instance.MascotController;
        if (_alertPrefab != null)
        {
            _alertPrefab.SetActive(false);
        }
    }

    // Setter for target position
    public void SetTargetPosition(MascotType position)
    {
        _targetPosition = position;
    }

    protected override void OnSelectedChanged(Mascot value)
    {
        var selected = _selectionList.FirstOrDefault(s => s.Mascot == value);
        _selectionList.Where(s => s != selected).ForEach(s => s.IsSelected = false);
        if (selected != null) selected.IsSelected = true;
    }

    public override void Clear()
    {
        var children = _container.GetComponentsInChildren<AdminSelectionUI>();
        children.ForEach(c => Destroy(c.gameObject));
        _selectionList.Clear();
    }

    public event Action<Mascot> onAdminSelected = delegate { };

    public void OnSelectionClicked(Mascot admin)
    {
        //Debug.Log($"OnSelectionClicked: admin={admin?.Name ?? "null"}, assigned department={_mascotController.GetAssignedDepartment(admin)}");
        var currentDepartment = _mascotController.GetAssignedDepartment(admin);
        var targetPosition = GetTargetPosition();
        //Debug.Log($"CurrentDepartment={currentDepartment}, TargetPosition={targetPosition}");

        if (admin != null && currentDepartment.HasValue && currentDepartment.Value != targetPosition)
        {
            //Debug.Log("Conflict detected, showing alert");
            ShowReassignmentAlert(admin, currentDepartment.Value, targetPosition);
        }
        else
        {
            //Debug.Log("No conflict, proceeding with selection");
            Current = admin;
            Save();
        }
    }

    private void ShowReassignmentAlert(Mascot admin, MascotType currentDepartment, MascotType targetPosition)
    {
        if (_alertPrefab == null || _alertMessageText == null || _alertYesButton == null || _alertNoButton == null)
        {
            //Debug.LogError("Alert UI components not assigned in SelectAdminUI!");
            Current = admin;
            Save();
            return;
        }

        string message = $"The mascot '{admin.Name}' is currently assigned to {currentDepartment}. " +
                         $"Do you want to reassign it to {targetPosition}?";
        _alertMessageText.text = message;
        //Debug.Log($"Showing alert: {message}");

        _alertYesButton.onClick.RemoveAllListeners();
        _alertYesButton.onClick.AddListener(() =>
        {
           // Debug.Log("Alert: Yes clicked, assigning mascot");
            Current = admin;
            Save();
            _alertPrefab.SetActive(false);
        });

        _alertNoButton.onClick.RemoveAllListeners();
        _alertNoButton.onClick.AddListener(() =>
        {
            //Debug.Log("Alert: No clicked, canceling");
            _alertPrefab.SetActive(false);
        });

        _alertPrefab.SetActive(true);
    }

    private MascotType GetTargetPosition()
    {
       // Debug.Log($"GetTargetPosition: Returning {_targetPosition}");
        return _targetPosition;
    }

    public override void Spawn()
    {
        Clear();
        var noneObj = Instantiate(_itemPrefab, _container.transform, true);
        var noneUi = noneObj.GetComponent<AdminSelectionUI>();
        noneUi.SetAdmin(null);
        noneUi.SetReturn(this);
        _selectionList.Add(noneUi);

        List.ForEach(admin =>
        {
            var obj = Instantiate(_itemPrefab, _container.transform, true);
            var ui = obj.GetComponent<AdminSelectionUI>();
            ui.SetAdmin(admin);
            ui.SetReturn(this);
            _selectionList.Add(ui);
        });
    }

    public override void Save()
    {
        onAdminSelected?.Invoke(Current);
        base.Save();
    }
}