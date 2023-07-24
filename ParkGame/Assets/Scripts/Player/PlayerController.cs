using System;
using System.Collections.Generic;
using System.Windows.Input;
using Managers;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace Player
{
    public class PlayerController : NetworkBehaviour, ILeader
    {
        [SerializeField] private float movementSpeed = 1;

        private SpriteRenderer spriteRenderer;
        private Animator animator;
        private NetworkAnimator networkAnimator;
        private Guid ownerPlayerId;
        private Formation formation;

        // Replicated variable for sprite orientation
        private NetworkVariable<bool> xSpriteFlip = new(false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner);

        private static readonly int MovementSpeed = Animator.StringToHash("MovementSpeed");

        NetworkVariable<int> _Team = new(0);
        public int Team { get => _Team.Value; set => _Team.Value = value; }

        List<GameObject> Units = new();

        public void InitializePlayerId(Guid playerId)
        {
            this.ownerPlayerId = playerId;
        }

        private void initialize()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
            formation = GetComponent<Formation>();

            if (!isActualOwner())
            {
                xSpriteFlip.OnValueChanged += onXSpriteFlipChanged;
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            initialize();
        }

        private void Update()
        {
            if (!isActualOwner()) return;
            if (!Application.isFocused) return;

            move();
        }

        private void move()
        {
            Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

            Vector2 movement = input * movementSpeed;

            if (movement == Vector2.zero)
            {
                formation.ListFormationPositions();
            }


            animator.SetFloat(MovementSpeed, movement.magnitude);

            if (input.magnitude < Mathf.Epsilon) return;

            spriteRenderer.flipX = movement.x < 0;
            xSpriteFlip.Value = spriteRenderer.flipX;

            transform.Translate(movement * Time.deltaTime);
        }

        private void onXSpriteFlipChanged(bool previousValue, bool newValue)
        {
            spriteRenderer.flipX = newValue;
        }

        // Normally IsOwner works well, but in case the client disconnects
        // the ownership is automatically transferred to the host.
        private bool isActualOwner()
        {
            return SessionManager.Singleton.LocalPlayerId == ownerPlayerId && IsOwner;
        }

        [ClientRpc]
        public void InitializePlayerIdClientRpc(SerializedGuid serializedGuid, ClientRpcParams clientRpcParams = default)
        {
            if (IsHost) return;

            ownerPlayerId = serializedGuid.Value;
        }

        public void ReportFollowing(GameObject go)
        {
            

            Units.Add(go);
        }

        public void ReportUnfollowing(GameObject go)
        {
            Units.Remove(go);
        }
    }
}
