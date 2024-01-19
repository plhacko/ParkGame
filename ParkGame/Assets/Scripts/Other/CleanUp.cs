using Managers;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CleanUp : MonoBehaviour
{
    void Start()
    {
        NetworkManager.Singleton.Shutdown();
        Destroy(LobbyManager.Singleton.gameObject);
        Destroy(NetworkManager.Singleton.gameObject);
        SceneManager.LoadScene("Menu");
    }
}
