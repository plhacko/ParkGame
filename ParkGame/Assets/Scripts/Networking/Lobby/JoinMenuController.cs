using System;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Networking
{
    public class JoinMenuController : MonoBehaviour
    {
        [SerializeField] private string lobbyMenuSceneName;
        [SerializeField] private string hostMenuSceneName;
        [SerializeField] private Button joinButton;
        [SerializeField] private Button hostButton;
        [SerializeField] private TMP_InputField joinCodeInputField;

        public static string RoomCode;
        
        private async void Start()
        {
            enableButtons(false);
            joinButton.onClick.AddListener(JoinGame);
            hostButton.onClick.AddListener(HostGame);
            
            await UnityServices.InitializeAsync();
            
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();   
            }

            enableButtons(true);
        }

        private void HostGame()
        {
            enableButtons(false);
            SceneManager.LoadScene(hostMenuSceneName, LoadSceneMode.Single);
        }

        public async void JoinGame()
        {
            enableButtons(false);
            
            RoomCode = joinCodeInputField.text.ToUpper();
            
            bool joined = await JoinGame(joinCodeInputField.text);
            if (!joined)
            {
                enableButtons(true);
            }
            
            joinCodeInputField.text = "";
        }

        private void enableButtons(bool isInteractable)
        {
            joinButton.interactable = isInteractable;
            hostButton.interactable = isInteractable;
            joinCodeInputField.interactable = isInteractable;
        }

        private static async Task<bool> JoinGame(string joinCode)
        {
            try
            {
                JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
                
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                    joinAllocation.RelayServer.IpV4,
                    (ushort)joinAllocation.RelayServer.Port,
                    joinAllocation.AllocationIdBytes,
                    joinAllocation.Key,
                    joinAllocation.ConnectionData,
                    joinAllocation.HostConnectionData);

                NetworkManager.Singleton.StartClient();
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
                return false;
            }
        }
    }
}