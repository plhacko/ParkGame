using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using FMODUnity;
using Mapbox.Map;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class PanelCameraController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler
{
    private SpriteRenderer GPSMap = null;
    private Bounds mapBounds;
    private Camera mainCamera;
    private Vector3? startDragPosition;
    private bool isDragging = false;
    private bool isZooming = false;

    private float maxSize;
    private const float minSize = 0.5f;
    private const float zoomSpeed = 0.01f;
    private void Start()
    {
        mainCamera = Camera.main;
        maxSize = mainCamera.orthographicSize;
        if (MapInitializer.GPSMap == null || MapInitializer.GPSMap.GetComponent<SpriteRenderer>() == null)
        {
            return;
        }
        GPSMap = MapInitializer.GPSMap.GetComponent<SpriteRenderer>();
        mapBounds = GPSMap.bounds;
    }

    private void Update()
    {
        // Scroll wheel zoom in/out
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (isDragging)
        {
            Vector3 input = Input.touchCount == 1 ? Input.GetTouch(0).position : Input.mousePosition;
            Vector3 currentDragPosition = mainCamera.ScreenToWorldPoint(input);
            Vector3 direction = startDragPosition.Value - currentDragPosition;

            // Check if the camera is within the map bounds
            Vector3 newPosition = mainCamera.transform.position + direction;
            if (GPSMap != null)
            {
                newPosition.x = Mathf.Clamp(newPosition.x, mapBounds.min.x, mapBounds.max.x);
                newPosition.y = Mathf.Clamp(newPosition.y, mapBounds.min.y, mapBounds.max.y);
            }
            mainCamera.transform.position = newPosition;
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
                // Zoom speed is too fast when using two fingers
                delta *= zoomSpeed;
            }

            mainCamera.orthographicSize = Mathf.Clamp(mainCamera.orthographicSize - delta, minSize, maxSize);
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

        startDragPosition = mainCamera.ScreenToWorldPoint(eventData.position);
    }

    public void OnDrag(PointerEventData eventData)
    {   
        if (startDragPosition == null)
        {
            return;
        }

        if (!isDragging)
        {
            isDragging = startDragPosition.HasValue && !isZooming;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        startDragPosition = null;
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
        if (isZooming)
        {
            isZooming = Input.touchCount >= 2;
        }
    }
}
