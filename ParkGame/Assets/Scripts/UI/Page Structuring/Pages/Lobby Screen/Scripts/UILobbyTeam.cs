using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Managers;

public class UILobbyTeam : MonoBehaviour
{
    [SerializeField] private UILobbyPlayer lobbyPlayerPrefab;
    [SerializeField] private TextMeshProUGUI teamLabel;
    [SerializeField] private Button joinButton;
    [SerializeField] private RectTransform teamLayoutGroup;

    private int teamNumber;

    private readonly Dictionary<Guid, UILobbyPlayer> playerGuidtoUI = new();

    public void Initialize(int teamNumber, Action<int> onJoinTeam)
    {
        teamLabel.text = $"Team {teamNumber + 1}";
        this.teamNumber = teamNumber;
        joinButton.onClick.AddListener(() => onJoinTeam?.Invoke(teamNumber));
    }

    public void AddPlayerUI(PlayerData playerData, Action onRemovePlayer)
    {
        UILobbyPlayer lobbyPlayerUI = Instantiate(lobbyPlayerPrefab, teamLayoutGroup);
        lobbyPlayerUI.Initialize(playerData, onRemovePlayer);
        playerGuidtoUI.Add(playerData.ID, lobbyPlayerUI);
        joinButton.interactable = !SessionManager.Singleton.IsTeamFull(teamNumber);
    }

    public void RemovePlayer(Guid playerId)
    {
        Destroy(playerGuidtoUI[playerId].gameObject);
        playerGuidtoUI.Remove(playerId);
        joinButton.interactable = !SessionManager.Singleton.IsTeamFull(teamNumber);
    }
}
