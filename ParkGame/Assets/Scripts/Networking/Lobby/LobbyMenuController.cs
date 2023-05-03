using System;
using Networking;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyMenuController : NetworkBehaviour
{
    [SerializeField] private UnityEditor.SceneAsset gameScene;
    [SerializeField] private UnityEditor.SceneAsset joinMenuScene;

    [SerializeField] private Button goBackButton;
    [SerializeField] private Button readyButton;
    [SerializeField] private Button unreadyButton;
    [SerializeField] private Button startGameButton;
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private TextMeshProUGUI roomCodeLabel;
    [SerializeField] private TextMeshProUGUI waitingStatusLabel;

    [SerializeField] private MatchData matchDataPrefab;

    private MatchData matchData;
    private bool isHostReady = false;
    private bool isClientReady = false;

    void initialize()
    {
        if (IsHost)
        {
            matchData = Instantiate(matchDataPrefab);
            matchData.GetComponent<NetworkObject>().Spawn();   
            startGameButton.onClick.AddListener(startGame);
        }
        else
        {
            startGameButton.gameObject.SetActive(false);
        }

        roomCodeLabel.text += JoinMenuController.RoomCode;
        
        goBackButton.onClick.AddListener(goBack);
        readyButton.onClick.AddListener(onReady);
        unreadyButton.onClick.AddListener(onUnready);
        nameInputField.onValueChanged.AddListener(onNameChanged);
        
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
    }

    private void startGame()
    {
        NetworkManager.Singleton.SceneManager.LoadScene(gameScene.name, LoadSceneMode.Single);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        initialize();
    }
    
    private void Update()
    {
        startGameButton.interactable = IsHost && isHostReady && isClientReady;

        if (IsHost)
        {
            if (NetworkManager.Singleton.ConnectedClients.Count > 1)
            {
                waitingStatusLabel.text = isClientReady ? matchData.ClientName + " is ready" : "Waiting for opponents name...";   
            }
            else
            {
                waitingStatusLabel.text = "Waiting for opponent to connect...";   
            }
        }
        else
        {
            waitingStatusLabel.text = isHostReady ? matchData.HostName + " is ready" : "Waiting for opponents name...";
        }
    }

    
    private void OnClientConnected(ulong clientId)
    {
        if (IsHost)
        {
            matchData.SetName(matchData.HostName);
            setIsReady(isHostReady);
            Debug.Log("Client connected: " + clientId);   
            
        }
        else
        {
            matchData = FindObjectOfType<MatchData>();
        }
    }
    
    private void OnClientDisconnected(ulong clientId)
    {
        if (IsHost)
        {
            isClientReady = false;   
        }
        else
        {
            goBack();
        }
        
        Debug.Log(clientId);
    }

    private void onUnready()
    {
        readyButton.interactable = true;
        nameInputField.interactable = true;
        unreadyButton.interactable = false;
        setIsReady(false);
    }

    private void onReady()
    {
        readyButton.interactable = false;
        nameInputField.interactable = false;
        unreadyButton.interactable = true;

        matchData.SetName(nameInputField.text);
        setIsReady(true);
    }

    private void setIsReady(bool isReady)
    {
        if (IsHost)
        {
            SetIsHostReadyClientRPC(isReady);
        }
        else
        {
            SetIsClientReadyServerRPC(isReady);
        }
    }
    
    [ClientRpc]
    private void SetIsHostReadyClientRPC(bool isReady)
    {
        isHostReady = isReady;
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void SetIsClientReadyServerRPC(bool isReady)
    {
        isClientReady = isReady;
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
