using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CraftingMaterialSprite
{
    public CraftingMaterial material;
    public Sprite sprite;
}

[Serializable]
public class BlindBoxWithNumber : BlindBox
{
    public int number;
}

public class BlindBoxMachine : QueueMachine<BlindBoxWithNumber>
{
    [SerializeField] public List<Recipe> recipes;
    [SerializeField] public int SlotNumber; 
    [SerializeField] public int ProduceSpeed;
    [SerializeField] public int level;

}
