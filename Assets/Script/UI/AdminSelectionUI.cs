using Script.HumanResource.Administrator;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; // Add this namespace for DOTween

public class AdminSelectionUI : MonoBehaviour
{
    [SerializeField] private Image _portrait;
    [SerializeField] private TextMeshProUGUI _firstName;
    [SerializeField] private TextMeshProUGUI _lastName;
    [SerializeField] private TextMeshProUGUI _policies;
    [SerializeField] private Image _borderImage; // New field for the border to animate
    public Mascot Mascot { get; private set; }
    private SelectAdminUI _ui;

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
    private Sequence _highlightSequence; // Store the animation sequence

    private void Awake()
    {
        // Ensure the border starts invisible
        if (_borderImage != null)
        {
            Color borderColor = _borderImage.color;
            borderColor.a = 0f;
            _borderImage.color = borderColor;
        }
    }

    private void OnDestroy()
    {
        // Clean up the animation when the object is destroyed
        _highlightSequence?.Kill();
    }

    private void Selected()
    {
        if (_borderImage == null) return;

        // Kill any existing animation
        _highlightSequence?.Kill();

        // Create a new sequence for the dashing light effect
        _highlightSequence = DOTween.Sequence();

        // Initial setup: make the border visible
        Color highlightColor = Color.yellow; // Customize this color as needed
        highlightColor.a = 0f;
        _borderImage.color = highlightColor;

        // Animate the border to simulate a dashing light running along it
        _highlightSequence.Append(_borderImage.DOFade(1f, 0.5f)) // Fade in
                         .Append(_borderImage.DOFade(0f, 0.5f)) // Fade out
                         .SetLoops(-1, LoopType.Yoyo); // Loop indefinitely, yoyo style
    }

    private void Unselected()
    {
        if (_borderImage == null) return;

        // Stop the animation and fade out the border
        _highlightSequence?.Kill();
        _borderImage.DOFade(0f, 0.2f); // Quick fade out when unselected
    }

    public void SelectedAdmin()
    {
        Debug.Log("Selected admin: " + this.gameObject.name);
        _ui.OnSelectionClicked(Mascot);
    }

    public void SetReturn(SelectAdminUI ui) => _ui = ui;

    public void SetAdmin(Mascot admin)
    {
        _portrait.sprite = admin.Portrait;
        _firstName.text = admin.Name.FirstName;
        _lastName.text = admin.Name.LastName;
        var policies = "";
        admin.Policies.ForEach(x => policies += x.Description + "\n");
        _policies.text = policies;
        Mascot = admin;
    }
}