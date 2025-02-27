using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BlindBoxQueueDisplay : MonoBehaviour
{
    public static BlindBoxQueueDisplay Instance { get; private set; }

    [SerializeField] public Transform contentParent;
    [SerializeField] GameObject itemPrefab;
    [SerializeField] TMP_Text currentText;
    [SerializeField] Image currentImage;

    BoxTypeManager boxTypeManager;

    void Awake()
    {
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
        boxTypeManager = FindObjectOfType<BoxTypeManager>();
        UpdateQueueUI();
    }
    private void OnEnable()
    {
        UpdateQueueUI();
    }

    //public void AddToQueue(BlindBoxNumberPerCraft newItem)
    //{
    //    blindBoxQueue.Enqueue(newItem);
    //    UpdateQueueUI();
    //}

    public void UpdateQueueUI()
    {
        // Clear previous items
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        var queueMachine = BlindBoxInformationDisplay.Instance.GetCurrentDisplayedObject();

        // Populate the ScrollView with queued items
        foreach (var item in queueMachine.GetProductQueue())
        {
            GameObject newItemUI = Instantiate(itemPrefab, contentParent);

            BoxData boxData = boxTypeManager.GetBoxData(item.boxTypeName);
            newItemUI.transform.Find("Text").GetComponent<TMP_Text>().text = $"Box: {item.boxTypeName} \n {item.number}";
            newItemUI.transform.Find("Image").GetComponent<Image>().sprite = boxData.sprite;
        }

        if (queueMachine.GetCurrentProduct() == null)
        {
            currentText.gameObject.SetActive(false);
            currentImage.gameObject.SetActive(false);
        }
        else
        {
            currentText.gameObject.SetActive(true);
            currentImage.gameObject.SetActive(true);

            BoxData currentBoxData = boxTypeManager.GetBoxData(queueMachine.GetCurrentProduct().boxTypeName);
            currentText.text = $"Box: {queueMachine.GetCurrentProduct().boxTypeName} " +
                $"\n {queueMachine.GetCurrentProduct().number}";
            currentImage.sprite = currentBoxData.sprite;
        }

        
    }

    private void DebugQueue()
    {
        Debug.Log("Current Queue Contents:");

        foreach (var item in RecipeListUI.Instance.machine.GetProductQueue())
        {
            Debug.Log($"BoxType: {item.boxTypeName}, Number: {item.number}");
        }
    }
}
