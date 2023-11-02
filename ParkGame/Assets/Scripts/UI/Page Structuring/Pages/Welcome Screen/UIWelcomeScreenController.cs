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

        if (FirebaseAuth.DefaultInstance.CurrentUser != null)
        {
            Debug.Log("User already logged in as " + FirebaseAuth.DefaultInstance.CurrentUser.DisplayName);
            UIController.Singleton.PushUIPage(mainMenuPage);
        }

        loginButton.interactable = true;
        signUpButton.interactable = true;
    }

    public override void OnExit()
    {
    }

    // Set name and go to join game menu scene
    private void Login()
    {
        UIController.Singleton.PushUIPage(loginPage);
    }
    
    private void SignUp()
    {
        UIController.Singleton.PushUIPage(signUpPage);
    }
}
