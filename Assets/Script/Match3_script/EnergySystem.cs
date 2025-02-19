using UnityEngine;
using UnityEngine.UI;
using System;
using System.Net;
using System.Net.Sockets;
using TMPro;
using System.IO;

[Serializable]
public class EnergySaveData
{
    public int currentEnergy;
    public long lastEnergyTimeTicks;
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
        if (File.Exists(saveFilePath))
        {
            string json = File.ReadAllText(saveFilePath);
            EnergySaveData data = JsonUtility.FromJson<EnergySaveData>(json);
            currentEnergy = data.currentEnergy;
            lastEnergyTime = new DateTime(data.lastEnergyTimeTicks);
        }
        else
        {
            // If no save exists, use default values.
            currentEnergy = currentEnergy;
            lastEnergyTime = GetCurrentNetworkTime();
        }

        // Offline energy calculation.
        DateTime currentTime = GetCurrentNetworkTime();
        if (currentTime < lastEnergyTime)
        {
            // If the current network time is before the saved time, assume cheating.
            cheatDetected = true;
            Debug.LogWarning("Time cheat detected! No offline energy gain.");
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
        // If we don't have full energy and no cheat was detected, try to regenerate energy.
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
        return string.Format("{0:D2}:{1:D2}", time.Minutes, time.Seconds);
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
            return true;
        }
        return false;
    }

    public void Use5Energy()
    {
        bool playable = SpendEnergy(5);
        if (playable)
            Debug.Log("Joining game");
        else
            Debug.Log("Not enough energy!");
    }

    public void PlayMatch3()
    {
        if (currentEnergy > 0)
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene("Match3_Scene");
            Debug.Log("Joining game");
        }
        else
        {
            Debug.Log("Not enough energy!");
        }
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
        EnergySaveData data = new EnergySaveData();
        data.currentEnergy = currentEnergy;
        data.lastEnergyTimeTicks = lastEnergyTime.Ticks;
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(saveFilePath, json);
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
