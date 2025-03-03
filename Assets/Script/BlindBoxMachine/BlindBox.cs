using Script.Machine;
using Script.Machine.Products;
using Script.Machine.ResourceManager;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BlindBox : ProductBase
{
    public override float MaxProgress => 10f;

    public override List<ResourceUse> ResourceUse => throw new NotImplementedException();

    public BoxTypeName boxTypeName;

    public override void OnProductCreated()
    {
        throw new NotImplementedException();
    }


}
