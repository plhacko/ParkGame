using System;
using Firebase;
using Firebase.Auth;
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
namespace UI.Lobby
{
    public class LoginUIController : MonoBehaviour
    {
        [SerializeField] private Button loginButton;    
        [SerializeField] private TMP_InputField emailInputField;
        [SerializeField] private TMP_InputField passwordInputField;
        [SerializeField] private string mainMenuSceneName;
    
        private bool isProcessingSignIn = false;
        private ColorBlock defaultColors; 
        
        void Awake()
        {
            loginButton.onClick.AddListener(login);
            loginButton.interactable = false;
            defaultColors = emailInputField.colors;
        }

        // Set name and go to join game menu scene
        private async void login()
        {
            isProcessingSignIn = true;
            
            AuthResult result = null;
            var auth = FirebaseAuth.DefaultInstance;
            await auth.SignInWithEmailAndPasswordAsync(emailInputField.text, passwordInputField.text).ContinueWith(task => {
                if (task.IsCanceled) {
                    Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");
                    isProcessingSignIn = false;
                    return;
                }
                if (task.IsFaulted) {
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
                    isProcessingSignIn = false;
                    return;
                }

                result = task.Result;
                Debug.LogFormat("User signed in successfully: {0} ({1})", result.User.DisplayName, result.User.UserId);
            });
            
            if (result != null)
            {
                SceneManager.LoadScene(mainMenuSceneName, LoadSceneMode.Single);
            }
            else
            {
                var colors = defaultColors;
                colors.normalColor = new Color(1, 0.7f, 0.7f);
                emailInputField.colors = colors;
                passwordInputField.colors = colors;
            }
        }
        
        // bool checkError(AggregateException exception, int firebaseExceptionCode)
        // {
        //     Firebase.FirebaseException fbEx = null;
        //     foreach (Exception e in exception.Flatten().InnerExceptions)
        //     {
        //         fbEx = e as Firebase.FirebaseException;
        //         if (fbEx != null)
        //             break;
        //     }
        //
        //     if (fbEx != null)
        //     {
        //         if (fbEx.ErrorCode == firebaseExceptionCode)
        //         {
        //             return true;
        //         }
        //         else
        //         {
        //             return false;
        //         }
        //     }
        //     return false;
        // }
    
        // Disable login button until the player has entered at least one character of their email and password
        void Update()
        {
            loginButton.interactable = !string.IsNullOrEmpty(emailInputField.text) &&
                                       !string.IsNullOrEmpty(passwordInputField.text) &&
                                       !isProcessingSignIn;
        }
    }
}
