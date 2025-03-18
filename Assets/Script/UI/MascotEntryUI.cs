using Script.HumanResource.Administrator;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MascotEntryUI : MonoBehaviour
{
    [SerializeField] private Image _portrait;
    [SerializeField] private Sprite _defaultPortrait;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private Button _detailsButton;
    [SerializeField] private Button _deleteButton;

    private Mascot _mascot;
    private MascotCollectionUI _manager;

    public void Setup(Mascot mascot, MascotCollectionUI manager)
    {
        _mascot = mascot;
        _manager = manager;

        _portrait.sprite = mascot.Portrait ?? _defaultPortrait;
        _nameText.text = mascot.Name != null ? mascot.Name.ToString() : "null name";

        if (_detailsButton != null)
        {
            _detailsButton.onClick.AddListener(() => _manager.ShowMascotDetails(_mascot));
        }
        if (_deleteButton != null)
        {
            _deleteButton.onClick.AddListener(() => _manager.DeleteMascot(_mascot));
        }
    }

    private void OnDestroy()
    {
        if (_detailsButton != null)
        {
            _detailsButton.onClick.RemoveAllListeners();
        }
        if (_deleteButton != null)
        {
            _deleteButton.onClick.RemoveAllListeners();
        }
    }
}