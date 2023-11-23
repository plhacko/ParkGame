using FreeDraw;
using Managers;
using Unity.Netcode;
using UnityEngine;

public class MapInitializer : MonoBehaviour
{
    [SerializeField] private CreateMapWithOverlay mapCreator;
    public GameObject mapSprite { get; private set; } = null; 

    private PlayerManager playerManager;
    
    void Awake()
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
        mapCreator.FitCameraToMap();
    }
}
