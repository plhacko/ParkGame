// using System;
// using System.Collections;
// using System.Collections.Generic;
// using FreeDraw;
// using UnityEngine;
//
// using UnityEngine.EventSystems;
//
// public class DragAndDrop : MonoBehaviour
// {
//     private Vector3 mousePositionOffset;
//     public Camera camera;
//     public Drawable mapDrawable;
//     private CanvasGroup canvasGroup;
//
//
//     private void Awake()
//     {
//         canvasGroup = GetComponent<CanvasGroup>();
//     }
//
//     private Vector3 GetMouseWorldPosition()
//     {
//         return camera.ScreenToWorldPoint(Input.mousePosition);
//     }
//
//
//     private void OnMouseDown()
//     {
//         Debug.Log("Mouse Down");
//         canvasGroup.blocksRaycasts = false;
//         canvasGroup.alpha = 0.7f;
//         mousePositionOffset = gameObject.transform.position - GetMouseWorldPosition();
//         mapDrawable.enabled = false;
//     }
//
//     private void OnMouseDrag()
//     {
//         transform.position = GetMouseWorldPosition() + mousePositionOffset;
//     }
//
//     private void OnMouseUp()
//     {
//         mapDrawable.enabled = true;
//         canvasGroup.blocksRaycasts = true;
//         canvasGroup.alpha = 1.0f;
//
//     }
// }

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
        InDragAction(true);
    }

    public void OnDrag(PointerEventData eventData)
    {
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
        InDragAction(false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        InDragAction(true);
    }
    public void OnPointerUp(PointerEventData eventData)
    {
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
        }
        else
        {
            itemSlot.ChangeSprite(trashIcon: false);
            if (mapDrawable)
                mapDrawable.SetDrawableState(placingStructure: false);
            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 1.0f;
        }
    }

}
