using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;

namespace Network
{
    public class LobbyManager : MonoBehaviour { 
        //internal Lobby connectedLobby;
        private QueryResponse lobbies;
        private UnityTransport transport;
        internal const string JoinCodeKey = "j";
        internal const string ActiveProjectSrcKey = "p";
        internal const string ActiveProjectNameKey = "n";
        internal const string ActiveSceneKey = "s";
        private string playerId;

        private NetworkController networkController;

        const int maxPlayers = 20;
        
        private void Awake()
        {
            transport = FindObjectOfType<UnityTransport>();
            if (!transport)
            {
                Debug.Log("No unity transport found in scene!");
            }
            networkController = FindObjectOfType<NetworkController>();
            if (!networkController)
            {
                Debug.Log("No NetworkController found in scene!");
            }
        }
        
        public async void HostMultiplayerSession()
        {
            await Authenticate();
            networkController.connectedLobby = await CreateLobby();
            networkController.OnHosted(networkController.connectedLobby);
        }

        public async void JoinMultiplayerSession(Lobby lobby)
        {
            await Authenticate();
            networkController.connectedLobby = lobby;
            if (networkController.connectedLobby != null)
            {
                await JoinLobby(lobby);
                networkController.PrepareAfterJoin(networkController.connectedLobby);
            }
            else
            {
                Debug.Log("No lobby found!");
            }
        }

        public async void LookForSessions(SessionList sessionList)
        {
            await Authenticate();
            var lobby = await Lobbies.Instance.QueryLobbiesAsync();
            sessionList.ReloadSessions(lobby.Results);
        }

        internal async Task<Lobby> QuickFindSession()
        {
            try {
                await Authenticate();
                var lobby = await Lobbies.Instance.QuickJoinLobbyAsync();
                return lobby;
            }
            catch (Exception e) {
                Debug.Log($"No lobbies available via quick join");
                return null;
            }
            
        }

        private async Task Authenticate() {
            var options = new InitializationOptions();
            await UnityServices.InitializeAsync(options);
            if (!AuthenticationService.Instance.IsAuthorized)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                playerId = AuthenticationService.Instance.PlayerId;
            }
        }

        internal async Task UpdateActiveSceneInLobby()
        {
            try
            {
                UpdateLobbyOptions options = new UpdateLobbyOptions();
                options.Data = new Dictionary<string, DataObject>()
                {
                    {
                        "ExamplePrivateData", new DataObject(
                            visibility: DataObject.VisibilityOptions.Private,
                            value: "PrivateData")
                    },
                    {
                        "ExamplePublicData", new DataObject(
                            visibility: DataObject.VisibilityOptions.Public,
                            value: "PublicData",
                            index: DataObject.IndexOptions.S1)
                    },
                };
                
            }
            catch (LobbyServiceException e)
            {
                Debug.Log(e);
            }
        }
        
        private async Task JoinLobby(Lobby lobby) {
            try {
                //grab the relay allocation details
                var a = await RelayService.Instance.JoinAllocationAsync(lobby.Data[JoinCodeKey].Value);

                // Set the details to the transform
                SetTransformAsClient(a);

                // Join the game room as a client
                NetworkManager.Singleton.StartClient();
                /*PlayerSpawner playerSpawner = FindObjectOfType<PlayerSpawner>();
                playerSpawner.SpawnOwnPlayer();*/
            }
            catch (Exception e) {
                Debug.Log($"No lobbies available via quick join");
            }
        }

        private async Task<Lobby> CreateLobby() {
            try {
                // Create a relay allocation and generate a join code to share with the lobby
                var a = await RelayService.Instance.CreateAllocationAsync(maxPlayers);
                var joinCode = await RelayService.Instance.GetJoinCodeAsync(a.AllocationId);

                // Create a lobby, adding the relay join code to the lobby data
                var options = new CreateLobbyOptions {
                    Data = new Dictionary<string, DataObject>
                    {
                        { JoinCodeKey, new DataObject(DataObject.VisibilityOptions.Public, joinCode) },
                        { ActiveSceneKey, new DataObject(DataObject.VisibilityOptions.Public, "") },
                        { ActiveProjectSrcKey, new DataObject(DataObject.VisibilityOptions.Public, networkController.activeHostedProjectSrc) },
                        { ActiveProjectNameKey, new DataObject(DataObject.VisibilityOptions.Public, networkController.activeHostedProjectName) }
                    }
                };
                var lobby = await Lobbies.Instance.CreateLobbyAsync(networkController.activeHostUsername + "(" + networkController.activeHostedProjectSrc + ")", maxPlayers, options);

                // Send a heartbeat every 15 seconds to keep the room alive
                StartCoroutine(HeartbeatLobbyCoroutine(lobby.Id, 15));

                // Set the game room to use the relay allocation
                transport.SetHostRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData);

                Debug.Log("INVITE CODE: " + await RelayService.Instance.GetJoinCodeAsync(a.AllocationId));
                // Start the room. I'm doing this immediately, but maybe you want to wait for the lobby to fill up
                NetworkManager.Singleton.StartHost();
                /*PlayerSpawner playerSpawner = FindObjectOfType<PlayerSpawner>();
                playerSpawner.SpawnOwnPlayer();*/
                return lobby;
            }
            catch (Exception e) {
                Debug.LogFormat("Failed creating a lobby");
                return null;
            }
        }

        private void SetTransformAsClient(JoinAllocation a) {
            transport.SetClientRelayData(a.RelayServer.IpV4, (ushort)a.RelayServer.Port, a.AllocationIdBytes, a.Key, a.ConnectionData, a.HostConnectionData);
        }

        private static IEnumerator HeartbeatLobbyCoroutine(string lobbyId, float waitTimeSeconds) {
            var delay = new WaitForSecondsRealtime(waitTimeSeconds);
            while (true) {
                Lobbies.Instance.SendHeartbeatPingAsync(lobbyId);
                yield return delay;
            }
        }

        private void OnDestroy() {
            try {
                StopAllCoroutines();
                // todo: Add a check to see if you're host
                if (networkController.connectedLobby != null) {
                    if (networkController.connectedLobby.HostId == playerId) Lobbies.Instance.DeleteLobbyAsync(networkController.connectedLobby.Id);
                    else Lobbies.Instance.RemovePlayerAsync(networkController.connectedLobby.Id, playerId);
                }
            }
            catch (Exception e) {
                Debug.Log($"Error shutting down lobby: {e}");
            }
        }
    }

}