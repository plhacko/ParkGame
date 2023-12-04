using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIAboutScreenController : UIPageController
{
    [SerializeField] private Button backButton;
    [SerializeField] private UIPage mainMenuPage;

    public override void OnEnter()
    {}

    public override void OnExit()
    {}

    private void Start()
    {
        backButton.onClick.AddListener(Back);
    }

    private void Back()
    {
        UIController.Singleton.PushUIPage(mainMenuPage);
    }
}
