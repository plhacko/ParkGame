using UnityEngine;

public interface IFormationType {
    public void Reset();

    public void RemoveFromFormation(GameObject soldier, GameObject position);

    public void SetRoots(Formation f, GameObject hr, GameObject br = null);

    public GameObject GetPosition(Soldier.UnitType unitType);
}
