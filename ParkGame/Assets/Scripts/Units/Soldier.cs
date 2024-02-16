using UnityEngine;
using UnityEngine.Events;
using Unity.Netcode;
using Unity.Netcode.Components;
using Managers;
using System;
using Player;
using UnityEngine.AI;
using static Formation;


public class Soldier : ISoldier {
    protected override void Initialize() {
//        shooting = GetComponent<ShootScript>(); // only for Archers
        _Team.OnValueChanged += OnTeamChanged;
        SpriteRenderer.flipX = XSpriteFlip.Value;
        XSpriteFlip.OnValueChanged += OnXSpriteFlipChanged;
        _SoldierCommand.OnValueChanged += OnCommandChange;
        OnCommandChange(0, Command);



    }
    public override void OnNetworkSpawn() {
        base.OnNetworkSpawn();
        base.Initialize();
        Initialize();
    }

    private void OnXSpriteFlipChanged(bool previousValue, bool newValue) => SpriteRenderer.flipX = newValue;

    private void OnTeamChanged(int previousValue, int newValue) //DEBUG (just tem membership visualization) // TODO: rm
    {
        if (circleRenderer != null && newValue != -1) {
            circleRenderer.color = colorSettings.Colors[newValue].Color;
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

    private void OnCommandChange(SoldierCommand previousValue, SoldierCommand newValue) {
        CommandChangedEvent.Invoke();
    }

    public bool IsFollowingCommander() {
        if (CommanderToFollow?.GetComponent<PlayerController>()) {
            return true;
        }
        return false;
        
    }

    Transform ClosestOutpost() {
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

    void StationedInOutpost() {
        float distanceFromOutpost = Vector3.Distance(CommanderToFollow.position, transform.position);
        //if (distanceFromOutpost <= OuterDistanceFromCommander) { // have a value for every unit the same depending on the outpost's range???
        if (distanceFromOutpost <= DefendDistanceFromCommander) { // have a value for every unit the same depending on the outpost's range???
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
            targetedEnemy = enemyT;
            EnemiesInAttackWaveCounter++;
        } else { // no enemy is close by, go to the commander
            // not reset the formation positions upon calling Attack?
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

        NavMeshStatusNow = Agent.pathStatus;

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
    
    
    /*
     * Get position to follow in formation - based on formationType 
     * Called from PlayerController 
     */
   
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
    private void SetSoldierSpeed()
    {
        if (TypeOfUnit == UnitType.Horseman) {
            Agent.speed = HorseManSpeed;
        }
        if (TypeOfUnit != UnitType.Horseman || (TypeOfUnit == UnitType.Horseman && FormationType == FormationType.Box)) {
            if (
                playerManager.GetLocalPlayerController().IsOnPath ||
                (ReturningToOutpost && pathTileChecker.IsNearbyPath(Agent.transform.position)) // short-circuiting for efficiency
            )
                Agent.speed = BaseMovementSpeed * PathMovementSpeedMultiplier;
            else
                Agent.speed = BaseMovementSpeed;
        }
    }

    // precise: follow directly to the position
    // ! precise: follow in free formation commander or within outposts
    private void FollowObjectWithAnimation(Transform toFollow, bool precise = false) {
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

    private Transform GetEnemy() {
        if (targetedEnemy != null) {
            return targetedEnemy;
        }
        Transform enemyT = EnemyObserver.GetClosestEnemy();
        if (TypeOfUnit == UnitType.Archer) {
            enemyT = EnemyObserver.GetEnemyInRange(MinAttackRange, MaxAttackRange);
        }
        if (TypeOfUnit == UnitType.Horseman) {
            if (EnemiesInAttackWaveCounter == 0) {
                enemyT = EnemyObserver.GetFarthestEnemy(); // else attack the closest enemy
            }
        }
        targetedEnemy = enemyT;
        return enemyT;
    }

    /// <summary> attacks the closest enemy in range</summary>
    /// <returns> returns if enemy was in range </returns>
    private bool AttackEnemyIfInRange(Transform enemyT, float maxAttackDistance = 0) {
        float maxRange = MaxAttackRange;
        if (maxAttackDistance > 0) {
            maxRange = maxAttackDistance;
        }
        if (Vector3.Distance(enemyT.position, transform.position) <= maxRange
            && Vector3.Distance(enemyT.position, transform.position) >= MinAttackRange) {
            if (AttackTimer >= Attackcooldown) {
                AttackTimer = 0.0f;
                Networkanimator.Animator.SetFloat(AnimatorMovementSpeedHash, 0.0f); //("MovementSpeed", 0);

                Networkanimator.SetTrigger("Attack");


                if (TypeOfUnit == UnitType.Pawn || TypeOfUnit == UnitType.Horseman)
                {
                    PlaySwordAttackSFXClientRpc();
                    enemyT.GetComponent<ISoldier>()?.TakeDamage(Damage);

                }
                if (TypeOfUnit == UnitType.Archer) {
                    Vector2 direction = enemyT.position - transform.position;
                    Direction directionE = GetDirectionEnum(direction);
                    bool flip = directionE == Direction.Left;

                    XSpriteFlip.Value = flip;
                    Debug.Log("shoot");
                    PlayArcherAttackSFXClientRpc();
                    shooting.Shoot(enemyT.transform.position, Damage, flip, Team);
                }
            }
            return true;
        }
        return false;
    }

    private void MoveTowardsEntity(Transform entityT) {
        // archers, don't go closer! you'd just die 
        if (Vector3.Distance(entityT.position, transform.position) < MinAttackRange && TypeOfUnit == UnitType.Archer) {
            return;
        }

        FollowObjectWithAnimation(entityT, true);
    }
    
    public void OnMouseDown()
    {
        Debug.Log("Sprite Clicked");
        if (gameSessionManager.IsOver) return;

        ulong clientID = NetworkManager.Singleton.LocalClientId;
    //    RequestChangingCommanderToFollowServerRpc(clientID: clientID);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestChangingCommanderToFollowServerRpc(ulong clientID, bool random=false) {
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
                SpeakEvent.Invoke();
                FormationType = CommanderToFollow.GetComponent<ICommander>().GetFormation(); // get type of formation
                FormationFromFollowedCommander = CommanderToFollow.GetComponent<Formation>();
                if (FormationType == FormationType.Box || FormationType == FormationType.Circle) {
                    NavMeshFormationSwitch(true, FormationFromFollowedCommander, FormationType);
                }
            }
            // return to outpost
            else
            {
                ReturningToOutpost = true;
                var closestOutpost = ClosestOutpost();
                FormationFromFollowedCommander?.RemoveFromFormation(gameObject, ObjectToFollowInFormation, FormationType);
                CommanderToFollow?.GetComponent<ICommander>()?.ReportUnfollowing(gameObject);
                SetCommanderToFollow(closestOutpost);
                NewCommand(SoldierCommand.Following);
                SpeakEvent.Invoke();
                NavMeshFormationSwitch(false, FormationFromFollowedCommander, FormationType.Free);
            }
        }
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
        }
    }

    /// <summary> !call only on server! </summary>
    public override void TakeDamage(int damage) {
        if (!IsServer) { throw new Exception($"soldier ({gameObject.name}) can take damage only on server"); }
        if (isDead) { return; }
        Debug.Log("take damage");
        int hp = HP - damage;
        if (hp <= 0) { Die(); } else { HP = hp; }
    }

    public Transform GetCommanderWhomIFollow() {
        return CommanderToFollow;
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

    private void TimeToDestroy(float time) {
        timeToDeath = time;
    }

    public override void NewCommand(SoldierCommand command) {
        if (!IsServer) { return; }

        Command = command;

        if (command == SoldierCommand.Attack) {
            EnemiesInAttackWaveCounter = 0; // reset counter of targeted enemies (because of moleman's modus operandi)
        }

    }
}
