using UnityEngine;
using TMPro;

public class MinigameLevelMove : MinigameLevel
{
    public int numMoves;
    public int targetScore;

    private int moveUsed = 0;

    [SerializeField]
    private TextMeshProUGUI movesText;

    void Start()
    {
        type = LevelType.MOVES;

        Debug.Log("No Move: " + numMoves + "| Target score: " + targetScore);

        // Initialize the moves text
        UpdateMovesText();
    }

    void Update()
    {
        // You can leave this empty if you don't need any per-frame updates
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