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

    // Initialize the list to prevent NullReferenceException
    public List<Resource> resources = new List<Resource>();
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

        // Initialize all ColorType values except ANY and COUNT with 0
        foreach (ColorPiece.ColorType color in System.Enum.GetValues(typeof(ColorPiece.ColorType)))
        {
            if (color == ColorPiece.ColorType.ANY || color == ColorPiece.ColorType.COUNT)
                continue;
            resourceDict[color] = 0;
        }

        // Override with values from the resources list (if any)
        foreach (Resource resource in resources)
        {
            if (resourceDict.ContainsKey(resource.color))
            {
                resourceDict[resource.color] = resource.amount;
            }
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