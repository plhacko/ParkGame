using UnityEngine;
using UnityEngine.UI;
using Managers;

public class UIPrepareGameMenuController : UIPageController
{
    [SerializeField] private Button backButton;
    [SerializeField] private MapPicker mapPicker;
    [SerializeField] private Button createButton;
    [SerializeField] private UIPage lobbyPage;
    private bool processing = false;
   
    void Start()
    {
        backButton.onClick.AddListener(Back);
        createButton.onClick.AddListener(Create);
    }

    private void Update()
    {
        setInteractable(mapPicker.IsInitialized() && !processing);
    }

    public override async void OnEnter()
    {
        setInteractable(false);
        processing = false;
        await mapPicker.DownloadMaps();
    }

    public override void OnExit()
    {
    }

    private void Back()
    {
        UIController.Singleton.PopUIPage();
    }

    private void setInteractable(bool interactable)
    {
        backButton.interactable = interactable;
        createButton.interactable = interactable && mapPicker.MapDatas.Count > 0;
    }

    private async void Create()
    {
        setInteractable(false);
        
        MapData mapData = mapPicker.GetCurrentMapData();
        
        processing = true;
        
        bool success = await LobbyManager.Singleton.CreateLobbyForMap(mapData);
        // TODO notify when unsuccessful create
        if (success)
        {
            UIController.Singleton.PushUIPage(lobbyPage);
        }

        processing = false;
    }
}
