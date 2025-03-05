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
        throw new NotImplementedException();
    }


}
