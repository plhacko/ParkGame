using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHowToPlayScreenController : UIPageController
{
    [SerializeField] private Button backButton;

    private void Awake()
    {
        backButton.onClick.AddListener(Back);
    }

    private void Back()
    {
        UIController.Singleton.PopUIPage();
    }

    public override void OnEnter()
    {}

    public override void OnExit()
    {}
}
