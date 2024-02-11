using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Managers;
using Unity.Services.Lobbies.Models;

public class UILobbyTeam : MonoBehaviour
{
    [SerializeField] private UILobbyPlayer lobbyPlayerPrefab;
    [SerializeField] private TextMeshProUGUI teamLabel;
    [SerializeField] private Image teamImage;
    [SerializeField] private Button joinButton;
    [SerializeField] private RectTransform teamLayoutGroup;
    [SerializeField] private ColorSettings colorSettings;
    
    private string playerId;

    public void Initialize(int teamNumber, Action<int> onJoinTeam) {
        
        teamLabel.text = colorSettings.Colors[teamNumber].Name + " Team";
        teamImage.color = colorSettings.Colors[teamNumber].Color;
        
        joinButton.onClick.AddListener(() => {
            onJoinTeam?.Invoke(teamNumber);
            AudioManager.Instance.PlayClickSFX();
        });
    
    }

    public void AddPlayerUI(Unity.Services.Lobbies.Models.Player player, Action onRemovePlayer, Func<bool> isHost)
    {
        UILobbyPlayer lobbyPlayerUI = Instantiate(lobbyPlayerPrefab, teamLayoutGroup);
        lobbyPlayerUI.Initialize(player, onRemovePlayer, isHost);
        // TODO disable join button if team is full
        // joinButton.interactable = !SessionManager.Singleton.IsTeamFull(teamNumber);
    }

    public void Clear()
    {
        foreach (Transform child in teamLayoutGroup)
        {
            Destroy(child.gameObject);
        }
    }
}
