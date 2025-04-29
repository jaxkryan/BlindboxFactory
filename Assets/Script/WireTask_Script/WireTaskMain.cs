using UnityEngine;
using Script.Controller; // For GameController and ResourceController
using Script.Resources; // For Resource enum

public class WireTaskMain : MonoBehaviour
{
    static public WireTaskMain Instance;

    public int switchCount;
    public GameObject winText;
    [SerializeField] private GameObject connectWiresText; // Text to display "Connect the wires"
    [SerializeField] private GameObject returnToMinigameButton; // Button to return to minigame
    [SerializeField] private WireTaskGeneration leftWireTaskGeneration; // Reference to WireTaskGeneration for left wires
    [SerializeField] private WireTaskGeneration rightWireTaskGeneration; // Reference to WireTaskGeneration for right wires
    private int oncount = 0;
    private float textDisplayDuration = 2f; // Duration to show the "Connect the wires" text

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
           // Debug.LogWarning("RightWireTaskGeneration not assigned in Inspector!");
        }
    }

    private void OnEnable()
    {
        //Debug.Log("WireTaskMain OnEnable called - Resetting game state");
        ResetGame();
    }

    private void ResetGame()
    {
       

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
           // Debug.LogWarning("ConnectWiresText not assigned in Inspector!");
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
        // Show win text and return button
        if (winText != null)
        {
            winText.SetActive(true);
        }
        if (returnToMinigameButton != null)
        {
            returnToMinigameButton.SetActive(true);
        }

        AddGemsToResources();
    }

    private void AddGemsToResources()
    {
        if (GameController.Instance != null && GameController.Instance.ResourceController != null)
        {
            if (GameController.Instance.ResourceController.TryGetAmount(Resource.Gem, out long currentGems))
            {
                long newGems = currentGems + Random.Range(1, 101); // random 1 to 100 gems
                if (GameController.Instance.ResourceController.TrySetAmount(Resource.Gem, newGems))
                {
                    //Debug.Log($"Added 20 Gems. New total: {newGems}");
                }
                else
                {
                   // Debug.LogWarning("Failed to set Gem amount in ResourceController.");
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