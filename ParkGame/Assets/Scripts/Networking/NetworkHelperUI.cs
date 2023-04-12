using Unity.Netcode;
using UnityEngine;

namespace Networking
{
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
