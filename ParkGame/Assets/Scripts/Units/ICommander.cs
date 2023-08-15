using System.Collections;
using UnityEngine;

public interface ICommander : ITeamMember
{

    public void ReportFollowing(GameObject go);
    public void ReportUnfollowing(GameObject go);
    public enum SoldierMovements { Free, Circle, Rectangle };

    public SoldierMovements GetFormation();
}
