using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    [System.Serializable]
    public class Resource
    {
        public ColorPiece.ColorType color;
        public int amount;
    }

    public List<Resource> resources;
    private Dictionary<ColorPiece.ColorType, int> resourceDict;

    private static ResourceManager instance;
    public static ResourceManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<ResourceManager>();
                if (instance == null)
                {
                    GameObject obj = new GameObject();
                    obj.name = typeof(ResourceManager).Name;
                    instance = obj.AddComponent<ResourceManager>();
                }
            }
            return instance;
        }
    }

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        InitializeResources();
    }

    private void InitializeResources()
    {
        resourceDict = new Dictionary<ColorPiece.ColorType, int>();

        foreach (Resource resource in resources)
        {
            resourceDict[resource.color] = resource.amount;
        }
    }

    public void AddResource(ColorPiece.ColorType color, int amount)
    {
        if (resourceDict.ContainsKey(color))
        {
            resourceDict[color] += amount;
            Debug.Log($"Added {amount} to {color}. New total: {resourceDict[color]}");
        }
        else
        {
            Debug.LogWarning($"Attempted to add resource for non-existent color: {color}");
        }
    }

   

    public int GetResourceAmount(ColorPiece.ColorType color)
    {
        if (resourceDict.ContainsKey(color))
        {
            return resourceDict[color];
        }
        else
        {
            return 0;
        }
    }

    public void ResetResources()
    {
        List<ColorPiece.ColorType> keys = new List<ColorPiece.ColorType>(resourceDict.Keys);
        Debug.Log($"Resetting resources: {string.Join(", ", keys)}");
        foreach (var key in keys)
        {
            resourceDict[key] = 0;
            Debug.Log($"Reset {key} resource to 0");
        }
    }
}