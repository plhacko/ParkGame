using UnityEngine;
using UnityEngine.UI;
using Managers;

public class UITitleScreenController : UIPageController
{
    [SerializeField] private Button enterButton;
    [SerializeField] private UIPage welcomePage;
    
    private void Start()
    {
        enterButton.onClick.AddListener(Enter);
    }

    public override void OnEnter()
    {
    }

    public override void OnExit()
    {
    }

    private async void Enter()
    {
        await LobbyManager.Singleton.UnityServicesInitializeTask;

        UIController.Singleton.PushUIPage(welcomePage);
    }
}
