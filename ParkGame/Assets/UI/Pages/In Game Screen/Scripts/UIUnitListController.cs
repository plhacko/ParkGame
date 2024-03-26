using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIUnitListController : MonoBehaviour
{
    [SerializeField] private UIUnit unitPrefab;
    private Dictionary<SoldierBase, UIUnit> units = new Dictionary<SoldierBase, UIUnit>();
    
    public void AddUnit(SoldierBase soldier, Action removeAction)
    {
        var unit = Instantiate(unitPrefab, transform);
        units.Add(soldier, unit);
        unit.Initialize(soldier, removeAction , () => RemoveUnit(soldier));
    }

    public void RemoveUnit(SoldierBase soldier)
    {
        if (units.ContainsKey(soldier))
        {
            Destroy(units[soldier].gameObject);
            units.Remove(soldier);
        }
    }
}
