using System;
using System.Collections;
using System.Collections.Generic;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIEndGameScreenController : UIPageController
{
    [SerializeField] private TextMeshProUGUI winnerLabel;
    [SerializeField] private Button backToMenuButton;
    [SerializeField] private string mainMenuSceneName = "Menu";

    private void Awake()
    {
        backToMenuButton.onClick.AddListener(BackToMenu);
    }

    private async void BackToMenu()
    {
        await LobbyManager.Singleton.LeaveLobby();
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public override void OnEnter()
    {
        SetWinnerLabel();
    }

    private void SetWinnerLabel()
    {
        winnerLabel.text = "Winner: " + GameManager.Instance.Winner;
    }

    public override void OnExit()
    {

    }
}
