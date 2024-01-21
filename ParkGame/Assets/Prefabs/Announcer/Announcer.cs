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
    
    public enum Wonable {
        game, outpost, vp
    }

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
    public void PlayOutpostConqueredSFXClientRpc(bool wins, ClientRpcParams clientRpcParams = default) {
        string sfxName = (wins ? "OutpostGained" : "OutpostLost");
        PlayNotificationClientRpc(sfxName);

    }

    [ClientRpc]
    public void PlayConqueredSFXClientRpc(int winners, int affiliation, Wonable what, ClientRpcParams clientRpcParams = default) {
        bool wins = (affiliation == winners);
        string sfxName = "";
        switch (what) {
            case Wonable.game:
                sfxName = (wins ? "GameWon" : "GameLost");
                break;
            case Wonable.vp:
                sfxName = (wins ? "VPGained" : "VPnotGained");
                break;
            default:
                break;
        }
        PlayNotificationClientRpc(sfxName);
    }

    [ClientRpc]
    public void PlayNotificationClientRpc(string sfxName, ClientRpcParams clientRpcParams = default) {
        AudioManager.Instance.PlayNotificationSFX(sfxName);
    }

    [ClientRpc]
    public void AnnounceEventClientRpc(FixedString512Bytes message, float duration = 45f, ClientRpcParams clientRpcParams = default)
    {
        AnnounceEvent(message.Value, duration);
    }
}
