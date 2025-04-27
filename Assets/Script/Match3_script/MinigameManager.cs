using Script.Controller;
using Script.Machine;
using Script.Machine.Machines.Canteen;
using Script.Machine.Machines.Generator;
using Script.Machine.Machines;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class MinigameManager : MonoBehaviour
{
    private MachineController machineController;
    [SerializeField] private string exclamationButtonName = "needfix";
    [SerializeField] private float minigameInterval = 3f * 3600f; // 3 hours
    [SerializeField] private float maxInactiveTime = 20f * 60f; // 20 minutes
    [SerializeField] private float minigameSpawnChance = 0.3f; // 30% chance
    [SerializeField] private Button minigameTriggerButton; // Single minigame trigger button
    [SerializeField] private GameObject match3GameObject;
    [SerializeField] private GameObject whackAMoleGameObject;
    [SerializeField] private GameObject wireConnectionGameObject;
    [SerializeField] private Camera mainCamera; // Main camera to adjust
    [SerializeField] private float referenceOrthoSize = 5f; // Reference orthographic size
    [SerializeField] private Vector2 referenceResolution = new Vector2(1080, 1920);
    [SerializeField] private float minOrthoSizeFactor = 0.8f;
    [SerializeField] private float maxOrthoSizeFactor = 1.2f;

    private const int MAX_MINIGAMES_PER_DAY = 5;
    private int remainingMinigamesToday = MAX_MINIGAMES_PER_DAY;
    private DateTime lastMinigameTime;
    private DateTime lastResetTime;
    private DateTime? minigameStartTime;
    private Dictionary<MachineBase, MinigameData> activeMinigames = new();
    private float originalOrthoSize; // Store original ortho size

    private class MinigameData
    {
        public GameObject MinigameObject;
        public Button ExclamationButton;
        public float TimeRemaining;
    }

    void Start()
    {
        lastMinigameTime = DateTime.UtcNow;
        lastResetTime = DateTime.UtcNow.Date;
        machineController = GameController.Instance.MachineController;

        // Register events
        machineController.onMachineAdded += OnMachineAdded;

        // Set up MinigameTriggerButton
        if (minigameTriggerButton != null)
        {
            minigameTriggerButton.gameObject.SetActive(false);
            minigameTriggerButton.onClick.RemoveAllListeners();
            minigameTriggerButton.onClick.AddListener(OnMinigameTriggerButtonClicked);
        }
        else
        {
            Debug.LogWarning("MinigameTriggerButton not assigned in Inspector!");
        }

        // Set up camera
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("No Main Camera found in the scene!");
            }
        }

        if (mainCamera != null)
        {
            originalOrthoSize = mainCamera.orthographicSize;
        }

        // Deactivate all minigame GameObjects initially
        DeactivateAllMinigameObjects();

        // Check existing machines for minigames
        CheckExistingMachinesForMinigames();
    }

    void OnDestroy()
    {
        if (machineController != null)
        {
            machineController.onMachineAdded -= OnMachineAdded;
        }

        // Restore original ortho size on exit
        if (mainCamera != null)
        {
            mainCamera.orthographicSize = originalOrthoSize;
        }
    }

    void Update()
    {
        CheckDailyReset();
        SpawnMinigames();
        UpdateActiveMinigames();

        if (minigameTriggerButton != null)
        {
            bool hasActiveMinigames = activeMinigames.Count > 0;
            minigameTriggerButton.gameObject.SetActive(hasActiveMinigames);
            minigameTriggerButton.interactable = hasActiveMinigames;
        }
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

    private void CheckExistingMachinesForMinigames()
    {
        if (activeMinigames.Count > 0) return;

        foreach (var machine in machineController.Machines)
        {
            if (remainingMinigamesToday <= 0) break;
            if (IsMinigameEligibleMachine(machine) && !activeMinigames.ContainsKey(machine))
            {
                float roll = Random.value;
                if (roll <= minigameSpawnChance)
                {
                    SpawnMinigameForMachine(machine);
                    remainingMinigamesToday--;
                    lastMinigameTime = DateTime.UtcNow;
                    Debug.Log($"Minigame spawned at startup for machine: {machine.name}");
                    break;
                }
            }
        }
    }

    private void SpawnMinigames()
    {
        if (remainingMinigamesToday <= 0) return;
        if (activeMinigames.Count > 0) return;

        DateTime currentTime = DateTime.UtcNow;
        float elapsedTime = (float)(currentTime - lastMinigameTime).TotalSeconds;

        if (elapsedTime >= minigameInterval)
        {
            int minigamesToSpawn = 1;
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
        if (remainingMinigamesToday > 0 && IsMinigameEligibleMachine(machine) && activeMinigames.Count == 0)
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

    private void SpawnMinigameForMachine(MachineBase selectedMachine)
    {
        GameObject minigameObject = GetMinigameObject(selectedMachine);
        if (minigameObject == null)
        {
            Debug.LogWarning($"No minigame GameObject assigned for machine: {selectedMachine.name}");
            return;
        }

        Button exclamationButton = selectedMachine.GetComponentInChildren<Button>(true);
        if (exclamationButton == null || exclamationButton.name != exclamationButtonName)
        {
            Debug.LogWarning($"No button named '{exclamationButtonName}' found in {selectedMachine.name}");
            return;
        }

        exclamationButton.gameObject.SetActive(true);
        exclamationButton.interactable = false;
        Graphic graphic = exclamationButton.GetComponent<Graphic>();
        if (graphic != null) graphic.raycastTarget = false;

        activeMinigames[selectedMachine] = new MinigameData
        {
            MinigameObject = minigameObject,
            ExclamationButton = exclamationButton,
            TimeRemaining = maxInactiveTime
        };

        string minigameType = GetMinigameType(minigameObject);
    }

    private void OnMinigameTriggerButtonClicked()
    {
        if (activeMinigames.Count == 0)
        {
            Debug.LogWarning("No active minigames to trigger!");
            return;
        }

        var enumerator = activeMinigames.GetEnumerator();
        if (enumerator.MoveNext())
        {
            var pair = enumerator.Current;
            MachineBase machine = pair.Key;
            GameObject minigameObject = pair.Value.MinigameObject;
            StartMinigame(machine, minigameObject);
        }
    }

    private void StartMinigame(MachineBase machine, GameObject minigameObject)
    {
        if (activeMinigames.ContainsKey(machine))
        {
            Debug.Log($"Activating minigame GameObject: {minigameObject.name} for machine: {machine.name}");
            activeMinigames[machine].ExclamationButton.gameObject.SetActive(false);
            activeMinigames.Remove(machine);
            minigameStartTime = DateTime.UtcNow;

            // Adjust camera orthographic size
            if (mainCamera != null)
            {
                float orthoSizeFactor = CalculateOrthoSizeFactor();
                mainCamera.orthographicSize = referenceOrthoSize * orthoSizeFactor;
                Debug.Log($"Applied ortho size factor: {orthoSizeFactor}, New ortho size: {mainCamera.orthographicSize}");

                // Position minigame at the center of the viewport
                Vector3 viewportCenter = new Vector3(0.5f, 0.5f, 10f); // Center of viewport, z = 10 from camera
                Vector3 spawnPosition = mainCamera.ViewportToWorldPoint(viewportCenter);
                spawnPosition.z = -5f; // Ensure minigame is closer to camera
                minigameObject.transform.position = spawnPosition;
                Debug.Log($"Positioned minigame {minigameObject.name} at viewport center: {spawnPosition}");
            }

            // Deactivate all other minigame GameObjects and activate the selected one
            DeactivateAllMinigameObjects();
            minigameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning($"Machine {machine.name} not found in activeMinigames!");
        }
    }

    private float CalculateOrthoSizeFactor()
    {
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        float screenAspect = screenWidth / screenHeight;

        float referenceAspect = referenceResolution.x / referenceResolution.y;

        // Calculate factor based on height
        float orthoSizeFactor = (screenHeight / referenceResolution.y) * (screenAspect / referenceAspect);

        // Clamp factor
        orthoSizeFactor = Mathf.Clamp(orthoSizeFactor, minOrthoSizeFactor, maxOrthoSizeFactor);
        return orthoSizeFactor;
    }

    private void DeactivateAllMinigameObjects()
    {
        if (match3GameObject != null) match3GameObject.SetActive(false);
        if (whackAMoleGameObject != null) whackAMoleGameObject.SetActive(false);
        if (wireConnectionGameObject != null) wireConnectionGameObject.SetActive(false);

        // Restore original ortho size when deactivating minigames
        if (mainCamera != null)
        {
            mainCamera.orthographicSize = originalOrthoSize;
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
        return machine is ResourceExtractor || machine is StoreHouse || machine is Canteen;
    }

    private GameObject GetMinigameObject(MachineBase machine)
    {
        if (machine is ResourceExtractor) return match3GameObject;
        if (machine is StoreHouse) return whackAMoleGameObject;
        if (machine is Canteen) return wireConnectionGameObject;
        return null;
    }

    private string GetMinigameType(GameObject minigameObject)
    {
        if (minigameObject == match3GameObject) return "Match-3";
        if (minigameObject == whackAMoleGameObject) return "Whack-a-Mole";
        if (minigameObject == wireConnectionGameObject) return "Wire-Connection";
        return "Unknown";
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