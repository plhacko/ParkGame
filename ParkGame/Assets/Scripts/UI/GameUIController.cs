using Networking;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameUIController : NetworkBehaviour
{
    [SerializeField] private string joinGameSceneName;
    [SerializeField] private Button backButton;
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        initialize();
    }

    private void initialize()
    {
        backButton.onClick.AddListener(goBack);
    }

    private void goBack()
    {
        OurNetworkManager.Singleton.Shutdown();
        SceneManager.LoadScene(joinGameSceneName, LoadSceneMode.Single);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
