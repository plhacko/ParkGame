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
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Networking
{
    public class JoinMenuController : NetworkBehaviour
    {
        [SerializeField] private UnityEditor.SceneAsset lobbyMenuScene;
        
        [SerializeField] private Button joinButton;
        [SerializeField] private Button hostButton;
        [SerializeField] private TMP_InputField joinCodeInputField;
        
        private const int numPlayers = 1;

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
                
                Debug.Log(joinCode);
                
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
                    allocation.RelayServer.IpV4,
                    (ushort)allocation.RelayServer.Port,
                    allocation.AllocationIdBytes,
                    allocation.Key,
                    allocation.ConnectionData);

                NetworkManager.Singleton.StartHost();
                
                var status = NetworkManager.SceneManager.LoadScene(lobbyMenuScene.name, LoadSceneMode.Single);
                if (status != SceneEventProgressStatus.Started)
                {
                    Debug.LogWarning($"Failed to load {lobbyMenuScene.name}");
                }
            }
            catch (RelayServiceException e)
            {
                Debug.LogWarning(e);
            }
        }

        public async void JoinGame()
        {
            enableButtons(false);
            await JoinGame(joinCodeInputField.text);
            
            joinCodeInputField.text = "";
        }

        private void enableButtons(bool isInteractable)
        {
            joinButton.interactable = isInteractable;
            hostButton.interactable = isInteractable;
            joinCodeInputField.interactable = isInteractable;
        }

        private static async Task JoinGame(string joinCode)
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
            }
            catch (RelayServiceException e)
            {
                Debug.LogWarning(e);
            }
        }
    }
}