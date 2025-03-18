using System;
using System.Collections.Generic;
using System.Linq;
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
    protected override void OnSelectedChanged(Mascot value)
    {
        var selected = _selectionList.FirstOrDefault(s => s.Mascot == value);
        _selectionList.Where(s => s != selected).ForEach(s => s.IsSelected = false);
        if (selected != null) selected.IsSelected = true;
    }

    private List<AdminSelectionUI> _selectionList = new();

    public override void Clear()
    {
        var children = _container.GetComponentsInChildren<AdminSelectionUI>();
        children.ForEach(c => Destroy(c.gameObject));
        _selectionList.Clear(); // Clear the list to avoid stale references
    }

    public event Action<Mascot> onAdminSelected = delegate { };

    public void OnSelectionClicked(Mascot admin)
    {
        Current = admin;
    }

    public override void Spawn()
    {
        // Clear any existing items to avoid duplicates
        Clear();

        // Add "None" option
        var noneObj = Instantiate(_itemPrefab, _container.transform, true);
        var noneUi = noneObj.GetComponent<AdminSelectionUI>();
        noneUi.SetAdmin(null);
        noneUi.SetReturn(this);
        _selectionList.Add(noneUi);

        // Add actual mascots
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