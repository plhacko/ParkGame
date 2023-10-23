using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Managers;

public class UILobbyPlayer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI playerNameLabel;
    [SerializeField] private Button removeButton;

    private Func<bool> isHost;
    public void Initialize(Unity.Services.Lobbies.Models.Player player, Action onRemovePlayer, Func<bool> isHost)
    {
        playerNameLabel.text = player.Data["PlayerName"].Value;
        removeButton.onClick.AddListener(() => onRemovePlayer?.Invoke());
        this.isHost = isHost;
    }

    private void Update()
    {
        removeButton.gameObject.SetActive(isHost?.Invoke() ?? false);
    }
}
