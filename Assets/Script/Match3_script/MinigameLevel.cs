using UnityEngine;
using TMPro;
using System.Collections.Generic;

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

    [System.Serializable]
    public class ResourceText
    {
        public ColorPiece.ColorType resourceType;
        public TextMeshProUGUI textComponent;
    }

    public List<ResourceText> resourceTexts;

    void Start()
    {
        UpdateResourceTexts();
    }

    void Update()
    {
        UpdateResourceTexts();
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

        ColorPiece colorPiece = piece.GetComponent<ColorPiece>();
        if (colorPiece != null)
        {
            ColorPiece.ColorType pieceColor = colorPiece.Color;
            if (pieceColor != ColorPiece.ColorType.ANY && pieceColor != ColorPiece.ColorType.COUNT)
            {
                ResourceManager.Instance.AddResource(pieceColor, 1);
                Debug.Log($"Added 1 {pieceColor} resource");
                UpdateResourceTexts();
            }
        }
    }

    private void UpdateResourceTexts()
    {
        foreach (ResourceText resourceText in resourceTexts)
        {
            if (resourceText.textComponent != null)
            {
                int amount = ResourceManager.Instance.GetResourceAmount(resourceText.resourceType);
                resourceText.textComponent.text = $"{amount}";
            }
        }
    }
}