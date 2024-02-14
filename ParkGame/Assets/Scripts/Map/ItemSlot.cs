using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using FreeDraw;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class ItemSlot : MonoBehaviour, IDropHandler, IPointerDownHandler
{
    public GameObject structureItem;
    public Drawable mapDrawable;
    public Tilemap tilemap;
    public Sprite trashSprite;
    
    [SerializeField] private StructureCounter counter;
    [SerializeField] private ColorSettings colorSettings;
    private Image image;
    private Camera mainCamera;
    private Sprite defaultSprite;

    public RectTransform foreground;
    public RectTransform background;

    private void Awake()
    {
        mainCamera = Camera.main;
        image = GetComponent<Image>();
        defaultSprite = image.sprite;
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

    public void ChangeSprite(bool trashIcon)
    {
        if (trashIcon)
        {
            image.sprite = trashSprite;
        }
        else
        {
            image.sprite = defaultSprite;
        }
    }
    
    
    
    public void InstantiateAndAddNewStructure(Vector3 position)
    {
        // Add new structure to center of screen
        var newStructure = Instantiate(structureItem, counter.transform.parent);
        var origScale = newStructure.transform.localScale;
        newStructure.transform.localScale = Vector3.zero;
        newStructure.transform.DOScale(origScale, 0.4f).SetEase(Ease.OutElastic);

        newStructure.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        newStructure.transform.position = position;
        
        counter.AddMapStructure(newStructure);
        var dragAndDrop = newStructure.GetComponent<DragAndDrop>();
        dragAndDrop.foreground = foreground;
        dragAndDrop.background = background;
        dragAndDrop.mapDrawable = mapDrawable;
        dragAndDrop.TilemapProperty = tilemap;
        dragAndDrop.itemSlot = this;
        if (counter.structureType == StructureCounter.StructureType.Castle)
        {
            var teamLabel = $"TEAM {(counter.GetIndexOfStructure(newStructure) + 1).ToString()}";
            newStructure.GetComponent<Image>().color = colorSettings.Colors[counter.GetIndexOfStructure(newStructure)].Color;
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
        AudioManager.Instance.PlayClickSFX();
        InstantiateAndAddNewStructure(new Vector3(Screen.width / 2.0f, Screen.height / 2.0f, 0));
    }
    
    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null)
            return;
        // This was used for snapping the item back to the slot 
        // eventData.pointerDrag.GetComponent<RectTransform>().anchoredPosition =
        //     GetComponent<RectTransform>().anchoredPosition;
        // counter.ItemDropped(eventData.pointerDrag.gameObject);
        if (counter.RemoveMapStructure(eventData.pointerDrag)) {
            AudioManager.Instance.PlayClickSFX();
            Destroy(eventData.pointerDrag);
        }
    }
}