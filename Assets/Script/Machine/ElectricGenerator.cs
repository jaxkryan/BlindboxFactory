using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

[System.Serializable]
public class GeneratorData
{
    public string Name;      // The original prefab name
    public Vector3 Position; // Position of the generator instance
    public int Level;
    public float MaxLoad;
}


[System.Serializable]
public class GeneratorsData
{
    public List<GeneratorData> Generators = new List<GeneratorData>();
    public float TotalEnergyUsed = 0f; // Shared among all generators
    public float TotalMaxLoad = 0f;    // Sum of all generators' MaxLoad
}

public class ElectricGenerator : MonoBehaviour
{
    public int Level { get; private set; } = 1;
    public float MaxLoad { get; private set; } = 100f;

    // Remove serialized UI references since they are now in the centralized manager.
    // [SerializeField] private Slider loadSliderReference;
    // [SerializeField] private TMP_Text energyText;

    private static Slider LoadSlider;
    private static Tween idleTween;
    private static string saveFilePath;
    private static GeneratorsData allData; // Global system data

    private void Awake()
    {
        // Instead of using a local slider reference, get it from the UI manager.
        if (LoadSlider == null && ElectricGeneratorUIManager.Instance != null)
        {
            LoadSlider = ElectricGeneratorUIManager.Instance.EnergySlider;
        }

        saveFilePath = Path.Combine(Application.persistentDataPath, "ElectricGenerators.json");

        LoadAllData();
        LoadData();
        UpdatePowerDistribution();
    }

    private void OnDestroy()
    {
        idleTween?.Kill();
        RemoveGenerator();
        SaveAllData();
    }

    public void Upgrade()
    {
        Level++;
        MaxLoad *= 1.1f;
        SaveData();
        UpdatePowerDistribution();
    }

    public static void ConsumeEnergy(float amount)
    {
        allData.TotalEnergyUsed += amount;
        SaveAllData();
        UpdatePowerDistribution();
    }

    public static void ReleaseEnergy(float amount)
    {
        allData.TotalEnergyUsed = Mathf.Max(0, allData.TotalEnergyUsed - amount);
        SaveAllData();
        UpdatePowerDistribution();
    }

    public static void UpdatePowerDistribution()
    {
        float consumedEnergy = allData.TotalEnergyUsed;

        if (LoadSlider != null)
        {
            float targetValue = allData.TotalMaxLoad > 0 ? consumedEnergy / allData.TotalMaxLoad : 0;
            idleTween?.Kill();
            idleTween = LoadSlider.DOValue(targetValue, 1.2f).SetEase(Ease.OutSine);
        }

        // Access the UI text from the centralized manager.
        if (ElectricGeneratorUIManager.Instance != null && ElectricGeneratorUIManager.Instance.EnergyText != null)
        {
            ElectricGeneratorUIManager.Instance.EnergyText.text = $"{consumedEnergy}/{allData.TotalMaxLoad}";
        }
    }

    private void SaveData()
    {
        // Try to find data matching both the name and position.
        GeneratorData generatorData = allData.Generators
            .Find(g => g.Name == gameObject.name &&
                       Vector3.Distance(g.Position, transform.position) < 0.1f);
        if (generatorData == null)
        {
            generatorData = new GeneratorData { Name = gameObject.name, Position = transform.position };
            allData.Generators.Add(generatorData);
        }

        generatorData.Level = Level;
        generatorData.MaxLoad = MaxLoad;

        UpdateTotalMaxLoad();
        SaveAllData();
    }


    private void RemoveGenerator()
    {
        allData.Generators.RemoveAll(g => g.Name == gameObject.name);
        UpdateTotalMaxLoad();
        SaveAllData();
    }

    private void LoadData()
    {
        GeneratorData generatorData = allData.Generators
            .Find(g => g.Name == gameObject.name &&
                       Vector3.Distance(g.Position, transform.position) < 0.1f);
        if (generatorData != null)
        {
            Level = generatorData.Level;
            MaxLoad = generatorData.MaxLoad;
        }
        else
        {
            SaveData(); // Create new data if not found
        }
    }


    private static void LoadAllData()
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            allData = JsonUtility.FromJson<GeneratorsData>(json);
        }
        else
        {
            allData = new GeneratorsData();
        }

        UpdateTotalMaxLoad();
    }

    private static void SaveAllData()
    {
        string json = JsonUtility.ToJson(allData, true);
        File.WriteAllText(saveFilePath, json);
    }

    private static void UpdateTotalMaxLoad()
    {
        allData.TotalMaxLoad = allData.Generators.Sum(g => g.MaxLoad);
    }

    public static bool HasEnoughEnergy(float required)
    {
        return allData.TotalEnergyUsed + required <= allData.TotalMaxLoad;
    }
}
