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
        private NetworkAnimator networkanimator;
        private Formation formation;

        private PlayerSoldiers soldiers;

        private NetworkVariable<bool> xSpriteFlip = new (false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner);
        
        private static readonly int MovementSpeed = Animator.StringToHash("MovementSpeed");

        private void Initialize()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
            formation = GetComponent<Formation>();
            soldiers = GetComponent<PlayerSoldiers>();

            if (!IsOwner)
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
            Initialize();
        }

        void FollowedByAllSwitch() {
            soldiers.SwitchMassSelect();
        }

        private void Update()
        {
            if (!IsOwner) return;

            if (Input.GetKeyDown(KeyCode.F)) {
                FollowedByAllSwitch();
                
            }

            if (Application.isFocused)
            {
                move();   
            }
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
    }
}
