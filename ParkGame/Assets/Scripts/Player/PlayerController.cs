using System;
using Networking;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

namespace Player
{
    public class PlayerController : NetworkBehaviour
    {
        [SerializeField] private float movementSpeed = 1;

        private SpriteRenderer spriteRenderer;
        private Animator animator;
        private NetworkAnimator networkAnimator;
        private Guid playerId;

        private NetworkVariable<bool> xSpriteFlip = new (false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner);
        
        private static readonly int MovementSpeed = Animator.StringToHash("MovementSpeed");

        private void initialize()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();

            if (!isActualOwner())
            {
                xSpriteFlip.OnValueChanged += OnXSpriteFlipChanged;   
            }
        }

        private void OnXSpriteFlipChanged(bool previousValue, bool newValue)
        {
            spriteRenderer.flipX = newValue;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            initialize();
        }

        private void Update()
        {
            if (!isActualOwner()) return;
            
            if (Application.isFocused)
            {
                move();   
            }
        }

        private void move()
        {
            Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        
            Vector2 movement = input * movementSpeed;
            
            animator.SetFloat(MovementSpeed, movement.magnitude);

            if (input.magnitude < Mathf.Epsilon) return;
            
            spriteRenderer.flipX = movement.x < 0;
            xSpriteFlip.Value = spriteRenderer.flipX;
            
            transform.Translate(movement * Time.deltaTime);
        }
        
        private bool isActualOwner()
        {
            if (IsHost)
            {
                return SessionManager.Singleton.LocalPlayerId == playerId;
            }

            return IsOwner;
        }

        public void Initialize(Guid playerId)
        {
            this.playerId = playerId;
        }
    }
}
