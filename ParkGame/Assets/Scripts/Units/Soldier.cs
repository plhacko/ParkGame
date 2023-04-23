using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;

public class Soldier : NetworkBehaviour
{
    [SerializeField] private float movementSpeed = 1;

    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private NetworkAnimator networkanimator;

    private NetworkVariable<bool> xSpriteFlip = new(false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    private static readonly int MovementSpeed = Animator.StringToHash("MovementSpeed");

    private void Initialize()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

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

    void Update()
    {
        NetworkObject no = NetworkManager?.LocalClient?.PlayerObject;
        if (no == null) { return; }

        move(no.transform.position - transform.position);
    }

    private void move(Vector2 direction)
    {
        direction = direction.normalized;

        Vector2 movement = direction * movementSpeed;

        animator.SetFloat(MovementSpeed, movement.magnitude);

        if (direction.magnitude < Mathf.Epsilon) return;

        spriteRenderer.flipX = movement.x < 0;
        xSpriteFlip.Value = spriteRenderer.flipX;

        transform.Translate(movement * Time.deltaTime);
    }
}
