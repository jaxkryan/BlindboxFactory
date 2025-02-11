using UnityEngine;

public class MinigameLevel : MonoBehaviour
{
    public enum LevelType
    {
        TIMER,
        OBSTACLE,
        MOVES,
    };

    public Grids grid;

    public int score1Star;
    public int score2Star;
    public int score3Star;

    protected LevelType type;

    public LevelType Type { get { return type; } }

    protected int currentScore;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public virtual void GameWin()
    {
        Debug.Log("Game win");
        grid.GameOver();
    }
    public virtual void GameLose()
    {
        Debug.Log("Game lose");
        grid.GameOver();
    }

    public virtual void OnMove()
    {

    }

    public virtual void OnPieceClear(GamePiece piece)
    {
        currentScore += piece.score;
        //Debug.Log("Score: " + currentScore);
    }
}
