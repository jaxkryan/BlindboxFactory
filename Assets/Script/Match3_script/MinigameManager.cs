using Script.Controller;
using Script.Machine;
using Script.Machine.Machines.Canteen;
using Script.Machine.Machines.Generator;
using Script.Machine.Machines;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class MinigameManager : MonoBehaviour
{
    private MachineController machineController;
    [SerializeField] private string match3SceneName = "Match3Scene";
    [SerializeField] private string whackAMoleSceneName = "WhackAMoleScene";
    [SerializeField] private string wireConnectionSceneName = "WireConnectionScene";
    [SerializeField] private string mainSceneName = "MainScreen";
    [SerializeField] private string exclamationButtonName = "needfix";
    [SerializeField] private float minigameInterval = 3f * 3600f; // 3 giờ
    [SerializeField] private float maxInactiveTime = 20f * 60f; // 20 phút
    [SerializeField] private bool testMode = true;
    [SerializeField] private float minigameSpawnChance = 0.3f; // Xác suất 30% spawn minigame khi máy được đặt

    private const int MAX_MINIGAMES_PER_DAY = 5;
    private int remainingMinigamesToday = MAX_MINIGAMES_PER_DAY;
    private DateTime lastMinigameTime;
    private DateTime lastResetTime;
    private DateTime? minigameStartTime;
    private Dictionary<MachineBase, MinigameData> activeMinigames = new();

    private class MinigameData
    {
        public string SceneName;
        public Button ExclamationButton;
        public float TimeRemaining;
    }

    void Start()
    {
        lastMinigameTime = DateTime.UtcNow;
        lastResetTime = DateTime.UtcNow.Date;
        machineController = GameController.Instance.MachineController;

        // Đăng ký sự kiện khi scene được load
        SceneManager.sceneLoaded += OnSceneLoaded;

        // Đăng ký sự kiện khi máy được thêm
        machineController.onMachineAdded += OnMachineAdded;

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

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (machineController != null)
        {
            machineController.onMachineAdded -= OnMachineAdded;
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
        SpawnMinigameForMachine(selectedMachine);
    }

    private void OnMachineAdded(MachineBase machine)
    {
        if (!testMode && remainingMinigamesToday > 0 && IsMinigameEligibleMachine(machine))
        {
            float roll = Random.value;
            if (roll <= minigameSpawnChance)
            {
                SpawnMinigameForMachine(machine);
                remainingMinigamesToday--;
                lastMinigameTime = DateTime.UtcNow;
                Debug.Log($"Minigame spawned due to machine placement: {machine.name}");
            }
        }
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
        return machine is Generator || machine is StorageMachine || machine is Canteen;
    }

    private string GetMinigameSceneName(MachineBase machine)
    {
        if (machine is Generator) return match3SceneName;
        if (machine is StorageMachine) return whackAMoleSceneName;
        if (machine is Canteen) return wireConnectionSceneName;
        return null;
    }

    private void SpawnMinigameForMachine(MachineBase selectedMachine)
    {
        string sceneName = GetMinigameSceneName(selectedMachine);
        if (string.IsNullOrEmpty(sceneName)) return;

        Button exclamationButton = selectedMachine.GetComponentInChildren<Button>(true);
        if (exclamationButton == null || exclamationButton.name != exclamationButtonName)
        {
            Debug.LogWarning($"No button named '{exclamationButtonName}' found in {selectedMachine.name}");
            return;
        }

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
            minigameStartTime = DateTime.UtcNow;
            Debug.Log($"Minigame started at machine: {machine.name}, loading scene: {sceneName}");
            SceneManager.LoadScene(sceneName);
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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == mainSceneName && minigameStartTime.HasValue)
        {
            DateTime currentTime = DateTime.UtcNow;
            float timeInMinigame = (float)(currentTime - minigameStartTime.Value).TotalSeconds;
            lastMinigameTime = lastMinigameTime.AddSeconds(timeInMinigame);
            minigameStartTime = null;

            Debug.Log($"Returned to main scene. Time spent in minigame: {timeInMinigame}s. Updated lastMinigameTime: {lastMinigameTime}");

            if (!testMode)
            {
                float elapsedTime = (float)(currentTime - lastMinigameTime).TotalSeconds;
                if (elapsedTime >= minigameInterval && remainingMinigamesToday > 0)
                {
                    SpawnMinigames();
                }
            }
        }
    }
}