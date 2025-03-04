using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Recipe
{
    public List<MaterialAmount> materials;
    public BlindBox result; 
}

[Serializable]
public class MaterialAmount
{
    public CraftingMaterial material; 
    public int amount;
}
