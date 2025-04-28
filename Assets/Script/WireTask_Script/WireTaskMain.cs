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
           // Debug.LogWarning("ReturnToMinigameButton not assigned in Inspector!");
        }
    }

    private void Start()
    {
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

        // Add 75 Gems to ResourceController
        AddGemsToResources();
    }

    private void AddGemsToResources()
    {
        if (GameController.Instance != null && GameController.Instance.ResourceController != null)
        {
            if (GameController.Instance.ResourceController.TryGetAmount(Resource.Gem, out long currentGems))
            {
                long newGems = currentGems + 20;
                if (GameController.Instance.ResourceController.TrySetAmount(Resource.Gem, newGems))
                {
                   // Debug.Log($"Added 75 Gems. New total: {newGems}");
                }
                else
                {
                   // Debug.LogWarning("Failed to set Gem amount in ResourceController.");
                }
            }
            else
            {
               // Debug.LogWarning("Failed to get current Gem amount from ResourceController.");
            }
        }
        else
        {
          //  Debug.LogWarning("GameController or ResourceController is not available.");
        }
    }
}