using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Storage;
using Managers;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Utils
{
    [Serializable]
    public class PlayerDataDebug
    {
        public int Team;
        public string Email;
        public string Password;
        
        [HideInInspector] public Button Button;
    }
    
    /*
     * This class is a helper to create a game session for debugging without going through the menus.
     * It actually doesn't work well anymore because it should initialize the SessionManager with some debug player data
     */
    public class NetworkHelperUI : MonoBehaviour
    {
        [SerializeField] private string GameSceneName;
        [SerializeField] private string mapId;
        [SerializeField] private Button startButton;
        [SerializeField] private Button hostButton; 
        [SerializeField] private Button clientButtonPrefab;
        [SerializeField] private Transform clientButtonParent;
        [SerializeField] private TextMeshProUGUI readyText;
        [SerializeField] private PlayerDataDebug host;
        [SerializeField] private PlayerDataDebug[] clients;

        private event Action<MapData> OnMapReceived = null;
        private MapData mapData;
        
        private async void Start()
        {
            foreach (var client in clients)
            {
                client.Button = Instantiate(clientButtonPrefab, clientButtonParent);
                client.Button.interactable = false;
                client.Button.GetComponentInChildren<TextMeshProUGUI>().text = $"team: {client.Team}, {client.Email}";
                client.Button.onClick.AddListener(() =>
                {
                    setClientButtonsInteractable(false);
                    hostButton.interactable = false;
                    startClient(client);
                });
            }

            hostButton.interactable = false;
            hostButton.onClick.AddListener(() =>
            {
                hostButton.interactable = false;
                setClientButtonsInteractable(false);
                startHost();
            });
            
            startButton.interactable = false; 
            startButton.onClick.AddListener(() =>
            {
                setClientButtonsInteractable(false);
                hostButton.interactable = false;
                startButton.interactable = false;
                startGame();
            });

            OnMapReceived += (mapData) =>
            {
                Debug.Log("map data received");
                this.mapData = mapData;
            };
         
            await UnityServices.InitializeAsync();
            
#if UNITY_EDITOR
            if (ParrelSync.ClonesManager.IsClone())
            {
                string customArgument = ParrelSync.ClonesManager.GetArgument();
                AuthenticationService.Instance.SwitchProfile($"Clone_{customArgument}_Profile");
            }
#endif
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            await downloadMapData(new Guid(mapId));

            hostButton.interactable = true;
            setClientButtonsInteractable(true);
        }

        private void startGame()
        {
            NetworkManager.Singleton.SceneManager.LoadScene(GameSceneName, LoadSceneMode.Single);
        }

        private async void startHost()
        {
            await login(host.Email, host.Password);
            
            bool lobbyCreated = await LobbyManager.Singleton.CreateLobbyForMap(mapData);
            if (lobbyCreated)
            {
                PlayerPrefs.SetString("DebugRoomCode", LobbyManager.Singleton.Lobby.LobbyCode);
                Debug.Log($"Host initialized, code: {LobbyManager.Singleton.Lobby.LobbyCode} email: {host.Email}");
                startButton.interactable = true;
            }
            else
            {
                Debug.LogError("Host initialization failed idk why");   
            }
            
            bool joinedTeam = await LobbyManager.Singleton.JoinTeam(host.Team);
            if (!joinedTeam)
            {
                Debug.LogError($"Failed to join team {host.Team}");
            }
            
            readyText.text = "Host initialized";
        }

        private async void startClient(PlayerDataDebug client)
        {
            await login(client.Email, client.Password);
            
            string joinCode = PlayerPrefs.GetString("DebugRoomCode", "");
            LobbyManager.Singleton.DebugSetMapData(mapData);

            var joined = await LobbyManager.Singleton.JoinLobbyByCode(joinCode);
            if (joined == LobbyManager.JoinLobbyResult.Success)
            {
                Debug.Log($"Client initialized email: {client.Email}");
            }
            else
            {
                Debug.LogError($"Failed to join game, join code: [{joinCode}], you probably need to wait longer for the host to initialize properly");
            }

            bool joinedTeam = await LobbyManager.Singleton.JoinTeam(client.Team);
            if (!joinedTeam)
            {
                Debug.LogError($"Failed to join team {client.Team}");
            }
            
            readyText.text = "Client initialized";
        }
        
        private async Task downloadMapData(Guid mapId)
        {
            await FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
            {
                if (task.Exception != null)
                {
                    Debug.LogError($"Failed to intialize Firebase with {task.Exception}");
                    return;
                }
                   
#if UNITY_EDITOR // Unity sometimes crashes when Firebase Persistence is Enabled and two editors use it 
                // FirebaseStorage.DefaultInstance.SetPersistenceEnabled(false);
                FirebaseDatabase.DefaultInstance.SetPersistenceEnabled(false);
#endif
                Debug.Log(task.Status);
            });

            var storageReference = FirebaseStorage.DefaultInstance.RootReference;
            var databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
        
            DataSnapshot dataSnapshot = await databaseReference.Child(FirebaseConstants.MAP_DATA_FOLDER).Child(mapId.ToString()).GetValueAsync();
            MapMetaData mapMetaData = JsonUtility.FromJson<MapMetaData>(dataSnapshot.GetRawJsonValue());

            var imageReference = storageReference.Child($"{FirebaseConstants.MAP_IMAGES_FOLDER}/{mapMetaData.MapId}.png");

            var imageBytes = await imageReference.GetBytesAsync(FirebaseConstants.MAX_MAP_SIZE);
            Texture2D texture = new Texture2D(mapMetaData.Width, mapMetaData.Height); 
            texture.LoadImage(imageBytes);
           
            var map = new MapData
            {
                MetaData = mapMetaData,
                DrawnTexture = texture
            };

            StartCoroutine(gpsTextureRequest(map));
        }
        
        IEnumerator gpsTextureRequest(MapData mapData)
        {
            UnityWebRequest request = UnityWebRequestTexture.GetTexture(mapData.MetaData.MapQuery);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning("API Request error: " + request.error);
            }
            else
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                mapData.GPSTexture = texture;
            }
            
            OnMapReceived?.Invoke(mapData);
        }

        private async Task login(string email, string password)
        {
            var auth = FirebaseAuth.DefaultInstance;
            await auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(
                task => 
                {
                    if (task.IsCanceled)
                    {
                        Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");
                        return;
                    }

                    if (task.IsFaulted)
                    {
                        AggregateException ex = task.Exception;
                    
                        if (ex != null) {
                            foreach (Exception e in ex.InnerExceptions) {
                                if (e is FirebaseException fbEx)
                                {
                                    Debug.LogError("Encountered a FirebaseException:" + fbEx.Message);
                                }
                            }
                        }

                        Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                        return;
                    }

                    Debug.LogFormat("User signed in successfully: {0} ({1})", task.Result.User.DisplayName, task.Result.User.UserId);
                }
            );
        }

        private void setClientButtonsInteractable(bool interactable)
        {
            foreach (var client in clients)
            {
                client.Button.interactable = interactable;
            }
        }
    }
}
