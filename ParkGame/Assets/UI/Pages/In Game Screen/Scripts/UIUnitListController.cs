using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIUnitListController : MonoBehaviour
{
    [SerializeField] private UIUnit unitPrefab;
    private Dictionary<Soldier, UIUnit> units = new Dictionary<Soldier, UIUnit>();
    
    public void AddUnit(Soldier soldier)
    {
        var unit = Instantiate(unitPrefab, transform);
        units.Add(soldier, unit);
        unit.Initialize(soldier);
    }

    public void RemoveUnit(Soldier soldier)
    {
        if (units.ContainsKey(soldier))
        {
            Destroy(units[soldier].gameObject);
            units.Remove(soldier);
        }
    }
}
