using System.Collections;
using UnityEngine;

public interface ILeader : ITeamMember
{
    public void ReportFollowing(GameObject go);
    public void ReportUnfollowing(GameObject go);
}
