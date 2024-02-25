using System.Collections;
using UnityEngine;

public class UILayoutFixer : MonoBehaviour
{
    [SerializeField] private GameObject uiToFix;

    private void Awake()
    {
        StartCoroutine(fix());
    }

    IEnumerator fix()
    {
        uiToFix.SetActive(false);
        yield return new WaitForSeconds(1);
        uiToFix.SetActive(true);
    }
}
