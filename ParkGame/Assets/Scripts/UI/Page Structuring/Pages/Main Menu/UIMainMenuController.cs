using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Managers;

public class UIMainMenuController : UIPageController
{
    [SerializeField] private string createMapMenuSceneName; 
    [SerializeField] private Button createMapButton; 
    [SerializeField] private UIPage prepareGamePage;
    [SerializeField] private Button hostButton;
    [SerializeField] private UIPage lobbyPage;
    [SerializeField] private TMP_InputField joinCodeInputField;
    [SerializeField] private Button joinButton;
    
    
    private void Start()
    {
        createMapButton.onClick.AddListener(Create);
        hostButton.onClick.AddListener(Host);
        joinButton.onClick.AddListener(Join);

        // TODO remove this
        joinCodeInputField.text = PlayerPrefs.GetString("DebugRoomCode", "");
    }

    private void OnDestroy()
    {
    }

    public override void OnEnter()
    {
        enableButtons(true);
    }

    public override void OnExit()
    {
    }
    
    private void Create()
    {
        SceneManager.LoadScene(createMapMenuSceneName, LoadSceneMode.Single);
    }

    private void Host()
    {
        UIController.Singleton.PushUIPage(prepareGamePage);
    }

    private async void Join()
    {
        enableButtons(false);
        // TODO notify when unsuccessful join
        bool success = await LobbyManager.Singleton.JoinLobbyByCode(joinCodeInputField.text.ToUpper());
        if (success)
            UIController.Singleton.PushUIPage(lobbyPage);

        enableButtons(true);
    }

    private void enableButtons(bool isInteractable)
    {
        createMapButton.interactable = isInteractable;
        hostButton.interactable = isInteractable;
        joinCodeInputField.interactable = isInteractable;
        joinButton.interactable = isInteractable;
    }
}
