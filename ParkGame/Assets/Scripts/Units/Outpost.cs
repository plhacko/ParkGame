using Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class Outpost : NetworkBehaviour, ICommander
{
    [SerializeField] int InitialTeam;
    [SerializeField] int MaxUnits = 3;
    [SerializeField] float SpawnTime = 4; // 4s
    [SerializeField] GameObject UnitPrefab;
    List<GameObject> Units = new List<GameObject>();

    NetworkVariable<float> _Timer = new(0.0f);
    public float Timer { get => _Timer.Value; private set => _Timer.Value = value; }

    [SerializeField] NetworkVariable<int> _Team = new(0);
    public int Team { get => _Team.Value; set => _Team.Value = value; }

    private void Start()
    {
        Team = InitialTeam;
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

    public void SpawnUnit()
    {
        // only server can spawn unit
        if (!IsServer)
        { throw new Exception("only server can spawn unit"); }

        Vector3 RndOffset = new Vector3(UnityEngine.Random.Range(-0.01f, 0.01f), UnityEngine.Random.Range(-0.01f, 0.01f), 0f);
        GameObject unit = Instantiate(UnitPrefab, position: transform.position + RndOffset, rotation: transform.rotation);
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

}
