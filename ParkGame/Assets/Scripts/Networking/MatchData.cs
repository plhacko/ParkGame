using Unity.Netcode;

namespace Networking
{
    public class MatchData : NetworkBehaviour
    {
        public string HostName;
        public string ClientName;
        
        public ulong ClientId;
        public ulong HostId;

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();
            initialize();
        }
        
        private void initialize()
        {
            DontDestroyOnLoad(this.gameObject);
        }
        
        [ClientRpc]
        private void UpdateHostNameClientRPC(string name)
        {
            HostName = name;
        }
        
        [ServerRpc(RequireOwnership = false)]
        private void UpdateClientNameServerRPC(string name)
        {
            ClientName = name;
        }

        public void SetName(string name)
        {
            if (IsHost)
            {
                HostName = name;
                UpdateHostNameClientRPC(name);
            }
            else
            {
                ClientName = name;
                UpdateClientNameServerRPC(name);
            }
        }
    }
}