using Script.Machine.Machines.Generator;
using TMPro;
using UnityEngine;

public class EnergyDisplay : MonoBehaviour
{
    [SerializeField]
    public Generator generator;
    public TextMeshProUGUI powerText;

    void Start()
    {
        if (generator != null && powerText != null)
        {
            powerText.text = "Power: " + generator.Power.ToString();
        }
    }
}
