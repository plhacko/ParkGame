using Managers;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Player;

public class Outpost : NetworkBehaviour, ICommander
{
    public bool IsCastle = false;

    [SerializeField] int InitialTeam;
    [SerializeField] int MaxUnits = 3; // in total
    [SerializeField] float SpawnTime = 4; // 4s
    [SerializeField] GameObject UnitPrefab;
    [SerializeField] GameObject ArcherPrefab;
    [SerializeField] GameObject HorsemanPrefab;


    [SerializeField] Sprite PawnIcon;
    [SerializeField] Sprite ArcherIcon;
    [SerializeField] Sprite HorsemanIcon;
    [SerializeField] private Soldier.UnitType InitOutpostUnitType;
    [SerializeField] private GameObject revealer;
    [SerializeField] private Collider2D switchCollider;
   
    [Tooltip("# seconds. After changing spawnedType wait until you can change it again.")]
    [SerializeField] private int TimeBetweenToggles;
    // if changing spawn type. small time reserve to retoggle (e.g. type0 -> -> type2)
    private const float FastRetoggleTime = 1.5f; // seconds
    private float RetoggleTime = 0; // counting since spawnToggle down. counter

    List<NetworkObjectReference> Units = new List<NetworkObjectReference>();

    NetworkVariable<float> _Timer = new(0.0f);
    public float Timer { get => _Timer.Value; private set => _Timer.Value = value; }

    [SerializeField] NetworkVariable<int> _Team = new(-1);
    public int Team { get => _Team.Value; set => _Team.Value = value; }

    private Soldier.UnitType outpostUnitType;
    private SpriteRenderer sr;
    private int counter;
    private PlayerManager playerManager;
    private ChangeMaterial changeMaterial;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        initialize();
    }

    private void initialize()
    {
        playerManager = FindObjectOfType<PlayerManager>();
        changeMaterial = GetComponent<ChangeMaterial>();
        sr = GetComponent<SpriteRenderer>();

        if (InitOutpostUnitType == Soldier.UnitType.Archer)
        {
            counter = 1;
        }
        sr.sprite = ChangeSpawnType(counter);
        
        _Team.OnValueChanged += onTeamChanged;

        Debug.Log(revealer);
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

        var previousPlayers = playerManager.GetPlayersWithTeam(previousTeam);
        foreach (var player in previousPlayers)
        {
            player.RemoveOutpost(this);
        }

        var teamPlayers = playerManager.GetPlayersWithTeam(newTeam);
        foreach (var player in teamPlayers)
        {
            player.AddOutpost(this);
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

        if (RetoggleTime > 0) {
            RetoggleTime -= Time.deltaTime;
        }
    }

    GameObject SpawnWhichUnit() {
        switch (outpostUnitType) {
            case Soldier.UnitType.Archer:
                return ArcherPrefab;
            case Soldier.UnitType.Horseman:
                return HorsemanPrefab;
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
            case 2:
                outpostUnitType = Soldier.UnitType.Horseman;
                return HorsemanIcon;
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

        if (playerController != null && playerController.Team == Team) {
            if (ControlToggleTime() == true) {
                Debug.Log("change spawning type");
                counter++;
                int numOfUnitTypes = Enum.GetNames(typeof(Soldier.UnitType)).Length;
                ChangeIconClientRpc(counter % numOfUnitTypes);
            }
        }
    }

    bool ControlToggleTime() {
        if ((RetoggleTime > 0) && (TimeBetweenToggles - RetoggleTime > FastRetoggleTime)) {
            return false;
        } else if (RetoggleTime <= 0) {
            RetoggleTime = TimeBetweenToggles;
        }
        return true;
    }

     void OnMouseDown() {
        if (IsCastle) {
            return;
        }

        ulong clientID = NetworkManager.Singleton.LocalClientId;
        RequestChangingSpawnTypeServerRpc(clientID);
        
    }
}
