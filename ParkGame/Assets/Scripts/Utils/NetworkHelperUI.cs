using Unity.Netcode;
using UnityEngine;

namespace Utils
{
    /*
     * This class is a helper to create a game session for debugging without going through the menus.
     * It actually doesn't work well anymore because it should initialize the SessionManager with some debug player data
     */
    public class NetworkHelperUI : MonoBehaviour
    {
        public void StartServer()
        {
            NetworkManager.Singleton.StartServer();
        }
        
        public void StartHost()
        {
            NetworkManager.Singleton.StartHost();
        }
        
        public void StartClient()
        {
            NetworkManager.Singleton.StartClient();
        }
    }
}
