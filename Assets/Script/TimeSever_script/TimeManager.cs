using System;
using System.Collections;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeSpan timeOffset = TimeSpan.Zero;
    public static bool IsInitialized { get; private set; } = false;

    // Returns trusted current time.
    public static DateTime Now => DateTime.UtcNow + timeOffset;

    private void Start()
    {
        StartCoroutine(InitializeTime());
    }

    private IEnumerator InitializeTime()
    {
        // Optionally, you could show a loading screen until time is verified.
        yield return null; // wait one frame if needed

        DateTime networkTime = NTPClient.GetNetworkTime();
        DateTime localTime = DateTime.UtcNow;
        timeOffset = networkTime - localTime;
        Debug.Log("Time offset (seconds): " + timeOffset.TotalSeconds);
        IsInitialized = true;
    }
    public static void ForceInitialize()
    {
        if (!IsInitialized)
        {
            timeOffset = TimeSpan.Zero; // Use local time as is.
            IsInitialized = true;
            Debug.LogWarning("TimeManager forced initialization: falling back to local time.");
        }
    }
}
