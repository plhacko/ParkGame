using UnityEngine;
using UnityEngine.UI;
using Managers;

public class UIPrepareGameMenuController : UIPageController
{
    [SerializeField] private Button backButton;
    [SerializeField] private MapPicker mapPicker;
    [SerializeField] private Button createButton;
    [SerializeField] private UIPage lobbyPage;
    [SerializeField] private UIPage mainMenuPage;
    private bool processing = false;
   
    void Start()
    {
        backButton.onClick.AddListener(Back);
        createButton.onClick.AddListener(Create);
        setInteractable(false);
    }

    private void Update()
    {
        setInteractable(mapPicker.IsInitialized() && !processing);
    }

    public override async void OnEnter()
    {
        setInteractable(false);
        mapPicker.SetInteractable(false);
        processing = false;
        await mapPicker.DownloadMaps();
    }

    public override void OnExit()
    {
        mapPicker.DeleteMaps();
    }

    private void Back()
    {
        AudioManager.Instance.PlayClickSFX();
        UIController.Singleton.PushUIPage(mainMenuPage);
    }

    private void setInteractable(bool interactable)
    {
        backButton.interactable = interactable;
        createButton.interactable = interactable && mapPicker.MapDatas.Count > 0;
    }

    private async void Create()
    {
        AudioManager.Instance.PlayClickSFX();
        setInteractable(false);
        mapPicker.SetInteractable(false);
        
        MapData mapData = mapPicker.GetCurrentMapData();
        
        processing = true;
        
        bool success = await LobbyManager.Singleton.CreateLobbyForMap(mapData);
        // TODO notify when unsuccessful create
        if (success)
        {
            UIController.Singleton.PushUIPage(lobbyPage);
        }
        else 
        {
            Debug.LogWarning("Failed to create lobby");
            setInteractable(true);
        }

        processing = false;
    }
}
