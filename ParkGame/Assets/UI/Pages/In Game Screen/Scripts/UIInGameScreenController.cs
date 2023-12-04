using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIInGameScreenController : UIPageController
{
    [SerializeField] private Button optionsButton;
    [SerializeField] private ToggleButton mapButton;
    [SerializeField] private ToggleButton lockUIButton;
    [SerializeField] private ToggleButton cameraButton;
    [SerializeField] private Button action1;
    [SerializeField] private ToggleButton action2;
    [SerializeField] private Button action3;
    [SerializeField] private RectTransform formationMask;
    [SerializeField] private RectTransform formationButtonParent;
    [SerializeField] private Button formationButton1;
    [SerializeField] private Button formationButton2;
    [SerializeField] private Button formationButtonClose;
    [SerializeField] private UIPage optionsPage;
    [SerializeField] private GameManager gameManager;

    private void Awake()
    {
        optionsButton.onClick.AddListener(Options);
        mapButton.AddListener("Show", ShowTilemap);
        mapButton.AddListener("Hide", HideTilemap);
        lockUIButton.AddListener("Unlock", UnlockUI);
        lockUIButton.AddListener("Lock", LockUI);
        cameraButton.AddListener("Zoom Out", ZoomOut);
        cameraButton.AddListener("Zoom In", ZoomIn);
        action1.onClick.AddListener(Attack);
        action2.AddListener("Move", Move);
        action2.AddListener("Idle", Idle);
        action3.onClick.AddListener(Formations);
        formationButton1.onClick.AddListener(Formation1);
        formationButton2.onClick.AddListener(Formation2);
        formationButtonClose.onClick.AddListener(FormationClose);
    }

    private void ZoomIn()
    {
        GameManager.Instance.CameraFollowCommander();
    }

    private void UnlockUI()
    {
        var canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.interactable = true;
    }

    private void Idle()
    {
        gameManager.CommandIdle();
    }

    private void Move()
    {
        gameManager.CommandMove();
    }

    private void HideTilemap()
    {
        gameManager.Hide();        
    }

    private void ShowTilemap()
    {
        gameManager.Show();
    }

    private void FormationClose()
    {
        var targetDelta = new Vector2(0, formationMask.sizeDelta.y);

        formationMask.DOSizeDelta(targetDelta, .25f).OnComplete(() => action3.interactable = true);
    }

    private void Formation2()
    {
        gameManager.FormationBox();
    }

    private void Formation1()
    {
        gameManager.FormationCircle();
    }

    private void Formations()
    {
        action3.interactable = false;

        var targetDelta = new Vector2(formationButtonParent.sizeDelta.x, formationMask.sizeDelta.y);

        formationMask.DOSizeDelta(targetDelta, .25f);
    }

    private void Attack()
    {
        gameManager.CommandAttack();
    }

    private void ZoomOut()
    {
        GameManager.Instance.Zoom(float.MaxValue);
    }

    private void LockUI()
    {
        var canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.interactable = false;
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
