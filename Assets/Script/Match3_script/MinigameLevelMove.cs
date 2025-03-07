using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MinigameLevelMove : MinigameLevel
{
    private EnergySystem energySystem;

    [SerializeField] private GameObject winPanel;
    [SerializeField] private TextMeshProUGUI winMessage;
    [SerializeField] private Button winButton;
    //[SerializeField] private TextMeshProUGUI movesText; // Optional UI to show remaining moves

    public static string currentSystemId;
    void Start()
    {
        type = LevelType.MOVES;
        energySystem = FindFirstObjectByType<EnergySystem>();

        if (energySystem != null)
        {
            // (Optionally) assign the desired ID for this minigame session.
            // For example, this could be passed from a GameManager.
            energySystem.systemId = currentSystemId != null? currentSystemId : "none";

            // Reload the energy state for this systemId.
            energySystem.LoadEnergyState();
            //Debug.Log("Minigame loaded energy for system (" + energySystem.systemId + "): " + energySystem.currentEnergy);
        }
        else
        {
            Debug.LogError("No EnergySystem found!");
        }
        Debug.Log("Current energy system: " + currentSystemId);
        if (winPanel != null)
            winPanel.SetActive(false);

        // Optionally update UI text.
        //UpdateMovesText();
    }

    void Update()
    {
        // Optionally update moves text UI continuously.
        //UpdateMovesText();
    }

    public override void OnMove()
    {
        if (energySystem != null && energySystem.SpendEnergy(1)) // Try using 1 energy per move
        {
            Debug.Log("Move made | Remaining Energy: " + energySystem.currentEnergy);
        }
        else
        {
            Debug.Log("No energy left! Wait for regeneration.");
        }
    }

    // Optional helper to update moves display UI.
    //void UpdateMovesText()
    //{
    //    if (movesText != null && energySystem != null)
    //    {
    //        movesText.text = "Moves: " + energySystem.currentEnergy;
    //    }
    //}

    public override void GameWin()
    {
        Debug.Log("Game Won!");

        if (winPanel != null)
        {
            winPanel.SetActive(true);
            if (winMessage != null)
                winMessage.text = "Congratulations! You won!";

            if (winButton != null)
                winButton.onClick.AddListener(() => { Debug.Log("TODO: Next steps"); });
        }
        else
        {
            Debug.LogWarning("Win Panel is not assigned in the inspector!");
        }
    }
}
