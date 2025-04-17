using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Script.Controller;
using Script.Machine.ResourceManager;
using System.Collections.Generic;
using System;
using Script.Resources;

public class BlindBoxQueueDisplay : MonoBehaviour
{
    public static BlindBoxQueueDisplay Instance { get; private set; }
    [SerializeField] BlindBoxMachine machine;
    [SerializeField] TMP_Text currentText;
    [SerializeField] Image currentImage;
    [SerializeField] Slider progessionSlider;

    BoxTypeManager boxTypeManager;
    int testInterval;
    void Awake()
    {
        machine = (BlindBoxMachine)BlindBoxInformationDisplay.Instance.currentMachine;
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        machine = (BlindBoxMachine)BlindBoxInformationDisplay.Instance.currentMachine;
        boxTypeManager = FindFirstObjectByType<BoxTypeManager>();
        UpdateQueueUI();
    }

    private void Update()
    {
        var blindboxMachine = (BlindBoxMachine)BlindBoxInformationDisplay.Instance.currentMachine;
        var blindbox = (BlindBox)blindboxMachine.Product;
        if (blindboxMachine.amount != 0 && blindbox.BoxTypeName != BoxTypeName.Null)
        {
            var bbm = BlindBoxInformationDisplay.Instance.currentMachine;
            float progession = bbm.CurrentProgress / bbm.MaxProgress;
            progessionSlider.value = progession;
        }
        UpdateQueueUI();
    }
    private void OnEnable()
    {
        machine = (BlindBoxMachine)BlindBoxInformationDisplay.Instance.currentMachine;
        UpdateQueueUI();
    }



    //public void AddToQueue(BlindBoxNumberPerCraft newItem)
    //{
    //    blindBoxQueue.Enqueue(newItem);
    //    UpdateQueueUI();
    //}

    //public void UpdateQueueUI()
    //{
    //    // Clear previous items
    //    foreach (Transform child in ContentParent)
    //    {
    //        Destroy(child.gameObject);
    //    }

    //    var blindboxMachine = BlindBoxInformationDisplay.Instance.GetCurrentDisplayedObject();

    //    // Populate the ScrollView with queued items
    //    foreach (var item in blindboxMachine.GetProductQueue())
    //    {
    //        GameObject newItemUI = Instantiate(itemPrefab, ContentParent);

    //        BoxData boxData = boxTypeManager.GetBoxData(item.boxTypeName);
    //        newItemUI.transform.Find("Text").GetComponent<TMP_Text>().text = $"Box: {item.boxTypeName} \n {item.number}";
    //        newItemUI.transform.Find("Image").GetComponent<Image>().sprite = boxData.sprite;
    //    }

    //    if (blindboxMachine.GetCurrentProduct() == null)
    //    {
    //        currentText.gameObject.SetActive(false);
    //        currentImage.gameObject.SetActive(false);
    //    }
    //    else
    //    {
    //        currentText.gameObject.SetActive(true);
    //        currentImage.gameObject.SetActive(true);

    //        BoxData currentBoxData = boxTypeManager.GetBoxData(blindboxMachine.GetCurrentProduct().boxTypeName);
    //        currentText.text = $"Box: {blindboxMachine.GetCurrentProduct().boxTypeName} " +
    //            $"\n {blindboxMachine.GetCurrentProduct().number}";
    //        currentImage.sprite = currentBoxData.sprite;
    //    }


    //}

    public void UpdateQueueUI()
    {
        var blindboxMachine = (BlindBoxMachine) BlindBoxInformationDisplay.Instance.currentMachine;
        var blindbox = (BlindBox) blindboxMachine.Product;


        if (blindboxMachine.amount == 0 || blindbox.BoxTypeName == BoxTypeName.Null)
        {
            currentText.gameObject.SetActive(false);
            currentImage.gameObject.SetActive(false);
        }
        else
        {
            currentText.gameObject.SetActive(true);
            currentImage.gameObject.SetActive(true);

            BoxData currentBoxData = boxTypeManager.GetBoxData(blindbox.BoxTypeName);
            currentText.text = $"Box: {blindbox.BoxTypeName} " +
                $"\n {blindboxMachine.amount}";
            currentImage.sprite = currentBoxData.sprite;
        }
    }

    public int CalculateMaxCraftableAmount(
        BlindBoxMachine machine,
        BlindBox currentBlindBox)
    {
        if (machine == null || GameController.Instance.ResourceController == null || currentBlindBox == null)
            return 0;

        if (!GameController.Instance.ResourceController.TryGetAllResourceAmounts(out var resourceAmounts))
            return 0;

        int machineLimit = machine.maxAmount;
        int resourceLimit = CalculateResourceLimit(resourceAmounts, currentBlindBox.ResourceUse);

        return Math.Min(machineLimit, resourceLimit);
    }

    public int CalculateResourceLimit(
        Dictionary<Resource, long> resourceAmounts,
        List<ResourceUse> recipe)
    {
        int maxAmount = int.MaxValue;

        foreach (var item in recipe)
        {
            if (resourceAmounts.TryGetValue(item.Resource, out long availableAmount))
            {
                int possibleAmount = (int)(availableAmount / item.Amount);
                maxAmount = Math.Min(maxAmount, possibleAmount);
            }
            else
            {
                return 0;
            }
        }

        return maxAmount;
    }

    public void FastCraft()
    {
        if (BlindBoxQueueDisplay.Instance == null)
        {
            return;
        }

        var machine = RecipeListUI.Instance.Machine;

        if (machine.GetLastUsedRecipe() == null)
        {
            return;
        }

        int maxAmount = machine.maxAmount;
        BlindBox lastBlindBox = machine.GetLastUsedRecipe();

        int number = CalculateMaxCraftableAmount(machine, lastBlindBox);

        int selectedAmount = Mathf.Min(number, maxAmount);

        if (machine.amount <= 0)
        {
            machine.Product = lastBlindBox;
            machine.lastBox = lastBlindBox.BoxTypeName;
            machine.amount = selectedAmount;
            machine.CurrentProgress = 0;
        }
        else if (machine.Product == lastBlindBox)
        {
            machine.amount = Mathf.Min(machine.amount + selectedAmount, maxAmount);
        }

        BlindBoxQueueDisplay.Instance.UpdateQueueUI();
    }



}
