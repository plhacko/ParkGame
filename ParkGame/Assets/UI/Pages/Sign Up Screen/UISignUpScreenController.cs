using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Firebase.Auth;
using Unity.Services.Authentication;

public class UISignUpScreenController : UIPageController 
{
    [SerializeField] private Button backButton;
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private TMP_InputField emailInputField;
    [SerializeField] private TMP_InputField passwordInputField;
    [SerializeField] private TMP_InputField confirmPasswordInputField;
    [SerializeField] private Button signUpButton;
    [SerializeField] private UIPage mainMenuPage;
    [SerializeField] private UIPage loginPage;
    [SerializeField] private UIPage welcomePage;

    private bool processing = false;
    private ColorBlock defaultColors; 
    
    private void Start()
    {
        defaultColors = confirmPasswordInputField.colors;
        signUpButton.onClick.AddListener(SignUp);
        backButton.onClick.AddListener(Back);
    }

    public override void OnEnter()
    {
        signUpButton.interactable = false;
        processing = false;
        nameInputField.text = "";
        emailInputField.text = "";
        passwordInputField.text = "";
        confirmPasswordInputField.text = "";
    }

    public override void OnExit()
    {
    }

    private async void SignUp()
    {
        processing = true;

        // Register user 
        AuthResult result = null;
        var auth = FirebaseAuth.DefaultInstance;
        await auth.CreateUserWithEmailAndPasswordAsync(emailInputField.text, passwordInputField.text).ContinueWith(
            task => 
            {
                if (task.IsCanceled) {
                    Debug.LogError("CreateUserWithEmailAndPasswordAsync was canceled.");
                    processing = false;
                    return;
                }
                if (task.IsFaulted) {
                    Debug.LogError("CreateUserWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                    processing = false;
                    return;
                }
                
                result = task.Result;
                Debug.LogFormat("Firebase user created successfully: {0} ({1})", result.User.DisplayName, result.User.UserId);
            }
        );
        
        // Login user
        if (result != null)
        {

#if UNITY_EDITOR
        if (ParrelSync.ClonesManager.IsClone())
        {
            string customArgument = ParrelSync.ClonesManager.GetArgument();
            AuthenticationService.Instance.SwitchProfile($"Clone_{customArgument}_Profile");
        }
#endif
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

            var userProfile = new UserProfile();
            userProfile.DisplayName = nameInputField.text;
            await result.User.UpdateUserProfileAsync(userProfile);

            AuthResult loginResult = null;
            await auth.SignInWithEmailAndPasswordAsync(emailInputField.text, passwordInputField.text).ContinueWith(
                task => 
                {
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
                }
            );

            if (loginResult != null)
            {
                UIController.Singleton.PushUIPage(mainMenuPage);
            }
            else
            {
                UIController.Singleton.PushUIPage(loginPage);
            }
        }
    }

    private void Back()
    {
        UIController.Singleton.PushUIPage(welcomePage);
    }

    void Update()
    {
        signUpButton.interactable = !string.IsNullOrEmpty(nameInputField.text) &&
                                    !string.IsNullOrEmpty(emailInputField.text) &&
                                    !string.IsNullOrEmpty(passwordInputField.text) &&
                                    !string.IsNullOrEmpty(confirmPasswordInputField.text) &&
                                    passwordInputField.text == confirmPasswordInputField.text &&
                                    !processing;

        backButton.interactable = !processing;

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
