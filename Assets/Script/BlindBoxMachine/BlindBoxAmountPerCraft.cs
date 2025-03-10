using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BlindBoxAmountPerCraft : MonoBehaviour
{
    [SerializeField]
    public Slider slider;
    [SerializeField]
    public TMP_Text valueText;

    public int maxAmount;

    void Start()
    {
        slider.onValueChanged.AddListener(UpdateValueText);
        UpdateValueText(slider.value); // Initialize the text
    }

    void UpdateValueText(float value)
    {
        valueText.text = ((int)value).ToString(); // Ensure it displays as an integer
    }
}
