using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
public class ToolbarSlot : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI costText;
    public Button slotButton;
    public GameObject lockIcon;

    private ToolbarItem item;
    private CanvasGroup canvasGroup;
    private ToolbarDragHandler dragHandler;
    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        if (slotButton == null)
            slotButton = GetComponent<Button>();
    }

    public void Initialize(ToolbarItem newItem)
    {
        item = newItem;

        // Set up visuals
        if (iconImage != null)
            iconImage.sprite = item.itemIcon;

        if (costText != null)
            costText.text = item.cost.ToString();

        if (lockIcon != null)
            lockIcon.SetActive(!item.isUnlocked);

        // Set up button
        if (slotButton != null)
        {
            slotButton.onClick.AddListener(() => OnSlotClicked());
        }

        // Set interactability
        canvasGroup.interactable = item.isUnlocked;
        canvasGroup.alpha = item.isUnlocked ? 1f : 0.5f;
        // Khởi tạo DragHandler
        if (dragHandler != null)
        {
            dragHandler.Initialize(item);
        }
        else
        {
            Debug.LogError("Không tìm thấy ToolbarDragHandler!");
        }
    }

    void OnSlotClicked()
    {
        if (item.isUnlocked)
        {
            ToolbarManager.Instance.OnItemSelected(item);
        }
    }

    void OnDestroy()
    {
        if (slotButton != null)
            slotButton.onClick.RemoveAllListeners();
    }
}