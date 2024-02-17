using UnityEngine;

public class UnitBehaviourDrawer : MonoBehaviour
{
    private ISoldier TheSoldier;
    private SpriteRenderer SpriteRenderer;
    private CommandEvent CommandAction;
    [SerializeField] Sprite EmptyIcon;
    [SerializeField] Sprite AttackIcon;
    [SerializeField] Sprite IdleIcon;
    [SerializeField] Sprite FormationIcon;
    [SerializeField] Sprite MovementIcon;
    [SerializeField] Sprite DeathIcon;

    void Awake()
    {
        SpriteRenderer = GetComponent<SpriteRenderer>();
        TheSoldier = GetComponentInParent<ISoldier>();
        CommandAction = TheSoldier.NewCommandEvent;
        CommandAction.AddListener(DrawCommandIcon);
    }

    private void DrawCommandIcon(SoldierCommand command) {
        Sprite icon;
        switch (command) {
            case SoldierCommand.Attack:
                icon = AttackIcon;
                break;
            case SoldierCommand.InOutpost:
                icon = IdleIcon;
                break;
            case SoldierCommand.Following:
                //icon = MovementIcon;
                icon = EmptyIcon;
                break;
            case SoldierCommand.Die:
                icon = DeathIcon;
                break;
            default:
                icon = EmptyIcon;
                break;
        }

        SpriteRenderer.sprite = icon;
    }

}
