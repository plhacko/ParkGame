using UnityEngine;
using System;
using System.Collections;
using UnityEngine.Events;

public class UnitBehaviourDrawer : MonoBehaviour
{
    private SoldierBase TheSoldier;
    private SpriteRenderer SpriteRenderer;
    private CommandEvent CommandAction;
    private UnityEvent speakEvent;
    [SerializeField] Sprite EmptyIcon;
    [SerializeField] Sprite AttackIcon;
    [SerializeField] Sprite IdleIcon;
    [SerializeField] Sprite FormationIcon;
    [SerializeField] Sprite MovementIcon;
    [SerializeField] Sprite DeathIcon;
    [SerializeField] private Sprite SpeakIcon;
    private bool isSpeaking = false;
    private SoldierCommand waitingCommand;

    void Awake()
    {
        SpriteRenderer = GetComponent<SpriteRenderer>();
        TheSoldier = GetComponentInParent<SoldierBase>();
        CommandAction = TheSoldier.NewCommandEvent;
        speakEvent = TheSoldier.SpeakEvent;
        speakEvent.AddListener(DisplaySpeakIcon);
        CommandAction.AddListener(DrawCommandIcon);
    }

    private void DisplaySpeakIcon() {
        isSpeaking = true;
        bool callback;
        StartCoroutine(DisplaySpeakIcon(SpeakIcon, SpriteRenderer, 1f, (callback) => { 
            isSpeaking = false;
            DrawCommandIcon(waitingCommand);
        }));
    }

    private static IEnumerator DisplaySpeakIcon(Sprite iconToShow, SpriteRenderer sr, float t, Action<bool> callback) {
        float timer = t;
        sr.sprite = iconToShow;
        while (timer > 0) {
            timer -= Time.deltaTime;
            yield return null;
        }
        callback(false);
        yield break;
    }

    public Sprite GetIconForCommand(SoldierCommand command) {
        Sprite icon = null;
        switch (command) {
            case SoldierCommand.Attack:
                icon = AttackIcon;
                break;
            case SoldierCommand.InOutpost:
                icon = IdleIcon;
                break;
            case SoldierCommand.Following:
                icon = EmptyIcon;
                break;
            case SoldierCommand.Die:
                icon = DeathIcon;
                break;
            default:
                icon = EmptyIcon;
                break;
        }
        return icon;
    }

    private void DrawCommandIcon(SoldierCommand command) {
        if (isSpeaking) {
            waitingCommand = command;
            return;
        }
        SpriteRenderer.sprite = GetIconForCommand(command);
    }

}
