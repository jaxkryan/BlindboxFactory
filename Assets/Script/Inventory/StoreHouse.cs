using Script.Controller;
using Script.Machine;
using Script.Resources;
using UnityEngine;

public class StoreHouse : MachineBase
{
    public long boxamount;
    public long resorceamount;

    BoxController _boxController => GameController.Instance.BoxController;
    ResourceController _resoruceController => GameController.Instance.ResourceController;

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
        _boxController.TrySetWarehouseMaxAmount(maxAmount - boxamount);
    }
}
