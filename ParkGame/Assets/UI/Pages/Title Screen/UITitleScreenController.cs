using UnityEngine;
using UnityEngine.UI;
using Managers;
using Firebase.Auth;
using Unity.Services.Authentication;

public class UITitleScreenController : UIPageController
{
    [SerializeField] private Button enterButton;
    [SerializeField] private UIPage welcomePage;
    [SerializeField] private UIPage mainMenuPage;
    
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

#if UNITY_EDITOR
    if (!ParrelSync.ClonesManager.IsClone())
    {
        if (FirebaseAuth.DefaultInstance.CurrentUser != null)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            Debug.Log("User already logged in as " + FirebaseAuth.DefaultInstance.CurrentUser.DisplayName);
            UIController.Singleton.PushUIPage(mainMenuPage);
            return;
        }
    }  
#else
        if (FirebaseAuth.DefaultInstance.CurrentUser != null)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            Debug.Log("User already logged in as " + FirebaseAuth.DefaultInstance.CurrentUser.DisplayName);
            UIController.Singleton.PushUIPage(mainMenuPage);
            return;
        }
#endif

        UIController.Singleton.PushUIPage(welcomePage);
    }
}
