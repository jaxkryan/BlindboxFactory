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
    [SerializeField] private GameObject match3GameObject; // GameObject cho Match-3
    [SerializeField] private GameObject whackAMoleGameObject; // GameObject cho Whack-a-Mole
    [SerializeField] private GameObject wireConnectionGameObject; // GameObject cho Wire-Connection

    private const int MAX_MINIGAMES_PER_DAY = 5;
    private int remainingMinigamesToday = MAX_MINIGAMES_PER_DAY;
    private DateTime lastMinigameTime;
    private DateTime lastResetTime;
    private DateTime? minigameStartTime;
    private Dictionary<MachineBase, MinigameData> activeMinigames = new();

    private class MinigameData
    {
        public GameObject MinigameObject; // Thay SceneName bằng GameObject
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
            minigameTriggerButton.gameObject.SetActive(false); // Ẩn ban đầu
            minigameTriggerButton.onClick.RemoveAllListeners();
            minigameTriggerButton.onClick.AddListener(OnMinigameTriggerButtonClicked);
        }
        else
        {
            Debug.LogWarning("MinigameTriggerButton not assigned in Inspector!");
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
    }

    void Update()
    {
        CheckDailyReset();
        SpawnMinigames();
        UpdateActiveMinigames();

        // Cập nhật trạng thái MinigameTriggerButton
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
        // Chỉ spawn một minigame nếu chưa có minigame nào
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
                    break; // Thoát sau khi spawn một minigame
                }
            }
        }
    }

    private void SpawnMinigames()
    {
        if (remainingMinigamesToday <= 0) return;
        if (activeMinigames.Count > 0) return; // Không spawn nếu đã có minigame

        DateTime currentTime = DateTime.UtcNow;
        float elapsedTime = (float)(currentTime - lastMinigameTime).TotalSeconds;

        if (elapsedTime >= minigameInterval)
        {
            int minigamesToSpawn = 1; // Chỉ spawn một minigame
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
        exclamationButton.interactable = false; // Không cho phép click trực tiếp
        Graphic graphic = exclamationButton.GetComponent<Graphic>();
        if (graphic != null) graphic.raycastTarget = false; // Tắt raycast để không nhận click

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

        // Chọn máy duy nhất trong activeMinigames
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

            // Tắt tất cả minigame GameObject khác và bật cái được chọn
            DeactivateAllMinigameObjects();
            minigameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning($"Machine {machine.name} not found in activeMinigames!");
        }
    }

    private void DeactivateAllMinigameObjects()
    {
        if (match3GameObject != null) match3GameObject.SetActive(false);
        if (whackAMoleGameObject != null) whackAMoleGameObject.SetActive(false);
        if (wireConnectionGameObject != null) wireConnectionGameObject.SetActive(false);
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