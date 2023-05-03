using Networking;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class GameUI : NetworkBehaviour
{
    [SerializeField] private TextMeshProUGUI myName;
    [SerializeField] private TextMeshProUGUI otherName;
    
    private MatchData matchData;
    
    private void initialize()
    {
        matchData = FindObjectOfType<MatchData>();

        if (IsHost)
        {
            myName.text = matchData.HostName;
            otherName.text = matchData.ClientName;
        }
        else
        {
            myName.text = matchData.ClientName;
            otherName.text = matchData.HostName;
        }
    }
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        initialize();
    }
}
