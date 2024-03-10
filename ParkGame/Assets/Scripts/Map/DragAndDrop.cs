using System.Collections;
using System.Collections.Generic;
using FreeDraw;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class DragAndDrop : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    [SerializeField] private Canvas canvas;
    public static bool CanDrag = false;
    public Drawable mapDrawable;
    public ItemSlot itemSlot;
    
    [SerializeField]
    private Tilemap tilemap;
    public Tilemap TilemapProperty
    {
        get => tilemap;
        set
        {
            tilemap = value;
            SetGridCellSize(value);
        }
    }
    private Vector2 gridCellSize;
    private Camera mainCamera;

    private GameObject draggedItem;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;

    public RectTransform foreground;
    public RectTransform background;

    private Vector3? worldPosition = null;
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
        mainCamera = Camera.main;
        
        if (tilemap)
            SetGridCellSize(tilemap);
    }

    private void SetGridCellSize(Tilemap value)
    {
        // component-wise multiplication (needed for grid-snapping)
        gridCellSize = Vector3.Scale(value.cellSize, value.layoutGrid.transform.localScale);
    }
    
    public Vector2 GetTilemapPosition()
    {
        return rectTransform.anchoredPosition;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!CanDrag)
            return;

        InDragAction(true);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!CanDrag)
            return;

        // Convert mouse position to world position
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            rectTransform, eventData.position, mainCamera,out var worldPosition
        );
        // rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        Vector3 snappedWorldPosition = new Vector3(
            Mathf.Round(worldPosition.x / gridCellSize.x) * gridCellSize.x,
            Mathf.Round(worldPosition.y / gridCellSize.y) * gridCellSize.y,
            worldPosition.z
        );
        Vector2 snappedCanvasPosition = RectTransformUtility.WorldToScreenPoint(mainCamera, snappedWorldPosition);
        // Calculate anchored position based on the canvas position
        Vector2 anchoredPosition = snappedCanvasPosition - (Vector2)rectTransform.parent.GetComponent<RectTransform>().position;

        // Set the RectTransform's anchored position to the snapped anchored position
        rectTransform.anchoredPosition = anchoredPosition / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!CanDrag)
            return;

        InDragAction(false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!CanDrag)
            return;

        InDragAction(true);
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        if (!CanDrag)
            return;
            
        InDragAction(false);
    }

    public void InDragAction(bool inDrag)
    {
        if (inDrag)
        {
            itemSlot.ChangeSprite(trashIcon: true);
            if (mapDrawable)
                mapDrawable.SetDrawableState(placingStructure: true);
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 0.7f;
            if (foreground != null && background != null)
                transform.SetParent(foreground);
        }
        else
        {
            itemSlot.ChangeSprite(trashIcon: false);
            if (mapDrawable)
                mapDrawable.SetDrawableState(placingStructure: false);
            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 1.0f;
            if (foreground != null && background != null)
                transform.SetParent(background);
        }
    }

    public void SetWorldPosition(Vector3? position)
    {
        worldPosition = position;
    }

    void Update()
    {

        if (!CanDrag)
        {
            if (worldPosition == null)
            {
                worldPosition = mainCamera.ScreenToWorldPoint(rectTransform.position);
            }
            canvasGroup.alpha = 0.7f;
            rectTransform.position = mainCamera.WorldToScreenPoint(worldPosition.Value);
        }
        else
        {
            canvasGroup.alpha = 1.0f;
            worldPosition = null;
        }
    }
}
