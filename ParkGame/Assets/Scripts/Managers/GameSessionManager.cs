﻿using System;
using Unity.Netcode;
using UnityEngine;

namespace Managers
{
    public class GameSessionManager : NetworkBehaviour
    {
        [SerializeField] ColorSettings colorSettings;
        [SerializeField] private int maxPoints;
        
        private PlayerManager playerManager;
        private Announcer announcer;
        private VictoryPoint victoryPoint;
        private readonly NetworkVariable<bool> isOver = new();
        public Action OnGameOver;
        
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            initialize();
        }

        private void initialize()
        {
            if (NetworkManager.Singleton.IsServer)
            {
                playerManager = FindObjectOfType<PlayerManager>();
                announcer = FindObjectOfType<Announcer>();
                playerManager.OnAllPlayersReady += onAllPlayersReady;
            }
            
            isOver.OnValueChanged += (previousValue, newValue) =>
            {
                if(!previousValue && newValue)
                {
                    OnGameOver?.Invoke();
                }
            };
        }

        private void onAllPlayersReady()
        {
            victoryPoint = FindObjectOfType<VictoryPoint>();
            victoryPoint.OnPointConquered += onVictoryPointConquered;
        }

        public override void OnNetworkDespawn()
        {
            base.OnNetworkDespawn();
            if (NetworkManager.Singleton.IsServer)
            {
                victoryPoint.OnPointConquered -= onVictoryPointConquered;
                playerManager.OnAllPlayersReady -= onAllPlayersReady;
            }
        }
        
        private void onVictoryPointConquered(int team, int numPoints)
        {
            if(numPoints >= maxPoints) {
                announcer.AnnounceEventClientRpc($"The {colorSettings.Colors[team].Name} Team has won!", 15);
                playerManager.DisableAllPlayers();
            }
            else
            {
                int endingDigit = numPoints % 10;
                string numberEnding = endingDigit switch
                {
                    1 => "st",
                    2 => "nd",
                    3 => "rd",
                    _ => "th"
                };
                var c = colorSettings.Colors[team];
                isOver.Value = true;
                announcer.AnnounceEventClientRpc($"The {c.Name} Team has scored their {numPoints}{numberEnding} point!", 5);
            }
        }
    }
}