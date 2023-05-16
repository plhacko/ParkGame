using Unity.Netcode;

namespace Networking
{
    public class OurNetworkManager : NetworkManager
    {
        public new static OurNetworkManager Singleton => (OurNetworkManager)NetworkManager.Singleton;

        public string RoomCode;

        private void Awake()
        {
            OnClientConnectedCallback += OnClientConnected;
            OnClientDisconnectCallback += OnClientDisconnected;
        }

        private void OnClientDisconnected(ulong obj)
        {
            // throw new System.NotImplementedException();
        }

        private void OnClientConnected(ulong obj)
        {
            // throw new System.NotImplementedException();
        }
    }
}