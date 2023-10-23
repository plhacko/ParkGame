using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using Unity.Services.Lobbies;
using System;
using Unity.Services.Authentication;
using Unity.Services.Core;
using System.Threading.Tasks;


namespace Managers
{
    public class LobbyManager : MonoBehaviour
    {
        public static LobbyManager Singleton { get; private set; }

        private Lobby lobby;
        public Lobby Lobby { get { return lobby; } }

        private MapData mapData;
        public MapData MapData { get { return mapData; } }
        
        public bool IsHost { get { return lobby != null && lobby.HostId == AuthenticationService.Instance.PlayerId; } }

        private void Awake()
        {
            if (Singleton != null)
            {
                Destroy(gameObject);
                return;
            }

            Singleton = this;
        }

        private void OnDestroy()
        {
            if (Singleton == this)
            {
                Singleton = null;
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.L))
            {
                PrintPlayers(lobby);
            }
        }


        public async void CreateLobbyForMap(MapData mapData)
        {
            try 
            {
                // TODO temporary lobby name
                string lobbyName = "My Lobby";
                // TODO temporary max players
                int maxPlayers = mapData.MetaData.NumTeams * 4;

                CreateLobbyOptions createLobbyOptions = new()
                {
                    IsPrivate = true,
                    Player = GetPlayer(),
                };

                Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);
                this.lobby = lobby;

                StartCoroutine(HeatbeatLobbyCoroutine(lobby));
                StartCoroutine(PollLobbyCoroutine());

                this.mapData = mapData;

                Debug.Log("Lobby created with ID: " + lobby.Id + " and code: " + lobby.LobbyCode);
            } 
            catch (Exception e)
            {
                Debug.LogError("Failed to create lobby: " + e.Message);
            }
        }

        public async void JoinLobbyByCode(string code)
        {
            try
            {
                JoinLobbyByCodeOptions joinLobbyByCodeOptions = new()
                {
                    Player = GetPlayer(),
                };

                Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(code, joinLobbyByCodeOptions);
                this.lobby = lobby;

                StartCoroutine(PollLobbyCoroutine());

                Debug.Log("Lobby joined with ID: " + lobby.Id + " and code: " + lobby.LobbyCode);
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to join lobby: " + e.Message);
            }
        }

        IEnumerator HeatbeatLobbyCoroutine(Lobby lobby)
        {
            var delay = new WaitForSeconds(15);

            while (true)
            {
                LobbyService.Instance.SendHeartbeatPingAsync(lobby.Id);
                yield return delay;
            }
        }

        IEnumerator PollLobbyCoroutine()
        {
            var delay = new WaitForSeconds(15);

            while (true)
            {
                var t = Task.Run(async () => await LobbyService.Instance.GetLobbyAsync(lobby.Id));
                yield return new WaitUntil(() => t.IsCompleted);

                lobby = t.Result;
                yield return delay;
            }
        }

        private void PrintPlayers(Lobby lobby)
        {
            foreach (var player in lobby.Players)
            {
                Debug.Log("Player: " + player.Id + " " + player.Data["PlayerName"].Value + " " + player.Data["TeamNumber"].Value);  
            }
        }

        private Unity.Services.Lobbies.Models.Player GetPlayer()
        {
            return new() 
            {
                Data = new()
                {
                    {
                        "PlayerName",
                        new PlayerDataObject
                        (
                            PlayerDataObject.VisibilityOptions.Member,
                            PlayerPrefs.GetString("PlayerName", "Player")
                        )
                    },
                    {
                        "TeamNumber",
                        new PlayerDataObject
                        (
                            PlayerDataObject.VisibilityOptions.Member,
                            "-1"
                        )
                    }
                }
            };
        }

        public async void JoinTeam(int teamNumber)
        {
            try 
            {
                var player = GetPlayer();
                player.Data["TeamNumber"].Value = teamNumber.ToString();

                UpdatePlayerOptions updatePlayerOptions = new()
                {
                    Data = player.Data,
                };

                await Lobbies.Instance.UpdatePlayerAsync(lobby.Id, AuthenticationService.Instance.PlayerId, updatePlayerOptions);

                Debug.Log("Player " + AuthenticationService.Instance.PlayerId + " joined team " + teamNumber);
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to join team: " + e.Message);
            }
        }
    }
}
