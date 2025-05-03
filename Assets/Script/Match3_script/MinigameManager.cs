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
    private Vector3 originalCameraPosition; // Store original camera position
    private readonly Vector3 minigamePlayPosition = new Vector3(10000f, 10000f); // Position to move minigame and camera
    private CameraController cameraController; // Reference to CameraController script

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
            originalCameraPosition = mainCamera.transform.position; // Store original camera position

            // Get the CameraController component
            cameraController = mainCamera.GetComponent<CameraController>();
            if (cameraController == null)
            {
                Debug.LogWarning("CameraController script not found on Main Camera!");
            }
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

        // Restore original ortho size and position on exit
        if (mainCamera != null)
        {
            mainCamera.orthographicSize = originalOrthoSize;
            mainCamera.transform.position = originalCameraPosition;
        }

        // Ensure CameraController is enabled on destroy
        if (cameraController != null)
        {
            cameraController.enabled = true;
        }
    }

    void Update()
    {
        CheckDailyReset();
        SpawnMinigames();
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

    private void CheckExistingMachinesForMinigames()
    {
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
                }
            }
        }
    }

    private void SpawnMinigames()
    {
        if (remainingMinigamesToday <= 0) return;

        DateTime currentTime = DateTime.UtcNow;
        float elapsedTime = (float)(currentTime - lastMinigameTime).TotalSeconds;

        if (elapsedTime >= minigameInterval)
        {
            int availableMachinesCount = GetAvailableMachines().Count;
            int minigamesToSpawn = Mathf.Min(availableMachinesCount, remainingMinigamesToday);

            for (int i = 0; i < minigamesToSpawn; i++)
            {
                SpawnRandomMinigame();
            }

            if (minigamesToSpawn > 0)
            {
                lastMinigameTime = currentTime;
                remainingMinigamesToday -= minigamesToSpawn;
            }
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
        if (remainingMinigamesToday > 0 && IsMinigameEligibleMachine(machine))
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
        exclamationButton.interactable = true; // Enable interaction
        Graphic graphic = exclamationButton.GetComponent<Graphic>();
        if (graphic != null) graphic.raycastTarget = true;

        // Add click listener to the exclamation button
        exclamationButton.onClick.RemoveAllListeners();
        exclamationButton.onClick.AddListener(() => StartMinigame(selectedMachine, minigameObject));

        activeMinigames[selectedMachine] = new MinigameData
        {
            MinigameObject = minigameObject,
            ExclamationButton = exclamationButton,
            TimeRemaining = maxInactiveTime
        };

        string minigameType = GetMinigameType(minigameObject);
        Debug.Log($"Minigame event spawned at machine: {selectedMachine.name} (Type: {selectedMachine.GetType().Name}, Minigame: {minigameType})");
    }

    private void StartMinigame(MachineBase machine, GameObject minigameObject)
    {
        if (activeMinigames.ContainsKey(machine))
        {
            Debug.Log($"Activating minigame GameObject: {minigameObject.name} for machine: {machine.name}");
            activeMinigames[machine].ExclamationButton.gameObject.SetActive(false);
            activeMinigames[machine].ExclamationButton.onClick.RemoveAllListeners(); // Clean up listeners
            activeMinigames.Remove(machine);
            minigameStartTime = DateTime.UtcNow;

            // Adjust camera orthographic size
            if (mainCamera != null)
            {
                // Disable CameraController to allow free movement
                if (cameraController != null)
                {
                    cameraController.enabled = false;
                    Debug.Log("Disabled CameraController to allow camera movement");
                }

                float orthoSizeFactor = CalculateOrthoSizeFactor();
                mainCamera.orthographicSize = referenceOrthoSize * orthoSizeFactor;
                Debug.Log($"Applied ortho size factor: {orthoSizeFactor}, New ortho size: {mainCamera.orthographicSize}");

                // Move camera to minigame play position, preserving z
                Vector3 cameraPosition = minigamePlayPosition;
                cameraPosition.z = mainCamera.transform.position.z;
                mainCamera.transform.position = cameraPosition;
                Debug.Log($"Moved camera to: {cameraPosition}");
            }

            // Deactivate all other minigame GameObjects
            DeactivateAllMinigameObjects();

            // Move minigame to play position *before* activation
            Vector3 minigamePosition = minigamePlayPosition;
            minigamePosition.z = -5f; // Explicitly set z to ensure visibility
            minigameObject.transform.position = minigamePosition;
            Debug.Log($"Moved minigame {minigameObject.name} to: {minigamePosition}");

            // Activate the minigame (this triggers Grid.OnEnable)
            minigameObject.SetActive(true);

            // Confirm the position after setting
            Debug.Log($"Final position of minigame {minigameObject.name} after activation: {minigameObject.transform.position}");
        }
        else
        {
            Debug.LogWarning($"Machine {machine.name} not found in activeMinigames!");
        }
    }

    public void EndMinigame()
    {
        Debug.Log("Ending minigame via MinigameManager.EndMinigame");
        DeactivateAllMinigameObjects();

        // Explicitly move camera to (0, 0), preserving z
        if (mainCamera != null)
        {
            Vector3 cameraResetPosition = Vector3.zero;
            cameraResetPosition.z = mainCamera.transform.position.z;
            mainCamera.transform.position = cameraResetPosition;
            Debug.Log($"Camera moved to: {cameraResetPosition}");

            // Re-enable CameraController after moving camera back
            if (cameraController != null)
            {
                cameraController.enabled = true;
                Debug.Log("Re-enabled CameraController after ending minigame");
            }
        }

        // After ending a minigame, check if more can be spawned
        SpawnMinigames();
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

        // Restore original ortho size (but not position, handled in EndMinigame)
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

        // Check if more minigames can be spawned after removing timed-out ones
        if (toRemove.Count > 0)
        {
            SpawnMinigames();
        }
    }

    private void CancelMinigame(MachineBase machine, MinigameData data)
    {
        data.ExclamationButton.gameObject.SetActive(false);
        data.ExclamationButton.onClick.RemoveAllListeners(); // Clean up listeners
        Debug.Log($"Minigame cancelled at machine: {machine.name} due to timeout.");
    }
}