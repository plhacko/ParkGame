using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class ProportionalLayoutGroup : LayoutGroup
{
    [Min(1)]
    public int columns = 2; // Define the number of columns
    private Vector2 cellSize = new Vector2(100, 100); // Size of each cell
    public Vector2 spacing = Vector2.zero; // Spacing between each cell
    public Vector2 elementRatio = Vector2.one; // Ratio of width to height

    public override void CalculateLayoutInputHorizontal()
    {
        base.CalculateLayoutInputHorizontal();

        float parentWidth = rectTransform.rect.width;
        float cellWidth = (parentWidth - (padding.left + padding.right + (spacing.x * (columns - 1)))) / columns;

        cellSize.x = cellWidth;
        cellSize.y = cellWidth * elementRatio.y / elementRatio.x;

        int rowCount = Mathf.CeilToInt(rectChildren.Count / (float)columns);

        float totalHeight = (cellSize.y * rowCount) + (spacing.y * (rowCount - 1)) + padding.top + padding.bottom;

        SetLayoutInputForAxis(totalHeight, totalHeight, totalHeight, 1);
    }

    public override void CalculateLayoutInputVertical()
    {
        // Not needed for grid layout
    }

    public override void SetLayoutHorizontal()
    {
        // Arrange children on the horizontal axis
        for (int i = 0; i < rectChildren.Count; i++)
        {
            int row = i / columns;
            int col = i % columns;

            RectTransform child = rectChildren[i];

            float xPos = (cellSize.x + spacing.x) * col + padding.left;
            float yPos = (cellSize.y + spacing.y) * row + padding.top;

            SetChildAlongAxis(child, 0, xPos, cellSize.x);
            SetChildAlongAxis(child, 1, yPos, cellSize.y);
        }
    }

    public override void SetLayoutVertical()
    {
        // Not needed for grid layout
    }
}
