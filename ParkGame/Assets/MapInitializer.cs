using FreeDraw;
using Managers;
using Unity.Netcode;
using UnityEngine;

public class MapInitializer : MonoBehaviour
{
    [SerializeField] private CreateMapWithOverlay mapCreator;

    private PlayerManager playerManager;
    
    void Start()
    {
        if (mapCreator == null || LobbyManager.Singleton == null || LobbyManager.Singleton.MapData == null)
        {
            return;
        }

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
        mapCreator.GetComponent<Drawable>().enabled = false;
        mapCreator.CreateTilemapFromFetchedMap(LobbyManager.Singleton.MapData);
        
        var baseMap = mapCreator.BaseMap;
        baseMap.OnMapLoaded += () =>
        {
            Debug.Log("Map loaded");
            mapCreator.FitCameraToMap();
        };  
    }
}
