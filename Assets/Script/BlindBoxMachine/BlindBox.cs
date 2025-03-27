using Script.Controller;
using Script.Machine;
using Script.Machine.Products;
using Script.Machine.ResourceManager;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class BlindBox : SingleProductBase
{

    [FormerlySerializedAs("boxTypeName")] public BoxTypeName BoxTypeName;


    public override void OnProductCreated()
    {
        Debug.Log("created");
        var boxcontroller = GameController.Instance.BoxController;
        if (boxcontroller.TryGetAmount(BoxTypeName, out long amount))
        {
            Debug.Log(amount);
            Debug.Log(boxcontroller.TrySetAmount(BoxTypeName, amount + 1));
        }
        else
        {
            boxcontroller.TrySetAmount(BoxTypeName, 1);
        }
    }

    public override IProduct.SaveData Save() {
        if (base.Save() is not BlindBoxSaveData data) return base.Save();
        
        data.BoxTypeName = BoxTypeName;

        return data;
    }

    public override void Load(IProduct.SaveData saveData) {
        BaseLoad(saveData);
        if (saveData is not BlindBoxSaveData data) return;
        
        BoxTypeName = data.BoxTypeName;
    }

    public class BlindBoxSaveData : IProduct.SaveData {
        public BoxTypeName BoxTypeName;
    }
}
