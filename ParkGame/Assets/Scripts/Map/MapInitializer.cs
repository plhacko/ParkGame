using FreeDraw;
using Managers;
using Unity.Netcode;
using UnityEngine;

public class MapInitializer : MonoBehaviour
{
    [SerializeField] private CreateMapWithOverlay mapCreator;
    public GameObject GPSMap { get; private set; } = null;
    public GameObject GridMap { get; private set; } = null;

    private PlayerManager playerManager;
    
    void Awake()
    {
        if (mapCreator == null || LobbyManager.Singleton == null || LobbyManager.Singleton.MapData == null)
        {
            return;
        }

        mapCreator.GetComponent<Drawable>().enabled = false;
        playerManager = FindObjectOfType<PlayerManager>();
        
        if (NetworkManager.Singleton.IsHost)
        {
            playerManager.OnAllPlayersSceneLoaded += loadMap;   
        }
        else
        {
            loadMap();
        }
    }

    private void OnDestroy()
    {
        if (playerManager)
        {
            playerManager.OnAllPlayersSceneLoaded -= loadMap;   
        }
    }

    private void loadMap()
    {
        mapCreator.CreateTilemapFromFetchedMap(LobbyManager.Singleton.MapData);
        GPSMap = mapCreator.BaseMap.gameObject;
        GridMap = GetComponentInChildren<Grid>().gameObject;
        mapCreator.FitCameraToMap();
    }
}
