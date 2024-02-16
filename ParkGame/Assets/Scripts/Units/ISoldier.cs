using UnityEngine;
using UnityEngine.Events;
using Unity.Netcode;
using Unity.Netcode.Components;
using Managers;
using System;
using Player;
using UnityEngine.AI;
using static Formation;

public enum SoldierCommand {
    InOutpost, // defensive, wary
    Following, // following commander or position in formation
    Attack,
    Die // :/
}

public class ISoldier : NetworkBehaviour, ITeamMember {
    public enum UnitType {
        Pawn,
        Archer,
        Horseman
    };

    public UnityEngine.AI.NavMeshPathStatus NavMeshStatusNow;

    // game logic
    protected Transform CommanderToFollow = null;
    public Transform TransformToFollow { get => CommanderToFollow; }
    [Header("initial values")]
    [SerializeField] protected int InitialHP = 3;
    [Header("game logic values")]
    [SerializeField] protected float BaseMovementSpeed = 1f;
    [SerializeField] protected float HorseManSpeed = 0.3f;
    [SerializeField] protected float PathMovementSpeedMultiplier = 1.5f;
    [SerializeField] protected float OuterDistanceFromCommander; // in outpost, was: sword 2, arch 2, mole 3
    [SerializeField] protected float DefendDistanceFromCommander;
    [SerializeField] protected float AttackDistanceFromCommander;
    [SerializeField] protected float MinAttackRange;
    [SerializeField] protected float MaxAttackRange;
    [SerializeField] protected float Attackcooldown = 1.0f;
    [SerializeField] protected int Damage = 1;
    [SerializeField] protected UnitType TypeOfUnit;
    protected float DeathFadeTime = 2f;
    [SerializeField] protected GameObject revealer;
    [SerializeField] protected ColorSettings colorSettings;

    public int MaxHP { get => InitialHP; }

    public UnitType Type { get => TypeOfUnit; }

    protected NetworkVariable<int> _HP = new();
    public int HP { get => _HP.Value; set => _HP.Value = value; }
    protected NetworkVariable<int> _Team = new(-1);
    public int Team { get => _Team.Value; set => _Team.Value = value; }
    protected NetworkVariable<bool> _ReturningToOutpost = new(false);
    public bool ReturningToOutpost { get => _ReturningToOutpost.Value; set => _ReturningToOutpost.Value = value; }
    protected EnemyObserver EnemyObserver;
    protected float AttackTimer = 0.0f;

    // animation
    protected static readonly int AnimatorMovementSpeedHash = Animator.StringToHash("MovementSpeed");
    protected static readonly int AnimatorDirection = Animator.StringToHash("Direction");
    protected SpriteRenderer SpriteRenderer;
    protected NetworkAnimator Networkanimator;
    protected NetworkVariable<bool> XSpriteFlip = new(false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    // formations
    protected UnityEngine.AI.NavMeshAgent Agent;
    public bool FollowInNavMeshFormation;
    public Formation FormationFromFollowedCommander;
    public GameObject ObjectToFollowInFormation; // other formation
    public FormationType FormationType;
    protected float Radius; // until what distance will follow some object - commander, outpost or castle

    public Vector3 midPointPositionForHorseman;
    public bool midPointReached;
    public Transform targetedEnemy;

    protected ShootScript shooting;
    protected PlayerManager playerManager;
    protected ChangeMaterial changeMaterial;
    protected PathTileChecker pathTileChecker;
    
    protected SpriteRenderer circleRenderer;
    protected GameSessionManager gameSessionManager;

    public Action OnDeath;
    protected int EnemiesInAttackWaveCounter; // counter in attack wave - how many were targeted in a row during one attack command

    public NetworkVariable<SoldierCommand> _SoldierCommand = new NetworkVariable<SoldierCommand>();
    public SoldierCommand Command { get => _SoldierCommand.Value; set => _SoldierCommand.Value = value; }

    public UnityEvent CommandChangedEvent;
    public UnityEvent SpeakEvent;
    protected bool isDead;
    protected float timeToDeath;

    public void SetCommanderToFollow(Transform go) { }
    public virtual void TakeDamage(int damage) { }
    //public virtual void NavMeshFormationSwitch(bool enable, Formation formation, Formation.FormationType type=Formation.FormationType.Free);
    virtual public void NewCommand(SoldierCommand command) { }

    public void NavMeshFormationSwitch(bool enable, Formation formation, FormationType formationType) {
        // if in Circle or Box Formation or Free, it is following something
        //Command = SoldierCommand.Following;
        FollowInNavMeshFormation = enable;

        if (!enable) { // disable, Unsubscribe from formation
            formation.RemoveFromFormation(gameObject, ObjectToFollowInFormation, FormationType);
            ObjectToFollowInFormation = null;

        } else {
            FormationFromFollowedCommander = formation;
            FormationType = formationType;
            switch (FormationType) {
                case FormationType.Circle:
                    ObjectToFollowInFormation = FormationFromFollowedCommander.GetPositionInFormation(gameObject, FormationType.Circle);
                    break;
                case FormationType.Box:
                    ObjectToFollowInFormation = FormationFromFollowedCommander.GetPositionInFormation(gameObject, FormationType.Box);
                    break;
                default:
                    break;
            }
        }
    }

    protected virtual void Initialize() {
        playerManager = FindObjectOfType<PlayerManager>();
        gameSessionManager = FindObjectOfType<GameSessionManager>();
        EnemyObserver = GetComponentInChildren<EnemyObserver>();
        SpriteRenderer = GetComponent<SpriteRenderer>();
        Agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        Networkanimator = GetComponent<NetworkAnimator>();
        shooting = GetComponent<ShootScript>();
        changeMaterial = GetComponent<ChangeMaterial>();
        circleRenderer = transform.Find("Circle")?.GetComponent<SpriteRenderer>();
        pathTileChecker = FindObjectOfType<PathTileChecker>();

        if (IsServer) HP = InitialHP;
        FormationType = FormationType.Free;
        isDead = false;
        timeToDeath = DeathFadeTime;
        Radius = UnityEngine.Random.Range(0.2f, 1.2f);
    }
    /*
    private Initialize - bez shooting there

     
     */

    [ClientRpc]
    protected void PlayDeathSFXClientRpc() {
        AudioManager.Instance.PlayDead(transform.position);
    }

    [ClientRpc]
    protected void PlaySwordAttackSFXClientRpc() {
        AudioManager.Instance.PlayPawnAttack(transform.position);
    }
    [ClientRpc]
    protected void PlayArcherAttackSFXClientRpc() {
        AudioManager.Instance.PlayArcherAttack(transform.position);
    }
    [ClientRpc]
    protected void PlaySelectedDwarfSFXClientRpc(ClientRpcParams clientRpcParams = default) {
        AudioManager.Instance.PlayClickOnDwarf(transform.position);
    }
}