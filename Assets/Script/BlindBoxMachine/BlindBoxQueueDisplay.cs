using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BlindBoxQueueDisplay : MonoBehaviour
{
    public static BlindBoxQueueDisplay Instance { get; private set; }
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

    //public void UpdateQueueUI()
    //{
    //    // Clear previous items
    //    foreach (Transform child in contentParent)
    //    {
    //        Destroy(child.gameObject);
    //    }

    //    var blindboxMachine = BlindBoxInformationDisplay.Instance.GetCurrentDisplayedObject();

    //    // Populate the ScrollView with queued items
    //    foreach (var item in blindboxMachine.GetProductQueue())
    //    {
    //        GameObject newItemUI = Instantiate(itemPrefab, contentParent);

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
        var blindboxMachine = BlindBoxInformationDisplay.Instance.GetCurrentDisplayedObject();
        var blindbox = (BlindBox) blindboxMachine.Product;


        Debug.Log($"Box: {blindbox.boxTypeName} " +
                $"\n {blindboxMachine.amount}");


        if (blindboxMachine.amount == 0)
        {
            currentText.gameObject.SetActive(false);
            currentImage.gameObject.SetActive(false);
        }
        else
        {
            currentText.gameObject.SetActive(true);
            currentImage.gameObject.SetActive(true);

            BoxData currentBoxData = boxTypeManager.GetBoxData(blindbox.boxTypeName);
            currentText.text = $"Box: {blindbox.boxTypeName} " +
                $"\n {blindboxMachine.amount}";
            currentImage.sprite = currentBoxData.sprite;
        }
    }


}
