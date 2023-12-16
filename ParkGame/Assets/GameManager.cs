using System;
using System.Collections;
using DG.Tweening;
using Managers;
using Player;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour 
{
    public static GameManager Instance { get; private set; }
    [SerializeField] private PlayerManager playerManager;
    public bool FollowCommander { get; private set;} = false;
    [SerializeField] private float followRefreshRate = 0.5f;
    private Coroutine panCoroutine = null;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;   
        }
    }

    private void Start()
    {
        StartCoroutine(FollowGPSPointer());
    }

    IEnumerator FollowGPSPointer()
    {
        while (true)
        {
            if (Map.GPSMap != null)
            {
                PlayerController playerController = playerManager.GetLocalPlayerController();
                if (playerController != null && !playerController.IsLocked)
                {
                    var newPosition = PlayerPointerPlacer.PinPosition;
                    playerController.MoveTowards(newPosition);
                }
            }
            yield return new WaitForSeconds(followRefreshRate);
        }
    }

    private void Update()
    {
        if (FollowCommander) // follow pin instead?
        {
            if (Map.GPSMap != null)
            {
                Camera.main.PointTo(PlayerPointerPlacer.PinPosition);
            }
        }
    }

    void Movement(KeyCode key) {
        PlayerController playerController = playerManager.GetLocalPlayerController();
        
        if (playerController != null && !playerController.IsLocked) {
            if (key == KeyCode.I) { playerController.CommandMovementServerRpc(); }
            if (key == KeyCode.O) { playerController.CommandIdleServerRpc(); }
            if (key == KeyCode.P) { playerController.CommandAttackServerRpc(); }
        }
    }

    void Formation(KeyCode key) {
        PlayerController playerController = playerManager.GetLocalPlayerController();
        
        if (playerController != null && !playerController.IsLocked) {
            playerController.FormatSoldiersServerRpc(key);
        }
    }

    public void FormationBox()
    {
        Formation(KeyCode.R);
    }

    public void FormationCircle()
    {
        Formation(KeyCode.C);
    }

    public void CommandMove()
    {
        Movement(KeyCode.I);
    }

    public void CommandIdle()
    {
        Movement(KeyCode.O);
    }

    public void CommandAttack()
    {
        Movement(KeyCode.P);
    }

    public void Hide()
    {
        foreach (Transform child in Map.GridMap.transform)
        {
            child.gameObject.GetComponent<TilemapRenderer>().enabled = false;
        }
    }

    public void Show()
    {
        foreach (Transform child in Map.GridMap.transform)
        {
            child.gameObject.GetComponent<TilemapRenderer>().enabled = true;
        }
    }

    public void CameraFollowCommander()
    {
        FollowCommander = true;
        Camera.main.ZoomTo(4);
    }

    public void ShowFullMap()
    {
        Map.MapCreator.FitCameraToMap();
    }

    public void Drag(Vector3 direction)
    {
        if (panCoroutine != null)
        {
            StopCoroutine(panCoroutine);
            panCoroutine = null;
        }

        FollowCommander = false;
        Camera.main.PointTo(Camera.main.transform.position + direction);
    }

    public void Zoom(float delta)
    {        
        if (panCoroutine != null)
        {
            StopCoroutine(panCoroutine);
            panCoroutine = null;
        }

        Camera.main.ZoomTo(Camera.main.orthographicSize + delta); 
    }

    public void PanTo(Vector3 position, float duration)
    {
        FollowCommander = false;
        
        if (panCoroutine != null)
        {
            StopCoroutine(panCoroutine);
            panCoroutine = null;
        }

        panCoroutine = StartCoroutine(Camera.main.PanToCoroutine(position, duration));
    }
}

public static class CameraExtensions
{
    public static IEnumerator PanToCoroutine(this Camera camera, Vector3 position, float duration)
    {
        var startPosition = camera.transform.position;
        var endPosition = camera.ClampCameraToMap(position);
        endPosition.z = camera.transform.position.z;

        var elapsed = 0f;

        while (elapsed < duration)
        {
            camera.transform.position = Vector3.Lerp(startPosition, endPosition, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        camera.transform.position = endPosition;
    }

    private static Vector3 ClampCameraToMap(this Camera camera, Vector3 position)
    {
        if (Map.GPSMap != null)
        {
            var mapBounds = Map.MapBounds;
            var cameraBounds = camera.CalculateOrthographicBounds(position);

            // dont allow camera to go outside of map bounds
            if (cameraBounds.min.x < mapBounds.min.x)
            {
                position.x = mapBounds.min.x + cameraBounds.extents.x;
            }
            if (cameraBounds.max.x > mapBounds.max.x)
            {
                position.x = mapBounds.max.x - cameraBounds.extents.x;
            }
            if (cameraBounds.min.y < mapBounds.min.y)
            {
                position.y = mapBounds.min.y + cameraBounds.extents.y;
            }
            if (cameraBounds.max.y > mapBounds.max.y)
            {
                position.y = mapBounds.max.y - cameraBounds.extents.y;
            }
        }

        return position;
    }

    public static void PointTo(this Camera camera, Vector3 position)
    {
        position.z = camera.transform.position.z;

        camera.transform.position = camera.ClampCameraToMap(position);
    }

    public static void ZoomTo(this Camera camera, float size)
    {
        var min = 0.5f;
        var max = Map.GPSMap != null ? camera.MaxOrthographicSizeFor(Map.MapBounds) : float.MaxValue;

         camera.orthographicSize = Mathf.Clamp(size, min, max);

        // check if camera is within map bounds
        if (Map.GPSMap != null)
        {
            var mapBounds = Map.MapBounds;
            var cameraBounds = camera.CalculateOrthographicBounds();

            // dont allow camera to go outside of map bounds
            if (cameraBounds.min.x < mapBounds.min.x)
            {
                camera.transform.position = new Vector3(mapBounds.min.x + cameraBounds.extents.x, camera.transform.position.y, camera.transform.position.z);
            }
            if (cameraBounds.max.x > mapBounds.max.x)
            {
                camera.transform.position = new Vector3(mapBounds.max.x - cameraBounds.extents.x, camera.transform.position.y, camera.transform.position.z);
            }
            if (cameraBounds.min.y < mapBounds.min.y)
            {
                camera.transform.position = new Vector3(camera.transform.position.x, mapBounds.min.y + cameraBounds.extents.y, camera.transform.position.z);
            }
            if (cameraBounds.max.y > mapBounds.max.y)
            {
                camera.transform.position = new Vector3(camera.transform.position.x, mapBounds.max.y - cameraBounds.extents.y, camera.transform.position.z);
            }
        }
    }

    public static Bounds CalculateOrthographicBounds(this Camera camera, Vector3? position = null)
    {
        if (position == null)
        {
            position = camera.transform.position;
        }

        float cameraHeight = camera.orthographicSize * 2;
        Bounds bounds = new Bounds(
            position.Value,
            new Vector3(cameraHeight * camera.aspect, cameraHeight, 0));
        return bounds;
    }

    public static float MaxOrthographicSizeFor(this Camera camera, Bounds bounds)
    {
        float minSide = Mathf.Min(bounds.size.x, bounds.size.y);
        float aspectRatio = camera.aspect;
        float horizontalSize = minSide / aspectRatio / 2;
        float verticalSize = minSide / 2;
        return Mathf.Min(horizontalSize, verticalSize);
    }
}
