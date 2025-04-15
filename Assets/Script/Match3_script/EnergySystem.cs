using UnityEngine;
using UnityEngine.UI;
using System;
using System.Net;
using System.Net.Sockets;
using TMPro;
using System.IO;
using System.Collections.Generic;

[Serializable]
public class EnergySaveData
{
    public string id;
    public int currentEnergy;
    public long lastEnergyTimeTicks;
}

[Serializable]
public class EnergySaveDataCollection
{
    public List<EnergySaveData> energyData = new List<EnergySaveData>();
}

public class EnergySystem : MonoBehaviour
{
    [Header("Energy Settings")]
    public int maxEnergy = 25;
    public int currentEnergy = 0;
    public float regenTime = 10f; // seconds per energy unit

    [Header("UI Elements")]
    public Slider energySlider;
    public TextMeshProUGUI energyText;
    public TextMeshProUGUI timerText;

    [Header("System ID")]
    [Tooltip("Unique identifier for this energy system instance.")]
    public string systemId = "defaultID";

    // Time variables
    private DateTime lastEnergyTime; // The time when energy last regenerated
    private bool cheatDetected = false;

    // These variables help us update time without querying NTP every frame.
    private DateTime networkStartTime;
    private float localStartTime;

    // Path to save our JSON file.
    private string saveFilePath;

    void Start()
    {
        // Get the initial network time from our NTP server.
        networkStartTime = GetNetworkTime();
        localStartTime = Time.realtimeSinceStartup;

        // Set file path for saving data.
        saveFilePath = Path.Combine(Application.persistentDataPath, "energyTimerJSON.json");

        // Make sure the slider displays energy values.
        energySlider.maxValue = maxEnergy;

        // Load saved energy and last update time from JSON file.
        LoadEnergyState();

        // Offline energy calculation.
        DateTime currentTime = GetCurrentNetworkTime();
        if (currentTime < lastEnergyTime)
        {
            // If the current network time is before the saved time, assume cheating.
            cheatDetected = true;
            Debug.LogWarning("Time cheat detected! No offline energy gain for system: " + systemId);
        }
        else if (!cheatDetected && currentEnergy < maxEnergy)
        {
            TimeSpan delta = currentTime - lastEnergyTime;
            int energyToAdd = (int)(delta.TotalSeconds / regenTime);
            if (energyToAdd > 0)
            {
                currentEnergy = Mathf.Min(maxEnergy, currentEnergy + energyToAdd);
                // Move the lastEnergyTime forward to account for the added energy.
                lastEnergyTime = lastEnergyTime.AddSeconds(energyToAdd * regenTime);
            }
        }

        UpdateUI();
    }

    void Update()
    {
        // Regenerate energy if not full and no cheat was detected.
        if (currentEnergy < maxEnergy && !cheatDetected)
        {
            DateTime currentTime = GetCurrentNetworkTime();
            double elapsed = (currentTime - lastEnergyTime).TotalSeconds;
            if (elapsed >= regenTime)
            {
                int energyToAdd = (int)(elapsed / regenTime);
                currentEnergy = Mathf.Min(maxEnergy, currentEnergy + energyToAdd);
                lastEnergyTime = lastEnergyTime.AddSeconds(energyToAdd * regenTime);
            }
        }
        // If energy is full, reset the timer.
        else if (currentEnergy >= maxEnergy)
        {
            lastEnergyTime = GetCurrentNetworkTime();
        }

        UpdateUI();
    }

    void UpdateUI()
    {
        // Update the energy text (e.g., "3/25")
        energyText.text = $"{currentEnergy}/{maxEnergy}";

        // Update the slider to display the current energy.
        energySlider.value = currentEnergy;

        // Update the timer for the next energy unit if energy isn't full.
        if (currentEnergy < maxEnergy)
        {
            DateTime currentTime = GetCurrentNetworkTime();
            double secondsPassed = (currentTime - lastEnergyTime).TotalSeconds;
            double secondsLeft = regenTime - secondsPassed;
            if (secondsLeft < 0) secondsLeft = 0;

            timerText.text = FormatTime(secondsLeft);
        }
        else
        {
            timerText.text = "";
        }
    }

    // Helper to format seconds into MM:SS.
    string FormatTime(double seconds)
    {
        TimeSpan time = TimeSpan.FromSeconds(seconds);
        return time.Minutes.ToString("D2") + ":" + time.Seconds.ToString("D2");
    }

    // Call this method to spend energy.
    public bool SpendEnergy(int amount)
    {
        if (currentEnergy >= amount)
        {
            currentEnergy -= amount;
            // Start the regeneration timer if energy drops below max.
            if (currentEnergy < maxEnergy)
            {
                lastEnergyTime = GetCurrentNetworkTime();
            }
            UpdateUI();
            SaveEnergy();
            return true;
        }
        return false;
    }

    // Public method to load energy state using the systemId.
    public void LoadEnergyState()
    {
        if (string.IsNullOrEmpty(saveFilePath))
        {
            saveFilePath = Path.Combine(Application.persistentDataPath, "energyTimerJSON.json");
        }

        if (File.Exists(saveFilePath))
        {
            EnergySaveData loadedData = LoadEnergyData(systemId);
            if (loadedData != null)
            {
                currentEnergy = loadedData.currentEnergy;
                lastEnergyTime = new DateTime(loadedData.lastEnergyTimeTicks);
            }
            else
            {
                // If no data is found for this id, initialize the lastEnergyTime.
                lastEnergyTime = GetCurrentNetworkTime();
            }
        }
        else
        {
            // No file exists, initialize default values.
            lastEnergyTime = GetCurrentNetworkTime();
        }
        UpdateUI();
    }

    // Save energy data when the application quits or pauses.
    void OnApplicationQuit() { SaveEnergy(); }
    void OnApplicationPause(bool pause)
    {
        if (pause)
            SaveEnergy();
    }

    void SaveEnergy()
    {
        // Load the current collection (if exists) or create a new one.
        EnergySaveDataCollection collection = new EnergySaveDataCollection();
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            collection = JsonUtility.FromJson<EnergySaveDataCollection>(json);
            if (collection == null || collection.energyData == null)
            {
                collection = new EnergySaveDataCollection();
            }
        }

        // Try to find an existing entry with this systemId.
        EnergySaveData data = collection.energyData.Find(x => x.id == systemId);
        if (data == null)
        {
            data = new EnergySaveData();
            data.id = systemId;
            collection.energyData.Add(data);
        }
        data.currentEnergy = currentEnergy;
        data.lastEnergyTimeTicks = lastEnergyTime.Ticks;

        string outputJson = JsonUtility.ToJson(collection, true);
        File.WriteAllText(saveFilePath, outputJson);
    }

    EnergySaveData LoadEnergyData(string id)
    {
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            EnergySaveDataCollection collection = JsonUtility.FromJson<EnergySaveDataCollection>(json);
            if (collection != null && collection.energyData != null)
            {
                return collection.energyData.Find(x => x.id == id);
            }
        }
        return null;
    }
    public void Use5Energy()
    {
        bool playable = SpendEnergy(5);
        if (playable)
            Debug.Log("Joining game with system: " + systemId);
        else
            Debug.Log("Not enough energy in system: " + systemId);
    }

    public void PlayMatch3()
    {
        if (currentEnergy > 0)
        {
            MinigameLevelMove.currentSystemId = systemId;
            UnityEngine.SceneManagement.SceneManager.LoadScene("Match3_Scene");
            Debug.Log("Joining game with system: " + systemId);
        }
        else
        {
            Debug.Log("Not enough energy in system: " + systemId);
        }
    }
    // Returns the current network time using the offset from the initial NTP query.
    DateTime GetCurrentNetworkTime()
    {
        return networkStartTime.AddSeconds(Time.realtimeSinceStartup - localStartTime);
    }

    // Get the network time from an NTP server.
    public static DateTime GetNetworkTime()
    {
        const string ntpServer = "pool.ntp.org";
        byte[] ntpData = new byte[48];

        // Set protocol version (LI, VN, Mode)
        ntpData[0] = 0x1B;

        try
        {
            var addresses = Dns.GetHostEntry(ntpServer).AddressList;
            IPEndPoint ipEndPoint = new IPEndPoint(addresses[0], 123);
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp))
            {
                socket.Connect(ipEndPoint);
                socket.ReceiveTimeout = 3000; // 3-second timeout
                socket.Send(ntpData);
                socket.Receive(ntpData);
            }

            // Offset to the "Transmit Timestamp" field is at byte 40.
            const byte serverReplyTime = 40;
            ulong intPart = BitConverter.ToUInt32(ntpData, serverReplyTime);
            ulong fractPart = BitConverter.ToUInt32(ntpData, serverReplyTime + 4);

            intPart = SwapEndianness(intPart);
            fractPart = SwapEndianness(fractPart);

            // Convert to milliseconds.
            ulong milliseconds = (intPart * 1000UL) + ((fractPart * 1000UL) / 0x100000000UL);
            DateTime networkDateTime = (new DateTime(1900, 1, 1)).AddMilliseconds((long)milliseconds);
            return networkDateTime.ToLocalTime();
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Failed to get NTP time: " + ex.Message);
            // Fallback to local time (not ideal, but necessary if NTP fails)
            return DateTime.Now;
        }
    }

    // Helper method to convert big-endian to little-endian.
    static uint SwapEndianness(ulong x)
    {
        return (uint)(((x & 0x000000ff) << 24) +
                      ((x & 0x0000ff00) << 8) +
                      ((x & 0x00ff0000) >> 8) +
                      ((x & 0xff000000) >> 24));
    }
}
