using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ElectricGeneratorUIManager : MonoBehaviour
{
    public static ElectricGeneratorUIManager Instance { get; private set; }

    [SerializeField] private Slider energySlider;
    [SerializeField] private TMP_Text energyText;

    public Slider EnergySlider => energySlider;
    public TMP_Text EnergyText => energyText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
}
