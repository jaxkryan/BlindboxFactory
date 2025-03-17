using Script.HumanResource.Administrator;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AdminSelectionUI : MonoBehaviour {
    [SerializeField] private Image _portrait;
    [SerializeField] private TextMeshProUGUI _firstName;
    [SerializeField] private TextMeshProUGUI _lastName;
    [SerializeField] private TextMeshProUGUI _policies;
    [SerializeField] private GameObject _selectedEffect;
    public Mascot Mascot { get; private set; }
    private SelectAdminUI _ui;

    public bool IsSelected {
        get => _isSelected;
        set {
            _isSelected = value;
            if (_isSelected) Selected();
            else Unselected();
        }
    }

    private bool _isSelected;


    private void Selected() { _selectedEffect?.SetActive(true); }

    private void Unselected() { _selectedEffect?.SetActive(false); }


    public void SelectedAdmin() {
        Debug.Log("Selected admin: " + this.gameObject.name);
        _ui.OnSelectionClicked(Mascot);
    }

    public void SetReturn(SelectAdminUI ui) => _ui = ui;

    public void SetAdmin(Mascot admin) {
        _portrait.sprite = admin.Portrait;
        _firstName.text = admin.Name.FirstName;
        _lastName.text = admin.Name.LastName;
        var policies = "";
        admin.Policies.ForEach(x => policies += x.Description + "\n");
        _policies.text = policies;
        Mascot = admin;
    }
}