using System;
using System.Collections.Generic;
using Managers;
using Unity.Netcode;
using UnityEngine;

public class VictoryPoint : NetworkBehaviour, IConquerable
{
    [Tooltip("Seconds since spawn to first opening of this Victory Point.")]
    [SerializeField] float openingTimeMin;
    [SerializeField] float openingTimeMax;
    [SerializeField] float timeToNotifyPlayersBeforeOpening;
    [SerializeField] ColorSettings colorSettings;
    
    public Action<int, int> OnPointConquered;
    
    private float lastConquestTime;
    private bool isOpen;
    private bool isNotified;
    private float openingTime;

    private NetworkList<int> teamScores;
    
    private SpriteRenderer spriteRenderer;
    private ConquerModule conquerModuleObject;
    private PlayerManager playerManager;
    private GameSessionManager gameSessionManager;
    private Announcer announcer;

    [ClientRpc]    
    void CloseVPClientRpc() {
        spriteRenderer.color = new Color(0.8f, 0.8f, 0.8f, 0.2f);
        isOpen = false;
        conquerModuleObject.gameObject.SetActive(false);
        isNotified = false;
    }

    [ClientRpc]
    void OpenVPClientRpc() {
        spriteRenderer.color = new Color(1, 1, 1, 1);
        isOpen = true;
        conquerModuleObject.gameObject.SetActive(true);
    }

    private void Awake()
    {
        teamScores = new NetworkList<int>();
        conquerModuleObject = GetComponentInChildren<ConquerModule>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        lastConquestTime = float.MaxValue;
        playerManager = FindObjectOfType<PlayerManager>();
        announcer = FindObjectOfType<Announcer>();
        playerManager.OnAllPlayersReady += onAllPlayersReady;
        openingTime = UnityEngine.Random.Range(openingTimeMin, openingTimeMax);
    }

    private void onAllPlayersReady()
    {
        lastConquestTime = Time.time;
    }

    private void Start()
    {
        if (IsServer) {
            teamScores.Add(0);
            teamScores.Add(0);
            teamScores.Add(0);
            teamScores.Add(0);
        }
        if (IsServer)
        {
            CloseVPClientRpc();
        }
        
        gameSessionManager = FindObjectOfType<GameSessionManager>();
    }

    void Update()
    {
        if (NetworkManager == null || !NetworkManager.Singleton.IsServer || lastConquestTime == float.MaxValue)
        {
            return;
        }

        if(gameSessionManager.IsOver) return;
        
        float timeToOpen = lastConquestTime + openingTime - Time.time;

        if (timeToOpen < timeToNotifyPlayersBeforeOpening && !isNotified)
        {
            announcer.AnnounceEventClientRpc($"Victory Point will open soon! ({timeToNotifyPlayersBeforeOpening} s)", Color.white, 5);
            //announcer.PlayNotificationClientRpc("Notification");
            isNotified = true;
        }

        if (timeToOpen < 0 && !isOpen)
        {
            openingTime = UnityEngine.Random.Range(openingTimeMin, openingTimeMax);

            OpenVPClientRpc();
            announcer.AnnounceEventClientRpc("Victory Point has opened!", Color.cyan, 5);
            announcer.PlayNotificationClientRpc("Notification");
        }
    }

    public void OnStartedConquering(int team)
    {
        //onStartedConqueringClientRpc(team);
        NamedColor c = colorSettings.Colors[team];
        announcer.AnnounceEventClientRpc($"Victory Point is being captured by team {c.Name}!", Color.white, 5);
        announcer.PlayNotificationClientRpc("Foreboding");
    }

    [ClientRpc]
    private void onStartedConqueringClientRpc(int team, ClientRpcParams clientRpcParams = default)
    {
        var c = colorSettings.Colors[team];
        c.Color.a = 0.8f;
        spriteRenderer.color = c.Color;
    }

    public void OnConquered(int team)
    {
        lastConquestTime = Time.time;
        CloseVPClientRpc();
        
        int numPoints = teamScores[team] + 1;
        teamScores[team] = numPoints;
        
        OnPointConquered?.Invoke(team, numPoints);
    }
    
    public void OnStoppedConquering(int team)
    {
        onStoppedConqueringClientRpc(team);
    }
    
    [ClientRpc]
    private void onStoppedConqueringClientRpc(int team, ClientRpcParams clientRpcParams = default)
    {
        if (isOpen)
        {
            spriteRenderer.color = Color.white;   
        }
    }

    public (int, int)[] GetTeamScores()
    {
        var scores = new (int, int)[4];
        for (int i = 0; i < 4; i++)
        {
            scores[i] = (i, this.teamScores[i]);
        }

        return scores;
    }
    
    public int GetTeam()
    {
        return -1;
    }

    public void TryRemoveUnit(ITeamMember tm)
    {
        conquerModuleObject.TryRemoveUnit(tm);
    }
}
