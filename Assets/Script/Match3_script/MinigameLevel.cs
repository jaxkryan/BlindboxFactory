using System;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using Script.Controller;
using Script.Resources;

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

    private readonly List<Resource> _nonCraftingResources = new () {
        Resource.Gold, Resource.Gem, 
    };
    private void CollectResources()
    {
        foreach (Resource resource in System.Enum.GetValues(typeof(Resource)))
        {
            if (!Enum.TryParse<ColorPiece.ColorType>(ToUpper(Enum.GetName(typeof(Resource), resource)), out var color)) continue;
            int amount = ResourceManager.Instance.GetResourceAmount(color);
            if (amount <= 0) continue;
            if (_nonCraftingResources.Contains(resource)) continue;

            Debug.Log($"Converted to CraftingMaterial: {resource} ({(int)resource})");
            if (GameController.Instance.ResourceController.TryGetAmount(resource, out var value))
                GameController.Instance.ResourceController.TrySetAmount(resource, amount + value);
        }
    }

    private Func<string, string> ToUpper =>
        (str) => {
            var list = new List<char>();
            foreach (var c in str) {
                list.Add(char.ToUpper(c));
            }

            return string.Join("", list);
        };


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