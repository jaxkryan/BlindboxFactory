using UnityEngine;

public class TestAdding : MonoBehaviour
{
    public void IncreaseMaterialMaxAmount()
    {
        foreach (var mat in MaterialManager.Instance.inventoryMaterials)
        {
            mat.amount += 110;
            mat.maxAmount += 110000;
        }
    }
}
