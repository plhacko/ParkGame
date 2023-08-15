using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
using Managers;
using System;
using System.Windows.Input;
using UnityEngine.AI;

public class Soldier : NetworkBehaviour, ISoldier
{
    // game logic
    private Transform CommanderToFollow = null;
    [Header("initial values")]
    [SerializeField] int InitialHP = 3;
    [Header("game logic values")]
    [SerializeField] float MovementSpeed = 1;
    [SerializeField] float InnerDistanceFromCommander = 1.0f;
    [SerializeField] float OuterDistanceFromCommander = 2.0f;
    [SerializeField] float DefendDistanceFromCommander = 2.5f;
    [SerializeField] float AttackDistanceFromCommander = 6.0f;
    [SerializeField] float AttackRange = 0.4f;
    [SerializeField] float Attackcooldown = 1.0f;
    [SerializeField] int Damage = 1;

    [SerializeField] float ClosestEnemyDEBUG; // DEBUG // TODO: rm

    private NetworkVariable<int> _HP = new();
    public int HP { get => _HP.Value; set => _HP.Value = value; }
    private NetworkVariable<int> _Team = new();
    public int Team { get => _Team.Value; set => _Team.Value = value; }
    private NetworkVariable<SoldierBehaviour> _SoldierBehaviour = new();
    public SoldierBehaviour SoldierBehaviour { get => _SoldierBehaviour.Value; set => _SoldierBehaviour.Value = value; } // derived form ISoldier
    public SoldierBehaviour Behaviour;
    EnemyObserver EnemyObserver;
    private float AttackTimer = 0.0f;

    // animation
    private static readonly int AnimatorMovementSpeedHash = Animator.StringToHash("MovementSpeed");
    private SpriteRenderer SpriteRenderer;
    private Animator Animator;
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
    public ICommander.SoldierMovements shape;
    //public Vector3 PositionToFollowInFormation; // circle formation

    private void Initialize()
    {
        EnemyObserver = GetComponentInChildren<EnemyObserver>();
        SpriteRenderer = GetComponent<SpriteRenderer>();
        Animator = GetComponent<Animator>();
        Agent = GetComponent<NavMeshAgent>();
        Debug.Log("agent" + Agent);

        _Team.OnValueChanged += OnTeamChanged;
        _SoldierBehaviour.OnValueChanged += OnBehaviourChange;
        OnTeamChanged(0, Team);
        OnBehaviourChange(0, SoldierBehaviour);

        if (IsServer)
            HP = InitialHP;

        if (!IsServer)
        {
            XSpriteFlip.OnValueChanged += OnXSpriteFlipChanged;
        }
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Initialize();
    }

    private void OnXSpriteFlipChanged(bool previousValue, bool newValue) => SpriteRenderer.flipX = newValue;
    private void OnTeamChanged(int previousValue, int newValue) //DEBUG (just tem membership visualization) // TODO: rm
    {
        SpriteRenderer sr = transform.Find("Circle")?.GetComponent<SpriteRenderer>();
        if (sr == null) { return; }
        if (newValue == 0) { sr.color = Color.blue; }
        else if (newValue == 1) { sr.color = Color.yellow; }
        else { sr.color = Color.grey; }
    }
    private void OnBehaviourChange(SoldierBehaviour previousValue, SoldierBehaviour newValue)
    {
        // tmp
        Behaviour = SoldierBehaviour;
        switch (newValue)
        {
            case SoldierBehaviour.Idle:
                GetComponent<SpriteRenderer>().color = Color.green; // DEBUG // TODO: rm
                break;
            case SoldierBehaviour.Move:
                GetComponent<SpriteRenderer>().color = Color.blue; // DEBUG // TODO: rm
                break;
            case SoldierBehaviour.Attack:
                GetComponent<SpriteRenderer>().color = Color.red; // DEBUG // TODO: rm
                break;
            case SoldierBehaviour.FormationCircle:
                GetComponent<SpriteRenderer>().color = Color.yellow; // DEBUG // TODO: rm
                break;
            case SoldierBehaviour.FormationRectangle:
                GetComponent<SpriteRenderer>().color = Color.grey; // DEBUG // TODO: rm
                break;
        }
    }
    void Update()
    {
        // following is done only on server
        if (!IsServer)
        { return; }

        // check for a Commander
        if (CommanderToFollow == null)
        { return; } // TODO: add function, that finds the nearest frienly outpost

        // attack timer
        if (AttackTimer <= Attackcooldown)
        { AttackTimer += Time.deltaTime; }

        // soldier behaviour
        switch (SoldierBehaviour)
        {
            case SoldierBehaviour.Idle:
                IdleBehaviour();
                break;
            case SoldierBehaviour.Move:
                MovementBehaviour();
                break;
            case SoldierBehaviour.Attack:
                AttackBehaviour();
                break;

            // when in move range??? or setup from playercontroller
            case SoldierBehaviour.FormationCircle:
                FormationCircleBehaviour();
                break;
            case SoldierBehaviour.FormationRectangle: // todo
                FormationRectangleBehaviour();
                break;

            default:
                break;
        }
    }

    private void IdleBehaviour()
    {
        Transform enemyT = EnemyObserver.GetClosestEnemy();
        float distanceFromCommander = Vector3.Distance(CommanderToFollow.position, transform.position);
        if (enemyT != null && distanceFromCommander < DefendDistanceFromCommander)
        {
            if (AttackEnemyIfInRange(enemyT)) { return; }
            else { MoveTowardsEntity(enemyT); return; }
        }


        if (distanceFromCommander > OuterDistanceFromCommander)
        {
            MoveTowardsEntity(CommanderToFollow);
            SoldierBehaviour = SoldierBehaviour.Move;
        }
        else
        { Animator.SetFloat(AnimatorMovementSpeedHash, 0.0f); }
    }

    private void MovementBehaviour()
    {
        // TODO: think if make sense or rm
        // Transform enemyT = EnemyObserver.GetClosestEnemy();
        // if (enemyT != null)
        // { AttackEnemyIfInRAnge(enemyT); }

        //  moves to the inner circle around the commander
        if (Vector3.Distance(CommanderToFollow.position, transform.position) > InnerDistanceFromCommander)
        { MoveTowardsEntity(CommanderToFollow); }
        else
        {
            SoldierBehaviour = SoldierBehaviour.Idle;
            Animator.SetFloat(AnimatorMovementSpeedHash, 0.0f);
        }
    }

    public void NavMeshFormationSwitch(bool enable, SoldierBehaviour newBehaviour, Formation formation, ICommander.SoldierMovements formationShape) {
        FollowInNavMeshFormation = enable;
        shape = formationShape;

        if (!enable) { // disable
            SoldierBehaviour = SoldierBehaviour.Idle;//PrevSoldierBehaviour;
            
            // unsubscribe
            // todo
            formation.RemoveFromFormation(gameObject, ObjectToFollowInFormation); // remove from list, recount circle if in cirle
            ObjectToFollowInFormation = null;

            //FormationFromFollowedCommander = null;

        } else {
            PrevSoldierBehaviour = SoldierBehaviour;
            SoldierBehaviour = newBehaviour;
            FormationFromFollowedCommander = formation;
            // todo
            switch (SoldierBehaviour) {
                case SoldierBehaviour.FormationCircle:
                    //NavMeshFormations.GetFormation(SoldierBehaviour.FormationCircle);
                    
                    ObjectToFollowInFormation = FormationFromFollowedCommander.GetFormation(gameObject, Formation.FormationShape.Circle); 

                    Debug.Log("follow in circle " + gameObject.name + " " + ObjectToFollowInFormation.name); // todo
                    //PositionToFollowInFormation = CommanderToFollow.position;
                    //ObjectToFollowInFormation = CommanderToFollow.gameObject;
                    break;
                case SoldierBehaviour.FormationRectangle:
                    //NavMeshFormations.GetFormation(SoldierBehaviour.FormationRectangle);
                    FormationFromFollowedCommander.GetFormation(gameObject, Formation.FormationShape.Rectangle);
                    Debug.Log("follow in rectangle"); // todo
                    ObjectToFollowInFormation = CommanderToFollow.gameObject;
                    break;

            }
            //NavMeshFormations.GetFormation();
        }
    }

    private void FormationCircleBehaviour() {
        if (FollowInNavMeshFormation) {
            //Agent.SetDestination(PositionToFollowInFormation);
            if (!ObjectToFollowInFormation) {
                Debug.Log("no object to follow!!!!!!");
                ObjectToFollowInFormation = FormationFromFollowedCommander.GetFormation(gameObject, Formation.FormationShape.Circle);
                Debug.Log("no object to follow?" + ObjectToFollowInFormation.name);

                return;
            }

            Agent.SetDestination(ObjectToFollowInFormation.transform.position);
            
            Vector2 direction = ObjectToFollowInFormation.transform.position - gameObject.transform.position;
        //    Debug.Log("direction 3d" + (ObjectToFollowInFormation.transform.position - gameObject.transform.position));
        //    Debug.Log("direction 2d" + direction);
            if (direction.magnitude < 0.001f) {
                Animator.SetFloat(AnimatorMovementSpeedHash, 0.0f);
            } else {
                Animator.SetFloat(AnimatorMovementSpeedHash, 1.0f);
            }
            SpriteRenderer.flipX = direction.x < 0;
            XSpriteFlip.Value = SpriteRenderer.flipX;
            
        }
    }
    
    private void FormationRectangleBehaviour() {
        if (FollowInNavMeshFormation) {
            Agent.SetDestination(ObjectToFollowInFormation.transform.position);
        }
    }

    private void AttackBehaviour()
    {
        Transform enemyT = EnemyObserver.GetClosestEnemy();

        // if the front of the group can see the enemy, the rest will go forward (to the commander), but will not stop attacking
        if (enemyT == null)
        { MoveTowardsEntity(CommanderToFollow); return; }

        // attack the closest enemy if in range
        if (AttackEnemyIfInRange(enemyT)) { return; }
        // go closer to the enemy
        MoveTowardsEntity(enemyT);

        // if the commander is too far, the soldier will stop attacking and will return back to the commander
        float distanceFromCommander = (CommanderToFollow.position - transform.position).magnitude;
        if (distanceFromCommander > AttackDistanceFromCommander)
        {
            SoldierBehaviour = SoldierBehaviour.Move;
        }
    }
    /// <summary> attacks the closest enemy in range</summary>
    /// <returns> returns if enemy was in range </returns>
    private bool AttackEnemyIfInRange(Transform enemyT)
    {
        ClosestEnemyDEBUG = Vector3.Distance(enemyT.position, transform.position);
        if (Vector3.Distance(enemyT.position, transform.position) < AttackRange)
        {
            if (AttackTimer >= Attackcooldown)
            {
                AttackTimer = 0.0f;
                enemyT.GetComponent<ISoldier>()?.TakeDamage(Damage);
                Animator.SetTrigger("Attack");
            }
            return true;
        }
        return false;
    }

    private void MoveTowardsEntity(Transform entityT)
    {

        Vector2 directionToCommander = entityT.position - transform.position;
        Move(directionToCommander, entityT);
    }

    private void Move(Vector2 direction, Transform entityT=null)
    {
        if (direction.magnitude < 0.01f)
        {
            Animator.SetFloat(AnimatorMovementSpeedHash, 0.0f);
            return;
        }

        direction = direction.normalized;

        Vector2 movement = direction * MovementSpeed;

        Animator.SetFloat(AnimatorMovementSpeedHash, movement.magnitude);

        if (direction.magnitude < Mathf.Epsilon)
        { return; }

        SpriteRenderer.flipX = movement.x < 0;
        XSpriteFlip.Value = SpriteRenderer.flipX;

        //transform.Translate(movement * Time.deltaTime);
        
        if (entityT) {
            Debug.Log("go to entityT" + gameObject.name);
            var pos = new Vector3(entityT.position.x, entityT.position.y, transform.position.z);
            Agent.SetDestination(pos);
        } else {
            transform.Translate(movement * Time.deltaTime);
        }
    }

    void OnMouseDown()
    {
        Debug.Log("Sprite Clicked");

        ulong clientID = NetworkManager.Singleton.LocalClientId;
        RequestChangingCommanderToFollowServerRpc(clientID: clientID);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestChangingCommanderToFollowServerRpc(ulong clientID)
    {
        NetworkObject clientNO = NetworkManager.Singleton?.ConnectedClients[clientID]?.PlayerObject;
        ITeamMember teamMember = clientNO.GetComponent<ITeamMember>();
        if (teamMember != null && teamMember.Team == Team)
        {
            SetCommanderToFollow(clientNO.gameObject.transform);
            if (SoldierBehaviour == SoldierBehaviour.Idle) {
                SoldierBehaviour = SoldierBehaviour.Move;
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

            shape = CommanderToFollow.GetComponent<ICommander>().GetFormation();
            FormationFromFollowedCommander = CommanderToFollow.GetComponent<Formation>();
            if (shape == ICommander.SoldierMovements.Circle) {
                SoldierBehaviour = SoldierBehaviour.FormationCircle;
                NavMeshFormationSwitch(true, SoldierBehaviour.FormationCircle, FormationFromFollowedCommander, ICommander.SoldierMovements.Circle);
            }
        } else // if already following, unfollow
        {
            CommanderToFollow?.GetComponent<ICommander>().ReportUnfollowing(gameObject);
            CommanderToFollow = null;

            NavMeshFormationSwitch(false, SoldierBehaviour.Idle, FormationFromFollowedCommander, ICommander.SoldierMovements.Free);

        }
    }
    /// <summary> !call only on server! </summary>
    public void TakeDamage(int damage)
    {
        if (!IsServer)
        { throw new Exception($"soldier ({gameObject.name}) can take damage only on server"); }

        int hp = HP - damage;
        if (hp < 0) { Die(); }
        else { HP = hp; }
    }
    /// <summary> !call only on server! </summary>
    public void Die()
    {
        HP = 0;
        CommanderToFollow?.GetComponent<ICommander>().ReportUnfollowing(gameObject);
        Destroy(gameObject);
    }
}
