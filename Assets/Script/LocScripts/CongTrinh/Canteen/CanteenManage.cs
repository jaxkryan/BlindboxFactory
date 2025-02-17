using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CanteenManage : MonoBehaviour
{
    [Header("Canteen Settings")]
    public float maxMeal = 100f;  // Số lượng bữa ăn tối đa
    public float mealPerSec = 5f; // Số bữa ăn tạo ra mỗi giây
    public float maxWorker = 10f; // Số công nhân tối đa mà canteen có thể phục vụ
    public float mealConsumptionPerWorker = 1f; // Số bữa ăn mỗi công nhân tiêu thụ mỗi giây

    [Header("Current Status")]
    [SerializeField] private float currentMeal = 0f; // Số bữa ăn hiện tại
    [SerializeField] private int currentWorker = 0;  // Số công nhân hiện tại trong canteen

    [Header("UI Elements")]
    public Text mealText;   // Hiển thị số lượng bữa ăn
    public Text workerText; // Hiển thị số lượng công nhân trong canteen

    void Start()
    {
        StartCoroutine(ProduceMeals());
        StartCoroutine(ConsumeMeals());
    }

    void Update()
    {
        if (mealText != null)
        {
            mealText.text = $"Meals: {currentMeal}/{maxMeal}";
        }

        if (workerText != null)
        {
            workerText.text = $"Workers: {currentWorker}/{maxWorker}";
        }
    }

    /// <summary>
    /// Coroutine sản xuất bữa ăn chính xác mỗi giây.
    /// </summary>
    private IEnumerator ProduceMeals()
    {
        while (true)
        {
            if (currentMeal < maxMeal)
            {
                float mealToAdd = mealPerSec; // Tăng đúng giá trị nhập vào

                if (currentMeal + mealToAdd > maxMeal)
                {
                    mealToAdd = maxMeal - currentMeal; // Đảm bảo không vượt quá giới hạn
                }

                currentMeal += mealToAdd;
            }
            yield return new WaitForSeconds(1f); // Chính xác mỗi giây
        }
    }

    /// <summary>
    /// Coroutine tiêu thụ bữa ăn mỗi giây theo số công nhân hiện tại.
    /// </summary>
    private IEnumerator ConsumeMeals()
    {
        while (true)
        {
            float totalConsumption = currentWorker * mealConsumptionPerWorker;

            if (currentMeal >= totalConsumption)
            {
                currentMeal -= totalConsumption;
            }
            else
            {
                currentMeal = 0;
                Debug.Log("⚠ Canteen is out of food! Workers might stop working.");
            }

            yield return new WaitForSeconds(1f);
        }
    }

    /// <summary>
    /// Thêm công nhân vào Canteen nếu chưa đạt tối đa.
    /// </summary>
    public void AddWorker(int amount)
    {
        currentWorker += amount;
        if (currentWorker > maxWorker)
        {
            currentWorker = (int)maxWorker;
        }
    }

    /// <summary>
    /// Giảm số công nhân trong Canteen.
    /// </summary>
    public void RemoveWorker(int amount)
    {
        currentWorker -= amount;
        if (currentWorker < 0)
        {
            currentWorker = 0;
        }
    }

    /// <summary>
    /// Thêm bữa ăn vào Canteen (có thể từ nhập liệu, quà tặng, v.v.).
    /// </summary>
    public void AddMeal(float amount)
    {
        currentMeal += amount;
        if (currentMeal > maxMeal)
        {
            currentMeal = maxMeal;
        }
    }

    /// <summary>
    /// Lấy số lượng bữa ăn hiện tại.
    /// </summary>
    public float GetCurrentMeals()
    {
        return currentMeal;
    }

    /// <summary>
    /// Lấy số lượng công nhân hiện tại.
    /// </summary>
    public int GetCurrentWorkers()
    {
        return currentWorker;
    }
}
