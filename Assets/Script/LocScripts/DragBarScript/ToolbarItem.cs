using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public class ToolbarItem
{
    public string itemName;
    public Sprite itemIcon;
    public GameObject itemPrefab;
    public int cost;
    public bool isUnlocked = true;
}