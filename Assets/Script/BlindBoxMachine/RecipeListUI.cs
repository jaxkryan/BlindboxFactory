using UnityEngine;

public class RecipeListUI : PersistentSingleton<RecipeListUI>
{
    [SerializeField] public BlindBoxMachine Machine;
    [SerializeField] public RecipeButton RecipeButtonPrefab;
    [SerializeField] public Transform ContentParent;

    protected override void Awake()
    {
        base.Awake();
        Machine = (BlindBoxMachine)BlindBoxInformationDisplay.Instance.currentMachine;

        if (Machine != null)
        {
            GenerateButtons();
        }
        else
        {
            Debug.LogWarning("[RecipeListUI] No BlindBoxMachine assigned yet.");
        }
    }

    protected void OnEnable()
    {
        Machine = (BlindBoxMachine)BlindBoxInformationDisplay.Instance.currentMachine;

        if (Machine != null)
        {
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
        foreach (Transform child in ContentParent)
        {
            Destroy(child.gameObject);
        }

        // Generate buttons with specific names
        for (int i = 0; i < Machine.recipes.Count; i++)
        {
            // Create the button GameObject
            GameObject buttonObj = Instantiate(RecipeButtonPrefab.gameObject, ContentParent);
            buttonObj.name = $"RecipeButton_{i}";

            // Find and setup the RecipeButton component
            RecipeButton recipeButton = buttonObj.GetComponent<RecipeButton>();
            if (recipeButton != null)
            {
                recipeButton.SetupRecipe(Machine.recipes[i]);
            }
        }
    }
}