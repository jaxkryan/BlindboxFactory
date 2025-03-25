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
            gemText.text = amount.ToString();
        else if (resource == Resource.Gold)
            goldText.text = amount.ToString();
    }
}
