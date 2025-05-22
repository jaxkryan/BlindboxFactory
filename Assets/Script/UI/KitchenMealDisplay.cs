using Script.Machine.Machines.Canteen;
using UnityEngine;
using UnityEngine.UI;

public class KitchenMealDisplay : MonoBehaviour
{
    [SerializeField] private CanteenFoodStorage foodStorage;
    [SerializeField] private Text mealText;

    private void Start()
    {
        foodStorage.onMealAmountChanged += UpdateMealUI;
        UpdateMealUI(0);
    }

    private void UpdateMealUI(long change)
    {
        if (mealText != null)
            mealText.text = $"Meals: {foodStorage.AvailableMeals}/{foodStorage.MaxCapacity}";
    }

}
