using Managers;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Player;

public class Outpost : NetworkBehaviour, ICommander, IConquerable
{
    [SerializeField] int MaxUnits = 3; // in total
    [SerializeField] float SpawnTime = 4; // 4s

    [SerializeField] private SoldierBase.UnitType InitOutpostUnitType;
    [SerializeField] private GameObject revealer;
    [SerializeField] ColorSettings colorSettings;
    [SerializeField] private Collider2D switchCollider;
    [SerializeField] private ConquerModule conquerModule;
    
    List<NetworkObjectReference> Units = new List<NetworkObjectReference>();
    private ShootSalvo shootSalvo;
    NetworkVariable<float> _Timer = new(0.0f);
    public float Timer { get => _Timer.Value; private set => _Timer.Value = value; }

    [SerializeField] NetworkVariable<int> _Team = new(-1);
    public int Team { get => _Team.Value; set => _Team.Value = value; }
    
    public bool IsCastle { get => _IsCastle.Value; set => _IsCastle.Value = value; }
    
    [SerializeField] NetworkVariable<bool> _IsCastle = new();
    
    private SpriteRenderer sr;
    private int counter;
    private PlayerManager playerManager;
    private GameSessionManager gameSessionManager;
    private ChangeMaterial changeMaterial;

    // TODO Action and Dictionary should be reinitialized on outpost owner change
    public Action OnUnitTypeCountChange;
    public Dictionary<SoldierBase.UnitType, int> UnitTypeCount = new() {
        { SoldierBase.UnitType.Pawn, 0 },
        { SoldierBase.UnitType.Archer, 0 },
        { SoldierBase.UnitType.Molerider, 0 }
    };
    private Announcer announcer;
    private bool isSpawning = false;
    private ToggleSpawner spawner;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        initialize();
    }
    
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        playerManager.OnAllPlayersReady -= onAllPlayersReady;
    }

    public void initialize()
    {
        gameSessionManager = FindObjectOfType<GameSessionManager>();
        playerManager = FindObjectOfType<PlayerManager>();
        announcer = FindObjectOfType<Announcer>();
        changeMaterial = GetComponent<ChangeMaterial>();
        sr = GetComponent<SpriteRenderer>();
        spawner = GetComponent<ToggleSpawner>();
        shootSalvo = GetComponent<ShootSalvo>(); // castle shoots salvo of arrows on enemies in its enemy detector
        conquerModule = GetComponentInChildren<ConquerModule>();

        playerManager.OnAllPlayersReady += onAllPlayersReady;
        if (InitOutpostUnitType == SoldierBase.UnitType.Archer)
        {
            counter = 1;
        }

        _Team.OnValueChanged += onTeamChanged;
    }

    private void onAllPlayersReady()
    {
        if(IsServer && IsCastle)
        {
            isSpawning = true;
        }   
    }

    void Start() {
        if (!IsCastle) {
            isSpawning = true;
        }
    }

    public void RegisterOnTeamChange(Action action)
    {
        _Team.OnValueChanged += (previous, current) => action();
    }

    public void UnregisterOnTeamChange(Action action)
    {
        _Team.OnValueChanged -= (previous, current) => action();
    }

    private void onTeamChanged(int previousTeam, int newTeam)
    {
        var playerData = LobbyManager.Singleton.GetLocalPlayerData();
        Debug.Log($"onTeamChanged on outpost, new team: {newTeam} for {gameObject.name}, (local player's team is: {playerData.Team})");

        if (playerData.Team == previousTeam)
        {
            var playerController = playerManager?.GetLocalPlayerController();
            if (playerController != null)
            {
                playerController.RemoveOutpostUI(this);
            }
        }

        if (playerData.Team == newTeam)
        {
            var playerController = playerManager?.GetLocalPlayerController();
            if (playerController != null)
            {
                playerController.AddOutpostUI(this);
            }
        }

        if (playerData.Team == newTeam)
        {
            revealer.gameObject.SetActive(true);
            changeMaterial.Change(false);
        }
        else
        {
            revealer.gameObject.SetActive(false);
            changeMaterial.Change(true);
        }
        
        if(newTeam == -1) return;
        sr.color = colorSettings.Colors[newTeam].Color;
    }

    [ClientRpc]
    void PlayArrowAttackSFXClientRpc() {
        AudioManager.Instance.PlayArrowAttack(transform.position);
    }

  
    void Update()
    {
        // updating only on server
        if (!IsServer || !isSpawning) { return; }
        
        if (IsCastle) {
            if (shootSalvo.Shoot(Time.deltaTime, Team)) {
                PlayArrowAttackSFXClientRpc();
            }
            
        }

        if (Team == -1) {
            return;
        }

        if (gameSessionManager.IsOver || (!IsCastle && conquerModule.IsBeingConquered)) return;

        if (Units.Count >= MaxUnits)
        { Timer = 0f; return; }

        Timer += Time.deltaTime;

        if (Timer >= SpawnTime)
        {
            SpawnUnit();
            Timer = 0;
        }
    }

    void Spawn() {
        GameObject prefab = spawner.GetSpawnPrefab();

        float r = 1f;
        Vector3 RndOffset = new Vector3(UnityEngine.Random.Range(-r, r), UnityEngine.Random.Range(-r, r), 0f);
        GameObject unit = Instantiate(prefab, position: transform.position + RndOffset, rotation: transform.rotation);
        unit.GetComponent<NetworkObject>().Spawn();
        SoldierBase soldier = unit.GetComponent<SoldierBase>();
        soldier.Team = Team;
        soldier.SetCommanderToFollow(transform);
        soldier.NewCommand(SoldierCommand.InOutpost);
        playerManager.AddSoldierToTeam(Team, unit.transform);
    }

    public void SpawnUnit()
    {
        // only server can spawn unit
        if (!IsServer)
        { throw new Exception("only server can spawn unit"); }

        Spawn();
    }


    void ICommander.ReportFollowing(NetworkObjectReference networkObjectReference)
    {
        if (!IsServer)
        { throw new Exception($"only on server can adding units to outpost be reported\n outpost: {gameObject.name}"); }

        Units.Add(networkObjectReference);

        if (!networkObjectReference.TryGet(out var networkObject, NetworkManager.Singleton))
        {
            Debug.LogError($"Could not find network object {networkObjectReference}");
            return;
        }

        if (!networkObject.TryGetComponent<SoldierBase>(out var soldier))
        {
            Debug.LogError($"Could not find soldier {networkObjectReference}");
            return;
        }

        addToUnitsClientRpc(networkObjectReference, Team, soldier.GetUnitType());
    }

    [ClientRpc]
    private void addToUnitsClientRpc(NetworkObjectReference networkObjectReference, int team, SoldierBase.UnitType unitType, ClientRpcParams clientRpcParams = default)
    {
        var localPlayer = playerManager.GetLocalPlayerController();
        if (team == localPlayer.Team)
        {
            UnitTypeCount[unitType]++;
            OnUnitTypeCountChange?.Invoke();
        }
    }

    void ICommander.ReportUnfollowing(NetworkObjectReference networkObjectReference)
    {
        if (!IsServer)
        { throw new Exception($"only on server can removing units to outpost be reported\n outpost: {gameObject.name}"); }

        Debug.Log($"removing unit from outpost: {gameObject.name}");
        Units.Remove(networkObjectReference);

        if (!networkObjectReference.TryGet(out var networkObject, NetworkManager.Singleton))
        {
            Debug.LogError($"Could not find network object {networkObjectReference}");
            return;
        }

        if (!networkObject.TryGetComponent<SoldierBase>(out var soldier))
        {
            Debug.LogError($"Could not find soldier {networkObjectReference}");
            return;
        }

        removeFromUnitsClientRpc(networkObjectReference, Team, soldier.GetUnitType());
    }

    [ClientRpc]
    private void removeFromUnitsClientRpc(NetworkObjectReference networkObjectReference, int team, SoldierBase.UnitType unitType, ClientRpcParams clientRpcParams = default)
    {
        var localPlayer = playerManager.GetLocalPlayerController();
        if (team == localPlayer.Team)
        {
            UnitTypeCount[unitType]--;
            OnUnitTypeCountChange?.Invoke();
        }
    }

    public Formation.FormationType GetFormation() {
        return Formation.FormationType.Free;
    }

    [ClientRpc]
    void ChangeIconClientRpc(int si) {
        sr.sprite = spawner.ChangeSpawnType(si);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestChangingSpawnTypeServerRpc(ulong clientID, bool canBeToggled=false) {
        PlayerController playerController = playerManager.GetPlayerController(clientID);

        if (playerController != null && playerController.Team == Team) {
            if (canBeToggled) {
                Debug.Log("change spawning type");
                counter++;
                int numOfUnitTypes = Enum.GetNames(typeof(SoldierBase.UnitType)).Length;
                ChangeIconClientRpc(counter % numOfUnitTypes);

                // play sfx just for the changing player -- maybe for the whole team?
                ClientRpcParams clientRpcParams = new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new[] { clientID } } };
                announcer.PlayNotificationClientRpc("OutpostSpawnChanged", clientRpcParams); // without this sound altogether?
            }
        }
    }

     void OnMouseDown() {
        if (IsCastle) {
            return;
        }

        ulong clientID = NetworkManager.Singleton.LocalClientId;
        RequestChangingSpawnTypeServerRpc(clientID);
    }

    public void OnStartedConquering(int team)
    {
        //onStartedConqueringClientRpc(team);
        
        NamedColor c = colorSettings.Colors[team];
        
        var teamMembers = playerManager.GetAllMembersOfTeam(Team);
        foreach (var teamMember in teamMembers)
        {
            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new []{ teamMember.OwnerClientId }
                }
            };
            announcer.AnnounceEventClientRpc($"Your outpost is being captured by team {c.Name}!", c.TextColor, 5, clientRpcParams);
            //announcer.PlayNotificationClientRpc("Notification");
        }
            
        var enemyMembers = playerManager.GetAllEnemyMembers(Team);
        foreach (var teamMember in enemyMembers)
        {
            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new []{ teamMember.OwnerClientId }
                }
            };
            announcer.AnnounceEventClientRpc($"Outpost is being captured by team {c.Name}!", c.TextColor, 5, clientRpcParams);
            //announcer.PlayNotificationClientRpc("Notification");
        }
    }
    
    [ClientRpc]
    private void onStartedConqueringClientRpc(int team, ClientRpcParams clientRpcParams = default)
    {
        NamedColor c = colorSettings.Colors[team];
        c.Color.a = 0.8f;
        sr.color = c.Color;
    }

    public void OnConquered(int team)
    {
        int originalTeam = Team;
        Team = team;
        NamedColor c = colorSettings.Colors[team];
        c.Color.a = 0.8f;
        sr.color = c.Color;
        announcer.AnnounceEventClientRpc($"Outpost has been captured by team {c.Name}!", c.TextColor, 5);
        announcer.NotifyInvolvedTeamsServerRpc(Team, originalTeam, Announcer.Wonable.Outpost);
        
        if (originalTeam == -1) {
            ChangeIconClientRpc(0);
        }
    }

    public void OnStoppedConquering(int team)
    {
        onStoppedConqueringClientRpc(team);
    }
    
    [ClientRpc]
    private void onStoppedConqueringClientRpc(int team, ClientRpcParams clientRpcParams = default)
    {
        if (Team == -1)
        {
            sr.color = Color.white;
            return;
        }
        
        sr.color = colorSettings.Colors[Team].Color;
    }
    
    public int GetTeam()
    {
        return Team;
    }
}
