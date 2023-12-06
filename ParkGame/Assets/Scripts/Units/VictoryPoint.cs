using Managers;
using NavMeshPlus.Extensions.Units;
using Unity.Netcode;
using UnityEngine;

public class VictoryPoint : NetworkBehaviour, IConquerable
{
    [Tooltip("Seconds since spawn to first opening of this Victory Point.")]
    [SerializeField] float openingTime;
    [SerializeField] float timeToNotifyPlayersBeforeOpening;
    [SerializeField] ColorSettings colorSettings;
    
    private float lastConquestTime;
    private bool isOpen;
    private bool isNotified;
        
    private SpriteRenderer spriteRenderer;
    private GameObject conquerModuleObject;
    private PlayerManager playerManager;
    private Announcer announcer;

    [ClientRpc]    
    void CloseVPClientRpc() {
        spriteRenderer.color = new Color(0.8f, 0.8f, 0.8f, 0.2f);
        isOpen = false;
        conquerModuleObject.SetActive(false);
        isNotified = false;
    }

    [ClientRpc]
    void OpenVPClientRpc() {
        spriteRenderer.color = new Color(1, 1, 1, 1);
        isOpen = true;
        conquerModuleObject.SetActive(true);
    }
    
    private void Awake()
    {
        conquerModuleObject = GetComponentInChildren<ConquerModule>().gameObject;
        spriteRenderer = GetComponent<SpriteRenderer>();
        lastConquestTime = float.MaxValue;
        announcer = FindObjectOfType<Announcer>();
        playerManager = FindObjectOfType<PlayerManager>();
        playerManager.OnAllPlayersReady += onAllPlayersReady;
    }

    private void onAllPlayersReady()
    {
        lastConquestTime = Time.time;
    }

    private void Start() {
        if (IsServer)
        {
            CloseVPClientRpc();   
        }
    }

    void Update()
    {
        if(!NetworkManager.Singleton.IsServer || lastConquestTime == float.MaxValue) return;        
        
        float timeToOpen = lastConquestTime + openingTime - Time.time;
        
        if(timeToOpen < timeToNotifyPlayersBeforeOpening && !isNotified) {
            announcer.AnnounceEventClientRpc($"Victory Point will open soon! ({timeToNotifyPlayersBeforeOpening}s)", 5);
            isNotified = true;
        }
        
        if (timeToOpen < 0 && !isOpen) { 
            OpenVPClientRpc();
            announcer.AnnounceEventClientRpc("Victory Point has opened!", 5);
        }
    }

    public void OnStartedConquering(int team)
    {
        NamedColor c = colorSettings.Colors[team];
        c.Color.a = 0.8f;
        spriteRenderer.color = c.Color;
        
        if (IsServer)
        {
            announcer.AnnounceEventClientRpc($"Victory Point is being captured by team {c.Name}!", 5);
        }
    }

    public void OnConquered(int team)
    {
        lastConquestTime = Time.time;

        if (IsServer)
        {
            CloseVPClientRpc();        
            NamedColor c = colorSettings.Colors[team];
            announcer.AnnounceEventClientRpc($"Victory Point has been captured by team {c.Name}!", 5);
        }
    }

    public int GetTeam()
    {
        return -1;
    }
}
