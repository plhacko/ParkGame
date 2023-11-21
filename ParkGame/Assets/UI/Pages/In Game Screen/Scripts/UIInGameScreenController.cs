using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIInGameScreenController : UIPageController
{
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button mapButton;
    [SerializeField] private Button followButton;
    [SerializeField] private Button cameraButton;
    [SerializeField] private Button action1;
    [SerializeField] private Button action2;
    [SerializeField] private Button action3;
    [SerializeField] private UIPage optionsPage;

    private void Awake()
    {
        optionsButton.onClick.AddListener(Options);
        mapButton.onClick.AddListener(Map);
        followButton.onClick.AddListener(Follow);
        cameraButton.onClick.AddListener(CameraResize);
        action1.onClick.AddListener(Action1);
        action2.onClick.AddListener(Action2);
        action3.onClick.AddListener(Action3);
    }

    private void Action3()
    {
        throw new NotImplementedException();
    }

    private void Action2()
    {
        throw new NotImplementedException();
    }

    private void Action1()
    {
        throw new NotImplementedException();
    }

    private void CameraResize()
    {
        throw new NotImplementedException();
    }

    private void Follow()
    {
        throw new NotImplementedException();
    }

    private void Map()
    {
        throw new NotImplementedException();
    }

    private void Options()
    {
        UIController.Singleton.PushUIPage(optionsPage);
    }

    public override void OnEnter()
    {
    }

    public override void OnExit()
    {
    }
}
