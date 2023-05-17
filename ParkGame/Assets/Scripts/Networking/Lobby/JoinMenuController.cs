using System;
using System.Threading.Tasks;
using TMPro;
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
            
            bool joined = await joinGame(joinCodeInputField.text);
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

        private static async Task<bool> joinGame(string joinCode)
        {
            try
            {
                JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
                
                OurNetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                    joinAllocation.RelayServer.IpV4,
                    (ushort)joinAllocation.RelayServer.Port,
                    joinAllocation.AllocationIdBytes,
                    joinAllocation.Key,
                    joinAllocation.ConnectionData,
                    joinAllocation.HostConnectionData);

                OurNetworkManager.Singleton.NetworkConfig.ConnectionData = SessionManager.Singleton.LocalPlayerId.ToByteArray();
                OurNetworkManager.Singleton.StartClient();
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