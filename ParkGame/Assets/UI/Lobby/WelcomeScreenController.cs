using Firebase.Auth;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WelcomeScreenController : MonoBehaviour
{
    [SerializeField] private Button loginButton;    
    [SerializeField] private Button signUpButton;

    [SerializeField] private string loginSceneName;
    [SerializeField] private string signUpSceneName; 

    [SerializeField] private string mainMenuSceneName;
    
    async void Awake()
    {
        loginButton.onClick.AddListener(login);
        loginButton.interactable = false;
        
        signUpButton.onClick.AddListener(signUp);
        signUpButton.interactable = false;
        
        // Initialize Unity Services and sign in anonymously
        await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();   
        }

        if (FirebaseAuth.DefaultInstance.CurrentUser != null)
        {
            SceneManager.LoadScene(mainMenuSceneName, LoadSceneMode.Single);
            return;
        }
        
        loginButton.interactable = true;
        signUpButton.interactable = true;
    }

    // Set name and go to join game menu scene
    private void login()
    {
        SceneManager.LoadScene(loginSceneName, LoadSceneMode.Single);
    }
    
    private void signUp()
    {
        SceneManager.LoadScene(signUpSceneName, LoadSceneMode.Single);
    }
}
