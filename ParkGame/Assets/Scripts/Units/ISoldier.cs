using System.Collections;
using UnityEngine;

public enum SoldierBehaviour { Idle, Move, Attack, FormationCircle, FormationRectangle }
public interface ISoldier : ITeamMember
{
    SoldierBehaviour SoldierBehaviour { get; set; }
    public void SetCommanderToFollow(Transform go);
    public void TakeDamage(int damage);
    public void NavMeshFormationSwitch(bool enable, SoldierBehaviour newBehaviour, Formation formation, ICommander.SoldierMovements asdf);
}