using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using UnityEngine;

public class Announcer : MonoBehaviour
{
    [SerializeField] private float fadeOutDuration = 1f;
    
    private TextMeshProUGUI text;
    private TweenerCore<Color, Color, ColorOptions> colorTween;
    
    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
        // AnnounceEvent("The win point will spawn in 30 seconds!");
    }
    
    public void AnnounceEvent(string message, float duration = 10f)
    {
        if (colorTween != null && !colorTween.IsComplete())
        {
            colorTween.Complete();
        }
        
        text.text = message;
        text.color = Color.white;
        colorTween = text.DOColor(Color.clear, fadeOutDuration).SetDelay(duration);
    }
}
