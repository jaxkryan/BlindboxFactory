using UnityEngine;
using TMPro; // For TextMeshProUGUI
using Script.Controller; // For GameController and ResourceController
using Script.Resources; // For Resource enum

public class WireTaskMain : MonoBehaviour
{
    public static WireTaskMain Instance;

    public int switchCount;
    public GameObject winText;
    [SerializeField] private GameObject connectWiresText; // Text to display "Connect the wires"
    [SerializeField] private GameObject returnToMinigameButton; // Button to return to minigame
    [SerializeField] private WireTaskGeneration leftWireTaskGeneration; // Reference to WireTaskGeneration for left wires
    [SerializeField] private WireTaskGeneration rightWireTaskGeneration; // Reference to WireTaskGeneration for right wires
    [SerializeField] private TextMeshProUGUI timerText; // TextMeshPro to display remaining time
    private int oncount = 0;
    private float textDisplayDuration = 2f; // Duration to show the "Connect the wires" text
    private float timer = 0f; // Tracks elapsed time
    private bool isTimerRunning = false; // Tracks if timer is active
    private const float timeLimit = 15f; // 15-second time limit
    private bool completedInTime = false; // Tracks if completed within 15s

    private void Awake()
    {
        Instance = this;

        // Ensure winText and return button are inactive at the start
        if (winText != null)
        {
            winText.SetActive(false);
        }
        if (returnToMinigameButton != null)
        {
            returnToMinigameButton.SetActive(false);
        }
        else
        {
            //Debug.LogWarning("ReturnToMinigameButton not assigned in Inspector!");
        }

        // Check WireTaskGeneration references
        if (leftWireTaskGeneration == null)
        {
            //Debug.LogWarning("LeftWireTaskGeneration not assigned in Inspector!");
        }
        if (rightWireTaskGeneration == null)
        {
            //Debug.LogWarning("RightWireTaskGeneration not assigned in Inspector!");
        }

        // Check TextMeshPro reference
        if (timerText == null)
        {
            //Debug.LogWarning("TimerText (TextMeshProUGUI) not assigned in Inspector!");
        }
    }

    private void OnEnable()
    {
        //Debug.Log("WireTaskMain OnEnable called - Resetting game state");
        ResetGame();
    }

    private void Update()
    {
        if (isTimerRunning)
        {
            timer += Time.deltaTime;
            UpdateTimerDisplay();
            if (timer >= timeLimit)
            {
                isTimerRunning = false;
                UpdateTimerDisplay(); // Ensure timer shows 0.0 when time's up
                //Debug.Log("Time's up! No gem reward.");
            }
        }
    }

    private void UpdateTimerDisplay()
    {
        if (timerText != null)
        {
            float timeRemaining = Mathf.Max(0f, timeLimit - timer);
            timerText.text = timeRemaining.ToString("F1"); // Display with 1 decimal place
        }
    }

    private void ResetGame()
    {
        // Reset timer and game state
        timer = 0f;
        isTimerRunning = true;
        completedInTime = false;
        UpdateTimerDisplay(); // Initialize timer display

        // Reset UI elements
        if (winText != null)
        {
            winText.SetActive(false);
        }
        if (returnToMinigameButton != null)
        {
            returnToMinigameButton.SetActive(false);
        }

        // Show "Connect the wires" text and hide after a delay
        if (connectWiresText != null)
        {
            connectWiresText.SetActive(true);
            Invoke(nameof(HideConnectWiresText), textDisplayDuration);
        }
        else
        {
            //Debug.LogWarning("ConnectWiresText not assigned in Inspector!");
        }

        // Randomize the wires
        if (leftWireTaskGeneration != null)
        {
            leftWireTaskGeneration.RandomizeWires();
        }
        else
        {
            //Debug.LogWarning("LeftWireTaskGeneration reference is null, cannot randomize wires!");
        }
        if (rightWireTaskGeneration != null)
        {
            rightWireTaskGeneration.RandomizeWires();
        }
        else
        {
            //Debug.LogWarning("RightWireTaskGeneration reference is null, cannot randomize wires!");
        }

        // Reset all wires
        Wire[] wires = FindObjectsOfType<Wire>();
        foreach (Wire wire in wires)
        {
            wire.Reset();
        }

        oncount = 0;
    }

    private void HideConnectWiresText()
    {
        if (connectWiresText != null)
        {
            connectWiresText.SetActive(false);
        }
    }

    public void SwitchChange(int points)
    {
        oncount += points;
        //Debug.Log($"SwitchChange called - points: {points}, oncount: {oncount}");
        if (oncount == switchCount)
        {
            EndGame();
        }
    }

    private void EndGame()
    {
        // Stop the timer
        isTimerRunning = false;
        UpdateTimerDisplay(); // Update display one last time

        // Check if completed within time limit
        completedInTime = timer <= timeLimit;

        // Show win text and return button
        if (winText != null)
        {
            winText.SetActive(true);
        }
        if (returnToMinigameButton != null)
        {
            returnToMinigameButton.SetActive(true);
        }

        // Award gems only if completed in time
        if (completedInTime)
        {
            AddGemsToResources();
        }
        else
        {
            //Debug.Log("Task completed after 15 seconds, no gems awarded.");
        }
    }

    private void AddGemsToResources()
    {
        if (GameController.Instance != null && GameController.Instance.ResourceController != null)
        {
            if (GameController.Instance.ResourceController.TryGetAmount(Resource.Gem, out long currentGems))
            {
                long newGems = currentGems + Random.Range(1, 101); // Random 1 to 100 gems
                if (GameController.Instance.ResourceController.TrySetAmount(Resource.Gem, newGems))
                {
                    //Debug.Log($"Added {newGems - currentGems} Gems. New total: {newGems}");
                }
                else
                {
                    //Debug.LogWarning("Failed to set Gem amount in ResourceController.");
                }
            }
            else
            {
                //Debug.LogWarning("Failed to get current Gem amount from ResourceController.");
            }
        }
        else
        {
            //Debug.LogWarning("GameController or ResourceController is not available.");
        }
    }
}