using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
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
    [SerializeField] private RectTransform formationMask;
    [SerializeField] private RectTransform formationButtonParent;
    [SerializeField] private Button formationButton1;
    [SerializeField] private Button formationButton2;
    [SerializeField] private Button formationButton3;
    [SerializeField] private Button formationButtonClose;
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
        formationButton1.onClick.AddListener(Formation1);
        formationButton2.onClick.AddListener(Formation2);
        formationButton3.onClick.AddListener(Formation3);
        formationButtonClose.onClick.AddListener(FormationClose);
    }

    private void FormationClose()
    {
        var targetDelta = new Vector2(0, formationMask.sizeDelta.y);

        formationMask.DOSizeDelta(targetDelta, .25f).OnComplete(() => action3.interactable = true);
    }

    private void Formation3()
    {
    }

    private void Formation2()
    {
    }

    private void Formation1()
    {
    }

    private void Action3()
    {
        action3.interactable = false;

        var targetDelta = new Vector2(formationButtonParent.sizeDelta.x, formationMask.sizeDelta.y);

        formationMask.DOSizeDelta(targetDelta, .25f);
    }

    private void Action2()
    {
    }

    private void Action1()
    {
    }

    private void CameraResize()
    {
    }

    private void Follow()
    {
    }

    private void Map()
    {
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
