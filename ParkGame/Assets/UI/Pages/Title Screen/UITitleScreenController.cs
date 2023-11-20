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
        enterButton.interactable = true;
    }

    public override void OnExit()
    {
    }

    private async void Enter()
    {
        enterButton.interactable = false;

        await ServicesManager.Instance.SignUpAndLogInToUnityAuth();

        if (ServicesManager.Instance.IsSignedToFirebase())
        {
            Debug.Log("User already logged in as " + FirebaseAuth.DefaultInstance.CurrentUser.DisplayName);
            UIController.Singleton.PushUIPage(mainMenuPage);
            return;
        }
        else
        {
            UIController.Singleton.PushUIPage(welcomePage);
        }

        enterButton.interactable = true;
    }
}
