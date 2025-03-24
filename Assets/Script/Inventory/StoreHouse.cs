using Script.Controller;
using Script.Resources;
using UnityEditor.Localization.Plugins.XLIFF.V12;
using UnityEngine;

public class StoreHouse : MonoBehaviour
{

    public BoxTypeName boxType;
    public long boxamount;
    public long resorceamount;

    BoxController _boxController = GameController.Instance.BoxController;
    ResourceController _resoruceController = GameController.Instance.ResourceController;

    private void Start() 
    {
        _resoruceController.TryGetAllResourceAmounts(out var materials);

        foreach (var res in materials)
        {
            ResourceData resourceData = new ResourceData();
            _resoruceController.TryGetData(res.Key, out var oldResData, out var curAmount);
            resourceData.MaxAmount = oldResData.MaxAmount + resorceamount;
            _resoruceController.TryUpdateData(res.Key, resourceData);
        }

        _boxController.TryGetWarehouseMaxAmount(out var maxAmount);
        bool success = _boxController.TrySetWarehouseMaxAmount(boxamount + maxAmount);
        if (success)
            Debug.Log($"Built {boxType}, decreasing warehouse by {boxamount}.");
        else
            Debug.LogWarning($"Failed to build {boxType}, not enough space.");
    }

    private void OnDestroy()
    {
        _resoruceController.TryGetAllResourceAmounts(out var materials);

        foreach (var res in materials)
        {
            ResourceData resourceData = new ResourceData();
            _resoruceController.TryGetData(res.Key, out var oldResData, out var curAmount);
            resourceData.MaxAmount = oldResData.MaxAmount - resorceamount;
            _resoruceController.TryUpdateData(res.Key, resourceData);
        }

        _boxController.TryGetWarehouseMaxAmount(out var maxAmount);
        _boxController.TrySetAmount(boxType, maxAmount - maxAmount);
        Debug.Log($"Destroyed {boxType}, restoring warehouse by {maxAmount}.");
    }
}
