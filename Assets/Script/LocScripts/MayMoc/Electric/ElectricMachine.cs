using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NewMonoBehaviourScript : MonoBehaviour
{
    public float energyPerSecond = 10f;  // Năng lượng sản xuất mỗi giây
    public float maxCapacity = 1000f;    // Dung lượng tối đa
    public float durability = 1000f;     // Độ bền
    public float durabilityLossPerSecond = 100f; // số durability cần giảm mỗi giây
    public Slider energySlider;
    public Text energyText;
    public Image wrenchImage;
    private float currentEnergy = 0f;    // Năng lượng hiện tại

    private void Start()
    {
        StartCoroutine(ReduceDurabilityOverTime());
        StartCoroutine(ProduceEnergyOverTime());

        wrenchImage.gameObject.SetActive(false);
        energyText.text = ((int)currentEnergy).ToString();

        if (energySlider != null)
        {
            energySlider.maxValue = maxCapacity;
        }
    }

    private void Update()
    {
        
        if (energyText != null)
        {
            energyText.text = $"Energy: {currentEnergy}/{maxCapacity}";
        }
        if (energySlider != null)
        {
            energySlider.value = currentEnergy;
        }
    }
    // ham de goi ham giam durabiliry moi giay
    private IEnumerator ReduceDurabilityOverTime()
    {
        while (durability > 0)
        {
            UseDurability(durabilityLossPerSecond);
            Debug.Log(durability);
            yield return new WaitForSeconds(1f); // Giảm độ bền mỗi giây
        }
    }
    //ham de tang nang luong moi giay
    private IEnumerator ProduceEnergyOverTime()
    {
        while (durability > 0)
        {
            if (currentEnergy + energyPerSecond <= maxCapacity)
            {
                currentEnergy += energyPerSecond;
            }
            else
            {
                currentEnergy = maxCapacity;
            }

            yield return new WaitForSeconds(1f); // Tăng năng lượng mỗi giây
        }
    }
    //giam durabiliry 
    public void UseDurability(float amount)
    {
        durability -= amount;

        if (durability > 0)
        {
            wrenchImage.gameObject.SetActive(false);
        }
        else
        {
            wrenchImage.gameObject.SetActive(true);
        }
    }
}
