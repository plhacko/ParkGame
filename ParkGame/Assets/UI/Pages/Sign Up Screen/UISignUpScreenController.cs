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
        nameInputField.colors = defaultColors;
        emailInputField.colors = defaultColors;
        passwordInputField.colors = defaultColors;
        confirmPasswordInputField.colors = defaultColors;
    }

    public override void OnExit()
    {
    }

    private async void SignUp()
    {
        AudioManager.Instance.PlayClickSFX();
        processing = true;

        var result = await ServicesManager.Instance.SignUpAndLoginToFirebase(emailInputField.text, passwordInputField.text, nameInputField.text);
        
        if (result == ServicesManager.FirebaseAuthServiceError.FailedToCreateUser)
        {
            UIController.Singleton.ShowPopUp("Sign Up failed", "Please check your email and password and try again.", "OK", null);
            var colors = defaultColors;
            colors.normalColor = new Color(1, 0.7f, 0.7f);
            nameInputField.colors = colors;
            emailInputField.colors = colors;
            passwordInputField.colors = colors;
            confirmPasswordInputField.colors = colors;
            processing = false;
            return;
        }
        else if (result == ServicesManager.FirebaseAuthServiceError.FailedToLoginUser)
        {
            UIController.Singleton.PushUIPage(loginPage);
            processing = false;
            return;
        }
        
        UIController.Singleton.PushUIPage(mainMenuPage);
        
        processing = false;
    }

    private void Back()
    {
        AudioManager.Instance.PlayClickSFX();
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
