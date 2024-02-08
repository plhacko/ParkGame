using Firebase.Auth;
using UnityEngine;
using UnityEngine.UI;

public class UIWelcomeScreenController : UIPageController
{
    [SerializeField] private Button signUpButton;
    [SerializeField] private Button loginButton;    
    [SerializeField] private UIPage mainMenuPage;
    [SerializeField] private UIPage loginPage;
    [SerializeField] private UIPage signUpPage;

    private void Start()
    {
        loginButton.onClick.AddListener(Login);    
        signUpButton.onClick.AddListener(SignUp);
    }

    public override void OnEnter()
    {
        loginButton.interactable = false;
        signUpButton.interactable = false;


        loginButton.interactable = true;
        signUpButton.interactable = true;
    }

    public override void OnExit()
    {
    }

    // Set name and go to join game menu scene
    private void Login()
    {
        AudioManager.Instance.PlayClickSFX();
        UIController.Singleton.PushUIPage(loginPage);
    }
    
    private void SignUp()
    {
        AudioManager.Instance.PlayClickSFX();
        UIController.Singleton.PushUIPage(signUpPage);
    }
}
