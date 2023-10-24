using UnityEngine;
using UnityEngine.UI;
using Managers;

public class UITitleScreenController : UIPageController
{
    [SerializeField] private Button enterButton;
    [SerializeField] private UIPage loginPage;
    
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
        await SessionManager.Singleton.UnityServicesInitializationTask;

        UIController.Singleton.PushUIPage(loginPage);
    }
}
