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
        [SerializeField] private UnityEditor.SceneAsset lobbyMenuScene;
        
        [SerializeField] private Button joinButton;
        [SerializeField] private Button hostButton;
        [SerializeField] private TMP_InputField joinCodeInputField;
        
        private const int numPlayers = 1;

        public static string RoomCode;
        
        private async void Start()
        {
            enableButtons(false);
            joinButton.onClick.AddListener(JoinGame);
            hostButton.onClick.AddListener(HostGame);
            
            await UnityServices.InitializeAsync();

            AuthenticationService.Instance.SignedIn += () =>
            {
                Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
            };

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();   
            }

            enableButtons(true);
        }

        private async void HostGame()
        {
            enableButtons(false);
            
            try
            {
                Allocation allocation = await RelayService.Instance.CreateAllocationAsync(numPlayers);
                string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

                RoomCode = joinCode;
                
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
                    allocation.RelayServer.IpV4,
                    (ushort)allocation.RelayServer.Port,
                    allocation.AllocationIdBytes,
                    allocation.Key,
                    allocation.ConnectionData);

                NetworkManager.Singleton.StartHost();
                NetworkManager.Singleton.SceneManager.LoadScene(lobbyMenuScene.name, LoadSceneMode.Single);
            }
            catch (RelayServiceException e)
            {
                Debug.LogWarning(e);
            }
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