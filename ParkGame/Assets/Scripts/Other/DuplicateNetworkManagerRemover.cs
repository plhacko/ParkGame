using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DuplicateNetworkManagerRemover : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(RemoveCoroutine());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator RemoveCoroutine()
    {
        while(NetworkManager.Singleton == null)
            yield return null;
 
        if (GetComponent<NetworkManager>() != NetworkManager.Singleton)
        {
            Debug.Log("Destroying this clone " + gameObject.name);
            Destroy(gameObject);
        }
    }
}
