using System.Collections;
using System.Collections.Generic;
using Managers;
using Player;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameManager : NetworkBehaviour 
{
    public static GameManager Instance { get; private set; }
    [SerializeField] private PlayerManager playerManager;
    private bool followCommander = false;
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
        if (Map.GPSMap != null)
        {
            Camera.main.orthographicSize = Camera.main.MaxOrthographicSizeFor(Map.MapBounds);
        }
    }

    private void Update()
    {
        if (followCommander)
        {
            var clientID = NetworkManager.Singleton.LocalClientId;
            PlayerController playerController = playerManager.GetPlayerController(clientID);
            if (playerController != null)
            {
                Camera.main.PointTo(playerController.transform.position);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void RequestChangeOfFormationServerRpc(ulong clientID, KeyCode key) {
        PlayerController playerController = playerManager.GetPlayerController(clientID);
        
        if (playerController != null) {
            playerController.FormatSoldiersServerRpc(key);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void RequestChangeOfSoldierCommandServerRpc(ulong clientID, KeyCode key) {
        PlayerController playerController = playerManager.GetPlayerController(clientID);

        if (playerController != null) {
            if (key == KeyCode.I) { playerController.CommandMovementServerRpc(); }
            if (key == KeyCode.O) { playerController.CommandIdleServerRpc(); }
            if (key == KeyCode.P) { playerController.CommandAttackServerRpc(); }
        }
    }

    void Movement(KeyCode key) {
        ulong clientID = NetworkManager.Singleton.LocalClientId;
        RequestChangeOfSoldierCommandServerRpc(clientID, key);
    }

    void Formation(KeyCode key) {
        ulong clientID = NetworkManager.Singleton.LocalClientId;
        RequestChangeOfFormationServerRpc(clientID, key);
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
        followCommander = true;
        Camera.main.ZoomTo(3);
    }

    public void ShowFullMap()
    {
        Map.MapCreator.FitCameraToMap();
    }

    public void Drag(Vector3 direction)
    {
        followCommander = false;
        Camera.main.PointTo(Camera.main.transform.position + direction);
    }

    public void Zoom(float delta)
    {        
        Camera.main.ZoomTo(Camera.main.orthographicSize + delta); 
    }
}

public static class CameraExtensions
{
    public static void PointTo(this Camera camera, Vector3 position)
    {
        position.z = Camera.main.transform.position.z;
        
        Camera.main.transform.position = position;

        if (Map.GPSMap != null)
        {   
            var mapBounds = Map.MapBounds;
            var cameraBounds = Camera.main.CalcculateOrthographicBounds();

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

        Camera.main.transform.position = position;
    }

    public static void ZoomTo(this Camera camera, float size)
    {
        var min = 0;
        var max = Map.GPSMap != null ? Camera.main.MaxOrthographicSizeFor(Map.MapBounds) : float.MaxValue;

         Camera.main.orthographicSize = Mathf.Clamp(size, min, max);

        // check if camera is within map bounds
        if (Map.GPSMap != null)
        {
            var mapBounds = Map.MapBounds;
            var cameraBounds = Camera.main.CalcculateOrthographicBounds();

            // dont allow camera to go outside of map bounds
            if (cameraBounds.min.x < mapBounds.min.x)
            {
                Camera.main.transform.position = new Vector3(mapBounds.min.x + cameraBounds.extents.x, Camera.main.transform.position.y, Camera.main.transform.position.z);
            }
            if (cameraBounds.max.x > mapBounds.max.x)
            {
                Camera.main.transform.position = new Vector3(mapBounds.max.x - cameraBounds.extents.x, Camera.main.transform.position.y, Camera.main.transform.position.z);
            }
            if (cameraBounds.min.y < mapBounds.min.y)
            {
                Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, mapBounds.min.y + cameraBounds.extents.y, Camera.main.transform.position.z);
            }
            if (cameraBounds.max.y > mapBounds.max.y)
            {
                Camera.main.transform.position = new Vector3(Camera.main.transform.position.x, mapBounds.max.y - cameraBounds.extents.y, Camera.main.transform.position.z);
            }
        }
    }

    public static Bounds CalcculateOrthographicBounds(this Camera camera)
    {
        float cameraHeight = camera.orthographicSize * 2;
        Bounds bounds = new Bounds(
            camera.transform.position,
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
