using System;
using FreeDraw;
using Managers;
using Unity.Netcode;
using UnityEngine;

public class Map : MonoBehaviour
{
    [SerializeField] private CreateMapWithOverlay mapCreator;
    public static CreateMapWithOverlay MapCreator => MapCreator;
    public static GameObject GPSMap { get; private set; } = null;
    public static GameObject GridMap { get; private set; } = null;
    public static Bounds MapBounds { get; private set; } = new Bounds();

    private PlayerManager playerManager;
        
    public event Action OnMapLoaded = null;
    
    void Awake()
    {
        if (mapCreator == null || LobbyManager.Singleton == null || LobbyManager.Singleton.MapData == null)
        {
            return;
        }

        mapCreator.GetComponent<Drawable>().enabled = false;
        playerManager = FindObjectOfType<PlayerManager>();
    }

    public void LoadMap()
    {
        mapCreator.CreateTilemapFromFetchedMap(LobbyManager.Singleton.MapData);
        GPSMap = mapCreator.BaseMap.gameObject;
        GridMap = GetComponentInChildren<Grid>().gameObject;
        MapBounds = GPSMap.GetComponent<SpriteRenderer>().bounds;
        OnMapLoaded?.Invoke();
    }
}
