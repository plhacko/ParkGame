using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Services.Authentication;

public class UILoginController : UIPageController
{
    [SerializeField] private TMP_InputField usernameInputField;
    [SerializeField] private Button loginButton;
    [SerializeField] private UIPage mainMenuPage;

    private void Start()
    {
        loginButton.onClick.AddListener(Login);
        usernameInputField.onEndEdit.AddListener(delegate { OnInputFieldValueChanged(usernameInputField.text); });
        usernameInputField.onValueChanged.AddListener(delegate { OnInputFieldValueChanged(usernameInputField.text); });

    }

    public override void OnEnter()
    {
        loginButton.interactable = false;
        usernameInputField.text = "";
    }

    public override void OnExit()
    {
    }

    public void OnInputFieldValueChanged(string text)
    {
        loginButton.interactable = !string.IsNullOrEmpty(text);
    }

    public async void Login()
    {
        loginButton.interactable = false;
        
        AuthenticationService.Instance.SignedIn += () => UIController.Singleton.PushUIPage(mainMenuPage);
#if UNITY_EDITOR
        if (ParrelSync.ClonesManager.IsClone())
        {
            string customArgument = ParrelSync.ClonesManager.GetArgument();
            AuthenticationService.Instance.SwitchProfile($"Clone_{customArgument}_Profile");
        }
#endif
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        Debug.Log("Signed in anonymously" + AuthenticationService.Instance.PlayerId + " as " + usernameInputField.text);

        // TODO change this to a more secure way of storing the player name
        PlayerPrefs.SetString("PlayerName", usernameInputField.text);
    }

}
