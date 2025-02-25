using Script.Machine;
using System;
using UnityEngine;

[Serializable]
public class BlindBox : IProduct
{
    public float MaxProgress => throw new System.NotImplementedException();

    public void OnProductCreated()
    {
        throw new System.NotImplementedException();
    }
    public string boxName;
    public Sprite boxSprite;
    public int value;
}
