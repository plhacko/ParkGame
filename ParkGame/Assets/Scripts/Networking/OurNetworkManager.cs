using Unity.Netcode;

namespace Networking
{
    public class OurNetworkManager : NetworkManager
    {
        public new static OurNetworkManager Singleton => (OurNetworkManager)NetworkManager.Singleton;

        public string RoomCode;

    }
}