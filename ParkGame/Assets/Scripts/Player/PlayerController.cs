using System;
using System.Collections.Generic;
using Firebase.Auth;
using Managers;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.AI;

namespace Player
{
    public class PlayerController : NetworkBehaviour, ICommander
    {
        [SerializeField] private float movementSpeed = 1;
        [SerializeField] private int initialTeam;
        [SerializeField] private string initialName;
        [SerializeField] private GameObject revealer;
        
        private PlayerManager playerManager;
        private SpriteRenderer spriteRenderer;
        private NetworkAnimator networkAnimator;
        private Guid ownerPlayerId;
        private Formation formationScript;
        private ChangeMaterial changeMaterial;
        private List<NetworkObjectReference> units = new();
        private List<GameObject> outposts = new();
        private string firebaseId;
        
        // Replicated variable for sprite orientation
        private readonly NetworkVariable<bool> xSpriteFlip = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        private readonly NetworkVariable<FixedString64Bytes> _Name = new(new FixedString64Bytes(""));
        private readonly NetworkVariable<FixedString64Bytes> _FirebaseId = new(new FixedString64Bytes(""));
        private readonly NetworkVariable<int> _Team = new(-1);
        private readonly NetworkVariable<bool> _IsLocked = new(true);
        private readonly NetworkVariable<Vector3> _PointerPosition = new(Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        public int Team { get => _Team.Value; set => _Team.Value = value; }
        public string Name { get => _Name.Value.Value; set => _Name.Value = value; }
        public string FirebaseId { get => _FirebaseId.Value.Value; set => _FirebaseId.Value = value; }
        public bool IsLocked { get => _IsLocked.Value; set => _IsLocked.Value = value; }
        
        public Vector3 PointerPosition { get => _PointerPosition.Value; set => _PointerPosition.Value = value; }

        public Formation.FormationType FormationType; // todo?
        public Formation.FormationType GetFormation() {
            return FormationType;
        }
        
        private static readonly int movementSpeedAnimationHash = Animator.StringToHash("MovementSpeed");
        private NavMeshAgent Agent;

        private UIInGameScreenController uiInGameScreenController;

        private void initialize()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            networkAnimator = GetComponent<NetworkAnimator>();
            formationScript = GetComponent<Formation>();
            changeMaterial = GetComponent<ChangeMaterial>();
            Agent = GetComponent<NavMeshAgent>();
            playerManager = FindObjectOfType<PlayerManager>();
            uiInGameScreenController = UIController.Singleton.GetComponentInChildren<UIInGameScreenController>();

            if (IsServer) {
                Team = initialTeam;
                Name = initialName;
                FirebaseId = firebaseId;
            }
            
            Debug.Log($"init player {Name} in {Team} with {FirebaseId}, is owner: {isActualOwner()}");
            if (isActualOwner())
            {
                // if (Camera.main != null)
                // {
                //     Camera.main.gameObject.transform.SetParent(transform);
                // }
                playerManager.SetLocalPlayerController(this);
            }
            else
            {
                xSpriteFlip.OnValueChanged += onXSpriteFlipChanged;
            }
            
            _IsLocked.OnValueChanged += onIsLockedChanged;

            var localPlayerData = LobbyManager.Singleton.GetLocalPlayerData();
            if (localPlayerData.Team == Team)
            {
                revealer.SetActive(true);
                changeMaterial.Change(false);
            }
            else
            {
                revealer.gameObject.SetActive(false);
                changeMaterial.Change(true);
            }
            
            formationScript.InitializeFormation(); // build prefab, get position of the commander
            FormationType = Formation.FormationType.Free; // movement without navmesh
        }

        private void onIsLockedChanged(bool previousValue, bool newValue)
        {
            PlayerPointerPlacer playerPointerPlacer = FindObjectOfType<PlayerPointerPlacer>();
            playerPointerPlacer.SetReadyColor(!newValue);
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

            PointerPosition = PlayerPointerPlacer.PinPosition;
            // move();

            if (Input.GetKeyDown(KeyCode.I))
            { CommandMovementServerRpc(); }
            if (Input.GetKeyDown(KeyCode.O))
            { CommandIdleServerRpc(); }
            if (Input.GetKeyDown(KeyCode.P))
            { CommandAttackServerRpc(); }

            // ...ServerRpc???
            //if (Input.GetKeyDown(KeyCode.C)) { FormatSoldiers(KeyCode.C); }
            //if (Input.GetKeyDown(KeyCode.R)) { FormatSoldiers(KeyCode.R); }
            if (Input.GetKeyDown(KeyCode.C)) { 
                //Debug.Log("Nu of Units: " + Units.Count);
                FormatSoldiersServerRpc(KeyCode.C); }
            if (Input.GetKeyDown(KeyCode.R)) { 
                //Debug.Log("Nu of Units: " + Units.Count);
                FormatSoldiersServerRpc(KeyCode.R); }
        }

        //[ServerRpc]
        [ServerRpc(RequireOwnership = false)]
        public void FormatSoldiersServerRpc(KeyCode key) {
            FormatSoldiers(key);
        }

        public void FormatSoldiers(KeyCode key) {

            if (key == KeyCode.C) {
                // circle formation
                switch (FormationType) {
                    case Formation.FormationType.Box:
                    case Formation.FormationType.Free:
                        FormationType = Formation.FormationType.Circle;
                        formationScript.ResetFormation();
                        // notify soldiers
                        NotifySoldiersServerRpc();
                        break;

                    case Formation.FormationType.Circle:
                        FormationType = Formation.FormationType.Free;
                        // notify soldiers
                        NotifySoldiersServerRpc();
                        break;

                    default:
                        break;
                }
            }
            if (key == KeyCode.R) {
                // rectangle formation
                switch (FormationType) {
                    case Formation.FormationType.Circle:
                    case Formation.FormationType.Free:
                        FormationType = Formation.FormationType.Box;
                        formationScript.ResetFormation();
                        // notify soldiers
                        NotifySoldiersServerRpc();
                        break;

                    case Formation.FormationType.Box:
                        FormationType = Formation.FormationType.Free;
                        formationScript.ResetFormation();
                        // notify soldiers
                        NotifySoldiersServerRpc();
                        break;

                    default:
                        break;
                }
            }
        }

        [ServerRpc]
        public void NotifySoldiersServerRpc() {

            Debug.Log("number of Units: " + units.Count);
            foreach (GameObject go in units) {
                if (go.TryGetComponent<ISoldier>(out ISoldier soldier)) {
                    switch (FormationType) {
                        case Formation.FormationType.Free:
                            soldier.NavMeshFormationSwitch(false, SoldierBehaviour.Idle, formationScript, FormationType);
                            formationScript.ResetFormation();
                            break;
                        case Formation.FormationType.Circle:
                            soldier.NavMeshFormationSwitch(false, SoldierBehaviour.Idle, formationScript, FormationType);
                            soldier.NavMeshFormationSwitch(true, SoldierBehaviour.Formation, formationScript, FormationType);
                            break;
                        case Formation.FormationType.Box:
                            soldier.NavMeshFormationSwitch(false, SoldierBehaviour.Idle, formationScript, FormationType);
                            soldier.NavMeshFormationSwitch(true, SoldierBehaviour.Formation, formationScript, FormationType);
                            break;
                        default: 
                            break;
                    }
                }
            }
        }

        public void MoveTowards(Vector3 position)
        {
            Agent.SetDestination(position);
            Vector2 direction = position - transform.position;
            if (direction.magnitude > 0.1f)
            {
                networkAnimator.Animator.SetFloat(movementSpeedAnimationHash, 1);
            }
            else
            {
                networkAnimator.Animator.SetFloat(movementSpeedAnimationHash, 0);
            }

            spriteRenderer.flipX = direction.x < 0;
            xSpriteFlip.Value = spriteRenderer.flipX;
        }

        private void move()
        {
            Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

            Vector2 movement = input * movementSpeed;

            if (movement == Vector2.zero)
            {
                //FormationScript.ListFormationPositions();
            }
            
            networkAnimator.Animator.SetFloat(movementSpeedAnimationHash, movement.magnitude);

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
            return FirebaseId == FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        }

        public void InitializePlayer(PlayerData clientData)
        {
            initialTeam = clientData.Team;
            initialName = clientData.Name;
            firebaseId = clientData.FirebaseId;
        }

        void ICommander.ReportFollowing(NetworkObjectReference networkObjectReference)
        {
            if (!IsServer)
            { throw new Exception($"only on server can adding units to commander be done: {gameObject.name}"); }
            
            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new []{ OwnerClientId }
                }
            };
            
            addToUnitsClientRpc(networkObjectReference, clientRpcParams);
        }
        
        [ClientRpc]
        private void addToUnitsClientRpc(NetworkObjectReference networkObjectReference, ClientRpcParams clientRpcParams = default)
        {
            units.Add(networkObjectReference);

            if (!networkObjectReference.TryGet(out var networkObject, NetworkManager.Singleton))
            {
                Debug.LogWarning($"could not get network object from reference");
                return;
            }

            if (!networkObject.TryGetComponent<Soldier>(out var soldier))
            {
                Debug.LogWarning($"could not get soldier from network object");
                return;
            }

            if (soldier.TransformToFollow == transform)
                uiInGameScreenController.AddUnit(soldier, soldier.OnMouseDown);
        }

        void ICommander.ReportUnfollowing(NetworkObjectReference networkObjectReference)
        {
            if (!IsServer)
            { throw new Exception($"only on server can removing units from commander be done: {gameObject.name}"); }
            
            removeFromUnitsClientRpc(networkObjectReference);
        }
        
        [ClientRpc]
        private void removeFromUnitsClientRpc(NetworkObjectReference networkObjectReference)
        {
            units.Remove(networkObjectReference);
            if (!networkObjectReference.TryGet(out var networkObject, NetworkManager.Singleton))
            {
                Debug.LogWarning($"could not get network object from reference");
                return;
            }

            if (!networkObject.TryGetComponent<Soldier>(out var soldier))
            {
                Debug.LogWarning($"could not get soldier from network object");
                return;
            }

            if (soldier.TransformToFollow == transform)
                uiInGameScreenController.RemoveUnit(soldier);
        }
        
        // commands to the units
        [ServerRpc]
        public void CommandMovementServerRpc()
        {
            foreach (GameObject go in units)
            {
                if (go.TryGetComponent<ISoldier>(out ISoldier soldier))
                { soldier.SoldierBehaviour = SoldierBehaviour.Move; }
            }
        }
        
        [ServerRpc]
        public void CommandIdleServerRpc()
        {
            foreach (GameObject go in units)
            {
                if (go.TryGetComponent<ISoldier>(out ISoldier soldier))
                { soldier.SoldierBehaviour = SoldierBehaviour.Idle; }
            }
        }
        
        [ServerRpc]
        public void CommandAttackServerRpc()
        {
            foreach (GameObject go in units)
            {
                if (go.TryGetComponent<ISoldier>(out ISoldier soldier))
                { soldier.SoldierBehaviour = SoldierBehaviour.Attack; }
            }
        }

        public void AddOutpost(Outpost outpost)
        {
            Debug.Log($"adding outpost {outpost.gameObject.name} to player {Name}");
            outposts.Add(outpost.gameObject);
            uiInGameScreenController.AddOutpost(outpost);
        }

        public void RemoveOutpost(Outpost outpost)
        {
            Debug.Log($"removing outpost {outpost.gameObject.name} from player {Name}");
            outposts.Remove(outpost.gameObject);
            uiInGameScreenController.RemoveOutpost(outpost);
        }
    }
}
