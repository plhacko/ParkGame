using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Unity.Services.Authentication;
using Firebase.Auth;
using System;
using Firebase;

public class UILoginController : UIPageController
{
    [SerializeField] private Button backButton;
    [SerializeField] private TMP_InputField emailInputField;
    [SerializeField] private TMP_InputField passwordInputField;
    [SerializeField] private Button loginButton;
    [SerializeField] private UIPage mainMenuPage;
    [SerializeField] private UIPage welcomePage;
    private bool processing = false;
    private ColorBlock defaultColors;

    private void Start()
    {
        defaultColors = emailInputField.colors;
        loginButton.onClick.AddListener(Login);
        backButton.onClick.AddListener(Back);
    }

    public override void OnEnter()
    {
        processing = false;
        loginButton.interactable = false;
    }

    public override void OnExit()
    {
    }

    private async void Login()
    {
        AudioManager.Instance.PlayClickSFX();

        processing = true;

        var result = await ServicesManager.Instance.LogInToFirebase(emailInputField.text, passwordInputField.text);
        
        if (result == ServicesManager.FirebaseAuthServiceError.None)
        {
            UIController.Singleton.PushUIPage(mainMenuPage);
        }
        else
        {
            Debug.LogError("Login failed");
            UIController.Singleton.ShowPopUp("Login failed", "Please check your email and password and try again.", "OK", null);
            var colors = defaultColors;
            colors.normalColor = new Color(1, 0.7f, 0.7f);
            emailInputField.colors = colors;
            passwordInputField.colors = colors;
        }

        processing = false;
    }

    private void Back()
    {
        AudioManager.Instance.PlayClickSFX();
        UIController.Singleton.PushUIPage(welcomePage);
    }

    private void Update()
    {
        loginButton.interactable = !string.IsNullOrEmpty(emailInputField.text) &&
                                   !string.IsNullOrEmpty(passwordInputField.text) &&
                                   !processing;

        backButton.interactable = !processing;
    }

}
