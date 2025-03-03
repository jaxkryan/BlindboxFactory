using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CraftingMaterialSprite
{
    public CraftingMaterial material;
    public Sprite sprite;
}

public class BlindBoxMachine : QueueMachine
{
    [SerializeField]public List<Recipe> recipes;
    [SerializeField] public int SlotNumber; 
    [SerializeField] public int ProduceSpeed;
    [SerializeField] public string Description;
    [SerializeField] public int level;
}
