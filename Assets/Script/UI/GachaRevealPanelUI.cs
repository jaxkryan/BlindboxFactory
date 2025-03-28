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

    private List<Mascot> _mascotsToReveal;
    private int _currentMascotIndex;
    private Sequence _animationSequence;
    private Action _onComplete;

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
        _nextButton.gameObject.SetActive(false);
        _confirmButton.gameObject.SetActive(false);

        _nextButton.onClick.AddListener(OnNextClicked);
        _confirmButton.onClick.AddListener(OnConfirmClicked);

        // Set up tap-to-skip using the existing Canvas
        var button = gameObject.GetComponent<Button>() ?? gameObject.AddComponent<Button>();
        button.onClick.AddListener(SkipAnimation);

        // Ensure the background is black
        //_backgroundCanvasGroup.alpha = 1f;
        //_backgroundCanvasGroup.GetComponent<Image>().color = Color.black;

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
        _animationSequence?.Kill();
    }

    public void RevealMascots(List<Mascot> mascots, Action onComplete)
    {
        _mascotsToReveal = mascots;
        _currentMascotIndex = 0;
        _onComplete = onComplete;

        RevealNextMascot();
    }

    private void RevealNextMascot()
    {
        if (_currentMascotIndex >= _mascotsToReveal.Count)
        {
            _confirmButton.gameObject.SetActive(true);
            return;
        }

        // Reset UI elements
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

        // Step 2: Change the card color to the grade color
        var gradeColor = _gradeColors[mascot.Grade];
        _animationSequence.Append(_cardImage.DOColor(gradeColor, 0.5f));

        // Step 3: Update the UI (portrait, name, policies, buttons)
        _animationSequence.AppendCallback(() => UpdateUI(mascot, gradeColor));
    }

    private void UpdateUI(Mascot mascot, Color gradeColor)
    {
        // Show the portrait and grade border
        _portraitImage.sprite = mascot.Portrait;
        _portraitImage.gameObject.SetActive(true);
        _gradeBorder.color = gradeColor;
        _gradeBorder.gameObject.SetActive(true);

        // Display the name and policies
        _nameText.text = mascot.Name.ToString();
        _policiesText.text = mascot.Policies.Aggregate("", (current, p) => current + $"• {p.Description}\n").TrimEnd('\n');

        // Show the appropriate button
        if (_currentMascotIndex == _mascotsToReveal.Count - 1)
        {
            _confirmButton.gameObject.SetActive(true);
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
        Destroy(gameObject);
    }

    private void SkipAnimation()
    {
        if (_animationSequence != null && _animationSequence.IsPlaying())
        {
            // Complete the animation sequence (finishes tweens like card movement)
            _animationSequence.Complete();

            // Ensure the UI is updated to its final state
            var mascot = _mascotsToReveal[_currentMascotIndex];
            var gradeColor = _gradeColors[mascot.Grade];
            UpdateUI(mascot, gradeColor);
        }
    }
}