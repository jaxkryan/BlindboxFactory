using DG.Tweening;
using UnityEngine;
using System;

[RequireComponent(typeof(RectTransform))]
public class DotweenAnimation : MonoBehaviour
{
    [Header("Animation Settings")]
    [SerializeField] private float _duration = 0.5f;
    [SerializeField] private Ease _scaleEaseIn = Ease.OutBack;
    [SerializeField] private Ease _scaleEaseOut = Ease.InBack;
    [SerializeField] private Ease _fadeEaseIn = Ease.OutQuad;
    [SerializeField] private Ease _fadeEaseOut = Ease.InQuad;
    [SerializeField] private Ease _slideEaseIn = Ease.OutQuad;
    [SerializeField] private Ease _slideEaseOut = Ease.InQuad;
    [SerializeField] private float _slideOffsetHorizontal = 1000f; // Distance for left/right slides
    [SerializeField] private float _slideOffsetVertical = 1000f;   // Distance for bottom/top slides

    private RectTransform _rectTransform;
    private CanvasGroup _canvasGroup;
    private Vector3 _originalScale;
    private Vector3 _originalPosition;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        _canvasGroup = GetComponent<CanvasGroup>();
        _originalScale = _rectTransform.localScale;
        _originalPosition = _rectTransform.anchoredPosition3D;
    }

    // Scale and fade in
    public void AnimateIn()
    {
        AnimateIn(null);
    }

    public void AnimateIn(Action onComplete)
    {
        _rectTransform.DOKill();
        if (_canvasGroup != null) _canvasGroup.DOKill();

        _rectTransform.localScale = Vector3.zero;
        if (_canvasGroup != null) _canvasGroup.alpha = 0f;

        gameObject.SetActive(true);

        Sequence sequence = DOTween.Sequence();
        sequence.Join(_rectTransform.DOScale(_originalScale, _duration).SetEase(_scaleEaseIn));
        if (_canvasGroup != null)
        {
            sequence.Join(_canvasGroup.DOFade(1f, _duration).SetEase(_fadeEaseIn));
        }

        if (onComplete != null)
        {
            sequence.OnComplete(() => onComplete.Invoke());
        }
    }

    // Scale and fade out
    public void AnimateOut()
    {
        AnimateOut(null);
    }

    public void AnimateOut(Action onComplete)
    {
        _rectTransform.DOKill();
        if (_canvasGroup != null) _canvasGroup.DOKill();

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        Sequence sequence = DOTween.Sequence();
        sequence.Join(_rectTransform.DOScale(Vector3.zero, _duration).SetEase(_scaleEaseOut));
        if (_canvasGroup != null)
        {
            sequence.Join(_canvasGroup.DOFade(0f, _duration).SetEase(_fadeEaseOut));
        }

        sequence.OnComplete(() =>
        {
            gameObject.SetActive(false);
            onComplete?.Invoke();
        });
    }

    // Slide in from left
    public void SlideInFromLeft()
    {
        SlideInFromLeft(null);
    }

    public void SlideInFromLeft(Action onComplete)
    {
        _rectTransform.DOKill();
        if (_canvasGroup != null) _canvasGroup.DOKill();

        Vector3 offScreenPos = _originalPosition - new Vector3(_slideOffsetHorizontal, 0, 0);
        _rectTransform.anchoredPosition3D = offScreenPos;
        if (_canvasGroup != null) _canvasGroup.alpha = 0f;

        gameObject.SetActive(true);

        Sequence sequence = DOTween.Sequence();
        sequence.Join(_rectTransform.DOAnchorPos3D(_originalPosition, _duration).SetEase(_slideEaseIn));
        if (_canvasGroup != null)
        {
            sequence.Join(_canvasGroup.DOFade(1f, _duration).SetEase(_fadeEaseIn));
        }

        if (onComplete != null)
        {
            sequence.OnComplete(() => onComplete.Invoke());
        }
    }

    // Slide out to left
    public void SlideOutToLeft()
    {
        SlideOutToLeft(null);
    }

    public void SlideOutToLeft(Action onComplete)
    {
        _rectTransform.DOKill();
        if (_canvasGroup != null) _canvasGroup.DOKill();

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        Vector3 offScreenPos = _originalPosition - new Vector3(_slideOffsetHorizontal, 0, 0);

        Sequence sequence = DOTween.Sequence();
        sequence.Join(_rectTransform.DOAnchorPos3D(offScreenPos, _duration).SetEase(_slideEaseOut));
        if (_canvasGroup != null)
        {
            sequence.Join(_canvasGroup.DOFade(0f, _duration).SetEase(_fadeEaseOut));
        }

        sequence.OnComplete(() =>
        {
            gameObject.SetActive(false);
            onComplete?.Invoke();
        });
    }

    // New: Slide in from bottom
    public void SlideInFromBottom()
    {
        SlideInFromBottom(null);
    }

    public void SlideInFromBottom(Action onComplete)
    {
        _rectTransform.DOKill();
        if (_canvasGroup != null) _canvasGroup.DOKill();

        // Set initial position off-screen below
        Vector3 offScreenPos = _originalPosition - new Vector3(0, _slideOffsetVertical, 0);
        _rectTransform.anchoredPosition3D = offScreenPos;
        if (_canvasGroup != null) _canvasGroup.alpha = 0f;

        gameObject.SetActive(true);

        Sequence sequence = DOTween.Sequence();
        sequence.Join(_rectTransform.DOAnchorPos3D(_originalPosition, _duration).SetEase(_slideEaseIn));
        if (_canvasGroup != null)
        {
            sequence.Join(_canvasGroup.DOFade(1f, _duration).SetEase(_fadeEaseIn));
        }

        if (onComplete != null)
        {
            sequence.OnComplete(() => onComplete.Invoke());
        }
    }

    // New: Slide out to bottom
    public void SlideOutToBottom()
    {
        SlideOutToBottom(null);
    }

    public void SlideOutToBottom(Action onComplete)
    {
        _rectTransform.DOKill();
        if (_canvasGroup != null) _canvasGroup.DOKill();

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }

        // Calculate off-screen position below
        Vector3 offScreenPos = _originalPosition - new Vector3(0, _slideOffsetVertical, 0);

        Sequence sequence = DOTween.Sequence();
        sequence.Join(_rectTransform.DOAnchorPos3D(offScreenPos, _duration).SetEase(_slideEaseOut));
        if (_canvasGroup != null)
        {
            sequence.Join(_canvasGroup.DOFade(0f, _duration).SetEase(_fadeEaseOut));
        }

        sequence.OnComplete(() =>
        {
            gameObject.SetActive(false);
            onComplete?.Invoke();
        });
    }

    public void ResetState()
    {
        _rectTransform.DOKill();
        if (_canvasGroup != null) _canvasGroup.DOKill();

        _rectTransform.localScale = _originalScale;
        _rectTransform.anchoredPosition3D = _originalPosition;
        if (_canvasGroup != null) _canvasGroup.alpha = 1f;
        gameObject.SetActive(true);
    }

    private void OnDestroy()
    {
        _rectTransform.DOKill();
        if (_canvasGroup != null) _canvasGroup.DOKill();
    }
}