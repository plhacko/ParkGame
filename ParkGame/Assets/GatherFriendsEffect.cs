using UnityEngine;
using DG.Tweening;
public class GatherFriendsEffect : MonoBehaviour
{

    [SerializeField] private ColorSettings colorSettings;
    private SpriteRenderer sr;
    private float minSize = 0f;
    private float maxSize = 4.9f;

    private void Start() {
        sr = GetComponent<SpriteRenderer>();
    }
    public void CallGatherCommand(int team) {
        var color = colorSettings.Colors[team].Color;
        var startColor = new Color(color.r, color.g, color.b, 0.1f);
        var endColor = new Color(color.r, color.g, color.b, 0.8f);
        sr.color = startColor;
        Sequence seqScale = DOTween.Sequence();
        Sequence seqColor = DOTween.Sequence();
        seqScale.Append(sr.gameObject.transform.DOScale(new Vector3(maxSize, maxSize, maxSize), 0.3f)).Append(sr.gameObject.transform.DOScale(new Vector3(minSize, minSize, minSize), 0.6f));
        seqColor.Append(sr.DOColor(endColor, 0.3f)).Append(sr.DOColor(startColor, 0.6f));
    }
}
