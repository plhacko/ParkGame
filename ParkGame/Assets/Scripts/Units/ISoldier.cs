using System.Collections;
using UnityEngine;


public interface ISoldier : ITeamMember
{
    public void SetCommanderToFollow(GameObject go);
}