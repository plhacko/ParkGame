using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UILoginController : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField usernameInputField;
    [SerializeField]
    private Button loginButton;

    private void Awake()
    {
        loginButton.interactable = false;
        usernameInputField.onEndEdit.AddListener(delegate { OnInputFieldValueChanged(usernameInputField.text); });
    }

    public void OnInputFieldValueChanged(string text)
    {
        loginButton.interactable = !string.IsNullOrEmpty(text);
    }
}
