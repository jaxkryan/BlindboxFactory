using Unity.VisualScripting;
using UnityEngine;

public class MinigameLevelMove : MinigameLevel
{
    public int numMoves;
    public int targetScore;

    private int moveUsed = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        type = LevelType.MOVES;

        Debug.Log("No Move: " + numMoves + "| Target score: " + targetScore);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public override void OnMove()
    {
        moveUsed++;

        Debug.Log("Moves remaining: " + (numMoves - moveUsed));

        if (numMoves == moveUsed)
        {
            GameWin();
        }
    }
}
