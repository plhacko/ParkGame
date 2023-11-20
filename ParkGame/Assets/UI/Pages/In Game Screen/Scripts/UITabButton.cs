using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Image))]
public class UITabButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    private UITabGroup tabGroup;

    [HideInInspector] public Image background;   
    [HideInInspector] public Color defaultColor;
    
    [SerializeField] private Color _selectedColor;
    public Color selectedColor => _selectedColor;
    [SerializeField] private Color _hoverColor;
    public Color hoverColor => _hoverColor;

    // Start is called before the first frame update
    void Awake()
    {
        background = GetComponent<Image>();        
        defaultColor = background.color;
    }

    public void SetTabGroup(UITabGroup tabGroup)
    {
        this.tabGroup = tabGroup;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        tabGroup.OnTabSelected(this);        
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tabGroup.OnTabExit(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        tabGroup.OnTabEnter(this);
    }
}