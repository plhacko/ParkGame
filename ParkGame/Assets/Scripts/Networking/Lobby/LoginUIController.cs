using Networking;
using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginUIController : MonoBehaviour
{
    [SerializeField] private Button loginButton;    
    [SerializeField] private TMP_InputField nameInputField;
    
    async void Awake()
    {
        loginButton.interactable = false;
        nameInputField.interactable = false;
        
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        
        loginButton.interactable = true;
        nameInputField.interactable = true;
        loginButton.onClick.AddListener(login);
    }

    private void login()
    {
        SessionManager.Singleton.SetName(nameInputField.text);
        SceneManager.LoadScene("JoinGameMenu", LoadSceneMode.Single);
    }
    
    void Update()
    {
        loginButton.interactable = !string.IsNullOrEmpty(nameInputField.text);
    }
}
