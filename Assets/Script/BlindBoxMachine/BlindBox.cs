using Script.Controller;
using Script.Machine;
using Script.Machine.Products;
using Script.Machine.ResourceManager;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BlindBox : SingleProductBase
{

    public BoxTypeName boxTypeName;


    public override void OnProductCreated()
    {
        Debug.Log("created");
        var boxcontroller = GameController.Instance.BoxController;
        if (boxcontroller.TryGetAmount(boxTypeName, out long amount))
        {
            boxcontroller.TrySetAmount(boxTypeName, amount + 1);
        }
        else
        {
            boxcontroller.TrySetAmount(boxTypeName, 1);
        }
    }


}
