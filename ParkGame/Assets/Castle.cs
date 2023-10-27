using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Castle : NetworkBehaviour {

    [SerializeField] NetworkVariable<int> _Team = new(0);
    public int Team { get => _Team.Value; set => _Team.Value = value; }

    private void Start() {
        Team = GetComponent<Outpost>().Team;
    }
}
