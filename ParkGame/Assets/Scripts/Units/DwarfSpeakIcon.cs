using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class DwarfSpeakIcon : MonoBehaviour
{
    private SpriteRenderer sr;
    private SoldierBase theSoldier;
    [SerializeField] private UnitBehaviourDrawer drawer;
    public UnityEvent speakEvent;
    [SerializeField] private Sprite SpeakIcon;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        theSoldier = GetComponentInParent<SoldierBase>();
        drawer = GetComponent<UnitBehaviourDrawer>();
        speakEvent = theSoldier.SpeakEvent;
        //speakEvent.AddListener(DisplaySpeakIcon);
    }

    void DisplaySpeakIcon() {
        Debug.Log("SPEAK ICON!!!");
        StartCoroutine(DisplaySpeakIcon(SpeakIcon, sr, 1f));//, () => { sr.sprite = drawer.GetIconForCommand(theSoldier.Command); }); );
    }

    public static IEnumerator DisplaySpeakIcon(Sprite iconToShow, SpriteRenderer sr, float t) {
        float timer = t;
        sr.sprite = iconToShow;
        while (timer > 0) {
            timer -= Time.deltaTime;
            yield return null;
        }
        yield break;
    }
}
