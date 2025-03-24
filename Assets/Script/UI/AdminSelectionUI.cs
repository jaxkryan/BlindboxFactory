using Script.HumanResource.Administrator;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Script.Gacha.Base;
using System.Collections.Generic;
using System.Linq;

public class AdminSelectionUI : MonoBehaviour
{
    [SerializeField] private Image _portrait;
    [SerializeField] private TextMeshProUGUI _nameText; // Merged first and last name into a single field
    [SerializeField] private TextMeshProUGUI _policies;
    [SerializeField] private Image _borderImage; // Selection border (yellow dashing effect)
    [SerializeField] private Image _gradeBorder; // Grade border (colored by grade)
    public Mascot Mascot { get; private set; }
    private SelectAdminUI _ui;

    // Grade colors
    private readonly Dictionary<Grade, Color> _gradeColors = new()
    {
        { Grade.Common, Color.green },
        { Grade.Rare, Color.blue },
        { Grade.Special, new Color(0.5f, 0f, 1f) },
        { Grade.Epic, new Color(1f, 0.5f, 0f) },
        { Grade.Legendary, Color.yellow }
    };

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            _isSelected = value;
            if (_isSelected) Selected();
            else Unselected();
        }
    }

    private bool _isSelected;
    private Sequence _highlightSequence;

    private void Awake()
    {
        // Initialize the selection border (yellow dashing effect)
        if (_borderImage != null)
        {
            Color borderColor = _borderImage.color;
            borderColor.a = 0f;
            _borderImage.color = borderColor;
        }

        // Initialize the grade border
        if (_gradeBorder != null)
        {
            Color gradeColor = Color.white; // Default color
            gradeColor.a = 1f; // Always fully visible
            _gradeBorder.color = gradeColor;
        }
    }

    private void OnDestroy()
    {
        _highlightSequence?.Kill();
    }

    private void Selected()
    {
        if (_borderImage == null) return;

        _highlightSequence?.Kill();
        _highlightSequence = DOTween.Sequence();

        Color highlightColor = Color.yellow;
        highlightColor.a = 0f;
        _borderImage.color = highlightColor;

        _highlightSequence.Append(_borderImage.DOFade(1f, 0.5f))
                         .Append(_borderImage.DOFade(0f, 0.5f))
                         .SetLoops(-1, LoopType.Yoyo);
    }

    private void Unselected()
    {
        if (_borderImage == null) return;

        _highlightSequence?.Kill();
        _borderImage.DOFade(0f, 0.2f);
    }

    public void SelectedAdmin()
    {
        Debug.Log("Selected admin: " + this.gameObject.name);
        _ui.OnSelectionClicked(Mascot);
    }

    public void SetReturn(SelectAdminUI ui) => _ui = ui;

    public void SetAdmin(Mascot admin)
    {
        if (admin == null)
        {
            _portrait.sprite = null;
            _nameText.text = "None";
            _policies.text = "";
            _policies.color = Color.white;
            if (_gradeBorder != null)
            {
                Color gradeColor = Color.white;
                gradeColor.a = 1f;
                _gradeBorder.color = gradeColor;
            }
            Mascot = null;
        }
        else
        {
            _portrait.sprite = admin.Portrait;
            _nameText.text = admin.Name.ToString(); // Display full name (FirstName LastName)
            var policies = admin.Policies.Aggregate("", (current, p) => current + $"• {p.Description}\n").TrimEnd('\n');
            _policies.text = policies;
            _policies.color = _gradeColors[admin.Grade];
            if (_gradeBorder != null)
            {
                Color gradeColor = _gradeColors[admin.Grade];
                gradeColor.a = 1f; // Ensure the grade border is fully visible
                _gradeBorder.color = gradeColor;
            }
            Mascot = admin;
        }
    }
}