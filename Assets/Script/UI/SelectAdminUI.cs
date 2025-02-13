using System;
using System.Collections.Generic;
using System.Linq;
using Script.HumanResource.Administrator;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class UIScrollable : MonoBehaviour {
    [SerializeField] protected ScrollRect _scrollRect;
    [SerializeField] protected GameObject _container;
    [SerializeField] protected TextMeshProUGUI _emptyListText;
    [SerializeField] protected GameObject _itemPrefab;

    public abstract void Clear();

    public abstract void Spawn();
    protected virtual void Awake() {
        _scrollRect = GetComponentInChildren<ScrollRect>();
        _container = _scrollRect.content.gameObject;
    }
    
    protected virtual void OnValidate() {
        _scrollRect = GetComponentInChildren<ScrollRect>();
        _container = _scrollRect.content.gameObject;
    }

    public virtual void Save() {
        Destroy(this.gameObject);
    }
}

public abstract class UIList<TItem> : UIScrollable where TItem : class {
    
    public List<TItem> List { get; set; }

    public TItem Current {
        get => _current;
        set {
            if (_current == value) return;
            OnSelectedChanged(value);
            _current = value;
        }
    }

    protected TItem _current;
    protected abstract void OnSelectedChanged(TItem value);

}

public class SelectAdminUI : UIList<Administrator> {
    public List<Administrator> List { get; set; }
    public Administrator Current {
        get => _current;
        set {
            OnSelectedChanged(value);
            _current = value;
        }
    }
    private Administrator _current;

    protected override void OnSelectedChanged(Administrator value) {
        var selected = _selectionList.FirstOrDefault(s => s.Administrator == value);
        _selectionList.Where(s => s != selected).ForEach(s => s.IsSelected = false);
        if (selected != null) selected.IsSelected = true;
    }


    private List<AdminSelectionUI> _selectionList = new();

    public override void Clear() {
        var children = _container.GetComponentsInChildren<AdminSelectionUI>();
        children.ForEach(c => Destroy(c.gameObject));
    }
    
    public event Action<Administrator> onAdminSelected = delegate { };
    public void OnSelectionClicked(Administrator admin) {
        Current = admin;
    }

    public override void Spawn() {
        List.ForEach(admin => {
            var obj = Instantiate(_itemPrefab, _container.transform, true);
            var ui = obj.GetComponent<AdminSelectionUI>();
            ui.SetAdmin(admin);
            ui.SetReturn(this);
        });
        if (List.Count == 0) Instantiate(_emptyListText, _container.transform);
    }

    public override void Save() {
        onAdminSelected?.Invoke(Current);
        base.Save();
    }
}
