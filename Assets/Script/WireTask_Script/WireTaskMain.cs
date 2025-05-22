using UnityEngine;
using TMPro; // For TextMeshProUGUI
using Script.Controller; // For GameController and ResourceController
using Script.Resources; // For Resource enum

public class WireTaskMain : MonoBehaviour
{
    public static WireTaskMain Instance;

    public int switchCount;
    [SerializeField] private GameObject invisPanel; // Panel to show on game end (win or lose)
    [SerializeField] private TextMeshProUGUI resultText; // Text to display "Success!" or "Failed!"
    [SerializeField] private GameObject connectWiresText; // Text to display "Connect the wires"
    [SerializeField] private WireTaskGeneration leftWireTaskGeneration; // Reference to WireTaskGeneration for left wires
    [SerializeField] private WireTaskGeneration rightWireTaskGeneration; // Reference to WireTaskGeneration for right wires
    [SerializeField] private TextMeshProUGUI timerText; // TextMeshPro to display remaining time
    private int oncount = 0;
    private float textDisplayDuration = 2f; // Duration to show the "Connect the wires" text
    private float timer = 0f; // Tracks elapsed time
    private bool isTimerRunning = false; // Tracks if timer is active
    private bool gameOver = false; // Tracks if the game has ended (win or lose)
    private const float timeLimit = 15f; // 15-second time limit

    private void Awake()
    {
        Instance = this;

        // Ensure UI elements are inactive at the start
        if (invisPanel != null)
        {
            invisPanel.SetActive(false);
        }
        //else
        //{
        //    Debug.LogWarning("InvisPanel not assigned in Inspector!");
        //}

        //if (resultText == null)
        //{
        //    Debug.LogWarning("ResultText (TextMeshProUGUI) not assigned in Inspector!");
        //}

        // Check WireTaskGeneration references
        //if (leftWireTaskGeneration == null)
        //{
        //    Debug.LogWarning("LeftWireTaskGeneration not assigned in Inspector!");
        //}
        //if (rightWireTaskGeneration == null)
        //{
        //    Debug.LogWarning("RightWireTaskGeneration not assigned in Inspector!");
        //}

        // Check TextMeshPro reference
        //if (timerText == null)
        //{
        //    Debug.LogWarning("TimerText (TextMeshProUGUI) not assigned in Inspector!");
        //}
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
                gameOver = true; // Mark game as over
                UpdateTimerDisplay(); // Ensure timer shows 0.0 when time's up
                HandleTimeUp(); // Handle time-up scenario
                //Debug.Log("Time's up! Game over.");
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
        gameOver = false; // Reset game over state
        UpdateTimerDisplay(); // Initialize timer display

        // Reset UI elements
        if (invisPanel != null)
        {
            invisPanel.SetActive(false);
        }

        // Show "Connect the wires" text and hide after a delay
        if (connectWiresText != null)
        {
            connectWiresText.SetActive(true);
            Invoke(nameof(HideConnectWiresText), textDisplayDuration);
        }
        //else
        //{
        //    Debug.LogWarning("ConnectWiresText not assigned in Inspector!");
        //}

        // Randomize the wires
        if (leftWireTaskGeneration != null)
        {
            leftWireTaskGeneration.RandomizeWires();
        }
        //else
        //{
        //    Debug.LogWarning("LeftWireTaskGeneration reference is null, cannot randomize wires!");
        //}
        if (rightWireTaskGeneration != null)
        {
            rightWireTaskGeneration.RandomizeWires();
        }
        //else
        //{
        //    Debug.LogWarning("RightWireTaskGeneration reference is null, cannot randomize wires!");
        //}

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
        // Prevent further interactions if game is over
        if (gameOver)
        {
            return;
        }

        oncount += points;

        AudioManager.Instance.PlaySfx("electric");
        if (oncount == switchCount)
        {
            EndGame();
        }
    }

    private void HandleTimeUp()
    {
        // Show the panel and set lose text
        if (invisPanel != null)
        {
            invisPanel.SetActive(true);
        }
        if (resultText != null)
        {
            resultText.text = "Failed!";
            resultText.color = Color.red; // Set text color to red for "Failed!"
        }
    }

    private void EndGame()
    {
        // Stop the timer
        isTimerRunning = false;
        gameOver = true; // Mark game as over
        UpdateTimerDisplay(); // Update display one last time

        // Show the panel and set win text
        if (invisPanel != null)
        {
            invisPanel.SetActive(true);
        }
        if (resultText != null)
        {
            resultText.text = "Success!";
            resultText.color = Color.green; // Set text color to green for "Success!"
        }

        // Award gems since the task was completed in time
        AddGemsToResources();
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
                //else
                //{
                //    Debug.LogWarning("Failed to set Gem amount in ResourceController.");
                //}
            }
            //else
            //{
            //    Debug.LogWarning("Failed to get current Gem amount from ResourceController.");
            //}
        }
        //else
        //{
        //    Debug.LogWarning("GameController or ResourceController is not available.");
        //}
    }
}