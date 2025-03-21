using UnityEngine;
using UnityEngine.UI;

public class DynamicGridLayout : MonoBehaviour
{
    [SerializeField] public GridLayoutGroup gridLayoutGroup;
    [SerializeField] public RectTransform container; // The parent RectTransform holding the GridLayoutGroup
    public int fixedColumnCount = 6; // Example: Fixed number of columns

    void Start()
    {
        // Ensure the GridLayoutGroup is set to Fixed Column Count
        gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayoutGroup.constraintCount = fixedColumnCount;

        // Dynamically adjust the cell size
        AdjustCellSize();
    }

    void AdjustCellSize()
    {
        // Get the width of the container
        float containerWidth = container.rect.width;

        // Subtract padding and spacing
        float totalSpacing = gridLayoutGroup.spacing.x * (fixedColumnCount - 1);
        float totalPadding = gridLayoutGroup.padding.left + gridLayoutGroup.padding.right;
        float availableWidth = containerWidth - totalSpacing - totalPadding;

        // Calculate the cell width based on the number of columns
        float cellWidth = availableWidth / fixedColumnCount;

        // Optionally, set a height dynamically or use a fixed value
        float cellHeight = cellWidth * 2.2f; // Example: Square cells; adjust as needed

        // Apply the new cell size
        gridLayoutGroup.cellSize = new Vector2(cellWidth, cellHeight);
    }

    // Optional: Call this if the container size changes (e.g., on screen resize)
    void Update()
    {
        AdjustCellSize();
    }
}