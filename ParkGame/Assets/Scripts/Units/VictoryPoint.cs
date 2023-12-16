using Managers;
using Unity.Netcode;
using UnityEngine;

public class VictoryPoint : NetworkBehaviour
{

    //public float timeSinceSpawn;
    // seconds after spawn till opening, and also time since the last conquest till opening
    [Tooltip("Seconds since spawn to first opening of this Victory Point.")]
    [SerializeField] float openingTime;  // and delta since conquering
    [SerializeField] float timeToNotifyPlayersBeforeOpening;  // and delta since conquering

    private float lastConquestTime; // and now also since spawning
    private bool isOpen;
    private bool isNotified;

    private SpriteRenderer spriteRendrr;
    private GameObject conquerModuleObject;
    private PlayerManager playerManager;
    private Announcer announcer;

    // maybe: add some random range for the delta time
    // maybe?: add some time after the conquest

    [ClientRpc]
    void CloseVPClientRpc()
    {
        spriteRendrr.color = new Color(0.8f, 0.8f, 0.8f, 0.2f);
        isOpen = false;
        conquerModuleObject.SetActive(false);
        isNotified = false;
    }

    [ClientRpc]
    void OpenVPClientRpc()
    {
        spriteRendrr.color = new Color(1, 1, 1, 1);
        isOpen = true;
        conquerModuleObject.SetActive(true);
    }

    private void Awake()
    {
        conquerModuleObject = GetComponentInChildren<ConquerModule>().gameObject;
        spriteRendrr = GetComponent<SpriteRenderer>();
        lastConquestTime = float.MaxValue;
        announcer = FindObjectOfType<Announcer>();
        playerManager = FindObjectOfType<PlayerManager>();
        playerManager.OnAllPlayersReady += onAllPlayersReady;
    }

    private void onAllPlayersReady()
    {
        lastConquestTime = Time.time;
    }

    private void Start()
    {
        if (IsServer)
        {
            CloseVPClientRpc();
        }
    }

    public void ConquerThisVP()
    {
        Debug.Log("CONQUER THIS vp");
        lastConquestTime = Time.time;

        if (IsServer)
        {
            CloseVPClientRpc();
        }
    }

    void Update()
    {
        if (NetworkManager == null || !NetworkManager.Singleton.IsServer || lastConquestTime == float.MaxValue)
        {
            return;
        }

        float timeToOpen = lastConquestTime + openingTime - Time.time;

        if (timeToOpen < timeToNotifyPlayersBeforeOpening && !isNotified)
        {
            announcer.AnnounceEventClientRpc($"Victory Point will open soon! ({timeToNotifyPlayersBeforeOpening}s)", 5);
            isNotified = true;
        }

        if (timeToOpen < 0 && !isOpen)
        {
            OpenVPClientRpc();
        }
    }
}
