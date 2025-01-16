using System;
using Unity.Netcode;
using UnityEngine;

namespace Network
{
    public class NetworkStartup : MonoBehaviour
    {
        private SimpleRelay relay;
        private LobbyManager lobbyManager;
        private NetworkController networkController;
        
        private void Awake()
        {
            relay = FindObjectOfType<SimpleRelay>();
            lobbyManager = FindObjectOfType<LobbyManager>();
            networkController = FindObjectOfType<NetworkController>();
        }
        private void Start()
        {
            SceneTransitionHandler sth = NetworkManager.Singleton.gameObject.GetComponent<SceneTransitionHandler>();
            if (sth.InitializeAsHost)
            {
                if (sth.InitAsMutliplayer)
                {
                    lobbyManager.HostMultiplayerSession();
                }
                else
                {
                    relay.StartSingleplayerSession();
                }
            }
            else
            {
                lobbyManager.JoinMultiplayerSession(networkController.connectedLobby);
            }
        }
    }
}