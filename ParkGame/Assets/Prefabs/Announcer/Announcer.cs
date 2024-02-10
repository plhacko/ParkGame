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

    public void AnnounceEvent(string message, Color color, float duration = 45f)
    {
        if (colorTween != null && !colorTween.IsComplete())
        {
            colorTween.Complete();
        }
        
        text.text = message;
        text.color = color;
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
    public void PlayNotificationClientRpc(string sfxName, ClientRpcParams clientRpcParams = default) {
        AudioManager.Instance.PlayNotificationSFX(sfxName);
    }
    public ulong[] CreateMemberList(List<PlayerController> members) {
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


        List<PlayerController> losers;
        if (losingTeam == -1) { // no one or everyone was loser. except for the winners
            if (what == Wonable.Outpost) {
                return; // no one is the loser. no one was conquered. the outpost just stood there. lonely...
            }
            losers = playerManager.GetAllEnemyMembers(winningTeam); // didn't win VP nor the game
        } else {
            losers = playerManager.GetAllMembersOfTeam(losingTeam);
        }

        clientRpcParams = new ClientRpcParams {
            Send = new ClientRpcSendParams {
                TargetClientIds = CreateMemberList(losers)
            }
        };
        PlayConqueredSFXClientRpc(false, what, clientRpcParams);
    }

    [ClientRpc]
    public void AnnounceEventClientRpc(FixedString512Bytes message, Color color, float duration = 45f, ClientRpcParams clientRpcParams = default)
    {
        AnnounceEvent(message.Value, color, duration);
    }
}
