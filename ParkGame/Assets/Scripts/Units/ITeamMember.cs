using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISoldier
{
    public int Team { get; set; }

    public void SetCommanderToFollow(GameObject go);
}

public interface ILeader
{
    public void ReportFollowing(GameObject go);
    public void ReportUnFollowing(GameObject go);
}