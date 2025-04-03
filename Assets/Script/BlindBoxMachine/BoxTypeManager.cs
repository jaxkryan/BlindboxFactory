using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct BoxData
{
    public BoxTypeName boxType;  
    public int value;            
    public Sprite sprite;
    public string description;
}

public class BoxTypeManager : MonoBehaviour
{
    public static BoxTypeManager Instance;

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