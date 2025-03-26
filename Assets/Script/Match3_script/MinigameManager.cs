using Script.Controller;
using Script.Machine;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; // Thêm namespace để dùng SceneManager
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class MinigameManager : MonoBehaviour
{
    private MachineController machineController;
    [SerializeField] private string match3SceneName = "Match3Scene"; // Tên scene cho Match-3
    [SerializeField] private string whackAMoleSceneName = "WhackAMoleScene"; // Tên scene cho Whack-a-Mole
    [SerializeField] private string wireConnectionSceneName = "WireConnectionScene"; // Tên scene cho Wire-Connection
    [SerializeField] private string exclamationButtonName = "needfix";
    [SerializeField] private float minigameInterval = 3f * 3600f;
    [SerializeField] private float maxInactiveTime = 20f * 60f;
    [SerializeField] private bool testMode = true;

    private const int MAX_MINIGAMES_PER_DAY = 5;
    private int remainingMinigamesToday = MAX_MINIGAMES_PER_DAY;
    private DateTime lastMinigameTime;
    private DateTime lastResetTime;
    private Dictionary<MachineBase, MinigameData> activeMinigames = new();

    private class MinigameData
    {
        public string SceneName; // Thay vì GameObject, lưu tên scene
        public Button ExclamationButton;
        public float TimeRemaining;
    }

    void Start()
    {
        lastMinigameTime = DateTime.UtcNow;
        lastResetTime = DateTime.UtcNow.Date;
        machineController = GameController.Instance.MachineController;

        if (testMode)
        {
            int minigamesToSpawn = Random.Range(1, 3);
            minigamesToSpawn = Mathf.Min(minigamesToSpawn, remainingMinigamesToday);

            for (int i = 0; i < minigamesToSpawn; i++)
            {
                SpawnRandomMinigame();
            }
            remainingMinigamesToday -= minigamesToSpawn;
        }
    }

    void Update()
    {
        CheckDailyReset();
        if (!testMode) SpawnMinigames();
        UpdateActiveMinigames();
    }

    private void CheckDailyReset()
    {
        DateTime currentTime = DateTime.UtcNow;
        if (currentTime.Date > lastResetTime)
        {
            remainingMinigamesToday = MAX_MINIGAMES_PER_DAY;
            lastResetTime = currentTime.Date;
        }
    }

    private void SpawnMinigames()
    {
        if (remainingMinigamesToday <= 0) return;

        DateTime currentTime = DateTime.UtcNow;
        float elapsedTime = (float)(currentTime - lastMinigameTime).TotalSeconds;

        if (elapsedTime >= minigameInterval)
        {
            int minigamesToSpawn = Random.Range(1, 3);
            minigamesToSpawn = Mathf.Min(minigamesToSpawn, remainingMinigamesToday);

            for (int i = 0; i < minigamesToSpawn; i++)
            {
                SpawnRandomMinigame();
            }

            lastMinigameTime = currentTime;
            remainingMinigamesToday -= minigamesToSpawn;
        }
    }

    private void SpawnRandomMinigame()
    {
        var availableMachines = GetAvailableMachines();
        if (availableMachines.Count == 0)
        {
            Debug.Log("No available machines for minigame.");
            return;
        }

        MachineBase selectedMachine = availableMachines[Random.Range(0, availableMachines.Count)];
        string sceneName = GetMinigameSceneName(selectedMachine);
        if (string.IsNullOrEmpty(sceneName)) return;

        Button exclamationButton = selectedMachine.GetComponentInChildren<Button>(true);
        if (exclamationButton == null || exclamationButton.name != exclamationButtonName)
        {
            Debug.LogWarning($"No button named '{exclamationButtonName}' found in {selectedMachine.name}");
            return;
        }

        // Đảm bảo button có thể tương tác
        exclamationButton.gameObject.SetActive(true);
        exclamationButton.interactable = true;
        Graphic graphic = exclamationButton.GetComponent<Graphic>();
        if (graphic != null) graphic.raycastTarget = true;

        exclamationButton.onClick.RemoveAllListeners();
        exclamationButton.onClick.AddListener(() => StartMinigame(selectedMachine, sceneName));

        activeMinigames[selectedMachine] = new MinigameData
        {
            SceneName = sceneName,
            ExclamationButton = exclamationButton,
            TimeRemaining = maxInactiveTime
        };

        string minigameType = GetMinigameType(sceneName);
        Debug.Log($"Minigame event spawned at machine: {selectedMachine.name} (Type: {selectedMachine.GetType().Name}, Minigame: {minigameType})");
    }

    private List<MachineBase> GetAvailableMachines()
    {
        List<MachineBase> available = new();
        foreach (var machine in machineController.Machines)
        {
            if (!activeMinigames.ContainsKey(machine) && IsMinigameEligibleMachine(machine))
            {
                available.Add(machine);
            }
        }
        return available;
    }

    private bool IsMinigameEligibleMachine(MachineBase machine)
    {
        return machine.GetType().Name == "Generator" ||
               machine.GetType().Name == "StorageMachine" ||
               machine.GetType().Name == "Canteen";
    }

    private string GetMinigameSceneName(MachineBase machine)
    {
        switch (machine.GetType().Name)
        {
            case "Generator": return match3SceneName;
            case "StorageMachine": return whackAMoleSceneName;
            case "Canteen": return wireConnectionSceneName;
            default: return null;
        }
    }

    private string GetMinigameType(string sceneName)
    {
        if (sceneName == match3SceneName) return "Match-3";
        if (sceneName == whackAMoleSceneName) return "Whack-a-Mole";
        if (sceneName == wireConnectionSceneName) return "Wire-Connection";
        return "Unknown";
    }

    private void StartMinigame(MachineBase machine, string sceneName)
    {
        if (activeMinigames.ContainsKey(machine))
        {
            activeMinigames[machine].ExclamationButton.gameObject.SetActive(false);
            activeMinigames.Remove(machine);
            Debug.Log($"Minigame started at machine: {machine.name}, loading scene: {sceneName}");
            SceneManager.LoadScene(sceneName); // Chuyển sang scene minigame
        }
    }

    private void UpdateActiveMinigames()
    {
        List<MachineBase> toRemove = new();
        foreach (var kvp in activeMinigames)
        {
            MinigameData data = kvp.Value;
            data.TimeRemaining -= Time.deltaTime;

            if (data.TimeRemaining <= 0)
            {
                CancelMinigame(kvp.Key, data);
                toRemove.Add(kvp.Key);
            }
        }

        foreach (var key in toRemove)
        {
            activeMinigames.Remove(key);
        }
    }

    private void CancelMinigame(MachineBase machine, MinigameData data)
    {
        data.ExclamationButton.gameObject.SetActive(false);
        Debug.Log($"Minigame cancelled at machine: {machine.name} due to timeout.");
    }
}