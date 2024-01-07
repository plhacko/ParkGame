using UnityEngine;
using UnityEngine.Events;
using Unity.Netcode;
using Unity.Netcode.Components;
using Managers;
using System;
using Player;
using UnityEngine.AI;
using static Formation;
using DG.Tweening;


public class Soldier : NetworkBehaviour, ISoldier
{
    public enum UnitType
    {
        Pawn,
        Archer,
        Horseman
    };

    public NavMeshPathStatus NavMeshStatusNow;

    // game logic
    private Transform CommanderToFollow = null;
    public Transform TransformToFollow { get => CommanderToFollow; }
    [Header("initial values")]
    [SerializeField] int InitialHP = 3;
    [Header("game logic values")]
    [SerializeField] float MovementSpeed = 1;
    [SerializeField] float InnerDistanceFromCommander;
    [SerializeField] float OuterDistanceFromCommander;
    [SerializeField] float DefendDistanceFromCommander;
    [SerializeField] float AttackDistanceFromCommander;
    //[SerializeField] float AttackRange = 0.4f; // old pawn
    [SerializeField] float MinAttackRange;
    [SerializeField] float MaxAttackRange;
    [SerializeField] float Attackcooldown = 1.0f;
    [SerializeField] int Damage = 1;
    [SerializeField] UnitType TypeOfUnit;
    [SerializeField] float DeathFadeTime = 2f;
    [SerializeField] private GameObject revealer;
    [SerializeField] ColorSettings colorSettings;
    
    public int MaxHP { get => InitialHP; }

    public float ClosestEnemyDEBUG; // DEBUG // TODO: rm

    public UnitType Type { get => TypeOfUnit; }

    private NetworkVariable<int> _HP = new();
    public int HP { get => _HP.Value; set => _HP.Value = value; }
    private NetworkVariable<int> _Team = new(-1);
    public int Team { get => _Team.Value; set => _Team.Value = value; }
    private NetworkVariable<SoldierBehaviour> _SoldierBehaviour = new();
    public SoldierBehaviour SoldierBehaviour { get => _SoldierBehaviour.Value; set => _SoldierBehaviour.Value = value; } // derived form ISoldier
    public SoldierBehaviour Behaviour;
    public UnityEvent BehaviourChangedEvent;
    EnemyObserver EnemyObserver;
    //private float AttackTimer = 0.0f;
    public float AttackTimer = 0.0f; // public for debug

    // animation
    private static readonly int AnimatorMovementSpeedHash = Animator.StringToHash("MovementSpeed");
    private static readonly int AnimatorDirection = Animator.StringToHash("Direction");
    private SpriteRenderer SpriteRenderer;
    private NetworkAnimator Networkanimator;
    private NetworkVariable<bool> XSpriteFlip = new(false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    // formations
    private NavMeshAgent Agent;
    public bool FollowInNavMeshFormation;
    private SoldierBehaviour PrevSoldierBehaviour;
    public Formation FormationFromFollowedCommander;
    public GameObject ObjectToFollowInFormation; // other formation
    public FormationType FormationType;

    public Vector3 midPointPositionForHorseman;
    public bool midPointReached;
    public Transform targetedEnemy;

    private ShootScript shooting;
    private PlayerManager playerManager;
    private ChangeMaterial changeMaterial;
    private SpriteRenderer circleRenderer;
    private GameSessionManager gameSessionManager;

    public Action OnDeath;

    private int EnemiesInAttackWaveCounter; // counter in attack wave - how many were targeted in a row during one attack command
    //public SoldierCommand Command;
    private NetworkVariable<SoldierCommand> _SoldierCommand = new();
    public SoldierCommand Command { get => _SoldierCommand.Value; set => _SoldierCommand.Value = value; } 

    public UnityEvent CommandChangedEvent;


    private void Initialize()
    {
        playerManager = FindObjectOfType<PlayerManager>();
        gameSessionManager = FindObjectOfType<GameSessionManager>();
        EnemyObserver = GetComponentInChildren<EnemyObserver>();
        SpriteRenderer = GetComponent<SpriteRenderer>();
        Agent = GetComponent<NavMeshAgent>();
        Networkanimator = GetComponent<NetworkAnimator>();
        shooting = GetComponent<ShootScript>();
        changeMaterial = GetComponent<ChangeMaterial>();
        circleRenderer = transform.Find("Circle")?.GetComponent<SpriteRenderer>();
        
        _Team.OnValueChanged += OnTeamChanged;
        _SoldierBehaviour.OnValueChanged += OnBehaviourChange;

        OnBehaviourChange(0, SoldierBehaviour);
        SpriteRenderer.flipX = XSpriteFlip.Value;

        if (IsServer) HP = InitialHP;
        
        XSpriteFlip.OnValueChanged += OnXSpriteFlipChanged;

        _SoldierCommand.OnValueChanged += OnCommandChange;
        NewCommand(SoldierCommand.InOutpost);


    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Initialize();
    }

    private void OnXSpriteFlipChanged(bool previousValue, bool newValue) => SpriteRenderer.flipX = newValue;

    private void OnTeamChanged(int previousValue, int newValue) //DEBUG (just tem membership visualization) // TODO: rm
    {
        if (circleRenderer != null && newValue != -1)
        {
            circleRenderer.color = colorSettings.Colors[newValue].Color;
        }

        var localPlayerData = LobbyManager.Singleton.GetLocalPlayerData();
        if (localPlayerData.Team == newValue)
        {
            revealer.SetActive(true);
            changeMaterial.Change(false);
        }
        else
        {
            revealer.SetActive(false);
            changeMaterial.Change(true);
        }
    }

    public void OnBehaviourChange(SoldierBehaviour previousValue, SoldierBehaviour newValue)
    {
        BehaviourChangedEvent.Invoke();
    }

    public void OnCommandChange(SoldierCommand c1, SoldierCommand c2) {
        CommandChangedEvent.Invoke();
    }

    Transform ClosestOutpost()
    {
        GameObject selectedCommander = gameObject; // just to be sure that something is returned
        float shortestDist = float.PositiveInfinity;

        var outposts = FindObjectsOfType<Outpost>();

        foreach (var iCom in outposts)
        {
            if (iCom.Team == Team)
            {
                float distCom = Vector3.Distance(transform.position, iCom.gameObject.transform.position);
                if (distCom < shortestDist)
                {
                    shortestDist = distCom;
                    selectedCommander = iCom.gameObject;
                }
            }
        }

        return selectedCommander.transform;

    }

    void StationedInOutpost() 
    {
        float distanceFromOutpost = Vector3.Distance(CommanderToFollow.position, transform.position);
        if (distanceFromOutpost <= OuterDistanceFromCommander) { // have a value for every unit the same depending on the outpost's range???
            Networkanimator.Animator.SetFloat(AnimatorMovementSpeedHash, 0.0f);
            Agent.SetDestination(transform.position);
        }

        Transform enemyT = EnemyObserver.GetClosestEnemy();
        float distanceOfEnemyToOutpost = Vector3.Distance(enemyT.position, CommanderToFollow.position);

        if (enemyT != null && distanceFromOutpost < DefendDistanceFromCommander) {
            if (AttackEnemyIfInRange(enemyT)) { return; }
            else if (distanceOfEnemyToOutpost <= DefendDistanceFromCommander) 
            { 
                MoveTowardsEntity(enemyT);
                return;
            }
        } else {
            FollowObjectWithAnimation(CommanderToFollow);
            return;
        }
    }

    void Follow() {
        if (ObjectToFollowInFormation != null) { // attack vlastne znici jejich puvodni formaci
            FollowObjectWithAnimation(ObjectToFollowInFormation.transform);
        } else if (CommanderToFollow != null) {
            FollowObjectWithAnimation(CommanderToFollow);
        }
    }

    void AttackOnCommand() {
        Transform enemyT = GetEnemy();
        if (enemyT != null) { 
            targetedEnemy = enemyT;
            EnemiesInAttackWaveCounter++;
        } else { // no enemy is close by, stay where you are
            Follow();
            return; 
        }

        // attack the closest enemy if in range
        if (AttackEnemyIfInRange(targetedEnemy)) { return; }
        // go closer to the enemy 
        FollowObjectWithAnimation(targetedEnemy);

        // if the commander is too far, the soldier will stop attacking and will return back to the commander
        float distanceFromCommander = (CommanderToFollow.position - transform.position).magnitude;
        if (distanceFromCommander > AttackDistanceFromCommander) {
            Follow();
        }
    }

    void Update()
    {
        // following is done only on server
        if (!IsServer)
        { return; }
        
        if(gameSessionManager.IsOver) return;

        // check for a Commander
        if (CommanderToFollow == null)
        {
            return;
        }

        if (HP <= 0)
        {
            return;
        }

        NavMeshStatusNow = Agent.pathStatus;

        // attack timer
        if (AttackTimer <= Attackcooldown)
        { AttackTimer += Time.deltaTime; }

        if (TypeOfUnit == UnitType.Horseman)
        {
            Agent.speed = 3.5f;
        }

        switch (Command) {
            case SoldierCommand.InOutpost:
                StationedInOutpost();
                break;
            case SoldierCommand.Following:
                Follow();
                break;
            case SoldierCommand.Attack:
                AttackOnCommand(); 
                break;
            default:
                break;
        }
    }

    private void IdleBehaviour()
    {
        Transform enemyT = GetEnemy();
        float distanceFromCommander = Vector3.Distance(CommanderToFollow.position, transform.position);
        if (enemyT != null && distanceFromCommander < DefendDistanceFromCommander)
        {
            if (AttackEnemyIfInRange(enemyT)) { return; }

            MoveTowardsEntity(enemyT);
            return;
        }

        if (distanceFromCommander > OuterDistanceFromCommander)
        {
            MoveTowardsEntity(CommanderToFollow);
            SoldierBehaviour = SoldierBehaviour.Move;
        }
        else
        {
            Networkanimator.Animator.SetFloat(AnimatorMovementSpeedHash, 0.0f);
            Agent.SetDestination(transform.position);
        }
    }

    public void NavMeshFormationSwitch(bool enable, SoldierBehaviour newBehaviour, Formation formation, FormationType formationType)
    {
        // if in Circle or Box Formation or Free, it is following something
        //Command = SoldierCommand.Following;
        
        FollowInNavMeshFormation = enable;
        //PrevSoldierBehaviour = SoldierBehaviour;
        SoldierBehaviour = newBehaviour;

        if (!enable)
        { // disable, Unsubscribe from formation
            formation.RemoveFromFormation(gameObject, ObjectToFollowInFormation, FormationType);
            ObjectToFollowInFormation = null;

        }
        else
        {
            FormationFromFollowedCommander = formation;
            FormationType = formationType;
            switch (FormationType)
            {
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
    public UnitType GetUnitType()
    {
        return TypeOfUnit;
    }
    private Direction GetDirectionEnum(Vector2 d)
    {
        if (Mathf.Abs(d.x) > Mathf.Abs(d.y))
        {
            return d.x > 0 ? Direction.Right : Direction.Left;
        }
        else
        {
            return d.y > 0 ? Direction.Up : Direction.Down;
        }
    }
    private void FollowObjectWithAnimation(Transform toFollow)
    {
        Agent.SetDestination(toFollow.position);
        Vector2 direction = toFollow.position - gameObject.transform.position;

        Direction directionE = GetDirectionEnum(direction);
        
        if (direction.magnitude < 0.001f) 
        {Networkanimator.Animator.SetFloat(AnimatorMovementSpeedHash, 0.0f);
            Networkanimator.Animator.SetFloat(AnimatorMovementSpeedHash, 0.0f);
            Agent.SetDestination(transform.position);
        } else
        {
            Networkanimator.Animator.SetFloat(AnimatorMovementSpeedHash, 1.0f);
            Networkanimator.Animator.SetInteger(AnimatorDirection, (int)directionE);
        }
        
        XSpriteFlip.Value = directionE == Direction.Left;
    }

    private Transform GetEnemy()
    {
        if (targetedEnemy != null)
        {
            return targetedEnemy;
        }
        Transform enemyT = EnemyObserver.GetClosestEnemy();
        if (TypeOfUnit == UnitType.Archer)
        {
            enemyT = EnemyObserver.GetEnemyInRange(MinAttackRange, MaxAttackRange);
        }
        if (TypeOfUnit == UnitType.Horseman)
        {
            if (EnemiesInAttackWaveCounter == 0) {
                enemyT = EnemyObserver.GetFarthestEnemy(); // else attack the closest enemy
            }
        }
        targetedEnemy = enemyT;
        return enemyT;
    }

    /// <summary> attacks the closest enemy in range</summary>
    /// <returns> returns if enemy was in range </returns>
    private bool AttackEnemyIfInRange(Transform enemyT)
    {
        ClosestEnemyDEBUG = Vector3.Distance(enemyT.position, transform.position);
        if (Vector3.Distance(enemyT.position, transform.position) <= MaxAttackRange
            && Vector3.Distance(enemyT.position, transform.position) >= MinAttackRange)
        {
            if (AttackTimer >= Attackcooldown)
            {
                AttackTimer = 0.0f;
                Networkanimator.Animator.SetFloat(AnimatorMovementSpeedHash, 0.0f); //("MovementSpeed", 0);

                Networkanimator.SetTrigger("Attack");


                if (TypeOfUnit == UnitType.Pawn || TypeOfUnit == UnitType.Horseman)
                {
                    enemyT.GetComponent<ISoldier>()?.TakeDamage(Damage);

                }
                if (TypeOfUnit == UnitType.Archer)
                {
                    Vector2 direction = enemyT.position - transform.position;
                    Direction directionE = GetDirectionEnum(direction);
                    bool flip = directionE == Direction.Left;
                    
                    XSpriteFlip.Value = flip;
                    Debug.Log("shoot");
                    shooting.Shoot(enemyT.transform.position, Damage, flip, Team);
                }
            }
            return true;
        }
        return false;
    }

    private void MoveTowardsEntity(Transform entityT)
    {
        // archers, don't go closer! you'd just die 
        if (Vector3.Distance(entityT.position, transform.position) < MinAttackRange && TypeOfUnit == UnitType.Archer)
        {
            SoldierBehaviour = SoldierBehaviour.Idle;
            return;
        }

        FollowObjectWithAnimation(entityT);
    }

    public void OnMouseDown()
    {
        Debug.Log("Sprite Clicked");
        if(gameSessionManager.IsOver) return;
        
        ulong clientID = NetworkManager.Singleton.LocalClientId;
        RequestChangingCommanderToFollowServerRpc(clientID: clientID);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestChangingCommanderToFollowServerRpc(ulong clientID)
    {
        PlayerController playerController = playerManager.GetPlayerController(clientID);
        if (playerController != null && playerController.Team == Team)
        {
            if (playerController.gameObject.transform != CommanderToFollow)
            {
                SetCommanderToFollow(playerController.gameObject.transform);
                FormationType = CommanderToFollow.GetComponent<ICommander>().GetFormation(); // get type of formation
                FormationFromFollowedCommander = CommanderToFollow.GetComponent<Formation>();

                if (FormationType == FormationType.Box || FormationType == FormationType.Circle)
                {
                    SoldierBehaviour = SoldierBehaviour.Formation;
                    NavMeshFormationSwitch(true, SoldierBehaviour.Formation, FormationFromFollowedCommander, FormationType);
                }
                else
                {
                    SoldierBehaviour = SoldierBehaviour.Move;
                }
            }
            else
            {
                var closestOutpost = ClosestOutpost();
                SetCommanderToFollow(closestOutpost);
                NavMeshFormationSwitch(false, SoldierBehaviour.Idle, FormationFromFollowedCommander, FormationType.Free);
            }
        }
    }

    /// <summary> !call only on server! </summary>
    public void SetCommanderToFollow(Transform commanderToFollow)
    {
        if (!IsServer)
        { throw new Exception($"only server can set what the unit ({gameObject.name}) can follow ({CommanderToFollow?.name})"); }

        if (CommanderToFollow != commanderToFollow) // change Commander to follow
        {
            CommanderToFollow?.GetComponent<ICommander>().ReportUnfollowing(gameObject);
            CommanderToFollow = commanderToFollow;
            CommanderToFollow?.GetComponent<ICommander>().ReportFollowing(gameObject);

            Command = SoldierCommand.Following; // following commander or returning to outpost
        }
    }

    /// <summary> !call only on server! </summary>
    public void TakeDamage(int damage)
    {
        if (!IsServer)
        { throw new Exception($"soldier ({gameObject.name}) can take damage only on server"); }
        Debug.Log("take damage");
        int hp = HP - damage;
        if (hp <= 0) { Die(); }
        else { HP = hp; }
    }

    public Transform GetCommanderWhomIFollow()
    {
        return CommanderToFollow;
    }

    /// <summary> !call only on server! </summary>
    public void Die()
    {
        HP = 0;
        SoldierBehaviour = SoldierBehaviour.Death;
        Agent.SetDestination(transform.position);
        CommanderToFollow?.GetComponent<ICommander>()?.ReportUnfollowing(gameObject);
        Networkanimator.SetTrigger("Die");
        handleDeath();
        handleDeathClientRpc();
    }

    [ClientRpc]
    public void handleDeathClientRpc(ClientRpcParams clientRpcParams = default)
    {
        if (IsServer) return;
        handleDeath();
    }

    private void handleDeath()
    {
        // visualize death: black shadow, fade soldier's sprite, and then self-destruct
        gameObject.transform.Find("Circle").GetComponent<SpriteRenderer>().color = Color.black;
        gameObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
        gameObject.GetComponent<SpriteRenderer>()?.DOFade(0, DeathFadeTime);

        if (IsServer)
        {
            OnDeath?.Invoke();
            Destroy(gameObject, DeathFadeTime);
        }
    }

    public void NewCommand(SoldierCommand command) {
        Command = command;

        if (command == SoldierCommand.Attack) {
            EnemiesInAttackWaveCounter = 0; // reset counter of targeted enemies (because of moleman's modus operandi)
        }
    }
}
