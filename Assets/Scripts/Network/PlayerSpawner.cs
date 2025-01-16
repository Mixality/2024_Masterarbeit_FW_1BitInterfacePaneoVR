using Unity.Netcode;
using UnityEngine;

namespace Network
{
    public class PlayerSpawner : NetworkBehaviour {
        [SerializeField] private GameObject playerPrefab;

        public override void OnNetworkSpawn()
        {
            SpawnPlayerServerRpc(NetworkManager.Singleton.LocalClientId, 0);
        }

        [ServerRpc(RequireOwnership=false)] //server owns this object but client can request a spawn
        private void SpawnPlayerServerRpc(ulong clientId,int prefabId) {
            GameObject newPlayer;
            newPlayer=Instantiate(playerPrefab);
            NetworkObject netObj=newPlayer.GetComponent<NetworkObject>();
            newPlayer.SetActive(true);
            netObj.SpawnAsPlayerObject(clientId,true);
        }
    }

}