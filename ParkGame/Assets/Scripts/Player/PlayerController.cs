using Unity.Netcode;
using UnityEngine;

namespace Player
{
    public class PlayerController : NetworkBehaviour
    {
        [SerializeField] private float movementSpeed = 1;

        private SpriteRenderer spriteRenderer;

        private void Initialize()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    
        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
        
            if (IsLocalPlayer)
            {
                Initialize();
            }
        }

        private void Update()
        {
            if (IsLocalPlayer && Application.isFocused)
            {
                move();
            }
        }
    
        private void move()
        {
            Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        
            if (input.magnitude < Mathf.Epsilon) return;
        
            spriteRenderer.flipX = input.x < 0;
        
            Vector2 movement = input * movementSpeed * Time.deltaTime;
            transform.Translate(movement);
        }
    }
}
