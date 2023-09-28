using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

public class StructureCounter : MonoBehaviour
{
    public string itemName;
    // Start is called before the first frame update
    
    public int maxStructures = 3;
    private List<GameObject> currentStructures;

    public Tilemap tilemap;

    private void Awake()
    {
        currentStructures = new List<GameObject>();
    }
    

    /**
     * Set max number of structures (Such as castles based on number of teams)
     */
    public void SetMaxStructureCount(int newStructureCount)
    {
        maxStructures = newStructureCount;
    }

    /**
     * Get cell positions in tilemap of concrete structure type
     * @returns tuple of name and list of cell positions  
     */
    public Tuple<string, List<Vector3Int>> GetPlacedStructurePositions()
    {
        var gridLayout = tilemap.GetComponentInParent<GridLayout>();

        var (structureName, structures) = GetStructures();
        var structureCellPositions = new List<Vector3Int>();
        foreach (var structure in structures)
        {
            structureCellPositions.Add(gridLayout.WorldToCell(Camera.main.ScreenToWorldPoint(structure.transform.position)));
        }
        return new Tuple<string, List<Vector3Int>>(structureName, structureCellPositions);
    }
    
    /**
     * Place structures from loaded map for additional adjustments
     * Not used now
     */
    public void LoadStructures(string structureName, SerializedVector3Int[] positions, int maxStructureCount)
    {
        if (positions.Length > maxStructureCount)
            throw new InvalidOperationException("Cannot add more structures than specified maxItems");
        itemName = structureName;
        maxStructures = maxStructureCount;
        var itemSlot = GetComponentInChildren<ItemSlot>();
        var gridLayout = tilemap.GetComponentInParent<GridLayout>();
        foreach (var pos in positions)
        {
            var screenPos = Camera.main.WorldToScreenPoint(
                gridLayout.CellToWorld(new Vector3Int(pos.x, pos.y, pos.z))
            );
            itemSlot.InstantiateAndAddNewStructure(screenPos);
        }
    }

    /**
     * Returns tuple of structure name and list of structures
     */
    public Tuple<string, List<GameObject>> GetStructures()
    {
        return new Tuple<string, List<GameObject>>(itemName, currentStructures);
    }
    
    public bool AllStructuresPlaced()
    {
        return currentStructures.Count == maxStructures;
    }

    public void AddMapStructure(GameObject structure)
    {
        if (currentStructures.Count < maxStructures)
            currentStructures.Add(structure);
        else
            throw new InvalidOperationException($"Cannot add more {itemName}, limit is {maxStructures}");
    }
    
    public void RemoveMapStructure(GameObject structure)
    {
        if (!currentStructures.Remove(structure))
            throw new InvalidOperationException("Structure not found");
    }
}
