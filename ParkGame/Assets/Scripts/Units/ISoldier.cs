using System.Collections;
using UnityEngine;
using Unity.Netcode;

public enum SoldierBehaviour { Idle, Move, Attack, Formation, Death }
public enum SoldierCommand {
    InOutpost, // defensive, wary
    Following, // following commander or position in formation
    FollowingCommander, // following commander or position in formation
    FollowingInFormationCircle, // following commander or position in formation
    FollowingInFormationBox, // following commander or position in formation
    Attack,
    ReturnToOutpost, // return to closest outpost, or also attack just when attacked? but be wary of close enemies??? 
    Fallback, // ? follow commander or return to closest outpost?
}

public interface ISoldier : ITeamMember
{
    SoldierBehaviour SoldierBehaviour { get; set; }
    SoldierCommand Command { get; set; }
    public void SetCommanderToFollow(Transform go);
    public void TakeDamage(int damage);
    public void NavMeshFormationSwitch(bool enable, SoldierBehaviour newBehaviour, Formation formation, Formation.FormationType type=Formation.FormationType.Free);
    public void NewCommand(SoldierCommand command);
}