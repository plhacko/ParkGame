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
    // logic
    [SerializeField] private float movementSpeed = 1;
    private GameObject LeaderToFollow = null;
    [SerializeField] float IdealDistanceFromCommander = 1.0f;
    private NetworkVariable<int> _Team = new();
    public int Team { get => _Team.Value; set => _Team.Value = value; }

    // animation
    private static readonly int AnimatorMovementSpeedHash = Animator.StringToHash("MovementSpeed");
    private SpriteRenderer SpriteRenderer;
    private Animator Änimator;
    private NetworkAnimator Networkanimator;
    private NetworkVariable<bool> XSpriteFlip = new(false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    private void Initialize()
    {
        SpriteRenderer = GetComponent<SpriteRenderer>();
        Änimator = GetComponent<Animator>();

        if (!IsOwner)
        {
            XSpriteFlip.OnValueChanged += OnXSpriteFlipChanged;
        }
    }

    private void OnXSpriteFlipChanged(bool previousValue, bool newValue) => SpriteRenderer.flipX = newValue;

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

        // check for a leader
        if (LeaderToFollow == null)
        { return; }


        // movement
        Vector2 direction = LeaderToFollow.transform.position - transform.position;
        float distance = direction.magnitude;
        if (distance > IdealDistanceFromCommander)
        {
            Move(direction);
        }
        else
        {
            Änimator.SetFloat(AnimatorMovementSpeedHash, 0.0f);
        }
    }

    private void Move(Vector2 direction)
    {
        if (direction.magnitude < 0.01f)
        {
            Änimator.SetFloat(AnimatorMovementSpeedHash, 0.0f);

            Debug.Log("direction magnitude " + direction.magnitude);

            return;
        }

        direction = direction.normalized;

        Vector2 movement = direction * movementSpeed;

        Änimator.SetFloat(AnimatorMovementSpeedHash, movement.magnitude);

        if (direction.magnitude < Mathf.Epsilon)
        {
            return;
        }
        // TODO: rm // Debug.Log("MOVEMENT " + movement);
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

    [ServerRpc]
    public void RequestChangingCommanderToFollowServerRpc(ulong clientID)
    {
        NetworkObject clientNO = NetworkManager.Singleton?.ConnectedClients[clientID]?.PlayerObject;
        ITeamMember teamMember = clientNO.GetComponent<ITeamMember>();
        if (teamMember != null && teamMember.Team == Team)
        {
            SetCommanderToFollow(clientNO.gameObject);
        }
    }

    /// <summary> !call only on server! </summary>
    public void SetCommanderToFollow(GameObject leaderToFollow)
    {
        if (!NetworkManager.Singleton.IsServer)
        { throw new Exception($"only server can set what the unit ({gameObject.name}) can follow ({leaderToFollow?.name})"); }

        if (LeaderToFollow != leaderToFollow) // change leader to follow
        {
            LeaderToFollow?.GetComponent<ILeader>().ReportUnfollowing(gameObject);
            LeaderToFollow = leaderToFollow;
            LeaderToFollow?.GetComponent<ILeader>().ReportFollowing(gameObject);
        }
        else // if already following, unfollow
        {
            LeaderToFollow?.GetComponent<ILeader>().ReportUnfollowing(gameObject);
            LeaderToFollow = null;
        }
    }
}
