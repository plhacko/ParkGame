using Networking;
using Unity.Netcode;
using UnityEngine;
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
        backButton.onClick.AddListener(() =>
            SessionManager.Singleton.EndSessionAndGoToScene(joinGameSceneName));
    }
}
