using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ToolbarManager : MonoBehaviour
{
    public static ToolbarManager Instance;

    [Header("Toolbar Settings")]
    public GameObject slotPrefab;
    public Transform contentPanel;
    public float slotSpacing = 10f;

    [Header("Items")]
    public List<ToolbarItem> availableItems = new List<ToolbarItem>();

    private List<GameObject> toolbarSlots = new List<GameObject>();
    private GridLayoutGroup gridLayout;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        gridLayout = contentPanel.GetComponent<GridLayoutGroup>();
        if (gridLayout == null)
            gridLayout = contentPanel.gameObject.AddComponent<GridLayoutGroup>();
    }

    void Start()
    {
        if (slotPrefab == null || contentPanel == null)
        {
            Debug.LogError("Vui lòng gán Slot Prefab và Content Panel!");
            return;
        }

        InitializeToolbar();
    }

    void InitializeToolbar()
    {
        foreach (var item in availableItems)
        {
            GameObject slotObj = Instantiate(slotPrefab, contentPanel);

            ToolbarSlot slotScript = slotObj.GetComponent<ToolbarSlot>();
            if (slotScript != null)
            {
                slotScript.Initialize(item);
            }
            else
            {
                Debug.LogError("Slot Prefab không có ToolbarSlot script!");
            }
        }
    }

    void SetupGridLayout()
    {
        gridLayout.spacing = new Vector2(slotSpacing, slotSpacing);
        gridLayout.cellSize = new Vector2(80, 80); // Adjust size as needed
        gridLayout.constraint = GridLayoutGroup.Constraint.Flexible;
        gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
        gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
        gridLayout.childAlignment = TextAnchor.UpperLeft;
    }

   
    void CreateToolbarSlot(ToolbarItem item)
    {
        GameObject slot = Instantiate(slotPrefab, contentPanel);
        toolbarSlots.Add(slot);

        // Setup slot UI
        ToolbarSlot slotScript = slot.GetComponent<ToolbarSlot>();
        if (slotScript != null)
        {
            slotScript.Initialize(item);
        }
    }

    public void OnItemSelected(ToolbarItem item)
    {
        // Handle item selection
        Debug.Log($"Selected item: {item.itemName}");
    }
    
}
