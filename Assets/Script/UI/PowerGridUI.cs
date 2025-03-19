using DG.Tweening;
using Script.Controller;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PowerGridUI : MonoBehaviour
{
    // Serialized fields to assign in the Inspector
    [SerializeField] private TextMeshProUGUI powerText;
    [SerializeField] private Slider powerSlider;
    [SerializeField] private Image fillAnimationOverlay;         // Child image for the moving animation
    [SerializeField] private float animationDuration = 0.5f;     // Duration of one left-to-right cycle
    [SerializeField] private float animationDistance = 50f;      // Distance the overlay moves

    private PowerGridController powerGridController;
    private int lastUsage = -1; // Track the last usage to detect changes
    private RectTransform fillRectTransform; // RectTransform of the fill area
    private RectTransform overlayRectTransform; // RectTransform of the animation overlay
    private Tween animationTween;

    private void Start()
    {
        // Access the PowerGridController from the GameController singleton
        powerGridController = GameController.Instance.PowerGridController;
        if (powerGridController == null)
        {
            Debug.LogError("PowerGridController not found in GameController!");
            return;
        }

        // Get the RectTransform of the slider's Fill Area
        fillRectTransform = powerSlider.fillRect;
        if (fillRectTransform == null)
        {
            Debug.LogError("Slider Fill RectTransform not found! Ensure the Slider has a Fill Area.");
            return;
        }

        // Get the RectTransform of the animation overlay
        overlayRectTransform = fillAnimationOverlay.GetComponent<RectTransform>();
        if (overlayRectTransform == null)
        {
            Debug.LogError("Fill Animation Overlay does not have a RectTransform!");
            return;
        }

        // Initial update of the power display
        UpdatePowerDisplay();
    }

    private void Update()
    {
        // Update the UI every frame and animate if there's a change
        UpdatePowerDisplay();
    }

    private void UpdatePowerDisplay()
    {
        if (powerGridController == null) return;

        int currentUsage = powerGridController.EnergyUsage;
        int maxCapacity = powerGridController.GridCapacity;

        // Update the text to show "currentUsage / maxCapacity"
        powerText.text = $"{currentUsage} / {maxCapacity}";

        // Update the slider value instantly (normalized between 0 and 1)
        float targetValue = maxCapacity > 0 ? (float)currentUsage / maxCapacity : 0f;
        powerSlider.value = targetValue; // Set directly, no animation here

        // Animate the fill overlay if the usage has changed
        if (currentUsage != lastUsage)
        {
            AnimateFillOverlay();
            lastUsage = currentUsage; // Update the tracked value
        }
    }

    private void AnimateFillOverlay()
    {
        // Kill any existing animation to avoid overlap
        animationTween?.Kill();

        // Reset the overlay position to the left
        Vector2 startPos = overlayRectTransform.anchoredPosition;
        startPos.x = -animationDistance; // Start off-screen to the left
        overlayRectTransform.anchoredPosition = startPos;

        // Animate the overlay from left to right
        animationTween = overlayRectTransform.DOAnchorPosX(
            animationDistance, // Move to the right
            animationDuration
        ).SetEase(Ease.Linear).SetLoops(2, LoopType.Restart); // Two loops for a clear effect
    }

    private void OnDestroy()
    {
        // Clean up the DOTween animation to prevent memory leaks
        animationTween?.Kill();
    }
}