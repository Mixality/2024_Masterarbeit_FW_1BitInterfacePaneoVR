using System;
using System.Collections;
using DefaultNamespace;
using Unity.Netcode;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.UI.SaveSystem;
using UnityEngine.XR.Interaction.Toolkit;

namespace Network
{
    public class NetworkController : MonoBehaviour
    {
        internal SceneTransitionHandler SceneTransitionHandler;

        internal string activeHostedProjectSrc;

        internal string activeHostedProjectName;
        
        internal string activeHostUsername;

        internal bool authoringMode;

        internal bool highQualityStreaming;
        
        public bool multiplayerOn;

        internal Lobby connectedLobby;
        
        internal ProjectController ProjectController { get; private set; }

        //null as long we just started the app. Then always has the name of the last active unity scene before the current;
        internal string LastUnityScene = null;
        
        private void Awake()
        {
            SceneTransitionHandler = FindObjectOfType<SceneTransitionHandler>();
            if (!SceneTransitionHandler)
            {
                Debug.LogError("Error in Project List: Couldn't find SceneTransitionHandler in Scene");
            }
        }
        
        internal void HostProject(string projectSrc, string projectName, string userName)
        {
            authoringMode = false;
            activeHostedProjectSrc = projectSrc;
            activeHostedProjectName = projectName;
            activeHostUsername = userName;
            SceneTransitionHandler.StartScenarioAsHost(projectSrc, projectName, multiplayerOn);
        }

        internal void OnHosted(Lobby connectedLobby)
        {
            InitializeProject();
        }

        internal void OnHostedLocally()
        {
            InitializeProject();
        }

        internal void JoinProject(Lobby lobby)
        {
            connectedLobby = lobby;
            activeHostedProjectSrc = lobby.Data[LobbyManager.ActiveProjectSrcKey].Value;
            activeHostedProjectName = lobby.Data[LobbyManager.ActiveProjectNameKey].Value;
            //activeHostUsername = lobby.Data[LobbyManager.ActiveProjectNameKey];
            authoringMode = false;
            SceneTransitionHandler.StartScenarioAsClient(activeHostedProjectSrc, activeHostedProjectName); //TODO: manual download required so far
        }

        private void InitializeProject()
        {
            ProjectController = FindObjectOfType<ProjectController>();
#if UNITY_WEBGL || UNITY_STANDALONE_WIN_DESKTOP
            ProjectController.Initialize(GameObject.FindWithTag("MainXRRig").GetComponent<XROrigin>(), this);
#else
            NetworkObject player = GetComponent<NetworkManager>().LocalClient.PlayerObject;
            ProjectController.Initialize(player.GetComponent<XROrigin>(), this);
#endif
            ProjectController.onProjectLoaded.AddListener(OnHostedAndPrepared);
        }

        private void OnHostedAndPrepared(string projectName)
        {
            FinishSceneTransition();
        }

        internal void FinishSceneTransition()
        {
            SceneTransitionHandler.FinishSceneTransition();
        }
        
        internal void PrepareAfterJoin(Lobby connectedLobby)
        {
            ProjectController = FindObjectOfType<ProjectController>();
            
            StartCoroutine(OnJoinedPrepared(connectedLobby));
        }

        IEnumerator OnJoinedPrepared(Lobby connectedLobby)
        {
            while (NetworkManager.Singleton.LocalClient == null)
            {
                yield return null;
            }
            while (NetworkManager.Singleton.LocalClient.PlayerObject == null)
            {
                yield return null;
            }

            /*activeHostedProjectName = connectedLobby.Data["p"].Value;
            Debug.Log(activeHostedProjectName);
            ProjectController.Initialize(NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<XROrigin>(), this);*/
            InitializeProject();
            yield return null;
        }

        internal void EditProject(string projectSrc, string projectName)
        {
            activeHostedProjectSrc = projectSrc;
            activeHostedProjectName = projectName;
            activeHostUsername = "local_editor";
            authoringMode = true;
            SceneTransitionHandler.StartScenarioAsHost(projectSrc,  projectName, false);
        }

        public void LeaveSession()
        {
#if UNITY_WEBGL || UNITY_STANDALONE_WIN_DESKTOP
                SceneTransitionHandler.StopScenarioAsHost();
#else 
            if (NetworkManager.Singleton.IsHost)
            {
                SceneTransitionHandler.StopScenarioAsHost();
            }
#endif
            
            
        }
    }
}