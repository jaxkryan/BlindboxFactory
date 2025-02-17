using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

public class EnergySystem : MonoBehaviour
{
    private const int MAX_ENERGY = 25;
    private const float ENERGY_REGEN_TIME = 10f; // For testing (set to 180f for 3 minutes)
    private int currentEnergy;

    public int CurrentEnergy => currentEnergy;

    private const string ENERGY_KEY = "Energy";
    private const string LAST_UPDATE_KEY = "LastEnergyUpdate";

    public Slider energyBar;             // UI Slider for energy
    public TextMeshProUGUI energyText;     // Text to display energy (e.g., "10/25")
    public TextMeshProUGUI timerText;      // Countdown timer text

    private DateTime lastUpdateTime;

    private void Start()
    {
        // Wait until TimeManager is initialized (we have a valid time source)
        StartCoroutine(WaitForTimeInitialization());
    }

    private IEnumerator WaitForTimeInitialization()
    {
        float timeout = 5f;
        float elapsed = 0f;
        while (!TimeManager.IsInitialized && elapsed < timeout)
        {
            yield return null;
            elapsed += Time.deltaTime;
        }
        if (!TimeManager.IsInitialized)
        {
            Debug.LogWarning("TimeManager initialization timed out. Forcing initialization using local time.");
            TimeManager.ForceInitialize();
        }

        LoadEnergy();
        UpdateUI();
        StartCoroutine(UpdateEnergyRoutine());
    }

    void LoadEnergy()
    {
        currentEnergy = PlayerPrefs.GetInt(ENERGY_KEY, MAX_ENERGY);
        string lastUpdateString = PlayerPrefs.GetString(LAST_UPDATE_KEY, "");
        DateTime now = TimeManager.Now;

        if (!string.IsNullOrEmpty(lastUpdateString))
        {
            lastUpdateTime = DateTime.Parse(lastUpdateString);
            double elapsedSeconds = (now - lastUpdateTime).TotalSeconds;

            if (elapsedSeconds < 0)
            {
                Debug.LogWarning("Detected time travel back! Ignoring regen calculation.");
                lastUpdateTime = now; // Prevent unintended energy gain
                PlayerPrefs.SetString(LAST_UPDATE_KEY, lastUpdateTime.ToString());
                PlayerPrefs.Save();
                return;
            }

            int energyGained = Mathf.FloorToInt((float)elapsedSeconds / ENERGY_REGEN_TIME);
            currentEnergy = Mathf.Min(currentEnergy + energyGained, MAX_ENERGY);

            if (currentEnergy < MAX_ENERGY)
            {
                double remainder = elapsedSeconds % ENERGY_REGEN_TIME;
                lastUpdateTime = now - TimeSpan.FromSeconds(remainder);
                PlayerPrefs.SetString(LAST_UPDATE_KEY, lastUpdateTime.ToString());
            }
        }
        else
        {
            lastUpdateTime = now;
            PlayerPrefs.SetString(LAST_UPDATE_KEY, lastUpdateTime.ToString());
        }

        PlayerPrefs.SetInt(ENERGY_KEY, currentEnergy);
        PlayerPrefs.Save();
    }



    IEnumerator UpdateEnergyRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);

            if (currentEnergy < MAX_ENERGY)
            {
                DateTime now = TimeManager.Now;
                double elapsedSeconds = (now - lastUpdateTime).TotalSeconds;
                if (elapsedSeconds < 0)
                {
                    elapsedSeconds = 0;
                }

                if (elapsedSeconds >= ENERGY_REGEN_TIME)
                {
                    // Calculate how many energy points to add.
                    int energyToGain = Mathf.FloorToInt((float)elapsedSeconds / ENERGY_REGEN_TIME);
                    // Ensure we don't exceed the maximum energy.
                    energyToGain = Mathf.Min(energyToGain, MAX_ENERGY - currentEnergy);
                    currentEnergy += energyToGain;

                    // Update lastUpdateTime to now minus the leftover seconds (the remainder).
                    double remainder = elapsedSeconds % ENERGY_REGEN_TIME;
                    lastUpdateTime = now - TimeSpan.FromSeconds(remainder);

                    PlayerPrefs.SetInt(ENERGY_KEY, currentEnergy);
                    PlayerPrefs.SetString(LAST_UPDATE_KEY, lastUpdateTime.ToString());
                    PlayerPrefs.Save();
                }
            }
            UpdateUI();
        }
    }


    void UpdateUI()
    {
        if (energyBar != null)
            energyBar.value = (float)currentEnergy / MAX_ENERGY;
        if (energyText != null)
            energyText.text = $"{currentEnergy}/{MAX_ENERGY}";

        if (currentEnergy < MAX_ENERGY)
        {
            DateTime nextEnergyTime = lastUpdateTime.AddSeconds(ENERGY_REGEN_TIME);
            TimeSpan timeRemaining = nextEnergyTime - TimeManager.Now;

            if (timeRemaining.TotalSeconds < 0)
            {
                timerText.text = "00:00"; // Ensure we never display negative times
            }
            else
            {
                timerText.text = $"{timeRemaining.Minutes:D2}:{timeRemaining.Seconds:D2}";
            }
        }
        else
        {
            timerText.text = "Full";
        }
    }

    public void Use5Energy()
    {
        bool playable = UseEnergy(5);
        if (playable)
            Debug.Log("Joining game");
        else
            Debug.Log("Not enough energy!");
    }

    public void PlayMatch3()
    {
        if (currentEnergy > 0)
        {
            int energyToUse = currentEnergy; // or any other logic for determining energy used
            bool playable = UseEnergy(energyToUse);
            if (playable)
            {
                // Save the number of moves (equal to energy used)
                PlayerPrefs.SetInt("numMoves", energyToUse);
                PlayerPrefs.Save();
                
                UnityEngine.SceneManagement.SceneManager.LoadScene("Match3_Scene");
                Debug.Log("Joining game");
            }
            else
            {
                Debug.Log("Not enough energy!");
            }
        }
    }


    public bool UseEnergy(int amount)
    {
        if (currentEnergy >= amount)
        {
            int previousEnergy = currentEnergy; // Store energy before spending.
            currentEnergy -= amount;

            // If we’re below max energy...
            if (currentEnergy < MAX_ENERGY)
            {
                // Only reset the timer if we were full before spending,
                // because that means no regen progress had been accumulated.
                if (previousEnergy == MAX_ENERGY)
                {
                    lastUpdateTime = TimeManager.Now;
                }
                // Otherwise, preserve the existing regen progress.
                PlayerPrefs.SetString(LAST_UPDATE_KEY, lastUpdateTime.ToString());
            }

            PlayerPrefs.SetInt(ENERGY_KEY, currentEnergy);
            PlayerPrefs.Save();
            UpdateUI();
            return true;
        }
        return false;
    }

}
