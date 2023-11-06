using System.Threading.Tasks;
using Managers;
using UnityEngine;

public class MapInitializer : MonoBehaviour
{
    [SerializeField] private CreateMapWithOverlay mapCreator;

    void Start()
    {
        if (mapCreator == null || LobbyManager.Singleton == null || LobbyManager.Singleton.MapData == null)
        {
            return;
        }
        mapCreator.CreateTilemapFromFetchedMap(LobbyManager.Singleton.MapData);
        var baseMap = mapCreator.BaseMap;
        baseMap.OnMapLoaded += () =>
        {
            Debug.Log("Map loaded");
            mapCreator.FitCamera();
        };  
    }



}
