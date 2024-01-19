using UnityEngine;
using UnityEngine.Events;

public class UnitBehaviourDrawer : MonoBehaviour
{
    private Soldier TheSoldier;
    public SpriteRenderer SpriteRenderer;
    public UnityEvent BehaviourEvent;
    [SerializeField] Sprite EmptyIcon;
    [SerializeField] Sprite AttackIcon;
    [SerializeField] Sprite IdleIcon;
    [SerializeField] Sprite FormationIcon;
    [SerializeField] Sprite MovementIcon;

    void Awake()
    {
        SpriteRenderer = GetComponent<SpriteRenderer>();
        TheSoldier = GetComponentInParent<Soldier>();
        BehaviourEvent = TheSoldier.CommandChangedEvent;
        BehaviourEvent.AddListener(DrawCommandIcon);
    }

    private void DrawCommandIcon() {
        SoldierCommand command = TheSoldier.Command;
        Sprite icon;
        switch (command) {
            case SoldierCommand.Attack:
                icon = AttackIcon;
                break;
            case SoldierCommand.InOutpost:
                icon = IdleIcon;
                break;
            case SoldierCommand.Following:
                icon = MovementIcon;
                break;
                icon = FormationIcon;
                break;
            case SoldierCommand.Die:
            default:
                icon = EmptyIcon;
                break;
        }

        SpriteRenderer.sprite = icon;
    }

}
