using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UILayoutFixer : MonoBehaviour
{
    [SerializeField] private GameObject uiToFix;
    [SerializeField] private List<GameObject> uiToFixList;

    private void Awake()
    {
        StartCoroutine(fix());
    }

    // peak engineering ahead
    IEnumerator fix()
    {
        // uiToFix.SetActive(false);
        SetGameObjectsActive(false);
        yield return null;
        // uiToFix.SetActive(true);
        SetGameObjectsActive(true);
        yield return null;
        // uiToFix.SetActive(false);
        SetGameObjectsActive(false);
        yield return new WaitForSeconds(0.5f);
        // uiToFix.SetActive(true);
        SetGameObjectsActive(true);
    }

    private void SetGameObjectsActive(bool active)
    {
        foreach (var ui in uiToFixList)
        {
            ui.SetActive(active);
        }
    }
}
