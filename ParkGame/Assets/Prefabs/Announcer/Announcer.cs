using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TMPro;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using Managers;
using System.Collections.Generic;
using Player;

public class Announcer : NetworkBehaviour
{
    [SerializeField] private float fadeOutDuration = 1f;
    
    private TextMeshProUGUI text;
    private TweenerCore<Color, Color, ColorOptions> colorTween;
    private PlayerManager playerManager;
    public enum Wonable {
        Game, Outpost, VP
    }

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
        playerManager = FindObjectOfType<PlayerManager>();
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
    public void PlayConqueredSFXClientRpc(bool wins, Wonable what, ClientRpcParams clientRpcParams = default) {
        string sfxName = "";
        switch (what) {
            case Wonable.Outpost: sfxName = (wins ? "OutpostGained" : "OutpostLost"); break;
            case Wonable.VP: sfxName = (wins ? "VPGained" : "VPnotGained"); break;
            case Wonable.Game: sfxName = (wins ? "GameWon" : "GameLost"); break;
            default: break;
        }
        PlayNotificationClientRpc(sfxName);

    }

    [ClientRpc]
    public void PlayConqueredSFXClientRpc(int winners, int affiliation, Wonable what, ClientRpcParams clientRpcParams = default) {
        bool wins = (affiliation == winners);
        string sfxName = "";
        switch (what) {
            case Wonable.Game:
                sfxName = (wins ? "GameWon" : "GameLost");
                break;
            case Wonable.VP:
                sfxName = (wins ? "VPGained" : "VPnotGained");
                break;
            default:
                break;
        }
        PlayNotificationClientRpc(sfxName);
    }

    [ClientRpc]
    public void PlayNotificationClientRpc(string sfxName, ClientRpcParams clientRpcParams = default) {
        Debug.Log("PLAY NOTIFICATION " + sfxName);
        AudioManager.Instance.PlayNotificationSFX(sfxName);
    }
    ulong[] CreateMemberList(List<PlayerController> members) {
        if (members != null || members.Count > 0) {
            ulong[] uList = new ulong[members.Count];
            int i = 0;
            foreach (var m in members) {
                uList[i] = m.OwnerClientId;
                i++;
            }
            return uList;
        }
        return null;
    }

    [ServerRpc]
    public void NotifyInvolvedTeamsServerRpc(int winningTeam, int losingTeam, Wonable what) {
        // sfx for winners
        var winners = playerManager.GetAllMembersOfTeam(winningTeam);

        ClientRpcParams clientRpcParams = new ClientRpcParams {
            Send = new ClientRpcSendParams {
                TargetClientIds = CreateMemberList(winners)
            }
        };
        PlayConqueredSFXClientRpc(true, what, clientRpcParams);

        // sfx for losers
        var losers = playerManager.GetAllMembersOfTeam(losingTeam);
        if (losingTeam == -1) { // all enemies of winning team are losers
            losers = playerManager.GetAllEnemyMembers(winningTeam);
        }

        clientRpcParams = new ClientRpcParams {
            Send = new ClientRpcSendParams {
                TargetClientIds = CreateMemberList(losers)
            }
        };
        PlayConqueredSFXClientRpc(false, what, clientRpcParams);
    }

    [ClientRpc]
    public void AnnounceEventClientRpc(FixedString512Bytes message, float duration = 45f, ClientRpcParams clientRpcParams = default)
    {
        AnnounceEvent(message.Value, duration);
    }
}
