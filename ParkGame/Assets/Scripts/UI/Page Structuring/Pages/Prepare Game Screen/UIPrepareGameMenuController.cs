using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Managers;
using UnityEngine.Events;

public class UIPrepareGameMenuController : MonoBehaviour
{
    [SerializeField] private string lobbySceneName;

    [SerializeField] private Button backButton;
    [SerializeField] private UnityEvent onBackPressed;
   
    [SerializeField] private MapPicker mapPicker;
    
    [SerializeField] private Button createButton;
    [SerializeField] private UnityEvent onCreatePressed;
    [SerializeField] private UnityEvent onCreateGame;

    void Start()
    {
        backButton.onClick.AddListener(onBackPressed.Invoke);

        createButton.onClick.AddListener(CreateLobby);
        createButton.onClick.AddListener(onCreatePressed.Invoke);

        setInteractable(false);
    }

    private void Update()
    {
        setInteractable(mapPicker.IsInitialized());
    }

    private void setInteractable(bool interactable)
    {
        backButton.interactable = interactable;
        createButton.interactable = interactable && mapPicker.MapDatas.Count > 0;
    }

    private async void CreateLobby()
    {
        MapData mapData = mapPicker.GetCurrentMapData();
        bool success = await LobbyManager.Singleton.CreateLobbyForMap(mapData);
        if (success)
            onCreateGame.Invoke();
    }
}
