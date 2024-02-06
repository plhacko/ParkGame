using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ToolbarDisplayer : MonoBehaviour
{

    [SerializeField] private RectTransform maskParent;
    [SerializeField] private RectTransform mask;

    public void ShowToolbar()
    {
        var targetDelta = maskParent.sizeDelta;

        mask.DOSizeDelta(targetDelta, 0.25f).OnComplete(() =>
        {
            mask.GetComponent<CanvasGroup>().interactable = true;
        });
    }

    public void HideToolbar()
    {
        var targetDelta = new Vector2(maskParent.sizeDelta.x, 0);

        mask.GetComponent<CanvasGroup>().interactable = false;
        
        mask.DOSizeDelta(targetDelta, 0.25f);
    }
}
