using System.Collections.Generic;
using UnityEngine;

public class MaterialSpriteManager : MonoBehaviour
{
    [SerializeField] private List<CraftingMaterialSprite> materialSprites;
    private Dictionary<CraftingMaterial, Sprite> spriteDictionary;

    private void Awake()
    {
        InitializeSpriteDictionary();
    }

    private void InitializeSpriteDictionary()
    {
        spriteDictionary = new Dictionary<CraftingMaterial, Sprite>();
        foreach (var ms in materialSprites)
        {
            if (ms.sprite != null)
            {
                spriteDictionary[ms.material] = ms.sprite;
            }
            else
            {
                Debug.LogWarning($"No sprite assigned for material: {ms.material}");
            }
        }
    }

    public Dictionary<CraftingMaterial, Sprite> GetMaterialSprites()
    {
        if (spriteDictionary == null)
        {
            InitializeSpriteDictionary();
        }
        return spriteDictionary;
    }
}