using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class PanelCameraController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler
{
    private Camera mainCamera;
    private Vector3? lastDragPosition;
    private bool isDragging = false;
    private bool isZooming = false;

    private float maxSize;
    private const float minSize = 0.5f;
    private const float zoomSpeed = 0.5f;
    [SerializeField] private float minDragDistance = 1f;
    private void Start()
    {
        mainCamera = Camera.main;
        maxSize = mainCamera.orthographicSize;
    }

    private void Update()
    {
        // Scroll wheel zoom in/out
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (isDragging)
        {
            Vector3 input = Input.touchCount == 1 ? Input.GetTouch(0).position : Input.mousePosition;

            Vector3 currentDragPosition = mainCamera.ScreenToWorldPoint(input);
            Vector3 delta = lastDragPosition.Value - currentDragPosition;
            mainCamera.transform.Translate(delta);
            lastDragPosition = currentDragPosition;
        }
        else if (isZooming || scroll != 0.0f)
        {
            float delta = scroll;
            if (Input.touchCount >= 2)
            {
                Touch touchZero = Input.GetTouch(0);
                Touch touchOne = Input.GetTouch(1);

                Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

                float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                float currentMagnitude = (touchZero.position - touchOne.position).magnitude;

                delta = currentMagnitude - prevMagnitude;
            }

            mainCamera.orthographicSize = Mathf.Clamp(mainCamera.orthographicSize - delta * zoomSpeed, minSize, maxSize);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Check if the user began dragging on the panel
        if (eventData.pointerCurrentRaycast.gameObject != gameObject)
        {
            return;
        }

        // Check if only one finger is used or if the user is using the mouse
        if (Input.touchCount != 1 && !Input.GetMouseButton(0))
        {
            return;
        }

        lastDragPosition = mainCamera.ScreenToWorldPoint(eventData.position);
    }

    public void OnDrag(PointerEventData eventData)
    {   
        if (lastDragPosition == null)
        {
            return;
        }

        if (!isDragging)
        {
            Vector3 currentDragPosition = mainCamera.ScreenToWorldPoint(eventData.position);
            isDragging = Vector3.Distance(currentDragPosition, lastDragPosition.Value) > minDragDistance && !isZooming;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        lastDragPosition = null;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.pointerCurrentRaycast.gameObject != gameObject)
        {
           return;
        }

        if (Input.touchCount == 2 && !isDragging)
        {
            isZooming = true;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log("Pointer up");

        if (isZooming)
        {
            isZooming = Input.touchCount >= 2;
        }
    }
}
