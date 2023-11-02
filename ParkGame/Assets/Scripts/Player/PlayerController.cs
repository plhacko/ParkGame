using System;
using System.Collections.Generic;
using System.Windows.Input;
using Managers;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
//using static Formation;

namespace Player
{
    public class PlayerController : NetworkBehaviour, ICommander
    {
        [SerializeField] private float movementSpeed = 1;

        private SpriteRenderer spriteRenderer;
        private NetworkAnimator networkAnimator;
        private Guid ownerPlayerId;
        private Formation FormationScript;

        // Replicated variable for sprite orientation
        private NetworkVariable<bool> xSpriteFlip = new(false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner);

        private static readonly int MovementSpeed = Animator.StringToHash("MovementSpeed");

        public int TeaM;
        NetworkVariable<int> _Team = new(0);
        public int Team { get => _Team.Value; set => _Team.Value = value; }

        List<GameObject> Units = new();

        public Formation.FormationType FormationType; // todo?
        public Formation.FormationType GetFormation() {
            return FormationType;
        }

        private void initialize()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            networkAnimator = GetComponent<NetworkAnimator>();
            FormationScript = GetComponent<Formation>();
            if (IsServer) {
                FormationScript.StartFormation(); // build prefab, get position of the commander
            }

            FormationType = Formation.FormationType.Free; // movement without navmesh
            
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
                        FormationScript.ResetFormation();
                        // notify soldiers
                        NotifySoldiersServerRpc();
                        break;

                    case Formation.FormationType.Box:
                        FormationType = Formation.FormationType.Free;
                        FormationScript.ResetFormation();
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

            Debug.Log("number of Units: " + Units.Count);
            foreach (GameObject go in Units) {
                if (go.TryGetComponent<ISoldier>(out ISoldier soldier)) {
                    switch (FormationType) {
                        case Formation.FormationType.Free:
                            Debug.Log("notify soldiers. C is OFF");
                            soldier.NavMeshFormationSwitch(false, SoldierBehaviour.Idle, FormationScript, FormationType);
                            FormationScript.ResetFormation();
                            break;
                        case Formation.FormationType.Circle:
                            Debug.Log("notify them. C is ON");
                            soldier.NavMeshFormationSwitch(false, SoldierBehaviour.Idle, FormationScript, FormationType);
                            soldier.NavMeshFormationSwitch(true, SoldierBehaviour.Formation, FormationScript, FormationType);
                            break;
                        case Formation.FormationType.Box:
                            Debug.Log("notify them. R is ON");
                            soldier.NavMeshFormationSwitch(false, SoldierBehaviour.Idle, FormationScript, FormationType);
                            soldier.NavMeshFormationSwitch(true, SoldierBehaviour.Formation, FormationScript, FormationType);
                            break;
                        default: 
                            break;
                    }
                }
            }
        }

        private void move()
        {
            Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

            Vector2 movement = input * movementSpeed;

            if (movement == Vector2.zero)
            {
                //FormationScript.ListFormationPositions();
            }
            
            networkAnimator.Animator.SetFloat(MovementSpeed, movement.magnitude);

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
            
            InitializePlayerId(serializedGuid.Value);
        }
        
        public void InitializePlayerId(Guid playerId)
        {
            ownerPlayerId = playerId;

            var playerData = SessionManager.Singleton.PlayersData.GetPlayerData(ownerPlayerId);
            
            if (playerData != null)
            {
                Team = playerData.Value.Team;
                TeaM = Team; // tmp 
            }
        }
        
        void ICommander.ReportFollowing(GameObject go) => Units.Add(go);
        void ICommander.ReportUnfollowing(GameObject go) => Units.Remove(go);

        // commands to the units
        [ServerRpc]
        public void CommandMovementServerRpc()
        {
            foreach (GameObject go in Units)
            {
                if (go.TryGetComponent<ISoldier>(out ISoldier soldier))
                { soldier.SoldierBehaviour = SoldierBehaviour.Move; }
            }
        }
        [ServerRpc]
        public void CommandIdleServerRpc()
        {
            foreach (GameObject go in Units)
            {
                if (go.TryGetComponent<ISoldier>(out ISoldier soldier))
                { soldier.SoldierBehaviour = SoldierBehaviour.Idle; }
            }
        }
        [ServerRpc]
        public void CommandAttackServerRpc()
        {
            foreach (GameObject go in Units)
            {
                if (go.TryGetComponent<ISoldier>(out ISoldier soldier))
                { soldier.SoldierBehaviour = SoldierBehaviour.Attack; }
            }
        }
    }
}
