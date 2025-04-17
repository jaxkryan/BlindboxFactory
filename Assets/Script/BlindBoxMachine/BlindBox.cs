using Script.Controller;
using Script.Machine;
using Script.Machine.Products;
using Script.Machine.ResourceManager;
using System;
using System.Collections.Generic;
using Script.Utils;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class BlindBox : SingleProductBase
{

    [FormerlySerializedAs("boxTypeName")] public BoxTypeName BoxTypeName;


    public override void OnProductCreated()
    {
        var boxcontroller = GameController.Instance.BoxController;
        if (boxcontroller.TryGetAmount(BoxTypeName, out long amount))
        {
            boxcontroller.TrySetAmount(BoxTypeName, amount + 1);
        }
        else
        {
            boxcontroller.TrySetAmount(BoxTypeName, 1);
        }
    }

    public override IProduct.SaveData Save()
    {
        var data = base.Save().CastToSubclass<BlindBoxSaveData, IProduct.SaveData>();
        if (data is null) return base.Save();

        data.BoxTypeName = BoxTypeName;

        return data;
    }

    public override void Load(IProduct.SaveData saveData)
    {
        BaseLoad(saveData);
        if (saveData is not BlindBoxSaveData data) return;

        BoxTypeName = data.BoxTypeName;
    }

    public class BlindBoxSaveData : IProduct.SaveData
    {
        public BoxTypeName BoxTypeName;
    }
    //note
}
