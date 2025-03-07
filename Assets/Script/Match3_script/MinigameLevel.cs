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
        CollectResources();
        grid.GameOver();
    }

    public virtual void GameLose()
    {
        Debug.Log("Game lose");
        CollectResources();
        grid.GameOver();
    }

    private void CollectResources()
    {
        foreach (ColorPiece.ColorType color in System.Enum.GetValues(typeof(ColorPiece.ColorType)))
        {
            int amount = ResourceManager.Instance.GetResourceAmount(color);
            if (amount > 0)
            {
                int colorValue = (int)color;
                Debug.Log($"Color: {color} ({colorValue})");

                if (System.Enum.IsDefined(typeof(CraftingMaterial), colorValue))
                {
                    CraftingMaterial material = (CraftingMaterial)colorValue;
                    Debug.Log($"Converted to CraftingMaterial: {material} ({(int)material})");
                    MaterialManager.Instance.AddMaterial(material, amount);
                }
                else
                {
                    Debug.LogWarning($"No matching CraftingMaterial for ColorType: {color} ({colorValue})");
                }
            }
        }
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

    public virtual void OnBigMatch(int matchSize)
    {
        int extraRewards = matchSize - 3;

        // Reward extra resources
        foreach (ResourceText resourceText in resourceTexts)
        {
            ResourceManager.Instance.AddResource(resourceText.resourceType, extraRewards);
        }
        UpdateResourceTexts();
    }

}