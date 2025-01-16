using System;
using System.Collections;
using Network;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.UI.SaveSystem;
using Utilities;

namespace DefaultNamespace
{
    public class ProjectManager : MonoBehaviour
    {
        private ProjectUpdater remoteUpdater;

        private ProjectLoader projectLoader;

        internal static string REMOTE_PROJECT_LIST_FILE = "remoteProjectList.json";

        internal UIController uiController;

        private NetworkController networkController;

        private WebSocketClient webSocketClient;
        
        /// <summary>
        /// ProjectList component needs to wait until the projects folder exists, before loading
        /// </summary>
        internal UnityEvent OnProjectsChanged = new UnityEvent();

        private void Awake()
        {
            uiController = FindObjectOfType<UIController>();
            networkController = FindObjectOfType<NetworkController>();
            remoteUpdater = gameObject.AddComponent<ProjectUpdater>();
            projectLoader = gameObject.AddComponent<ProjectLoader>();
            webSocketClient = FindObjectOfType<WebSocketClient>();
            webSocketClient.InitMainMenuConnection(this);
        }

        private void Start()
        {
            UserChanged();
            remoteUpdater.OnDownloadFinished.AddListener(OnProjectsChanged.Invoke);
        }

        internal void deleteProject(string src)
        {
            projectLoader.deleteProject(src, true);
            OnProjectsChanged.Invoke();
        }

        public void UserChanged()
        {
            projectLoader.InitializeDirectories();
            OnProjectsChanged.Invoke();
        }
        
        internal void uploadProject(string projectName)
        {
            remoteUpdater.remoteUploadProject(projectName);
        }

        internal IEnumerator updateRemoteProjectList()
        {
            if (PlayerPrefs.GetInt("loggedIn") != 0)
            {
                yield return remoteUpdater.GetRemoteProjectList();
                yield return true;
            }
            else
            {
                Debug.Log("not logged in, using local user");
                yield return false;
            }
        }

        public void pullRemoteProject(string projectSrc, string projectName)
        {
            if (PlayerPrefs.GetInt("loggedIn") != 0)
            {
                remoteUpdater.pullRemoteProject(projectSrc, projectName);
            }
            else
            {
                Debug.Log("not logged in to download project!");
            }
        }
        
        /// <summary>
        /// call that only right before uploading for a correct timestamp on server
        /// </summary>
        public bool setLastSaveToCurrentTime(string projectSrc)
        {
            ProjectData projectData = projectLoader.loadProjectData(projectSrc);
            projectData.lastRemoteSave = TimeUtils.GetUnixTimestamp();
            //TODO: May cause issues due to async file saving?
            projectLoader.updateProjectConfig(projectData);
            return true;
        }
        
        /// <summary>
        /// call that right after video download to set the current project video quality (on clients with download function)
        /// </summary>
        internal bool setProjectVideoQuality(string projectSrc)
        {
            ProjectData projectData = projectLoader.loadProjectData(projectSrc);
            projectData.currentVideoQuality = PlayerPrefs.GetString("videoQuality");
            //TODO: May cause issues due to async file saving?
            projectLoader.updateProjectConfig(projectData);
            return true;
        }
    }
}