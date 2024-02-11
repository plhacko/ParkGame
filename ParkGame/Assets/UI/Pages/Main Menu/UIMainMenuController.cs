using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Managers;
using Firebase.Auth;
using Unity.Services.Authentication;
using DG.Tweening;

public class UIMainMenuController : UIPageController
{
    [SerializeField] private Button createMapButton; 
    [SerializeField] private Button hostButton;
    [SerializeField] private TMP_InputField joinCodeInputField;
    [SerializeField] private Button joinButton;
    [SerializeField] private Button aboutButton;
    [SerializeField] private Button signOutButton;
    [SerializeField] private RectTransform headerPanel;
    private Vector2 finalHeaderPanelPos;
    [SerializeField] private RectTransform buttonsPanel;
    private Vector2 finalButtonsPanelPos;
    [SerializeField] private UIPage welcomePage;
    [SerializeField] private UIPage lobbyPage;
    [SerializeField] private UIPage prepareGamePage;
    [SerializeField] private UIPage aboutPage;
    [SerializeField] private string createMapMenuSceneName; 
    
    
    private void Start()
    {
        createMapButton.onClick.AddListener(Create);
        hostButton.onClick.AddListener(Host);
        joinButton.onClick.AddListener(Join);
        aboutButton.onClick.AddListener(About);
        signOutButton.onClick.AddListener(SignOut);

        // TODO remove this
        joinCodeInputField.text = PlayerPrefs.GetString("DebugRoomCode", "");

        finalHeaderPanelPos = headerPanel.anchoredPosition;
        finalButtonsPanelPos = buttonsPanel.anchoredPosition;

        AudioManager.Instance.notificationsSource = Camera.main.GetComponent<AudioSource>();

        Prepare();
    }

    private void OnDestroy()
    {
    }

    public override void OnEnter()
    {
        enableButtons(true);

        float animationLength = .5f;
        headerPanel.DOAnchorPosY(finalHeaderPanelPos.y, animationLength).SetEase(Ease.OutExpo);
        buttonsPanel.DOAnchorPosY(finalButtonsPanelPos.y, animationLength).SetEase(Ease.OutExpo);
    }

    public override void OnExit()
    {
        Prepare();
    }
    
    private void Create()
    {
        // click might not be heard here because of the loading!
        // AudioManager.Instance.PlayClickSFX(); nope. tested it. didn't help. idk... so still hotcakefix. fcuk it
        SceneManager.LoadScene(createMapMenuSceneName, LoadSceneMode.Single);
    }

    private void Host()
    {
        AudioManager.Instance.PlayClickSFX();
        UIController.Singleton.PushUIPage(prepareGamePage);
    }

    private async void Join()
    {
        enableButtons(false);
        AudioManager.Instance.PlayClickSFX();

        // TODO notify when unsuccessful join
        bool success = await LobbyManager.Singleton.JoinLobbyByCode(joinCodeInputField.text.ToUpper());
        if (success)
            UIController.Singleton.PushUIPage(lobbyPage);

        enableButtons(true);
    }

    private void About()
    {
        AudioManager.Instance.PlayClickSFX();
        UIController.Singleton.PushUIPage(aboutPage);
    }

    private void SignOut()
    {
        AudioManager.Instance.PlayClickSFX();
        enableButtons(false);
        ServicesManager.Instance.SignOutFromFirebase();
        UIController.Singleton.PopUIPage();
        UIController.Singleton.PushUIPage(welcomePage);
    }

    private void enableButtons(bool isInteractable)
    {
        createMapButton.interactable = isInteractable;
        hostButton.interactable = isInteractable;
        joinCodeInputField.interactable = isInteractable;
        joinButton.interactable = isInteractable;
        signOutButton.interactable = isInteractable;
        aboutButton.interactable = isInteractable;
    }

    private void Prepare()
    {
        var headerPivot = headerPanel.pivot.y;
        var headerHeight = headerPanel.rect.height;

        var headerOffset = headerHeight * headerPivot;

        headerPanel.anchoredPosition = new Vector2(0, headerOffset);

        // Get the height of the buttonsPanel
        float buttonsPanelHeight = buttonsPanel.rect.height;
        var entirePanel = GetComponent<RectTransform>();
        var btnBottomPadding = buttonsPanel.anchorMin.y * entirePanel.rect.height; 

        // Set the y-coordinate of the buttonsPanel's anchoredPosition to be the negative of its own height
        buttonsPanel.anchoredPosition = new Vector2(0, -buttonsPanelHeight - btnBottomPadding);
    }
}
