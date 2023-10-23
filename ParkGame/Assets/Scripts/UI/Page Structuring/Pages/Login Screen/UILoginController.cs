using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Managers;

public class UILoginController : MonoBehaviour
{
    [SerializeField] private TMP_InputField usernameInputField;
    [SerializeField] private Button loginButton;
    [SerializeField] private UnityEvent onLoginPressed;
    [SerializeField] private UnityEvent onLoginSuccess;

    private void Start()
    {
        loginButton.interactable = false;
        loginButton.onClick.AddListener(onLoginPressed.Invoke);
        loginButton.onClick.AddListener(Login);

        usernameInputField.onEndEdit.AddListener(delegate { OnInputFieldValueChanged(usernameInputField.text); });
        usernameInputField.onValueChanged.AddListener(delegate { OnInputFieldValueChanged(usernameInputField.text); });

    }

    public void OnInputFieldValueChanged(string text)
    {
        loginButton.interactable = !string.IsNullOrEmpty(text);
    }

    public async void Login()
    {
        loginButton.interactable = false;
        
        AuthenticationService.Instance.SignedIn += () => onLoginSuccess.Invoke();
#if UNITY_EDITOR
        if (ParrelSync.ClonesManager.IsClone())
        {
            string customArgument = ParrelSync.ClonesManager.GetArgument();
            AuthenticationService.Instance.SwitchProfile($"Clone_{customArgument}_Profile");
        }
#endif
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        Debug.Log("Signed in anonymously" + AuthenticationService.Instance.PlayerId + " as " + usernameInputField.text);

        PlayerPrefs.SetString("PlayerName", usernameInputField.text);

        loginButton.interactable = true;
    }

}
