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

    public void Initialize(PlayerData playerData, Action onRemovePlayer)
    {
        playerNameLabel.text = playerData.Name;
        removeButton.onClick.AddListener(() => onRemovePlayer?.Invoke());
    }

    private void Update()
    {
        removeButton.gameObject.SetActive(SessionManager.Singleton.IsLobbyHost);
    }
}
