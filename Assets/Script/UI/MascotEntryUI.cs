using Script.Gacha.Base;
using Script.HumanResource.Administrator;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MascotEntryUI : MonoBehaviour
{
    [SerializeField] private Image _portrait;
    [SerializeField] private Sprite _defaultPortrait;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private Image _gradeBorder;
    [SerializeField] private Button _detailsButton;
    [SerializeField] private Button _deleteButton;

    private Mascot _mascot;
    private MascotCollectionUI _manager;

    // Grade colors
    private readonly Dictionary<Grade, Color> _gradeColors = new()
    {
        { Grade.Common, Color.green },
        { Grade.Rare, Color.blue },
        { Grade.Special, new Color(0.5f, 0f, 1f) },
        { Grade.Epic, new Color(1f, 0.5f, 0f) },
        { Grade.Legendary, Color.yellow }
    };

    public void Setup(Mascot mascot, MascotCollectionUI manager)
    {
        _mascot = mascot;
        _manager = manager;

        _portrait.sprite = mascot.Portrait ?? _defaultPortrait;
        _nameText.text = mascot.Name != null ? mascot.Name.ToString() : "Unknown";
        if (_gradeBorder != null)
        {
            _gradeBorder.color = _gradeColors[mascot.Grade];
        }

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