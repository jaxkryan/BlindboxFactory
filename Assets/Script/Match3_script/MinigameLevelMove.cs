using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MinigameLevelMove : MinigameLevel
{
    public int numMoves;
    public int targetScore;
    private int moveUsed = 0;

    [SerializeField] private TextMeshProUGUI movesText;

    // UI elements for the win screen
    [SerializeField] private GameObject winPanel;
    [SerializeField] private TextMeshProUGUI winMessage;
    [SerializeField] private Button winButton;

    void Start()
    {
        numMoves = PlayerPrefs.GetInt("numMoves", numMoves);
        type = LevelType.MOVES;

        Debug.Log("No Move: " + numMoves + " | Target score: " + targetScore);

        UpdateMovesText();

        // Hide the win panel at the start
        if (winPanel != null) winPanel.SetActive(false);
    }

    public override void OnMove()
    {
        moveUsed++;
        UpdateMovesText();

        Debug.Log("Moves remaining: " + (numMoves - moveUsed));

        if (moveUsed >= numMoves)
        {
            GameWin();
        }
    }

    public override void GameWin()
    {
        Debug.Log("Game Won!");

        // Display the win panel and message
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

    private void UpdateMovesText()
    {
        if (movesText != null)
        {
            movesText.text = "Moves: " + (numMoves - moveUsed);
        }
        else
        {
            Debug.LogWarning("Moves Text component is not assigned!");
        }
    }
}
