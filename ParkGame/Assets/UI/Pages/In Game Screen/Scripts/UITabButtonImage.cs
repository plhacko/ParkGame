using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Image))]
public class UITabButtonImage : Selectable, IPointerClickHandler
{
    private UITabGroupImage tabGroup;

    public void SetTabGroup(UITabGroupImage tabGroup)
    {
        this.tabGroup = tabGroup;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!IsInteractable()) return;
        AudioManager.Instance.PlayClickSFX();
        tabGroup.OnTabSelected(this);        
    }
}
