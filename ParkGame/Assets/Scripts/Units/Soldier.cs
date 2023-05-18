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

    private NetworkVariable<bool> xSpriteFlip = new(false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    private static readonly int MovementSpeed = Animator.StringToHash("MovementSpeed");
    private bool following;
    private bool gotPosition;
    private bool inPosition;
    private Vector3 positionInFormation;
    private Vector3 randomIdlePosition;


    NavMeshAgent navMeshAgent;


    [SerializeField] float DistanceFromCommander = 1.0f;//2.0f;

    private void Initialize()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        positionInFormation = Vector3.zero;
        
        GenerateIdlePosition();

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

    void GenerateIdlePosition() {
        float rx = Random.Range(-0.4f, 0.4f);
        float ry = Random.Range(-0.4f, 0.4f);
        randomIdlePosition = new Vector3(transform.position.x + rx, transform.position.y + ry, 0);
        Debug.Log("new idle position ");
        Debug.Log(rx);
        Debug.Log(ry);
    }
    void Wander() {
        float dist = Vector3.Distance(transform.position, randomIdlePosition);
        Debug.Log("wander");
        if (dist >= 0f && dist <= 0.6f) {
            GenerateIdlePosition();
        }
        navMeshAgent.SetDestination(randomIdlePosition);
    }

    void Update()
    {
        NetworkObject commander = NetworkManager?.LocalClient?.PlayerObject;
        if (commander == null) { return; }

        Vector2 direction = commander.transform.position - transform.position;
        float distance = direction.magnitude;

        if (following) {
            positionInFormation = commander.GetComponent<Formation>().GetPositionInFormation(transform.position);

            navMeshAgent.SetDestination(positionInFormation);
        } else {
            Wander();
            //jitter/wander
        }
        

        /*
        if (distance > DistanceFromCommander && following) {
            move(direction);
            gotPosition = false;
        } else if (following && !gotPosition) {
            positionInFormation = no.GetComponent<Formation>().GetPositionInFormation();
            inPosition = false;
            gotPosition = true;
        } else if (following && gotPosition && !inPosition) {
            navMeshAgent.SetDestination(positionInFormation);

            //Vector2 dirToPos = positionInFormation - transform.position;
            //move(dirToPos);

        } else {
            animator.SetFloat(MovementSpeed, 0.0f);
        }
        */

    }

    private void move(Vector2 direction)
    {
        
        if (direction.magnitude < 0.01f) {
            animator.SetFloat(MovementSpeed, 0.0f);

            Debug.Log("direction magnitude " + direction.magnitude);
    
            return;
        }

        direction = direction.normalized;

        Vector2 movement = direction * movementSpeed;

        animator.SetFloat(MovementSpeed, movement.magnitude);

        if (direction.magnitude < Mathf.Epsilon)
        {
            return;
        }
        Debug.Log("MOVEMENT " + movement);
        spriteRenderer.flipX = movement.x < 0;
        xSpriteFlip.Value = spriteRenderer.flipX;

        transform.Translate(movement * Time.deltaTime);
    }

    public void Follow(bool follow) {
        following = follow;
        NetworkObject commander = NetworkManager?.LocalClient?.PlayerObject;
        var formation = commander.GetComponent<Formation>();
        if (follow) {
            formation.addFollower();
        } else {
            GenerateIdlePosition();
            formation.removeFollower();
        }
    }

    void OnMouseDown()
    {
        Debug.Log("Sprite Clicked");
        if (!IsOwner) { return; }

        following = !following; // flip boolean
        if (following) 
        {
            // increase commander's counter - Formation
            Follow(true);
        } else 
        {
            // decrease commander's counter - Formation
            Follow(false);
        }
    }
}
