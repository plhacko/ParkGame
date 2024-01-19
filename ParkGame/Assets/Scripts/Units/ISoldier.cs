using System.Collections;
using UnityEngine;
using Unity.Netcode;

public enum SoldierCommand {
    InOutpost, // defensive, wary
    Following, // following commander or position in formation
    Attack,
    Die // :/
}

public interface ISoldier : ITeamMember
{
    SoldierCommand Command { get; set; }
    public void SetCommanderToFollow(Transform go);
    public void TakeDamage(int damage);
    public void NavMeshFormationSwitch(bool enable, Formation formation, Formation.FormationType type=Formation.FormationType.Free);
    public void NewCommand(SoldierCommand command);
}