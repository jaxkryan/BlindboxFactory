using Script.Gacha.Base;
using Script.HumanResource.Administrator;
using System.Collections.Generic;
using ZLinq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MascotDetailUI : MonoBehaviour
{
    [SerializeField] private Image _portrait;
    [SerializeField] private Sprite _defaultPortrait;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _policiesText;
    [SerializeField] private TextMeshProUGUI _gradeText;
    [SerializeField] private Button _closeButton;

    // Grade colors
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
        if (_closeButton != null)
        {
            _closeButton.onClick.AddListener(() => gameObject.SetActive(false));
        }
    }

    private void OnDestroy()
    {
        if (_closeButton != null)
        {
            _closeButton.onClick.RemoveAllListeners();
        }
    }

    public void DisplayDetails(Mascot mascot)
    {
        if (mascot == null)
        {
            // Display empty state
            _portrait.sprite = _defaultPortrait;
            _nameText.text = "";
            _policiesText.text = "";
            _policiesText.color = Color.white; // Default color when no mascot
            _gradeText.text = "";
        }
        else
        {
            _portrait.sprite = mascot.Portrait ?? _defaultPortrait;
            _nameText.text = mascot.Name.ToString();
            var policies = mascot.Policies.AsValueEnumerable().Aggregate("", (current, p) => current + $"• {p.Description}\n").TrimEnd('\n');
            _policiesText.text = policies;
            _policiesText.color = _gradeColors[mascot.Grade];
            _gradeText.text = $"Grade: {mascot.Grade}";
        }
    }
}