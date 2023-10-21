using Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using Player;

public class Outpost : NetworkBehaviour, ICommander
{
    [SerializeField] int InitialTeam;
    [SerializeField] int MaxUnits = 3; // in total
    [SerializeField] float SpawnTime = 4; // 4s
    [SerializeField] GameObject UnitPrefab;
    [SerializeField] GameObject ArcherPrefab;

    [SerializeField] Sprite PawnIcon;
    [SerializeField] Sprite ArcherIcon;
    [SerializeField] Sprite HorsemanIcon;
    [SerializeField] private UnitType OutpostUnitType;
    
    //[SerializeField] GameObject HorsemanPrefab; // todo
    List<GameObject> Units = new List<GameObject>();
    //ToggleSpawnedUnitScript OutpostSpawnerChanger;

    NetworkVariable<float> _Timer = new(0.0f);
    public float Timer { get => _Timer.Value; private set => _Timer.Value = value; }

    [SerializeField] NetworkVariable<int> _Team = new(0);
    public int Team { get => _Team.Value; set => _Team.Value = value; }

    private SpriteRenderer sr;
    private int counter;
    private PlayerManager playerManager;
    
    //private void Start()
    private void Awake()
    {
        Team = InitialTeam;
        //OutpostSpawnerChanger = transform.Find("IconToggler").GetComponent<ToggleSpawnedUnitScript>();
        playerManager = FindObjectOfType<PlayerManager>();

        OutpostUnitType = UnitType.Pawn;
        sr = GetComponent<SpriteRenderer>();
        sr.sprite = GetIcon(0);
    }

    void Update()
    {
        // updating only on server
        if (!IsServer)
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
        //UnitType t = OutpostSpawnerChanger.OutpostUnitType;
        UnitType t = OutpostUnitType;
        switch (t) {
            case UnitType.Archer:
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

    void ICommander.ReportFollowing(GameObject go)
    {
        if (!IsServer)
        { throw new Exception($"only on server can adding units to outpost be reported\n outpost: {gameObject.name}"); }

        Units.Add(go);
    }

    void ICommander.ReportUnfollowing(GameObject go)
    {
        if (!IsServer)
        { throw new Exception($"only on server can removing units to outpost be reported\n outpost: {gameObject.name}"); }

        Units.Remove(go);
    }

    public Formation.FormationType GetFormation() {
        return Formation.FormationType.Free;
    }

    // change spawn type and icon
    Sprite GetIcon(int n) {
        switch (n) {
            case 1:
                OutpostUnitType = UnitType.Archer;
                return Instantiate(ArcherIcon);
            //case 2:
            //    OutpostUnitType = UnitType.Horseman;
            //    return Instantiate(HorsemanIcon);
            default:
                OutpostUnitType = UnitType.Pawn;
                return Instantiate(PawnIcon);
        }
    }

    [ClientRpc]
    void ChangeIconClientRpc(int si) {
        sr.sprite = GetIcon(si);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestChangingSpawnTypeServerRpc(ulong clientID) {
        PlayerController playerController = playerManager.GetPlayerController(clientID);
        Debug.Log("teams: " + playerController.Team + " outpost from: " + Team);

        if (playerController != null && playerController.Team == Team) {
            Debug.Log("ZMEN IKONU!");
            counter++;
            //sr.sprite = GetIcon(counter % 2);
        }
        // + ClientRpc
        ChangeIconClientRpc(counter % 2);
    }

    void OnMouseDown() {
        Debug.Log("ICON CLICKED");

        ulong clientID = NetworkManager.Singleton.LocalClientId;
        RequestChangingSpawnTypeServerRpc(clientID);
    }
}
