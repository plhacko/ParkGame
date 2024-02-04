using System;
using System.Collections.Generic;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIEndGameScreenController : UIPageController
{
    [SerializeField] private List<GameObject> teams;
    [SerializeField] private GameObject otherUI;
    [SerializeField] private Button backToMenuButton;
    [SerializeField] private DisconnectionHandler disconnectionHandler;
    [SerializeField] ColorSettings colorSettings;

    private GameSessionManager gameSessionManager;
    private UIPage uiPage;
    private UIController uiController;
    
    private void Awake()
    {
        backToMenuButton.onClick.AddListener(BackToMenu);
    }

    private void Start()
    {
        uiPage = GetComponent<UIPage>();
        uiController = FindObjectOfType<UIController>();
        gameSessionManager = FindObjectOfType<GameSessionManager>();
        gameSessionManager.OnGameOver += OnGameOver;
    }

    private void OnDestroy()
    {
        gameSessionManager.OnGameOver -= OnGameOver;
    }

    private void OnGameOver()
    {
        otherUI.SetActive(false);
        uiController.PushUIPage(uiPage);
        
        var victoryPoint = FindObjectOfType<VictoryPoint>();
        var teamScores = victoryPoint.GetTeamScores();
        Array.Sort(teamScores, (tuple, valueTuple) =>
        {
            if(valueTuple.Item2 == tuple.Item2) return tuple.Item1 - valueTuple.Item1;
            
            return valueTuple.Item2 - tuple.Item2;
        });
        
        for (int i = 0; i < LobbyManager.Singleton.MapData.MetaData.NumTeams; i++)
        {
            int teamIndex = teamScores[i].Item1;
            teams[i].SetActive(true);
            teams[i].GetComponentInChildren<TextMeshProUGUI>().text = colorSettings.Colors[teamIndex].Name + " Team " + ": " + teamScores[i].Item2;
        }
    }

    private void BackToMenu()
    {
        disconnectionHandler.DisconnectAndLeave();
    }

    public override void OnEnter()
    {
    }
    

    public override void OnExit()
    {

    }
}
