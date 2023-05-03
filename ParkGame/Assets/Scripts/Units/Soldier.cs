using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine.AI;

public class Soldier : NetworkBehaviour
{
    [SerializeField] private float movementSpeed = 1;

    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private NetworkAnimator networkanimator;
    private NavMeshAgent agent;
    public GameObject target;

    private NetworkVariable<bool> xSpriteFlip = new(false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    private static readonly int MovementSpeed = Animator.StringToHash("MovementSpeed");
    private bool following;

    [SerializeField] float DistanceFromCommander = 2.0f;

    private void Initialize()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        agent = gameObject.GetComponentInParent<NavMeshAgent>();
        Debug.Log("agent " + agent);

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

        Vector2 direction = no.transform.position - transform.position;
        float distance = direction.magnitude;

        if (distance > DistanceFromCommander && following)
        {   
            agent.SetDestination(target.transform.position);
            //move(direction);
        }
        else
        {
            animator.SetFloat(MovementSpeed, 0.0f);
        }
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

    // formation -> go there
    public void MoveToPosition(Vector2 position) 
    {

    }

    void OnMouseDown()
    {
        Debug.Log("Sprite Clicked");
        if (!IsOwner) { return; }
        following = !following; // flip boolean
    }
}
