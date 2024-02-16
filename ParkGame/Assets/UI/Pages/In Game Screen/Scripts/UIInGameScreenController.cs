using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class UIInGameScreenController : UIPageController
{
    [SerializeField] private ToggleButtonImage cameraButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button action1;
    [SerializeField] private Button action2;
    [SerializeField] private Button action3;
    [SerializeField] private RectTransform formationMask;
    [SerializeField] private RectTransform formationButtonParent;
    [SerializeField] private Button formationButton1;
    [SerializeField] private Button formationButton2;
    [SerializeField] private Button formationButton3;
    [SerializeField] private Button formationButtonClose;
    [SerializeField] private UIUnitListController unitsList;
    [SerializeField] private UIOutpostListController outpostsList;
    [SerializeField] private UIPage optionsPage;
    [SerializeField] private GameManager gameManager;

    private void Awake()
    {
        optionsButton.onClick.AddListener(Options);
        action1.onClick.AddListener(Attack);
        action2.onClick.AddListener(Move); // Gather
        action3.onClick.AddListener(Formations);
        formationButton1.onClick.AddListener(Formation1);
        formationButton2.onClick.AddListener(Formation2);
        formationButton3.onClick.AddListener(Formation3);
        formationButtonClose.onClick.AddListener(FormationClose);
    }

    private void Start()
    {
        GameManager.Instance.CameraFollowCommander();
    }

    public void ZoomIn()
    {
        GameManager.Instance.CameraFollowCommander();
    }

    public void UnlockUI()
    {
        var canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.interactable = true;
    }

    // Gather
    private void Move()
    {
        gameManager.CommandMove();
        AudioManager.Instance.PlayCommandSFX("Fallback");
    }

    public void HideTilemap()
    {
        gameManager.Hide();        
    }

    public void ShowTilemap()
    {
        gameManager.Show();
    }

    private void FormationClose()
    {
        var targetDelta = new Vector2(0, formationMask.sizeDelta.y);

        formationMask.DOSizeDelta(targetDelta, .25f).OnComplete(() => action3.interactable = true);
    }

    private void Formation3()
    {
        gameManager.CommandMove();
        AudioManager.Instance.PlayCommandSFX("FormationFree");

    }

    private void Formation2() 
    {
        gameManager.FormationBox();
        AudioManager.Instance.PlayCommandSFX("FormationBox");
    }

    private void Formation1() 
    {
        gameManager.FormationCircle();
        AudioManager.Instance.PlayCommandSFX("FormationCircle");
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
        AudioManager.Instance.PlayCommandSFX("Attack");
    }

    public void ZoomOut()
    {
        GameManager.Instance.Zoom(float.MaxValue);
    }

    public void LockUI()
    {
        var canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.interactable = false;
    }

    private void Options()
    {
        UIController.Singleton.PushUIPage(optionsPage);
    }

    public void AddUnit(ISoldier unit, Action removeAction)
    {
        unitsList.AddUnit(unit, removeAction);
    }

    public void RemoveUnit(ISoldier unit)
    {
        unitsList.RemoveUnit(unit);
    }   

    public void AddOutpost(Outpost outpost)
    {
        outpostsList.AddOutpost(outpost);
    }

    public void RemoveOutpost(Outpost outpost)
    {
        outpostsList.RemoveOutpost(outpost);
    }

    public override void OnEnter()
    {
    }

    public override void OnExit()
    {
    }

    void Update()
    {
        if (!GameManager.Instance.FollowCommander)
        {
            cameraButton.SwitchState(1);
        }
    }
}
