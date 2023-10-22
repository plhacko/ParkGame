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
        BehaviourEvent = TheSoldier.BehaviourChangedEvent;
        BehaviourEvent.AddListener(DrawIcon);
    }

    private void DrawIcon() {
        SoldierBehaviour behaviour = TheSoldier.SoldierBehaviour;
        Sprite icon;
        switch (behaviour) {
            case SoldierBehaviour.Attack:
                icon = AttackIcon;
                break;
            case SoldierBehaviour.Idle:
                icon = IdleIcon;
                break;
            case SoldierBehaviour.Move:
                icon = MovementIcon;
                break;
            case SoldierBehaviour.Formation:
                icon = FormationIcon;
                break;
            default: icon = EmptyIcon;
                break;
        }

        SpriteRenderer.sprite = icon;
    }

}
