using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class Announcer : NetworkBehaviour
{
    [SerializeField] private float fadeOutDuration = 1f;
    
    private TextMeshProUGUI text;
    private TweenerCore<Color, Color, ColorOptions> colorTween;
    
    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
    }
    
    public void AnnounceEvent(string message, float duration = 45f)
    {
        if (colorTween != null && !colorTween.IsComplete())
        {
            colorTween.Complete();
        }
        
        text.text = message;
        text.color = Color.white;
        colorTween = text.DOColor(Color.clear, fadeOutDuration).SetDelay(duration);
    }

    [ClientRpc]
    public void AnnounceEventClientRpc(FixedString512Bytes message, float duration = 45f)
    {
        AnnounceEvent(message.Value, duration);
    }
}
