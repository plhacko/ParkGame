using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Managers;

public class UIInGameScreenController : UIPageController
{
    [SerializeField] private ToggleButtonImage cameraButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button action1;
    [SerializeField] private Button action2;
    [SerializeField] private Button action3;
    [SerializeField] private RectTransform formationMask;
    [SerializeField] private RectTransform formationButtonParent;
    [SerializeField] private Button toggleFormationButton;
    [SerializeField] private Button formationButton1;
    [SerializeField] private Button formationButton2;
    [SerializeField] private Button formationButton3;
    [SerializeField] private Button formationButtonClose;
    [SerializeField] private UIUnitListController unitsList;
    [SerializeField] private UIOutpostListController outpostsList;
    [SerializeField] private UIPage optionsPage;
    [SerializeField] private GameManager gameManager;
    private PlayerManager playerManager;
    private bool attackToggleOn = false;
    private bool boxFormationOn = true;
    private ToggleButtonImage attackToggler;

    private void Awake()
    {
        playerManager = FindObjectOfType<PlayerManager>();
        playerManager.OnAllPlayersReady += ShowCommandButtons;
        optionsButton.onClick.AddListener(Options);
        action1.onClick.AddListener(Attack);
        action2.onClick.AddListener(Gather);
        action3.onClick.AddListener(Formations);
        toggleFormationButton.onClick.AddListener(ToggleFormation);
        formationButton1.onClick.AddListener(FormationCircle);
        formationButton2.onClick.AddListener(FormationBox);
        formationButton3.onClick.AddListener(Formation3);
        formationButtonClose.onClick.AddListener(FormationClose);
        ShowCommandButtons(false);
        attackToggler = action1.GetComponent<ToggleButtonImage>();
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
    private void ShowCommandButtons(bool show) 
    {
        action1.gameObject.SetActive(show);
        action2.gameObject.SetActive(show);
        toggleFormationButton.gameObject.SetActive(show);
    }

    public void ShowCommandButtons() 
    {
        ShowCommandButtons(true);
    }

    private void Gather()
    {
        gameManager.CommandGather();
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

    private void ToggleFormation() 
    {
        boxFormationOn = !boxFormationOn;
        if (attackToggleOn) {
            attackToggler.ChangeSprite();
            attackToggleOn = false;
        }
        if (boxFormationOn) {
            FormationBox();
        } else {
            FormationCircle();
        }
    }

    private void Formation3()
    {
        gameManager.CommandFallback();
        AudioManager.Instance.PlayCommandSFX("FormationFree");

    }

    private void FormationBox() 
    {
        gameManager.FormationBox();
        AudioManager.Instance.PlayCommandSFX("FormationBox");
    }

    private void FormationCircle() 
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
        attackToggleOn = !attackToggleOn;

        if (attackToggleOn) {
            gameManager.CommandAttack();
            AudioManager.Instance.PlayCommandSFX("Attack");
        } else {
            if (boxFormationOn) {
                FormationBox();
            } else {
                FormationCircle();
            }
        }
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
