using UnityEngine;

public interface IFormationType {
    public void Reset();

    public void RemoveFromFormation(GameObject soldier, GameObject position);

    public void SetRoots(Formation f, GameObject r);

    public GameObject GetPosition(SoldierBase.UnitType unitType);
}
