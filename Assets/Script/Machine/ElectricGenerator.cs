using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ElectricGenerator : MonoBehaviour
{
    public int Level { get; private set; } = 1;
    public float MaxLoad { get; private set; } = 100f;
    public static List<ElectricGenerator> AllElectricGenerators = new List<ElectricGenerator>();

    [SerializeField] private Slider loadSliderReference; // Manually assigned slider
    private static Slider LoadSlider; // Shared energy slider
    private static Tween idleTween;

    private void Awake()
    {
        if (LoadSlider == null && loadSliderReference != null)
        {
            LoadSlider = loadSliderReference; // Assign the correct slider
        }

        AllElectricGenerators.Add(this);
        UpdatePowerDistribution();
    }

    private void OnDestroy()
    {
        idleTween?.Kill(); // Stop idle animation when destroyed
    }

    public void Upgrade()
    {
        Level++;
        MaxLoad *= 1.1f; // Increase power capacity by 10%
        UpdatePowerDistribution();
    }

    public void DestroyGenerator()
    {
        AllElectricGenerators.Remove(this);
        UpdatePowerDistribution();
        Destroy(gameObject);
    }

    public static void UpdatePowerDistribution()
    {
        float totalEnergy = GetTotalAvailableEnergy() / 2; // Simulating energy usage
        AnimateSlider(totalEnergy);
    }

    public static float GetTotalAvailableEnergy()
    {
        return AllElectricGenerators.Sum(generator => generator.MaxLoad);
    }

    public static void AnimateSlider(float totalEnergy)
    {
        if (LoadSlider != null)
        {
            float maxEnergy = AllElectricGenerators.Count > 0 ? AllElectricGenerators.Max(g => g.MaxLoad) * AllElectricGenerators.Count : 1f;
            float targetValue = totalEnergy / maxEnergy;

            // Kill idle animation while updating
            idleTween?.Kill();

            // Smooth transition and immediately start idle animation to avoid reset
            LoadSlider.DOValue(targetValue, 1.2f)
                .SetEase(Ease.OutSine);
        }
    }

  
}
