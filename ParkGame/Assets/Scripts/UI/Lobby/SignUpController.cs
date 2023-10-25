using Firebase.Auth;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SignUpController : MonoBehaviour
{
    [SerializeField] private string mainMenuSceneName;
    [SerializeField] private string signInMenuSceneName;
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private TMP_InputField emailInputField;
    [SerializeField] private TMP_InputField passwordInputField;
    [SerializeField] private TMP_InputField confirmPasswordInputField;
    
    [SerializeField] private Button signUpButton;

    private bool isProcessingSignUp = false;
    private ColorBlock defaultColors; 
    
    void Awake()
    {
        defaultColors = confirmPasswordInputField.colors;
        signUpButton.interactable = false;
        signUpButton.onClick.AddListener(signUp);
    }

    private async void signUp()
    {
        isProcessingSignUp = true;
        
        AuthResult result = null;
        var auth = FirebaseAuth.DefaultInstance;
        await auth.CreateUserWithEmailAndPasswordAsync(emailInputField.text, passwordInputField.text).ContinueWith(task => {
            if (task.IsCanceled) {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync was canceled.");
                isProcessingSignUp = false;
                return;
            }
            if (task.IsFaulted) {
                Debug.LogError("CreateUserWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                isProcessingSignUp = false;
                return;
            }
            
            result = task.Result;
            Debug.LogFormat("Firebase user created successfully: {0} ({1})", result.User.DisplayName, result.User.UserId);
        });
        
        if (result != null)
        {
            var userProfile = new UserProfile();
            userProfile.DisplayName = nameInputField.text;
            await result.User.UpdateUserProfileAsync(userProfile);

            AuthResult loginResult = null;
            await auth.SignInWithEmailAndPasswordAsync(emailInputField.text, passwordInputField.text).ContinueWith(task => {
                if (task.IsCanceled) {
                    Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");
                    return;
                }
                if (task.IsFaulted) {
                    Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                    return;
                }

                loginResult = task.Result;
                Debug.LogFormat("User signed in successfully: {0} ({1})", result.User.DisplayName, result.User.UserId);
            });

            if (loginResult != null)
            {
                SceneManager.LoadScene(mainMenuSceneName, LoadSceneMode.Single);  
            }
            else
            {
                SceneManager.LoadScene(signInMenuSceneName, LoadSceneMode.Single);  
            }
        }
    }

    void Update()
    {
        signUpButton.interactable = !string.IsNullOrEmpty(nameInputField.text) &&
                                    !string.IsNullOrEmpty(emailInputField.text) &&
                                    !string.IsNullOrEmpty(passwordInputField.text) &&
                                    !string.IsNullOrEmpty(confirmPasswordInputField.text) &&
                                    passwordInputField.text == confirmPasswordInputField.text &&
                                    !isProcessingSignUp;

        if (passwordInputField.text != confirmPasswordInputField.text)
        {
            var colors = defaultColors;
            colors.normalColor = Color.red;
            colors.selectedColor = new Color(1, 0.7f, 0.7f);
            confirmPasswordInputField.colors = colors;
        }
        else
        {
            confirmPasswordInputField.colors = defaultColors;
        }
    }
}
