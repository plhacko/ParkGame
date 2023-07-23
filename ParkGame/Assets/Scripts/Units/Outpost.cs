using Managers;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class Outpost : NetworkBehaviour, ITeamMember
{
    [SerializeField] int InitialTeam = 0;
    [SerializeField] int MaxUnits = 3;
    [SerializeField] float SpawnTime = 4; // 4s
    [SerializeField] GameObject UnitPrefab;
    List<GameObject> Units = new List<GameObject>();

    NetworkVariable<float> _Timer = new NetworkVariable<float>(0);
    public float Timer { get => _Timer.Value; private set => _Timer.Value = value; }

    NetworkVariable<int> _Team = new NetworkVariable<int>();
    public int Team { get => _Team.Value; set => _Team.Value = value; }

    private void Start()
    {
        // rest only on server
        if (!NetworkManager.Singleton.IsServer)
        { return; }

        Team = InitialTeam;
    }

    void Update()
    {
        // updating only on server
        if (!NetworkManager.Singleton.IsServer)
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
        if (!NetworkManager.Singleton.IsServer)
        { throw new Exception("only server can spawn unit"); }

        GameObject unit = Instantiate(UnitPrefab, position: transform.position, rotation: transform.rotation);
        unit.GetComponent<NetworkObject>().Spawn();
        unit.GetComponent<ITeamMember>().Team = Team;

        Units.Add(unit);
    }
}
