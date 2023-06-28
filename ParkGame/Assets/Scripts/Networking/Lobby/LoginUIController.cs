using Networking;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/*
 * This class is responsible for the UI of the Login menu.
 * Player can choose a name here and go into join game menu scene
 */
public class LoginUIController : MonoBehaviour
{
    [SerializeField] private Button loginButton;    
    [SerializeField] private TMP_InputField nameInputField;
    
    async void Awake()
    {
        loginButton.onClick.AddListener(login);
        
        loginButton.interactable = false;
        nameInputField.interactable = false;
        
        // Initialize Unity Services and sign in anonymously
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        
        loginButton.interactable = true;
        nameInputField.interactable = true;
    }

    // Set name and go to join game menu scene
    private void login()
    {
        SessionManager.Singleton.SetName(nameInputField.text);
        SceneManager.LoadScene("JoinGameMenu", LoadSceneMode.Single);
    }
    
    // Disable login button until the player has entered at least one character of their name
    void Update()
    {
        loginButton.interactable = !string.IsNullOrEmpty(nameInputField.text);
    }
}
