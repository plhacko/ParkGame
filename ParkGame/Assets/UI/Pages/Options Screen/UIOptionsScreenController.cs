using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIOptionsScreenController : UIPageController
{
    [SerializeField] private Button backButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button notificationSoundsToggleButton;
    [SerializeField] private Button soundEffectsToggleButton;
    [SerializeField] private Button howToPlayButton;
    [SerializeField] private UIPage howToPlayPage;
    [SerializeField] private string mainMenuSceneName = "Menu";
    [SerializeField] private DisconnectionHandler disconnectionHandler;
    
    private void Awake()
    {
        backButton.onClick.AddListener(Back);
        quitButton.onClick.AddListener(Quit);
        notificationSoundsToggleButton.onClick.AddListener(ToggleNotifications);
        soundEffectsToggleButton.onClick.AddListener(ToggleSfx);
        howToPlayButton.onClick.AddListener(HowToPlay);        

    }

    public override void OnEnter()
    {}

    public override void OnExit()
    {}

    private void Back()
    {
        UIController.Singleton.PopUIPage();
    }

    private void Quit()
    {
        // todo show do you want to disconnect for host
        disconnectionHandler.DisconnectAndLeave();
    }

    private void ToggleNotifications()
    {
        AudioManager.Instance.ToggleNotificationSound();
        notificationSoundsToggleButton.GetComponent<ToggleSwitchIcon>().Toggle();
    }

    private void ToggleSfx()
    {
        AudioManager.Instance.ToggleSfx();
        soundEffectsToggleButton.GetComponent<ToggleSwitchIcon>().Toggle();
    }

    private void HowToPlay()
    {
        UIController.Singleton.PushUIPage(howToPlayPage);
    }
}
