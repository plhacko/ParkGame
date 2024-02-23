using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Managers;
using Unity.Services.Authentication;

public class UILobbyPlayer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerNameLabel;
    [SerializeField] private Button removeButton;
    private string PlayerSlotUnityAuthPlayerId;

    private Func<bool> isHost;
    public void Initialize(Unity.Services.Lobbies.Models.Player player, Action onRemovePlayer, Func<bool> isHost)
    {
        playerNameLabel.text = player.Data["PlayerName"].Value;
        removeButton.onClick.AddListener(() => onRemovePlayer?.Invoke());
        this.isHost = isHost;
        PlayerSlotUnityAuthPlayerId = player.Id;
    }

    private void Update()
    {
        var currentUserOwner = PlayerSlotUnityAuthPlayerId == AuthenticationService.Instance.PlayerId;
        var show = (isHost?.Invoke() ?? false) && !currentUserOwner;
        removeButton.gameObject.SetActive(show);
    }
}
