using UnityEngine;

public class RecipeListUI : MonoBehaviour
{
    [SerializeField] private BlindBoxMachine machine;
    [SerializeField] private RecipeButton recipeButtonPrefab;
    [SerializeField] private Transform contentParent;
    [SerializeField] private MaterialSpriteManager materialSpriteManager;

    private void Awake()
    {
        machine = BlindBoxInformationDisplay.Instance.GetCurrentDisplayedObject();

        if (machine != null)
        {
            Debug.LogError(machine.name);
            GenerateButtons();
        }
        else
        {
            Debug.LogWarning("[RecipeListUI] No BlindBoxMachine assigned yet.");
        }
    }
    private void Start()
    {
        GenerateButtons();
    }

    private void GenerateButtons()
    {
        // Clear existing buttons
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        var materialSprites = materialSpriteManager.GetMaterialSprites();

        // Generate buttons with specific names
        for (int i = 0; i < machine.recipes.Count; i++)
        {
            // Create the button GameObject
            GameObject buttonObj = Instantiate(recipeButtonPrefab.gameObject, contentParent);
            buttonObj.name = $"RecipeButton_{i}";

            // Find and setup the RecipeButton component
            RecipeButton recipeButton = buttonObj.GetComponent<RecipeButton>();
            if (recipeButton != null)
            {
                recipeButton.SetupRecipe(machine.recipes[i], materialSprites);
            }
        }
    }
}