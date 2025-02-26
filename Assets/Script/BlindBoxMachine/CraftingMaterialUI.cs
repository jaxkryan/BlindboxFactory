using System.Text;
using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class CraftingMaterialUI : MonoBehaviour
{
    [SerializeField]
    public List<TextMeshProUGUI> materialText; 

    private void Update()
    {
        UpdateMaterialUI();
    }

    private void UpdateMaterialUI()
    {
        if (MaterialManager.Instance == null || materialText == null)
            return;
        for(int i = 0; i < materialText.Count ; i++)
        {
            var material = MaterialManager.Instance.inventoryMaterials[i];
            //materialText[i].text = $"{material.material}: {material.amount} / {material.maxAmount}";
            materialText[i].text = $" {FormatNumber(material.amount)} / {FormatNumber(material.maxAmount)}";
        }
    }

    private string FormatNumber(int number)
    {
        if (number >= 1_000_000_000)
            return (number / 1_000_000_000f).ToString("0.##") + "B";
        else if (number >= 1_000_000)
            return (number / 1_000_000f).ToString("0.##") + "M";
        else if (number >= 1_000)
            return (number / 1_000f).ToString("0.##") + "k";
        else
            return number.ToString(); // normal number
    }

}
