using Networking;
using UI;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PrepareGameMenuController : MonoBehaviour
{
    [SerializeField] private string lobbySceneName;
    [SerializeField] private string joinGameSceneName;
    [SerializeField] private Button backButton;
    [SerializeField] private Button createButton;
    [SerializeField] private NumberPicker numberPicker;

    void Awake()
    {
        createButton.onClick.AddListener(createGame);
        backButton.onClick.AddListener(backJoinGameScene);
    }

    private void backJoinGameScene()
    {
        setInteractable(false);
        OurNetworkManager.Singleton.Shutdown();
        Destroy(OurNetworkManager.Singleton.gameObject);
        SceneManager.LoadScene(joinGameSceneName, LoadSceneMode.Single);
    }

    private async void createGame()
    {
        setInteractable(false);
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(numberPicker.Number);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            
            OurNetworkManager.Singleton.RoomCode = joinCode;
                
            OurNetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData);
            
            OurNetworkManager.Singleton.StartHost();
            OurNetworkManager.Singleton.SceneManager.LoadScene(lobbySceneName, LoadSceneMode.Single);
        }
        catch (RelayServiceException e)
        {
            Debug.LogWarning(e);
            setInteractable(true);
        }
    }

    private void setInteractable(bool interactable)
    {
        backButton.interactable = interactable;
        createButton.interactable = interactable;
        numberPicker.SetInteractable(interactable);
    }
}
