using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct BoxData
{
    public BoxTypeName boxType;  // Enum for the type
    public int value;            // Value of the box
    public Sprite sprite;        // Sprite of the box
}

public class BoxTypeManager : MonoBehaviour
{
    public List<BoxData> boxDataList = new List<BoxData>();  // Assign in Inspector
    private Dictionary<BoxTypeName, BoxData> boxTypeDictionary;

    void Awake()
    {
        // Convert list to dictionary at runtime
        boxTypeDictionary = new Dictionary<BoxTypeName, BoxData>();
        foreach (var data in boxDataList)
        {
            boxTypeDictionary[data.boxType] = data;
        }
    }

    public BoxData GetBoxData(BoxTypeName boxType)
    {
        if (boxTypeDictionary.TryGetValue(boxType, out BoxData data))
        {
            return data;
        }
        else
        {
            Debug.LogError($"BoxType {boxType} not found!");
            return default;
        }
    }
}