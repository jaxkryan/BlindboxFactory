using Script.Controller;
using Script.Machine;
using Script.Resources;
using Script.Utils;
using UnityEngine;

public class StoreHouse : MachineBase
{
    public long boxamount;
    public long resorceamount;

    BoxController _boxController => GameController.Instance.BoxController;
    ResourceController _resoruceController => GameController.Instance.ResourceController;

    protected override void Start() 
    {
        base.Start();
        
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

    public override MachineBaseData Save() {
        var data = base.Save().CastToSubclass<StoreHouseData, MachineBaseData>();
        if (data is null) return base.Save();

        data.ResourceAmount = resorceamount;
        data.BoxAmount = boxamount;
        return data;
    }

    public override void Load(MachineBaseData data) {
        base.Load(data);
        if (data is not StoreHouseData saveData) return;
        
        resorceamount = saveData.ResourceAmount;
        boxamount = saveData.BoxAmount;
    }


    public class StoreHouseData : MachineBase.MachineBaseData {
        public long BoxAmount;
        public long ResourceAmount;
    }
}
