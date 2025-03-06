using Script.Resources;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct CMData
{
    public Resource cmType;
    public int value;
    public Sprite sprite;
}

public class CraftingMaterialTypeManager : MonoBehaviour
{
    public static CraftingMaterialTypeManager Instance { get; private set; }

    public List<CMData> boxDataList = new List<CMData>();
    private Dictionary<Resource, CMData> boxTypeDictionary;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        boxTypeDictionary = new Dictionary<Resource, CMData>();
        foreach (var data in boxDataList)
        {
            boxTypeDictionary[data.cmType] = data;
        }
    }

    public CMData GetCraftingMaterialData(Resource cmType)
    {
        if (boxTypeDictionary.TryGetValue(cmType, out CMData data))
        {
            return data;
        }
        else
        {
            Debug.LogError($"CraftingMaterial {cmType} not found!");
            return default;
        }
    }
}
