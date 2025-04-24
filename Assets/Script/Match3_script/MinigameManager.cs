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
    [SerializeField] private float minigameInterval = 3f * 3600f; // 3 giờ
    [SerializeField] private float maxInactiveTime = 20f * 60f; // 20 phút
    [SerializeField] private float minigameSpawnChance = 0.3f; // Xác suất 30%
    [SerializeField] private Button minigameTriggerButton; // Nút kích hoạt minigame duy nhất
    [SerializeField] private GameObject match3GameObject;
    [SerializeField] private GameObject whackAMoleGameObject;
    [SerializeField] private GameObject wireConnectionGameObject;
    [SerializeField] private Camera mainCamera; // Camera chính để điều chỉnh
    [SerializeField] private float referenceOrthoSize = 5f; // Kích thước orthographic tham chiếu
    [SerializeField] private Vector2 referenceResolution = new Vector2(1080, 1920);
    [SerializeField] private float minOrthoSizeFactor = 0.8f;
    [SerializeField] private float maxOrthoSizeFactor = 1.2f;

    private const int MAX_MINIGAMES_PER_DAY = 5;
    private int remainingMinigamesToday = MAX_MINIGAMES_PER_DAY;
    private DateTime lastMinigameTime;
    private DateTime lastResetTime;
    private DateTime? minigameStartTime;
    private Dictionary<MachineBase, MinigameData> activeMinigames = new();
    private float originalOrthoSize; // Lưu trữ ortho size gốc

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

        // Đăng ký sự kiện
        machineController.onMachineAdded += OnMachineAdded;

        // Thiết lập MinigameTriggerButton
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

        // Thiết lập camera
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

        // Tắt tất cả minigame GameObject ban đầu
        DeactivateAllMinigameObjects();

        // Kiểm tra tất cả máy hiện có để spawn minigame
        CheckExistingMachinesForMinigames();
    }

    void OnDestroy()
    {
        if (machineController != null)
        {
            machineController.onMachineAdded -= OnMachineAdded;
        }

        // Khôi phục ortho size gốc khi thoát
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
        Debug.Log($"Minigame event spawned at machine: {selectedMachine.name} (Type: {selectedMachine.GetType().Name}, Minigame: {minigameType})");
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
            Debug.Log($"MinigameTriggerButton clicked, starting minigame for machine: {machine.name}, GameObject: {minigameObject.name}");
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

            // Điều chỉnh camera orthographic size
            if (mainCamera != null)
            {
                float orthoSizeFactor = CalculateOrthoSizeFactor();
                mainCamera.orthographicSize = referenceOrthoSize * orthoSizeFactor;
                Debug.Log($"Applied ortho size factor: {orthoSizeFactor}, New ortho size: {mainCamera.orthographicSize}");
            }

            // Tắt tất cả minigame GameObject khác và bật cái được chọn
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

        // Tính tỷ lệ dựa trên chiều cao
        float orthoSizeFactor = (screenHeight / referenceResolution.y) * (screenAspect / referenceAspect);

        // Giới hạn tỷ lệ
        orthoSizeFactor = Mathf.Clamp(orthoSizeFactor, minOrthoSizeFactor, maxOrthoSizeFactor);
        return orthoSizeFactor;
    }

    private void DeactivateAllMinigameObjects()
    {
        if (match3GameObject != null) match3GameObject.SetActive(false);
        if (whackAMoleGameObject != null) whackAMoleGameObject.SetActive(false);
        if (wireConnectionGameObject != null) wireConnectionGameObject.SetActive(false);

        // Khôi phục ortho size gốc khi tắt minigame
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
        return machine is Generator || machine is StorageMachine || machine is Canteen;
    }

    private GameObject GetMinigameObject(MachineBase machine)
    {
        if (machine is Generator) return match3GameObject;
        if (machine is StorageMachine) return whackAMoleGameObject;
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