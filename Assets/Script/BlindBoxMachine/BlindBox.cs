using Script.Machine;
using Script.Machine.Products;
using System;
using UnityEngine;

[Serializable]
public class BlindBox : ProductBase
{
    public override float MaxProgress => 10f;

    public BoxTypeName boxTypeName;

    public override void OnProductCreated()
    {
        throw new NotImplementedException();
    }


}
