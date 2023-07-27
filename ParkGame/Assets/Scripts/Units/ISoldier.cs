using System.Collections;
using UnityEngine;

public enum SoldierBehaviour { Idle, Move, Attack }
public interface ISoldier : ITeamMember
{
    SoldierBehaviour SoldierBehaviour { get; set; }
    public void SetCommanderToFollow(Transform go);
    public void TakeDamage(int damage);
}