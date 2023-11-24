using Managers;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Player;

public class Outpost : NetworkBehaviour, ICommander
{
    [SerializeField] int InitialTeam;
    [SerializeField] bool IsCastle = false; 
    [SerializeField] int MaxUnits = 3; // in total
    [SerializeField] float SpawnTime = 4; // 4s
    [SerializeField] GameObject UnitPrefab;
    [SerializeField] GameObject ArcherPrefab;

    [SerializeField] Sprite PawnIcon;
    [SerializeField] Sprite ArcherIcon;
    [SerializeField] Sprite HorsemanIcon;
    [SerializeField] private Soldier.UnitType InitOutpostUnitType;

    //[SerializeField] GameObject HorsemanPrefab; // todo
    List<NetworkObjectReference> Units = new List<NetworkObjectReference>();
    //ToggleSpawnedUnitScript OutpostSpawnerChanger;

    NetworkVariable<float> _Timer = new(0.0f);
    public float Timer { get => _Timer.Value; private set => _Timer.Value = value; }

    [SerializeField] NetworkVariable<int> _Team = new(-1);
    public int Team { get => _Team.Value; set => _Team.Value = value; }

    private Soldier.UnitType outpostUnitType;
    private SpriteRenderer sr;
    private int counter;
    private PlayerManager playerManager;
    private FogOfWar fogOfWar;
    private Revealer revealer;
    private ChangeMaterial changeMaterial;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        initialize();
    }

    private void initialize()
    {
        playerManager = FindObjectOfType<PlayerManager>();
        fogOfWar = FindObjectOfType<FogOfWar>();
        revealer = GetComponent<Revealer>();
        changeMaterial = GetComponent<ChangeMaterial>();
        sr = GetComponent<SpriteRenderer>();

        if (InitOutpostUnitType == Soldier.UnitType.Archer)
        {
            counter = 1;
        }
        sr.sprite = ChangeSpawnType(counter);
        
        _Team.OnValueChanged += onTeamChanged;

        if (IsServer)
        {
            Team = InitialTeam;   
        }
        else
        {
            onTeamChanged(-1, Team);
        }
    }

    private void onTeamChanged(int previousTeam, int newTeam)
    {
        var playerData = LobbyManager.Singleton.GetLocalPlayerData();
        Debug.Log($"onTeamChanged on outpost, new team: {newTeam} for {gameObject.name}, (local player's team is: {playerData.Team})");
        if (playerData.Team == newTeam)
        {
            if (fogOfWar)
            {
                fogOfWar.RegisterAsRevealer(revealer);   
            }
        }
        else
        {
            if (fogOfWar)
            {
                changeMaterial.Change();   
            }
        }
    }

    void Update()
    {
        // updating only on server
        if (!IsServer || Team == -1)
        { return; }

        if (Units.Count >= MaxUnits)
        { Timer = 0f; return; }

        Timer += Time.deltaTime;

        if (Timer >= SpawnTime)
        {
            SpawnUnit();
            Timer = 0;
        }
    }

    GameObject SpawnWhichUnit() {
        switch (outpostUnitType) {
            case Soldier.UnitType.Archer:
                return ArcherPrefab;
            default:
                return UnitPrefab;
        }
    }

    public void SpawnUnit()
    {
        // only server can spawn unit
        if (!IsServer)
        { throw new Exception("only server can spawn unit"); }

        Vector3 RndOffset = new Vector3(UnityEngine.Random.Range(-0.01f, 0.01f), UnityEngine.Random.Range(-0.01f, 0.01f), 0f);
        GameObject unit = Instantiate(SpawnWhichUnit(), position: transform.position + RndOffset, rotation: transform.rotation);
        unit.GetComponent<NetworkObject>().Spawn();
        unit.GetComponent<ISoldier>().Team = Team;
        unit.GetComponent<ISoldier>().SetCommanderToFollow(transform);
    }

    public void SetCastle(int team)
    {
        IsCastle = true;
        InitialTeam = team;
    }

    void ICommander.ReportFollowing(NetworkObjectReference networkObjectReference)
    {
        if (!IsServer)
        { throw new Exception($"only on server can adding units to outpost be reported\n outpost: {gameObject.name}"); }

        Units.Add(networkObjectReference);
    }

    void ICommander.ReportUnfollowing(NetworkObjectReference networkObjectReference)
    {
        if (!IsServer)
        { throw new Exception($"only on server can removing units to outpost be reported\n outpost: {gameObject.name}"); }

        Units.Remove(networkObjectReference);
    }

    public Formation.FormationType GetFormation() {
        return Formation.FormationType.Free;
    }

    // change spawn type and icon
    Sprite ChangeSpawnType(int n) {
        switch (n) {
            case 1:
                outpostUnitType = Soldier.UnitType.Archer;
                return ArcherIcon;
            //case 2:
            //    OutpostUnitType = UnitType.Horseman;
            //    return Instantiate(HorsemanIcon);
            default:
                outpostUnitType = Soldier.UnitType.Pawn;
                return PawnIcon;
        }
    }

    [ClientRpc]
    void ChangeIconClientRpc(int si) {
        sr.sprite = ChangeSpawnType(si);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestChangingSpawnTypeServerRpc(ulong clientID) {
        PlayerController playerController = playerManager.GetPlayerController(clientID);
        Debug.Log("teams: " + playerController.Team + " outpost from: " + Team);

        if (playerController != null && playerController.Team == Team) {
            Debug.Log("ZMEN IKONU!");
            counter++;
            int numOfUnitTypes = Enum.GetNames(typeof(Soldier.UnitType)).Length;
            ChangeIconClientRpc(counter % numOfUnitTypes);
        }
    }

    void OnMouseDown() {
        /*
        Debug.Log("ICON CLICKED");

        ulong clientID = NetworkManager.Singleton.LocalClientId;
        RequestChangingSpawnTypeServerRpc(clientID);
        */
    }
}
