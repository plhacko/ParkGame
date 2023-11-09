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
    private bool processing = false;
    private ColorBlock defaultColors;

    private void Start()
    {
        defaultColors = emailInputField.colors;
        loginButton.onClick.AddListener(Login);
        backButton.onClick.AddListener(Back);
    }

    public override async void OnEnter()
    {
        loginButton.interactable = false;
#if UNITY_EDITOR
    if (!ParrelSync.ClonesManager.IsClone())
    {
        if (FirebaseAuth.DefaultInstance.CurrentUser != null)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            Debug.Log("User already logged in as " + FirebaseAuth.DefaultInstance.CurrentUser.DisplayName);
            UIController.Singleton.PushUIPage(mainMenuPage);
        }
    }  
#else
        if (FirebaseAuth.DefaultInstance.CurrentUser != null)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

            Debug.Log("User already logged in as " + FirebaseAuth.DefaultInstance.CurrentUser.DisplayName);
            UIController.Singleton.PushUIPage(mainMenuPage);
        }
#endif
    }

    public override void OnExit()
    {
    }

    private async void Login()
    {
        processing = true;

#if UNITY_EDITOR
        if (ParrelSync.ClonesManager.IsClone())
        {
            string customArgument = ParrelSync.ClonesManager.GetArgument();
            AuthenticationService.Instance.SwitchProfile($"Clone_{customArgument}_Profile");
        }
#endif
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        AuthResult result = null;
        var auth = FirebaseAuth.DefaultInstance;
        await auth.SignInWithEmailAndPasswordAsync(emailInputField.text, passwordInputField.text).ContinueWith(
            task => 
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");

                    processing = false;
                    return;
                }

                if (task.IsFaulted)
                {
                    AggregateException ex = task.Exception;
                    
                    if (ex != null) {
                        foreach (Exception e in ex.InnerExceptions) {
                            if (e is FirebaseException fbEx)
                            {
                                Debug.LogError("Encountered a FirebaseException:" + fbEx.Message);
                            }
                        }
                    }

                    Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);

                    processing = false;
                    return;
                }

                result = task.Result;
                Debug.LogFormat("User signed in successfully: {0} ({1})", result.User.DisplayName, result.User.UserId);
            }
        );
        
        if (result != null)
        {
            UIController.Singleton.PushUIPage(mainMenuPage);
        }
        else
        {
            Debug.LogError("Login failed");
            var colors = defaultColors;
            colors.normalColor = new Color(1, 0.7f, 0.7f);
            emailInputField.colors = colors;
            passwordInputField.colors = colors;
            AuthenticationService.Instance.SignOut();
        }
    }

    private void Back()
    {
        UIController.Singleton.PopUIPage();
    }

    private void Update()
    {
        loginButton.interactable = !string.IsNullOrEmpty(emailInputField.text) &&
                                   !string.IsNullOrEmpty(passwordInputField.text) &&
                                   !processing;

        backButton.interactable = !processing;
    }

}
