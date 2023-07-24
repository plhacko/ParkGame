using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
using Managers;
using System;
using System.Windows.Input;

public class Soldier : NetworkBehaviour, ISoldier
{
    // game logic
    [SerializeField] private float movementSpeed = 1;
    private Transform CommanderToFollow = null;
    [SerializeField] float InnerDistanceFromCommander = 1.0f;
    [SerializeField] float OuterDistanceFromCommander = 2.0f;
    [SerializeField] float DefendDistanceFromCommander = 2.5f;
    [SerializeField] float AttackDistanceFromCommander = 6.0f;
    [SerializeField] float AttackRange = 0.3f;

    private NetworkVariable<int> _Team = new();
    public int Team { get => _Team.Value; set => _Team.Value = value; }
    public SoldierBehaviour SoldierBehaviour { get; set; } // derived form ISoldier
    EnemyObserver EnemyObserver;

    // animation
    private static readonly int AnimatorMovementSpeedHash = Animator.StringToHash("MovementSpeed");
    private SpriteRenderer SpriteRenderer;
    private Animator Animator;
    private NetworkAnimator Networkanimator;
    private NetworkVariable<bool> XSpriteFlip = new(false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    private void Initialize()
    {
        EnemyObserver = GetComponentInChildren<EnemyObserver>();
        SpriteRenderer = GetComponent<SpriteRenderer>();
        Animator = GetComponent<Animator>();

        _Team.OnValueChanged += OnTeamChanged;
        OnTeamChanged(0, Team);
        if (!IsOwner)
        {
            XSpriteFlip.OnValueChanged += OnXSpriteFlipChanged;
        }
    }

    private void OnXSpriteFlipChanged(bool previousValue, bool newValue) => SpriteRenderer.flipX = newValue;
    private void OnTeamChanged(int previousValue, int newValue) //DEBUG // TODO: rm
    {
        SpriteRenderer sr = transform.Find("Circle")?.GetComponent<SpriteRenderer>();
        if (sr == null) { return; }
        if (newValue == 0) { sr.color = Color.blue; }
        else if (newValue == 1) { sr.color = Color.yellow; }
        else { sr.color = Color.grey; }
    }
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        Initialize();
    }
    void Update()
    {
        // following is done only on server
        if (!NetworkManager.Singleton.IsServer)
        { return; }

        // check for a Commander
        if (CommanderToFollow == null)
        { return; }

        switch (SoldierBehaviour)
        {
            case SoldierBehaviour.Idle:
                IdleBehaviour();
                GetComponent<SpriteRenderer>().color = Color.green; // DEBUG // TODO: rm
                break;
            case SoldierBehaviour.Move:
                MovementBehaviour();
                GetComponent<SpriteRenderer>().color = Color.blue; // DEBUG // TODO: rm
                break;
            case SoldierBehaviour.Attack:
                AttackBehaviour();
                GetComponent<SpriteRenderer>().color = Color.red; // DEBUG // TODO: rm
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
            MoveTowardsEntity(enemyT);
            AttackEnemyIfInRAnge(enemyT);
            return;
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
        Transform enemyT = EnemyObserver.GetClosestEnemy();
        if (enemyT != null)
        { AttackEnemyIfInRAnge(enemyT); }

        //  moves to the inner circle around the commander
        if (Vector3.Distance(CommanderToFollow.position, transform.position) > InnerDistanceFromCommander)
        { MoveTowardsEntity(CommanderToFollow); }
        else
        {
            SoldierBehaviour = SoldierBehaviour.Idle;
            Animator.SetFloat(AnimatorMovementSpeedHash, 0.0f);
        }
    }

    private void AttackBehaviour()
    {
        Transform enemyT = EnemyObserver.GetClosestEnemy();

        // if the front of the group can see the enemy, the rest will go forward (to the commander), but will not stop attacking
        if (enemyT == null)
        { MoveTowardsEntity(CommanderToFollow); return; }

        // attack the closest enemy if in range
        AttackEnemyIfInRAnge(enemyT);
        // go closer to the enemy
        MoveTowardsEntity(enemyT);

        // if the commander is too far, the soldier will stop attacking and will return back to the commander
        float distanceFromCommander = (CommanderToFollow.position - transform.position).magnitude;
        if (distanceFromCommander > AttackDistanceFromCommander)
        {
            SoldierBehaviour = SoldierBehaviour.Move;
        }
    }

    private void AttackEnemyIfInRAnge(Transform enemyT)
    {
        if (Vector3.Distance(enemyT.position, transform.position) < AttackRange)
        {
            // TODO: do attack
        }
    }

    private void MoveTowardsEntity(Transform entityT)
    {
        Vector2 directionToCommander = entityT.position - transform.position;
        Move(directionToCommander);
    }

    private void Move(Vector2 direction)
    {
        if (direction.magnitude < 0.01f)
        {
            Animator.SetFloat(AnimatorMovementSpeedHash, 0.0f);
            return;
        }

        direction = direction.normalized;

        Vector2 movement = direction * movementSpeed;

        Animator.SetFloat(AnimatorMovementSpeedHash, movement.magnitude);

        if (direction.magnitude < Mathf.Epsilon)
        { return; }

        SpriteRenderer.flipX = movement.x < 0;
        XSpriteFlip.Value = SpriteRenderer.flipX;

        transform.Translate(movement * Time.deltaTime);
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
        }
    }

    /// <summary> !call only on server! </summary>
    public void SetCommanderToFollow(Transform commanderToFollow)
    {
        if (!NetworkManager.Singleton.IsServer)
        { throw new Exception($"only server can set what the unit ({gameObject.name}) can follow ({CommanderToFollow?.name})"); }

        if (CommanderToFollow != commanderToFollow) // change Commander to follow
        {
            CommanderToFollow?.GetComponent<ICommander>().ReportUnfollowing(gameObject);
            CommanderToFollow = commanderToFollow;
            CommanderToFollow?.GetComponent<ICommander>().ReportFollowing(gameObject);
        }
        else // if already following, unfollow
        {
            CommanderToFollow?.GetComponent<ICommander>().ReportUnfollowing(gameObject);
            CommanderToFollow = null;
        }
    }
}
