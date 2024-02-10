using System;
using System.Collections.Generic;
using Firebase.Auth;
using Managers;
using Unity.Collections;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;

namespace Player
{
    public class PlayerController : NetworkBehaviour, ICommander
    {
        [SerializeField] private float movementSpeed = 1;
        [SerializeField] private int initialTeam;
        [SerializeField] private string initialName;
        [SerializeField] private GameObject revealer;
        
        private PlayerManager playerManager;
        private GameSessionManager gameSessionManager;
        private SpriteRenderer spriteRenderer;
        private NetworkAnimator networkAnimator;
        private Guid ownerPlayerId;
        private Formation formationScript;
        private ChangeMaterial changeMaterial;
        private List<NetworkObjectReference> units = new();
        private PathTileChecker pathTileChecker;
        private string firebaseId;
        
        // Replicated variable for sprite orientation
        private readonly NetworkVariable<bool> xSpriteFlip = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        private readonly NetworkVariable<FixedString64Bytes> _Name = new(new FixedString64Bytes(""));
        private readonly NetworkVariable<FixedString64Bytes> _FirebaseId = new(new FixedString64Bytes(""));
        private readonly NetworkVariable<int> _Team = new(-1);
        private readonly NetworkVariable<bool> _IsLocked = new(true);
        private readonly NetworkVariable<Vector3> _PointerPosition = new(Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        private readonly NetworkVariable<bool> _IsOnPath = new (false);
        public int Team { get => _Team.Value; set => _Team.Value = value; }
        public string Name { get => _Name.Value.Value; set => _Name.Value = value; }
        public string FirebaseId { get => _FirebaseId.Value.Value; set => _FirebaseId.Value = value; }
        public bool IsLocked { get => _IsLocked.Value; set => _IsLocked.Value = value; }
        
        public Vector3 PointerPosition { get => _PointerPosition.Value; set => _PointerPosition.Value = value; }
        public bool IsOnPath { get => _IsOnPath.Value; set => _IsOnPath.Value = value; }

        public Formation.FormationType FormationType;
        public bool followPin = false;

        public Formation.FormationType GetFormation() {
            return FormationType;
        }
        
        private static readonly int movementSpeedAnimationHash = Animator.StringToHash("MovementSpeed");

        private UIInGameScreenController uiInGameScreenController;

        private void initialize()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            networkAnimator = GetComponent<NetworkAnimator>();
            formationScript = GetComponent<Formation>();
            changeMaterial = GetComponent<ChangeMaterial>();
            playerManager = FindObjectOfType<PlayerManager>();
            gameSessionManager = FindObjectOfType<GameSessionManager>();
            uiInGameScreenController = UIController.Singleton.GetComponentInChildren<UIInGameScreenController>();
            pathTileChecker = FindObjectOfType<PathTileChecker>();

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
                gameObject.AddComponent<AudioListener>();
                gameObject.AddComponent<AudioSource>();
                AudioManager.Instance.notificationsSource = gameObject.GetComponent<AudioSource>();
                
                // for trumpet sounds only
                gameObject.AddComponent<AudioSource>();
                AudioManager.Instance.commandsSource = gameObject.GetComponent<AudioSource>();
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

            if (IsServer)
            {
                ClientRpcParams clientRpcParams = new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new []{ OwnerClientId }
                    }
                };

                AddCastleUIClientRpc(clientRpcParams);
            }
        }

        [ClientRpc]
        private void AddCastleUIClientRpc(ClientRpcParams clientRpcParams = default)
        {
            var castles = FindObjectsOfType<Outpost>();
            foreach (var castle in castles)
            {
                if (castle.IsCastle && castle.Team == Team)
                {
                    AddOutpost(castle);
                }
            }
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
            if (gameSessionManager.IsOver) return;

            PointerPosition = PlayerPointerPlacer.PinPosition;
            if (followPin) {
                MoveTowards(PointerPosition);
            }
            
            // Attack
            if (Input.GetKeyDown(KeyCode.P)) {
                CommandAttackServerRpc();
            }

            // Circular formation
            if (Input.GetKeyDown(KeyCode.C))
            {
                FormatSoldiersServerRpc(KeyCode.C);
            }

            // Box (Rectangular) formation
            if (Input.GetKeyDown(KeyCode.R)) 
            { 
                FormatSoldiersServerRpc(KeyCode.R); 
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void FormatSoldiersServerRpc(KeyCode key) {
            FormatSoldiers(key);
        }

        public void FormatSoldiers(KeyCode key) {

            if (key == KeyCode.C) {
                // circle (Circular) formation
                FormationType = Formation.FormationType.Circle;
                formationScript.ResetFormation();
                notifySoldiers();
            }
            if (key == KeyCode.R) {
                // box (Rectangular) formation
                FormationType = Formation.FormationType.Box;
                formationScript.ResetFormation();
                notifySoldiers();
            }
        }


        private void notifySoldiers() {
            foreach (GameObject go in units) {
                if (go.TryGetComponent<ISoldier>(out ISoldier soldier)) {
                    soldier.NewCommand(SoldierCommand.Following);

                    switch (FormationType) {
                        case Formation.FormationType.Free:
                            soldier.NavMeshFormationSwitch(false, formationScript, FormationType);
                            formationScript.ResetFormation();
                            break;
                        case Formation.FormationType.Circle:
                            soldier.NavMeshFormationSwitch(false, formationScript, FormationType);
                            soldier.NavMeshFormationSwitch(true, formationScript, FormationType);
                            break;
                        case Formation.FormationType.Box:
                            soldier.NavMeshFormationSwitch(false, formationScript, FormationType);
                            soldier.NavMeshFormationSwitch(true, formationScript, FormationType);
                            break;
                        default: 
                            break;
                    }
                }
            }
        }

        public void MoveTowards(Vector3 position)
        {
            followPin = true;
            Vector2 direction = position - transform.position;
            transform.DOMove(position, Time.deltaTime * 0.8f);

            if (direction.magnitude > 0.03f)
            {
                networkAnimator.Animator.SetFloat(movementSpeedAnimationHash, 1);
            }
            else
            {
                networkAnimator.Animator.SetFloat(movementSpeedAnimationHash, 0);
            }

            spriteRenderer.flipX = direction.x < 0;
            xSpriteFlip.Value = spriteRenderer.flipX;
            IsOnPath = pathTileChecker.IsNearbyPath(transform.position);
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
                    TargetClientIds = new []{ OwnerClientId, NetworkManager.Singleton.LocalClientId }
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

            if (isActualOwner())
            {
                uiInGameScreenController.AddUnit(soldier,
                    () => soldier.RequestChangingCommanderToFollowServerRpc(NetworkManager.Singleton.LocalClientId)
                );   
            }
        }

        void ICommander.ReportUnfollowing(NetworkObjectReference networkObjectReference) {
            if (!IsServer) { throw new Exception($"only on server can removing units from commander be done: {gameObject.name}"); }

            ClientRpcParams clientRpcParams = new ClientRpcParams {
                Send = new ClientRpcSendParams {
                    TargetClientIds = new[] { OwnerClientId, NetworkManager.Singleton.LocalClientId }
                }
            };

            removeFromUnitsClientRpc(networkObjectReference, clientRpcParams);
        }
        
        [ClientRpc]
        private void removeFromUnitsClientRpc(NetworkObjectReference networkObjectReference, ClientRpcParams clientRpcParams = default)
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

            if (isActualOwner())
            {
                uiInGameScreenController.RemoveUnit(soldier);
            }
        }
        
        [ServerRpc]
        public void CommandMovementServerRpc()
        {
            formationScript.ResetFormation();
            FormationType = Formation.FormationType.Free;
            foreach (GameObject go in units)
            {
                if (go.TryGetComponent<ISoldier>(out ISoldier soldier))
                {
                    soldier.NewCommand(SoldierCommand.Following);
                }
            }
        }

        [ServerRpc]
        public void CommandIdleServerRpc() {
            foreach (GameObject go in units) {
                if (go.TryGetComponent<ISoldier>(out ISoldier soldier)) {
                    soldier.NewCommand(SoldierCommand.Following);
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void CommandAttackServerRpc(ServerRpcParams serverRpcParams = default) {
            //formationScript.ResetFormation();
            //FormationType = Formation.FormationType.Free;
            foreach (GameObject go in units) {
                if (go.TryGetComponent<ISoldier>(out ISoldier soldier)) {
                    soldier.NewCommand(SoldierCommand.Attack);
                }
            }
        }

        public void AddOutpost(Outpost outpost) {
            uiInGameScreenController.AddOutpost(outpost);
        }

        public void RemoveOutpost(Outpost outpost) {
            uiInGameScreenController.RemoveOutpost(outpost);
        }
    }
}
