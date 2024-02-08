using UnityEngine;
using UnityEngine.UI;
using Firebase.Auth;
using TMPro;
using DG.Tweening;

public class UITitleScreenController : UIPageController
{
    [SerializeField] private TextMeshProUGUI pressEnterText;
    [SerializeField] private Button enterButton;
    [SerializeField] private UIPage welcomePage;
    [SerializeField] private UIPage mainMenuPage;
    
    private void Start()
    {
        enterButton.onClick.AddListener(Enter);
        pressEnterText.transform.DOPunchPosition(new Vector3(0, 10, 0), 0.75f, 0, 0.5f).SetLoops(-1);
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
        AudioManager.Instance.PlayClickSFX();

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
