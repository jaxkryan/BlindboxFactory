using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MinigameLevelMove : MinigameLevel
{
    private EnergySystem energySystem;

    [SerializeField] private GameObject winPanel;
    [SerializeField] private TextMeshProUGUI winMessage;
    [SerializeField] private Button winButton;
    //[SerializeField] private TextMeshProUGUI movesText; // UI to show remaining moves

    void Start()
    {
        type = LevelType.MOVES;
        energySystem = FindObjectOfType<EnergySystem>();

        Debug.Log("Game Started | Current Energy: " + energySystem.currentEnergy);

        if (winPanel != null) winPanel.SetActive(false);

        //UpdateMovesText();
    }

    void Update()
    {
        //UpdateMovesText(); // Continuously update move count based on energy
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

    //void UpdateMovesText()
    //{
    //    if (movesText != null)
    //    {
    //        movesText.text = "Moves: " + energySystem.CurrentEnergy;
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
                winButton.onClick.AddListener(() => { Debug.Log("TODO"); });
        }
        else
        {
            Debug.LogWarning("Win Panel is not assigned in the inspector!");
        }
    }
}
