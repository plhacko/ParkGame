using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;

public class DwarfSpeakIcon : MonoBehaviour
{
    private UnitBehaviourDrawer statusDrawer;
    private SpriteRenderer sr;
    private Soldier theSoldier;
    public UnityEvent speakEvent;
    [SerializeField] Sprite SpeakIcon;

    void Start()
    {
        statusDrawer = GetComponent<UnitBehaviourDrawer>();
        sr = GetComponent<SpriteRenderer>();
        theSoldier = GetComponentInParent<Soldier>();
        speakEvent = theSoldier.SpeakEvent;
        speakEvent.AddListener(DisplaySpeakIcon);
    }

    void DisplaySpeakIcon() {
        StartCoroutine(DisplaySpeakIcon(SpeakIcon, sr, 1f));
    }

    public static IEnumerator DisplaySpeakIcon(Sprite iconToShow, SpriteRenderer sr, float t) {
        float timer = t;
        Sprite prevIcon = sr.sprite;
        sr.sprite = iconToShow;
        while (timer > 0) {
            timer -= Time.deltaTime;
            yield return null;
        }
        sr.sprite = prevIcon;
        yield break;
    }
}
