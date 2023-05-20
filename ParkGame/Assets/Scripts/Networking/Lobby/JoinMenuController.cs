using TMPro;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Networking
{
    public class JoinMenuController : MonoBehaviour
    {
        [SerializeField] private string hostMenuSceneName;
        [SerializeField] private Button joinButton;
        [SerializeField] private Button hostButton;
        [SerializeField] private TMP_InputField joinCodeInputField;

        private async void Start()
        {
            enableButtons(false);
            joinButton.onClick.AddListener(joinGame);
            hostButton.onClick.AddListener(hostGame);
            
            await UnityServices.InitializeAsync();
            
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();   
            }

            OurNetworkManager.Singleton.OnClientDisconnectCallback += onClientDisconnect;
            
            enableButtons(true);
        }

        private void OnDestroy()
        {
            if(OurNetworkManager.Singleton != null)
                OurNetworkManager.Singleton.OnClientDisconnectCallback -= onClientDisconnect;
        }

        private void onClientDisconnect(ulong clientId)
        {
            OurNetworkManager.Singleton.RoomCode = "";
            joinCodeInputField.text = "";
            enableButtons(true);
        }

        private void hostGame()
        {
            enableButtons(false);
            SceneManager.LoadScene(hostMenuSceneName, LoadSceneMode.Single);
        }

        private async void joinGame()
        {
            enableButtons(false);
            
            OurNetworkManager.Singleton.RoomCode = joinCodeInputField.text.ToUpper();
            
            bool joined = await OurNetworkManager.Singleton.JoinGame(joinCodeInputField.text);
            if (!joined)
            {
                enableButtons(true);
                OurNetworkManager.Singleton.RoomCode = "";
                joinCodeInputField.text = "";
            }
        }

        private void enableButtons(bool isInteractable)
        {
            joinButton.interactable = isInteractable;
            hostButton.interactable = isInteractable;
            joinCodeInputField.interactable = isInteractable;
        }
    }
}