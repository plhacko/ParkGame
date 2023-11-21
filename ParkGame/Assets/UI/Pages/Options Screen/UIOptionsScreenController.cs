using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIOptionsScreenController : UIPageController
{
    [SerializeField] private Button backButton;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button backgroundNoiseButton;
    [SerializeField] private Button mainMusicButton;
    [SerializeField] private Button howToPlayButton;
    [SerializeField] private UIPage howToPlayPage;
    [SerializeField] private string mainMenuSceneName = "Menu";

    private void Awake()
    {
        backButton.onClick.AddListener(Back);
        quitButton.onClick.AddListener(Quit);
        backgroundNoiseButton.onClick.AddListener(ToggleBackgroundNoise);
        mainMusicButton.onClick.AddListener(ToggleMainMusic);
        howToPlayButton.onClick.AddListener(HowToPlay);        

    }

    public override void OnEnter()
    {}

    public override void OnExit()
    {}

    private void Back()
    {
        UIController.Singleton.PopUIPage();
    }

    private void Quit()
    {
        // TODO handle disconnecting from server
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void ToggleBackgroundNoise()
    {
        // TODO toggle background noise
    }

    private void ToggleMainMusic()
    {
        // TODO toggle main music
    }

    private void HowToPlay()
    {
        UIController.Singleton.PushUIPage(howToPlayPage);
    }
}
