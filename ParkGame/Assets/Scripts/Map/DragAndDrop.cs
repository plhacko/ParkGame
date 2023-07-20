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

public class DragAndDrop : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    [SerializeField] private Canvas canvas;
    public Drawable mapDrawable;
    public GameObject itemSlot;

    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("OnBeginDrag");
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.7f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("OnDrag");
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("OnEndDrag");
        mapDrawable.enabled = true;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1.0f;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("OnPointerDown");
        mapDrawable.enabled = false;
    }

}
