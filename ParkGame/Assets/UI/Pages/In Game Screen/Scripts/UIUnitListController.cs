using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIUnitListController : MonoBehaviour
{
    [SerializeField] private UIUnit unitPrefab;
    private Dictionary<ISoldier, UIUnit> units = new Dictionary<ISoldier, UIUnit>();
    
    public void AddUnit(ISoldier soldier, Action removeAction)
    {
        var unit = Instantiate(unitPrefab, transform);
        units.Add(soldier, unit);
        unit.Initialize(soldier, removeAction , () => RemoveUnit(soldier));
    }

    public void RemoveUnit(ISoldier soldier)
    {
        if (units.ContainsKey(soldier))
        {
            Destroy(units[soldier].gameObject);
            units.Remove(soldier);
        }
    }
}
