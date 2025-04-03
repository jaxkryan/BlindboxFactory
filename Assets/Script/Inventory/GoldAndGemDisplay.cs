using Script.Controller;
using Script.Resources;
using TMPro;
using UnityEngine;

public class GoldAndGemDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text gemText;
    [SerializeField] private TMP_Text goldText;
    private void Start()
    {
        UpdateResourceUI(Resource.Gem);
        UpdateResourceUI(Resource.Gold);

        GameController.Instance.ResourceController.onResourceAmountChanged += OnResourceAmountChanged;
    }

    private void Update()
    {
        UpdateResourceUI(Resource.Gem);
        UpdateResourceUI(Resource.Gold);

        GameController.Instance.ResourceController.onResourceAmountChanged += OnResourceAmountChanged;
    }

    private void OnResourceAmountChanged(Resource resource, long oldAmount, long newAmount)
    {
        UpdateResourceUI(resource);
    }

    private void UpdateResourceUI(Resource resource)
    {
        if (!GameController.Instance.ResourceController.TryGetAmount(resource, out long amount))
        {
            amount = 0;
        }

        if (resource == Resource.Gem)
            gemText.text = FormatNumber(amount);
        else if (resource == Resource.Gold)
            goldText.text = FormatNumber(amount);
    }

    private string FormatNumber(long number)
    {
        if (number >= 1_000_000_000_000)
            return (number / 1_000_000_000_000f).ToString("0.##") + "T";
        else if (number >= 1_000_000_000)
            return (number / 1_000_000_000f).ToString("0.##") + "B";
        else if (number >= 1_000_000)
            return (number / 1_000_000f).ToString("0.##") + "M";
        else if (number >= 1_000)
            return (number / 1_000f).ToString("0.##") + "k";
        else
            return number.ToString(); // normal number
    }
}
