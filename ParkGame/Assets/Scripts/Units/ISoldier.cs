using UnityEngine;
using UnityEngine.Events;
using Unity.Netcode;
using Unity.Netcode.Components;
using Managers;
using System;
using Player;
using UnityEngine.AI;
using static Formation;
using Unity.Collections.LowLevel.Unsafe;

public enum SoldierCommand {
    InOutpost, // defensive, wary
    Following, // following commander or position in formation
    Attack,
    Die // :/
}

public class CommandEvent : UnityEvent<SoldierCommand> { }

public class ISoldier : NetworkBehaviour, ITeamMember {
    public enum UnitType {
        Pawn,
        Archer,
        Horseman
    };

    // game logic
    protected Transform CommanderToFollow = null;
    [Header("initial values")]
    [SerializeField] protected int InitialHP = 3;
    [Header("game logic values")]
    [SerializeField] protected float BaseMovementSpeed = 1f;
    [SerializeField] protected float PathMovementSpeedMultiplier = 1.5f;
    [SerializeField] protected float DefendDistanceFromCommander;
    [SerializeField] protected float AttackDistanceFromCommander;
    [SerializeField] protected float MinAttackRange;
    [SerializeField] protected float MaxAttackRange;
    [SerializeField] protected float Attackcooldown = 1.0f;
    [SerializeField] protected int Damage = 1;
    protected float DeathFadeTime = 2f;
    [SerializeField] protected GameObject revealer;
    [SerializeField] protected ColorSettings colorSettings;

    public int MaxHP { get => InitialHP; }
    
    protected UnitType TypeOfUnit;
    public UnitType Type { get => TypeOfUnit; }

    protected NetworkVariable<int> _HP = new();
    public int HP { get => _HP.Value; set => _HP.Value = value; }
    protected NetworkVariable<int> _Team = new(-1);
    public int Team { get => _Team.Value; set => _Team.Value = value; }
    protected NetworkVariable<bool> _ReturningToOutpost = new(false);
    protected bool ReturningToOutpost { get => _ReturningToOutpost.Value; set => _ReturningToOutpost.Value = value; }
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
    protected NavMeshAgent Agent;
    protected Formation FormationFromFollowedCommander;
    private GameObject ObjectToFollowInFormation; // other formation
    protected FormationType FormationType;
    protected float Radius; // until what distance will follow some object - commander, outpost or castle
    

    protected PlayerManager playerManager;
    protected ChangeMaterial changeMaterial;
    protected PathTileChecker pathTileChecker;
    
    protected SpriteRenderer circleRenderer;
    protected GameSessionManager gameSessionManager;

    protected Transform targetedEnemy;
    protected int EnemiesInAttackWaveCounter; // counter in attack wave - how many were targeted in a row during one attack command

    public NetworkVariable<SoldierCommand> _SoldierCommand = new NetworkVariable<SoldierCommand>();
    public SoldierCommand Command { get => _SoldierCommand.Value; set => _SoldierCommand.Value = value; }
    
    // events for icons
    public UnityEvent CommandChangedEvent;
    public UnityEvent SpeakEvent;
    public CommandEvent NewCommandEvent = new CommandEvent();

    public Action OnDeath;
    protected bool isDead;
    protected float timeToDeath;

    protected void OnXSpriteFlipChanged(bool previousValue, bool newValue) => SpriteRenderer.flipX = newValue;

    protected void OnTeamChanged(int previousValue, int newValue) //DEBUG (just tem membership visualization) // TODO: rm
    {
        if (circleRenderer != null && newValue != -1) {
            InitializeTeamColor();
        }

        var localPlayerData = LobbyManager.Singleton.GetLocalPlayerData();
        if (localPlayerData.Team == newValue) {
            revealer.SetActive(true);
            changeMaterial.Change(false);
        } else {
            revealer.SetActive(false);
            changeMaterial.Change(true);
        }
    }

    /// <summary>Colors the unit with the color of its respective team</summary>
    public void InitializeTeamColor()
    {
        Color teamColor = colorSettings.Colors[Team].Color;
        circleRenderer.color = teamColor;
        SpriteRenderer.material.SetColor("_TargetColor", teamColor);
    }

    public void OnCommandChange(SoldierCommand previousValue, SoldierCommand newValue) {
        CommandChangedEvent.Invoke();
    }

    protected virtual void SetSoldierSpeed() {
        if (playerManager.GetLocalPlayerController().IsOnPath ||
            (ReturningToOutpost && pathTileChecker.IsNearbyPath(Agent.transform.position)) // short-circuiting for efficiency
        ) {
            Agent.speed = BaseMovementSpeed * PathMovementSpeedMultiplier;
        } else {
            Agent.speed = BaseMovementSpeed;
        }
    }

    public Transform GetCommanderWhomIFollow() {
        return CommanderToFollow;
    }

    /// <summary> !call only on server! </summary>
    public void TakeDamage(int damage) {
        if (!IsServer) { throw new Exception($"soldier ({gameObject.name}) can take damage only on server"); }
        if (isDead) { return; }
        Debug.Log("take damage");
        int hp = HP - damage;
        if (hp <= 0) { Die(); } else { HP = hp; }
    }

    /// <summary> !call only on server! </summary>
    public void SetCommanderToFollow(Transform commanderToFollow) {
        if (!IsServer) { throw new Exception($"only server can set what the unit ({gameObject.name}) can follow ({CommanderToFollow?.name})"); }

        if (CommanderToFollow != commanderToFollow) // change Commander to follow
        {
            CommanderToFollow?.GetComponent<ICommander>().ReportUnfollowing(gameObject);
            CommanderToFollow = commanderToFollow;
            CommanderToFollow?.GetComponent<ICommander>().ReportFollowing(gameObject);
            NewCommand(SoldierCommand.Following);

            FormationType = CommanderToFollow.GetComponent<ICommander>().GetFormation(); // get type of formation
            FormationFromFollowedCommander = CommanderToFollow.GetComponent<Formation>();
            if (FormationType == FormationType.Box || FormationType == FormationType.Circle) {
                NavMeshFormationSwitch(true, FormationFromFollowedCommander, FormationType);
            }

        }
    }

    public void OnMouseDown() {
        Debug.Log("Sprite Clicked");
        if (gameSessionManager.IsOver) return;

        ulong clientID = NetworkManager.Singleton.LocalClientId;
        //    RequestChangingCommanderToFollowServerRpc(clientID: clientID);
    }

    protected virtual bool AttackEnemyIfInRange(Transform enemyT, float maxAttackDistance = 0) {
        float maxRange = MaxAttackRange;
        if (maxAttackDistance > 0) {
            maxRange = maxAttackDistance;
        }
        if (Vector3.Distance(enemyT.position, transform.position) <= maxRange
            && Vector3.Distance(enemyT.position, transform.position) >= MinAttackRange) {
            if (AttackTimer >= Attackcooldown) {
                AttackTimer = 0.0f;
                Networkanimator.Animator.SetFloat(AnimatorMovementSpeedHash, 0.0f);

                Networkanimator.SetTrigger("Attack");

                PlaySwordAttackSFXClientRpc();
                enemyT.GetComponent<ISoldier>()?.TakeDamage(Damage);

            }
            return true;
        }
        return false;
    }

    protected virtual Transform GetEnemy() {
        if (targetedEnemy != null) {
            return targetedEnemy;
        }
        Transform enemyT = EnemyObserver.GetClosestEnemy();
        targetedEnemy = enemyT;
        return enemyT;
    }

    [ClientRpc]
    private void NewCommandClientRpc(SoldierCommand command) {
        NewCommandEvent.Invoke(command);
    }

    public void NewCommand(SoldierCommand command) {
        if (!IsServer) { return; }
        Command = command;
        NewCommandClientRpc(command);

        if (command == SoldierCommand.Attack) {
            EnemiesInAttackWaveCounter = 0;
        }
    }

    public void NavMeshFormationSwitch(bool enable, Formation formation, FormationType formationType) {
        // if in Circle or Box Formation or Free, it is following something
        if (!enable) { // disable, unsubscribe from formation
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

    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();
        Initialize();
    }

    protected virtual void Initialize() {
        playerManager = FindObjectOfType<PlayerManager>();
        gameSessionManager = FindObjectOfType<GameSessionManager>();
        EnemyObserver = GetComponentInChildren<EnemyObserver>();
        SpriteRenderer = GetComponent<SpriteRenderer>();
        Agent = GetComponent<NavMeshAgent>();
        Networkanimator = GetComponent<NetworkAnimator>();
        changeMaterial = GetComponent<ChangeMaterial>();
        circleRenderer = transform.Find("Circle")?.GetComponent<SpriteRenderer>();
        pathTileChecker = FindObjectOfType<PathTileChecker>();

        _Team.OnValueChanged += OnTeamChanged;
        SpriteRenderer.flipX = XSpriteFlip.Value;
        XSpriteFlip.OnValueChanged += OnXSpriteFlipChanged;
        _SoldierCommand.OnValueChanged += OnCommandChange;
        OnCommandChange(0, Command);

        if (IsServer) {
            HP = InitialHP;
        }
        FormationType = FormationType.Box;
        isDead = false;
        timeToDeath = DeathFadeTime;
        Radius = UnityEngine.Random.Range(0.2f, 1.2f);
    }

    void Update() {
        // following is done only on server
        if (!IsServer) { return; }

        if (gameSessionManager.IsOver || !IsSpawned) return;

        // check for a Commander
        if (CommanderToFollow == null) {
            return;
        }

        if (HP <= 0 && !isDead) {
            Die();
            return;
        }
        if (isDead && timeToDeath > 0) {
            timeToDeath -= Time.deltaTime;
            return;
        }
        if (isDead && timeToDeath <= 0) {
            Destroy(this.gameObject);
        }

        // attack timer
        if (AttackTimer <= Attackcooldown) { AttackTimer += Time.deltaTime; }

        SetSoldierSpeed();

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

    void StationedInOutpost() {
        float distanceFromOutpost = Vector3.Distance(CommanderToFollow.position, transform.position);
        if (distanceFromOutpost <= DefendDistanceFromCommander) { 
            Networkanimator.Animator.SetFloat(AnimatorMovementSpeedHash, 0.0f);
            Agent.SetDestination(transform.position);
        }

        Transform enemyT = EnemyObserver.GetClosestEnemy();

        if (enemyT != null && distanceFromOutpost < DefendDistanceFromCommander) {
            float distanceOfEnemyToOutpost = Vector3.Distance(enemyT.position, CommanderToFollow.position);
            if (AttackEnemyIfInRange(enemyT, DefendDistanceFromCommander)) {
                return;
            } else if (distanceOfEnemyToOutpost <= DefendDistanceFromCommander) {
                MoveTowardsEntity(enemyT);
                return;
            }
        } else {
            FollowObjectWithAnimation(CommanderToFollow);
            return;
        }
    }

    void Follow() {
        if (ObjectToFollowInFormation != null) {
            FollowObjectWithAnimation(ObjectToFollowInFormation.transform, true); // follow precisely object in formation
        } else if (CommanderToFollow != null) {
            FollowObjectWithAnimation(CommanderToFollow); // follow till some distance (commander in free formation or outpost)
        }
    }

    void AttackOnCommand() {
        Transform enemyT = GetEnemy();
        if (enemyT != null) {
            if (enemyT != targetedEnemy) {
                EnemiesInAttackWaveCounter++;
            }
            targetedEnemy = enemyT;
        } else {
            // or reset the formation positions upon calling Attack?
            Follow();
            return;
        }

        // attack the targeted enemy if in range
        if (AttackEnemyIfInRange(targetedEnemy)) { return; }
        // go closer to the enemy 
        FollowObjectWithAnimation(targetedEnemy, true);

        // if the commander is too far, the soldier will stop attacking and will return back to the commander
        float distanceFromCommander = (CommanderToFollow.position - transform.position).magnitude;
        if (distanceFromCommander > AttackDistanceFromCommander) {
            Follow();
        }
    }

    public bool IsFollowingCommander() {
        if (CommanderToFollow?.GetComponent<PlayerController>()) {
            return true;
        }
        return false;

    }

    public UnitType GetUnitType() {
        return TypeOfUnit;
    }
    public static Direction GetDirectionEnum(Vector2 d) {
        if (Mathf.Abs(d.x) > Mathf.Abs(d.y)) {
            return d.x > 0 ? Direction.Right : Direction.Left;
        } else {
            return d.y > 0 ? Direction.Up : Direction.Down;
        }
    }

    // precise: follow directly to the position
    // ! precise: follow in free formation commander or within outposts
    protected void FollowObjectWithAnimation(Transform toFollow, bool precise = false) {
        Vector3 toFollowPosition = new Vector3(toFollow.position.x, toFollow.position.y, transform.position.z);
        Agent.SetDestination(toFollowPosition);
        Vector2 direction = toFollowPosition - gameObject.transform.position;

        // get back to the outpost
        Direction directionE = GetDirectionEnum(direction);
        if (direction.magnitude < 1f && Command != SoldierCommand.InOutpost) {
            if (toFollow == CommanderToFollow && CommanderToFollow.GetComponent<Outpost>()) {
                NewCommand(SoldierCommand.InOutpost);
            }
        }

        // got somewhere
        if ((direction.magnitude < Radius && !precise) || (direction.magnitude < 0.1f && precise)) {
            Networkanimator.Animator.SetFloat(AnimatorMovementSpeedHash, 0.0f);
            Agent.SetDestination(transform.position);
        } else {
            Networkanimator.Animator.SetFloat(AnimatorMovementSpeedHash, 1.0f);
            Networkanimator.Animator.SetInteger(AnimatorDirection, (int)directionE);
        }

        XSpriteFlip.Value = directionE == Direction.Left;
    }

    protected virtual void MoveTowardsEntity(Transform entityT) {
        FollowObjectWithAnimation(entityT, true);
    }

    private Transform ClosestOutpost() {
        GameObject selectedCommander = gameObject; // just to be sure that something is returned
        float shortestDist = float.PositiveInfinity;

        var outposts = FindObjectsOfType<Outpost>();

        foreach (var iCom in outposts) {
            if (iCom.Team == Team) {
                float distCom = Vector3.Distance(transform.position, iCom.gameObject.transform.position);
                if (distCom < shortestDist) {
                    shortestDist = distCom;
                    selectedCommander = iCom.gameObject;
                }
            }
        }
        return selectedCommander.transform;
    }

    [ClientRpc]
    private void SpeakClientRpc() {
        SpeakEvent.Invoke();
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestChangingCommanderToFollowServerRpc(ulong clientID, bool random = false) {
        PlayerController playerController = playerManager.GetPlayerController(clientID);
        if (playerController != null && playerController.Team == Team) {

            // play dwarf's 'mrouk'
            ClientRpcParams clientRpcParams = new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { clientID } } };
            // random false: play sfx every click on unit
            // random true: play only sometimes, on gathering call
            if (!random || UnityEngine.Random.Range(0f, 20f) < 8f) { PlaySelectedDwarfSFXClientRpc(clientRpcParams); }

            // follow commander
            if (playerController.gameObject.transform != CommanderToFollow) {
                ReturningToOutpost = false;
                SetCommanderToFollow(playerController.gameObject.transform);
                NewCommand(SoldierCommand.Following);
                SpeakClientRpc();
                FormationType = CommanderToFollow.GetComponent<ICommander>().GetFormation(); // get type of formation
                FormationFromFollowedCommander = CommanderToFollow.GetComponent<Formation>();
                if (FormationType == FormationType.Box || FormationType == FormationType.Circle) {
                    NavMeshFormationSwitch(true, FormationFromFollowedCommander, FormationType);
                }
            }
            // return to outpost
            else {
                ReturningToOutpost = true;
                var closestOutpost = ClosestOutpost();
                FormationFromFollowedCommander?.RemoveFromFormation(gameObject, ObjectToFollowInFormation, FormationType);
                CommanderToFollow?.GetComponent<ICommander>()?.ReportUnfollowing(gameObject);
                SetCommanderToFollow(closestOutpost);
                NewCommand(SoldierCommand.Following);
                SpeakClientRpc();
                NavMeshFormationSwitch(false, FormationFromFollowedCommander, FormationType.Free);
            }
        }
    }

    private void TimeToDestroy(float time) {
        timeToDeath = time;
    }

    /// <summary> !call only on server! </summary>
    public void Die() {
        if (!IsServer) { return; }
        isDead = true;
        HP = 0;
        NewCommand(SoldierCommand.Die);

        Agent.SetDestination(transform.position);
        gameObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;

        Networkanimator.SetTrigger("Die");
        PlayDeathSFXClientRpc();
        CommanderToFollow?.GetComponent<ICommander>()?.ReportUnfollowing(gameObject);

        FormationFromFollowedCommander?.RemoveFromFormation(gameObject, ObjectToFollowInFormation, FormationType);
        OnDeath?.Invoke();
        playerManager.RemoveSoldierFromTeam(Team, transform);
        TimeToDestroy(DeathFadeTime);
    }

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