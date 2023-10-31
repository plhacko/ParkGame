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
        loginButton.interactable = false;
        
        signUpButton.onClick.AddListener(SignUp);
        signUpButton.interactable = false;

        // Already logged in
        if (FirebaseAuth.DefaultInstance.CurrentUser != null)
        {
            UIController.Singleton.PushUIPage(mainMenuPage);
        }
        
        loginButton.interactable = true;
        signUpButton.interactable = true;
    }

    public override void OnEnter()
    {
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
