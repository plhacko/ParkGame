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

    public string Winner { get; private set;} = "No one";
    
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
                    break;
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
            if (key == KeyCode.T) { playerController.Gather(); }
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

    public void CommandGather()
    {
        Movement(KeyCode.T);
    }

    public void CommandFallback()
    {
        Formation(KeyCode.O);
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
        Camera.main.InGameZoomTo(4);
    }

    public void ShowFullMap()
    {
        Map.MapCreator.FitCameraToBaseMap();
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

        Camera.main.InGameZoomTo(Camera.main.orthographicSize + delta); 
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
