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
    public static BoxTypeManager Instance { get; private set; }

    public List<BoxData> boxDataList = new List<BoxData>();
    private Dictionary<BoxTypeName, BoxData> boxTypeDictionary;

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