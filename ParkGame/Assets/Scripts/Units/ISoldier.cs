using System.Collections;
using UnityEngine;

public enum SoldierBehaviour { Idle, Move, Attack, Formation, Death }
public interface ISoldier : ITeamMember
{
    SoldierBehaviour SoldierBehaviour { get; set; }
    public void SetCommanderToFollow(Transform go);
    public void TakeDamage(int damage);
    public void NavMeshFormationSwitch(bool enable, SoldierBehaviour newBehaviour, Formation formation, Formation.FormationType type=Formation.FormationType.Free);
}