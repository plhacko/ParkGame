using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Managers;
using Player;
using Unity.Netcode;

// NetworkBehaviour na child objektu, kde je taky skript s NB?
public class ToggleSpawnedUnitScript : NetworkBehaviour {
    [SerializeField] Sprite PawnIcon;
    [SerializeField] Sprite ArcherIcon;
    [SerializeField] Sprite HorsemanIcon;
    private SpriteRenderer sr;
    private int counter;
    public Soldier.UnitType OutpostUnitType;
    private PlayerManager playerManager;
    private int Team; 

    private void Start() {
        playerManager = FindObjectOfType<PlayerManager>();

        OutpostUnitType = Soldier.UnitType.Pawn;
        sr = GetComponent<SpriteRenderer>();
        sr.sprite = GetIcon(0);
        Team = gameObject.GetComponentInParent<Outpost>().Team;
    }

    Sprite GetIcon(int n) {
        switch (n) {
            case 1:
                OutpostUnitType = Soldier.UnitType.Archer;
                return Instantiate(ArcherIcon);
            //case 2:
            //    OutpostUnitType = UnitType.Horseman;
            //    return Instantiate(HorsemanIcon);
            default:
                OutpostUnitType = Soldier.UnitType.Pawn;
                return Instantiate(PawnIcon);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestChangingSpawnTypeServerRpc(ulong clientID) {
        PlayerController playerController = playerManager.GetPlayerController(clientID);
        //Debug.Log("teams: " + playerController.Team + " outpost from: " + Team);

        if (playerController != null && playerController.Team == Team) {
            counter++;
            sr.sprite = GetIcon(counter % 2);
        }
    }

    void OnMouseDown() {
        //Debug.Log("ICON CLICKED");
        ulong clientID = NetworkManager.Singleton.LocalClientId;
        RequestChangingSpawnTypeServerRpc(clientID);
    }

}
