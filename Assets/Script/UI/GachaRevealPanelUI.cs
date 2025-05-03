using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Script.Gacha.Base;
using Script.HumanResource.Administrator;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GachaRevealPanelUI : MonoBehaviour
{
    [SerializeField] private Image _cardImage;
    [SerializeField] private Image _portraitImage;
    [SerializeField] private Image _gradeBorder;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _policiesText;
    [SerializeField] private Button _nextButton;
    [SerializeField] private Button _confirmButton;
    [SerializeField] private CanvasGroup _backgroundCanvasGroup;
    [SerializeField] private Button _skipAllButton; // New "Skip All" button
    [SerializeField] private GameObject _summaryPanel; // New summary panel for 10-pull
    [SerializeField] private MascotDetailUI _mascotDetailPanel; // New info panel for mascot details

    [SerializeField] private List<MascotSummarySlot> _mascotSlots = new List<MascotSummarySlot>(); // 10 slots
    [SerializeField] private Button _summaryExitButton; // Exit button for summary screen

    // Gradient colors for the background (one per grade)
    [SerializeField] private List<Gradient> _gradeBackgroundGradients = new List<Gradient>(); // 5 gradients for Common, Rare, Special, Epic, Legendary

    // Sprites for the grade border (one per grade)
    [SerializeField] private List<Sprite> _gradeBorderSprites = new List<Sprite>(); // 5 sprites for Common, Rare, Special, Epic, Legendary

    private List<Mascot> _mascotsToReveal;
    private int _currentMascotIndex;
    private Sequence _animationSequence;
    private Action _onComplete;

    private void Awake()
    {
        _nextButton.gameObject.SetActive(false);
        _confirmButton.gameObject.SetActive(false);
        _skipAllButton.gameObject.SetActive(false);
        _summaryPanel.SetActive(false);
        _mascotDetailPanel.gameObject.SetActive(false);

        _nextButton.onClick.AddListener(OnNextClicked);
        _confirmButton.onClick.AddListener(OnConfirmClicked);
        _skipAllButton.onClick.AddListener(OnSkipAllClicked);
        _summaryExitButton.onClick.AddListener(OnSummaryExitClicked);

        // Set up tap-to-skip using the existing Canvas
        var button = gameObject.GetComponent<Button>() ?? gameObject.AddComponent<Button>();
        button.onClick.AddListener(SkipAnimation);

        // Hide UI elements initially
        _portraitImage.gameObject.SetActive(false);
        _gradeBorder.gameObject.SetActive(false);
        _nameText.text = "";
        _policiesText.text = "";
    }

    private void OnDestroy()
    {
        _nextButton.onClick.RemoveAllListeners();
        _confirmButton.onClick.RemoveAllListeners();
        _skipAllButton.onClick.RemoveAllListeners();
        _summaryExitButton.onClick.RemoveAllListeners();
        _animationSequence?.Kill();
    }

    public void RevealMascots(List<Mascot> mascots, Action onComplete)
    {
        // Reset the UI state before starting a new reveal
        ResetUIState();

        _mascotsToReveal = mascots;
        _currentMascotIndex = 0;
        _onComplete = onComplete;

        // Show "Skip All" button only for 10-pulls
        _skipAllButton.gameObject.SetActive(mascots.Count > 1);

        RevealNextMascot();
    }

    private void ResetUIState()
    {
        // Hide summary panel and mascot detail panel
        _summaryPanel.SetActive(false);
        _mascotDetailPanel.gameObject.SetActive(false);

        // Show reveal UI elements
        _cardImage.gameObject.SetActive(true);
        _portraitImage.gameObject.SetActive(false); // Will be shown during animation
        _gradeBorder.gameObject.SetActive(false); // Will be shown during animation
        _nameText.gameObject.SetActive(true);
        _policiesText.gameObject.SetActive(true);

        // Reset text
        _nameText.text = "";
        _policiesText.text = "";

        // Hide buttons until needed
        _nextButton.gameObject.SetActive(false);
        _confirmButton.gameObject.SetActive(false);
        _skipAllButton.gameObject.SetActive(false);

        // Stop any ongoing animations
        _animationSequence?.Kill();
    }

    private void RevealNextMascot()
    {
        if (_currentMascotIndex >= _mascotsToReveal.Count)
        {
            if (_mascotsToReveal.Count == 1)
            {
                // For single pull, show the confirm button to exit
                _confirmButton.gameObject.SetActive(true);
            }
            else
            {
                // For 10-pull, show the summary screen
                ShowSummaryScreen();
            }
            return;
        }

        // Reset UI elements for the next mascot
        _cardImage.color = Color.white;
        _portraitImage.gameObject.SetActive(false);
        _gradeBorder.gameObject.SetActive(false);
        _nameText.text = "";
        _policiesText.text = "";
        _nextButton.gameObject.SetActive(false);
        _confirmButton.gameObject.SetActive(false);

        var mascot = _mascotsToReveal[_currentMascotIndex];

        _animationSequence?.Kill();
        _animationSequence = DOTween.Sequence();

        // Step 1: Spin the card (stay at top center)
        _cardImage.transform.rotation = Quaternion.identity;
        _animationSequence.Append(_cardImage.transform.DORotate(new Vector3(0, 360, 0), 2f, RotateMode.FastBeyond360)
            .SetLoops(3, LoopType.Restart)
            .SetEase(Ease.InOutQuad));

        // Step 2: Apply the grade background gradient to the card
        int gradeIndex = (int)mascot.Grade;
        if (gradeIndex >= 0 && gradeIndex < _gradeBackgroundGradients.Count)
        {
            Gradient gradient = _gradeBackgroundGradients[gradeIndex];
            _animationSequence.Append(_cardImage.DOGradientColor(gradient, 0.5f));
        }

        // Step 3: Update the UI (portrait, name, policies, buttons)
        _animationSequence.AppendCallback(() => UpdateUI(mascot));
    }

    private void UpdateUI(Mascot mascot)
    {
        // Show the portrait and grade border
        _portraitImage.sprite = mascot.Portrait;
        _portraitImage.gameObject.SetActive(true);

        int gradeIndex = (int)mascot.Grade;
        if (gradeIndex >= 0 && gradeIndex < _gradeBorderSprites.Count)
        {
            _gradeBorder.sprite = _gradeBorderSprites[gradeIndex];
        }
        _gradeBorder.gameObject.SetActive(true);

        // Display the name and policies
        _nameText.text = mascot.Name.ToString();
        _policiesText.text = mascot.Policies.Aggregate("", (current, p) => current + $"• {p.Description}\n").TrimEnd('\n');

        // Show the appropriate button
        if (_currentMascotIndex == _mascotsToReveal.Count - 1)
        {
            if (_mascotsToReveal.Count == 1)
            {
                _confirmButton.gameObject.SetActive(true);
            }
            else
            {
                _nextButton.gameObject.SetActive(true);
            }
        }
        else
        {
            _nextButton.gameObject.SetActive(true);
        }
    }

    private void OnNextClicked()
    {
        _currentMascotIndex++;
        RevealNextMascot();
    }

    private void OnConfirmClicked()
    {
        _onComplete?.Invoke();
    }

    private void OnSkipAllClicked()
    {
        // Skip all remaining animations and go to the final display
        _animationSequence?.Kill();
        _currentMascotIndex = _mascotsToReveal.Count;
        RevealNextMascot();
    }

    private void SkipAnimation()
    {
        if (_animationSequence != null && _animationSequence.IsPlaying())
        {
            // Complete the animation sequence for the current mascot
            _animationSequence.Complete();

            // Update the UI to its final state for the current mascot
            var mascot = _mascotsToReveal[_currentMascotIndex];
            UpdateUI(mascot);

            // Do NOT move to the next mascot; let the "Next" button handle that
        }
    }

    private void ShowSummaryScreen()
    {
        AudioManager.Instance.PlaySfx("button");
        // Hide reveal UI elements
        _cardImage.gameObject.SetActive(false);
        _portraitImage.gameObject.SetActive(false);
        _gradeBorder.gameObject.SetActive(false);
        _nameText.gameObject.SetActive(false);
        _policiesText.gameObject.SetActive(false);
        _nextButton.gameObject.SetActive(false);
        _confirmButton.gameObject.SetActive(false);
        _skipAllButton.gameObject.SetActive(false);

        // Show the summary panel
        _summaryPanel.SetActive(true);

        // Populate the summary slots
        for (int i = 0; i < _mascotSlots.Count && i < _mascotsToReveal.Count; i++)
        {
            var mascot = _mascotsToReveal[i];
            var slot = _mascotSlots[i];
            int gradeIndex = (int)mascot.Grade;
            Gradient backgroundGradient = gradeIndex >= 0 && gradeIndex < _gradeBackgroundGradients.Count ? _gradeBackgroundGradients[gradeIndex] : null;
            Sprite borderSprite = gradeIndex >= 0 && gradeIndex < _gradeBorderSprites.Count ? _gradeBorderSprites[gradeIndex] : null;
            slot.Setup(mascot, backgroundGradient, borderSprite, () =>
            {
                // Show the mascot details when the portrait is clicked
                _mascotDetailPanel.gameObject.SetActive(true);
                _mascotDetailPanel.DisplayDetails(mascot);
            });
        }
    }

    private void OnSummaryExitClicked()
    {
        _onComplete?.Invoke();
    }
}

// Helper class for summary screen slots
[System.Serializable]
public class MascotSummarySlot
{
    [SerializeField] private Image _portraitImage;
    [SerializeField] private Image _backgroundImage; // New background image for gradient
    [SerializeField] private Image _gradeBorder;
    [SerializeField] private Button _portraitButton;

    public void Setup(Mascot mascot, Gradient backgroundGradient, Sprite borderSprite, Action onClick)
    {
        _portraitImage.sprite = mascot.Portrait;
        if (backgroundGradient != null)
        {
            _backgroundImage.material = new Material(Shader.Find("UI/Default"));
            _backgroundImage.material.SetColor("_Color", backgroundGradient.Evaluate(0.5f)); // Use a midpoint color for simplicity
        }
        if (borderSprite != null)
        {
            _gradeBorder.sprite = borderSprite;
        }
        _portraitButton.onClick.RemoveAllListeners();
        _portraitButton.onClick.AddListener(() => onClick?.Invoke());
    }
}