using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class MapCameraHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler
{
    private Camera mainCamera;
    private MapDisplayer mapDisplayer;
    private Image panelImageRegister;
    private bool active = false;
    private bool isDragging = false;
    private bool isZooming = false; 
    private Vector3? startDragPosition;
    private const float zoomSpeed = 5f;
    void Awake()
    {
        mainCamera = Camera.main;
        mapDisplayer = FindObjectOfType<MapDisplayer>();
        panelImageRegister = GetComponent<Image>();
    }

    // Start is called before the first frame update
    void Start()
    {

    }


    public void Toggle()
    {
        active = !active;
        panelImageRegister.raycastTarget = active;
    }

    public void Activate()
    {
        active = true;
        panelImageRegister.raycastTarget = true;
    }

    public void Deactivate()
    {
        active = false;
        panelImageRegister.raycastTarget = false;
    }

    // Update is called once per frame
    void Update()
    {

        if (!active)
        {
            return;
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (isDragging)
        {
            Vector3 input = Input.touchCount == 1 ? Input.GetTouch(0).position : Input.mousePosition;
            Vector3 currentDragPosition = mainCamera.ScreenToWorldPoint(input);
            Vector3 direction = startDragPosition.Value - currentDragPosition;

            var cameraViewBounds = mainCamera.CalculateOrthographicBounds(Vector3.zero);
            mainCamera.PointToInBounds(mainCamera.transform.position + direction, cameraViewBounds);
        }
        else if (isZooming || scroll != 0.0f)
        {
            float delta = scroll;
            if (Input.touchCount >= 2)
            {
                Touch touchZero = Input.GetTouch(0);
                Touch touchOne = Input.GetTouch(1);

                Vector2 touchZeroPos = mainCamera.ScreenToViewportPoint(touchZero.position);
                Vector2 touchOnePos = mainCamera.ScreenToViewportPoint(touchOne.position);

                Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
                Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

                touchZeroPrevPos = mainCamera.ScreenToViewportPoint(touchZeroPrevPos);
                touchOnePrevPos = mainCamera.ScreenToViewportPoint(touchOnePrevPos);

                float prevMagnitude = (touchZeroPrevPos - touchOnePrevPos).magnitude;
                float currentMagnitude = (touchZeroPos - touchOnePos).magnitude;

                delta = prevMagnitude - currentMagnitude;
            }

            var mapBounds = mapDisplayer.GetComponent<SpriteRenderer>().bounds;
            var maxOrthographicSize = mainCamera.MaxOrthographicSizeFor(mapBounds, false);
            mainCamera.orthographicSize = Mathf.Clamp(mainCamera.orthographicSize + delta * zoomSpeed, 0.5f, maxOrthographicSize);
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
