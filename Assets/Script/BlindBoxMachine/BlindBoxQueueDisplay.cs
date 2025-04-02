using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BlindBoxQueueDisplay : MonoBehaviour
{
    public static BlindBoxQueueDisplay Instance { get; private set; }
    [SerializeField] TMP_Text currentText;
    [SerializeField] Image currentImage;
    [SerializeField] Slider progessionSlider;

    BoxTypeManager boxTypeManager;
    int testInterval;
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
        boxTypeManager = FindFirstObjectByType<BoxTypeManager>();
        UpdateQueueUI();
    }

    private void Update()
    {
        var bbm = BlindBoxInformationDisplay.Instance.currentMachine;
        float progession = bbm.CurrentProgress/bbm.MaxProgress;
        progessionSlider.value = progession;
        UpdateQueueUI();
    }
    private void OnEnable()
    {
        UpdateQueueUI();
    }

    public string FormatTimeFull(float timeInSeconds)
    {
        int hours = Mathf.FloorToInt(timeInSeconds / 3600);
        int minutes = Mathf.FloorToInt((timeInSeconds % 3600) / 60);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60);

        return string.Format("{0:D2}:{1:D2}:{2:D2}", hours, minutes, seconds);
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


        Debug.Log($"Box: {blindbox.BoxTypeName} " +
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

            BoxData currentBoxData = boxTypeManager.GetBoxData(blindbox.BoxTypeName);
            currentText.text = $"Box: {blindbox.BoxTypeName} " +
                $"\n {blindboxMachine.amount}";
            currentImage.sprite = currentBoxData.sprite;
        }
    }


}
