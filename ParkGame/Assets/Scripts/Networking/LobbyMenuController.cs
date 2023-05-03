using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class LobbyMenuController : MonoBehaviour
{
    [SerializeField] private UnityEditor.SceneAsset joinMenuScene;
     
    [SerializeField] private Button goBackButton;
    [SerializeField] private Button readyButton;
    [SerializeField] private Button unreadyButton;
    [SerializeField] private TMP_InputField nameInputField;
    
    void Start()
    {
        goBackButton.onClick.AddListener(goBack);
        readyButton.onClick.AddListener(onReady);
        unreadyButton.onClick.AddListener(onUnready);
        nameInputField.onValueChanged.AddListener(onNameChanged);
    }

    private void onUnready()
    {
        readyButton.interactable = true;
        nameInputField.interactable = true;
        unreadyButton.interactable = false;
    }

    private void onReady()
    {
        readyButton.interactable = false;
        nameInputField.interactable = false;
        unreadyButton.interactable = true;
    }

    private void onNameChanged(string newName)
    {
        readyButton.interactable = newName.Length > 0;
    }

    private void goBack()
    {
        NetworkManager.Singleton.Shutdown();
        Destroy(NetworkManager.Singleton.gameObject);
        SceneManager.LoadScene(joinMenuScene.name);
    }
}
