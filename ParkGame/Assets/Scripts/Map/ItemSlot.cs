using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FreeDraw;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class ItemSlot : MonoBehaviour, IDropHandler, IPointerDownHandler
{
    public GameObject structureItem;
    public Drawable mapDrawable;
    public Tilemap tilemap;
    public Camera mainCamera;
    
    private StructureCounter counter;
    private void Awake()
    {
        counter = gameObject.GetComponentInParent<StructureCounter>();
    }

    /**
     * Get cell positions in tilemap of concrete structure type
     * @returns tuple of name and list of cell positions  
     */
    public Tuple<string, List<Vector3Int>> SavePlacedStructures()
    {
        var gridLayout = tilemap.GetComponentInParent<GridLayout>();
        
        var (structureName, structures) = counter.GetStructures();
        var structureCellPositions = new List<Vector3Int>();
        foreach (var structure in structures)
        {
            structureCellPositions.Add(gridLayout.WorldToCell(mainCamera.ScreenToWorldPoint(structure.transform.position)));
        }
        return new Tuple<string, List<Vector3Int>>(structureName, structureCellPositions);
    }

    public void InstantiateAndAddNewStructure(Vector3 position)
    {
        // Add new structure to center of screen
        var newStructure = Instantiate(structureItem, transform.parent);

        newStructure.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        newStructure.transform.position = position;
        counter.AddMapStructure(newStructure);
        var dragAndDrop = newStructure.GetComponent<DragAndDrop>();
        dragAndDrop.mapDrawable = mapDrawable;
        dragAndDrop.TilemapProperty = tilemap;
        dragAndDrop.mainCamera = mainCamera;
        if (counter.structureType == StructureCounter.StructureType.Castle)
        {
            var teamLabel = $"TEAM {(counter.GetIndexOfStructure(newStructure) + 1).ToString()}";
            newStructure.GetComponentInChildren<TextMeshProUGUI>().text = teamLabel;
        }
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        if (counter.AllStructuresPlaced())
        {
            Debug.Log("All structures placed");
            return;
        }
        InstantiateAndAddNewStructure(new Vector3(Screen.width / 2.0f, Screen.height / 2.0f, 0));
    }
    
    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            // This was used for snapping the item back to the slot 
            // eventData.pointerDrag.GetComponent<RectTransform>().anchoredPosition =
            //     GetComponent<RectTransform>().anchoredPosition;
            // counter.ItemDropped(eventData.pointerDrag.gameObject);
            counter.RemoveMapStructure(eventData.pointerDrag);
            Destroy(eventData.pointerDrag);
        }
    }
}