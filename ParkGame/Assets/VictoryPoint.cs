using UnityEngine;
using Unity.Netcode;

public class VictoryPoint : NetworkBehaviour
{

    //public float timeSinceSpawn;
    // seconds after spawn till opening, and also time since the last conquest till opening
    [Tooltip("Seconds since spawn to first opening of this Victory Point.")]
    [SerializeField] float firstOpeningTime;  // and delta since conquering
    public float timeSinceLastConquest; // and now also since spawning

    //[Tooltip("Time in seconds between opening the VP.")]
    //[SerializeField] float deltaTime;
    //[Tooltip("How many times will the VP open during the game.")]
    //[SerializeField] int numberOfOpenings;

    public bool isOpen;

    private SpriteRenderer spriteRendrr;
    private GameObject conquerModuleObject;

    // maybe: add some random range for the delta time
    // maybe?: add some time after the conquest
    
    void CloseVP() {
        spriteRendrr.color = new Color(0.8f, 0.8f, 0.8f, 0.2f);
        isOpen = false;
        conquerModuleObject.SetActive(false);
    }

    void OpenVP() {
        spriteRendrr.color = new Color(1, 1, 1, 1);
        isOpen = true;
        conquerModuleObject.SetActive(true);
    }

    private void Start() {
        conquerModuleObject = GetComponentInChildren<ConquerModule>().gameObject;
        spriteRendrr = GetComponent<SpriteRenderer>();
        CloseVP();
    }

    public void ConquerThisVP() {
        Debug.Log("CONQUER THIS vp");
        timeSinceLastConquest = 0;
        CloseVP();
    }

    void Update()
    {
        timeSinceLastConquest += Time.deltaTime;
        // first opening
        if (timeSinceLastConquest >= firstOpeningTime) { 
            OpenVP();
        }
    }
}
