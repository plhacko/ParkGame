using System.Collections;
using UnityEngine;

public class UILayoutFixer : MonoBehaviour
{
    [SerializeField] private GameObject uiToFix;

    private void Awake()
    {
        StartCoroutine(fix());
    }

    // peak engineering ahead
    IEnumerator fix()
    {
        uiToFix.SetActive(false);
        yield return null;
        uiToFix.SetActive(true);
        yield return null;
        uiToFix.SetActive(false);
        yield return new WaitForSeconds(2);
        uiToFix.SetActive(true);
    }
}
