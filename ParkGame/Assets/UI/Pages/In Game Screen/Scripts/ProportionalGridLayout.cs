using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class ProportionalLayoutGroup : LayoutGroup
{
    private Vector2 cellSize = new Vector2(100, 100); // Size of each cell
    public Vector2 spacing = Vector2.zero; // Spacing between each cell
    public Vector2 elementRatio = Vector2.one; // Ratio of width to height
    public enum FixedCount { Rows, Columns };
    public FixedCount fixedCount = FixedCount.Columns;
    [Min(1)]
    public int count = 2; // Define the number of columns


    public override void CalculateLayoutInputHorizontal()
    {
        base.CalculateLayoutInputHorizontal();

        if (fixedCount == FixedCount.Columns)
        {
            float parentWidth = rectTransform.rect.width;
            float cellWidth = (parentWidth - (padding.left + padding.right + (spacing.x * (count - 1)))) / count;

            cellSize.x = cellWidth;
            cellSize.y = cellWidth * elementRatio.y / elementRatio.x;

            int rowCount = Mathf.CeilToInt(rectChildren.Count / (float)count);

            float totalHeight = (cellSize.y * rowCount) + (spacing.y * (rowCount - 1)) + padding.top + padding.bottom;

            SetLayoutInputForAxis(totalHeight, totalHeight, totalHeight, 1);
        }
        else
        {
            float parentHeight = rectTransform.rect.height;
            float cellHeight = (parentHeight - (padding.top + padding.bottom + (spacing.y * (count - 1)))) / count;

            cellSize.y = cellHeight;
            cellSize.x = cellHeight * elementRatio.x / elementRatio.y;

            int colCount = Mathf.CeilToInt(rectChildren.Count / (float)count);

            float totalWidth = (cellSize.x * colCount) + (spacing.x * (colCount - 1)) + padding.left + padding.right;

            SetLayoutInputForAxis(totalWidth, totalWidth, totalWidth, 0);
        }

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
            int row, col;
            if (fixedCount == FixedCount.Columns)
            {
                row = i / count;
                col = i % count;
            }
            else
            {
                row = i % count;
                col = i / count;
            }
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
