using System;
using System.Collections.Generic;
using System.Windows.Input;
using Managers;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;
using static Formation;

namespace Player
{
    public class PlayerController : NetworkBehaviour, ICommander
    {
        [SerializeField] private float movementSpeed = 1;

        private SpriteRenderer spriteRenderer;
        private Animator animator;
        private NetworkAnimator networkAnimator;
        private Guid ownerPlayerId;
        public Formation FormationScript;

        // Replicated variable for sprite orientation
        private NetworkVariable<bool> xSpriteFlip = new(false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner);

        private static readonly int MovementSpeed = Animator.StringToHash("MovementSpeed");

        NetworkVariable<int> _Team = new(0);
        public int Team { get => _Team.Value; set => _Team.Value = value; }

        List<GameObject> Units = new();

        public FormationType FormationType; // todo?
        public FormationType GetFormation() {
            return FormationType;
        }

        private void initialize()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();
            FormationScript = GetComponent<Formation>();
            FormationType = FormationType.Free; // movement without navmesh
            
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
            if (Input.GetKeyDown(KeyCode.C)) { FormatSoldiers(KeyCode.C); }
            if (Input.GetKeyDown(KeyCode.R)) { FormatSoldiers(KeyCode.R); }
        }

        public void FormatSoldiers(KeyCode key) {
            if (key == KeyCode.C) {
                // circle formation
                switch (FormationType) {
                    case FormationType.Box:
                    case FormationType.Free:
                        FormationType = FormationType.Circle;
                        // notify soldiers
                        NotifySoldiersServerRpc();
                        break;

                    case FormationType.Circle:
                        FormationType = FormationType.Free;
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
                    case FormationType.Circle:
                    case FormationType.Free:
                        FormationType = FormationType.Box;
                        FormationScript.ResetFormation();
                        // notify soldiers
                        NotifySoldiersServerRpc();
                        break;

                    case FormationType.Box:
                        FormationType = FormationType.Free;
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

            Debug.Log("nu of Units: " + Units.Count);
            foreach (GameObject go in Units) {
                if (go.TryGetComponent<ISoldier>(out ISoldier soldier)) {
                    switch (FormationType) {
                        case FormationType.Free:
                            Debug.Log("notify soldiers. C is OFF");
                            soldier.NavMeshFormationSwitch(false, SoldierBehaviour.Idle, FormationScript, FormationType);
                            FormationScript.ResetFormation();
                            break;
                        case FormationType.Circle:
                            Debug.Log("notify them. C is ON");
                            soldier.NavMeshFormationSwitch(false, SoldierBehaviour.Idle, FormationScript, FormationType);
                            soldier.NavMeshFormationSwitch(true, SoldierBehaviour.Formation, FormationScript, FormationType);
                            break;
                        case FormationType.Box:
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
            
            InitializePlayerId(serializedGuid.Value);
        }
        
        public void InitializePlayerId(Guid playerId)
        {
            ownerPlayerId = playerId;

            var playerData = SessionManager.Singleton.PlayersData.GetPlayerData(ownerPlayerId);
            
            if (playerData != null)
            {
                Team = playerData.Value.Team;
            }
        }
        
        void ICommander.ReportFollowing(GameObject go) => Units.Add(go);
        void ICommander.ReportUnfollowing(GameObject go) => Units.Remove(go);

        // commands to the units
        [ServerRpc]
        void CommandMovementServerRpc()
        {
            foreach (GameObject go in Units)
            {
                if (go.TryGetComponent<ISoldier>(out ISoldier soldier))
                { soldier.SoldierBehaviour = SoldierBehaviour.Move; }
            }
        }
        [ServerRpc]
        void CommandIdleServerRpc()
        {
            foreach (GameObject go in Units)
            {
                if (go.TryGetComponent<ISoldier>(out ISoldier soldier))
                { soldier.SoldierBehaviour = SoldierBehaviour.Idle; }
            }
        }
        [ServerRpc]
        void CommandAttackServerRpc()
        {
            foreach (GameObject go in Units)
            {
                if (go.TryGetComponent<ISoldier>(out ISoldier soldier))
                { soldier.SoldierBehaviour = SoldierBehaviour.Attack; }
            }
        }
    }
}
