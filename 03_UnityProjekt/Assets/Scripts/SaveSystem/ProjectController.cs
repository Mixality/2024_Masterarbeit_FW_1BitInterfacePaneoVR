using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DefaultNamespace;
using Network;
using Unity.Netcode;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI.WebService;
using Utilities;

namespace UnityEngine.UI.SaveSystem
{
    public class ProjectController : MonoBehaviour
    {
        internal static string SCENE_LIST_FILE = "sceneList.txt";

        internal ProjectData activeProject { get; private set; }

        internal SceneData activeScene;
        
        internal string activeProjectPath;

        internal IngameUIController ingameUIController;

        internal SaveController saveController;

        private EditModeController editModeController;
        
        internal ProjectLoader projectLoader;

        private ImageBuffer ImageBuffer;

        internal bool isProjectRunning;

        internal NetworkController networkController;
        
        public UnityEvent<string> onProjectLoaded = new UnityEvent<string>();

        internal WebSocketClient webSocketClient;
        
        private void Awake()
        {
            projectLoader = gameObject.AddComponent<ProjectLoader>();
        }

        private void Start()
        {
            saveController = FindObjectOfType<SaveController>();
            if (!saveController)
            {
                Debug.LogError("Error in ProjectController: No save controller found in scene");
            }
            editModeController = FindObjectOfType<EditModeController>();
            if (!editModeController)
            {
                Debug.LogError("Error in ProjectController: No editModeController found in scene");
            }
            saveController = FindObjectOfType<SaveController>();
            if (!saveController)
            {
                Debug.LogError("Error in ProjectController: No save controller found in scene");
            }
            ImageBuffer = FindObjectOfType<ImageBuffer>();
            if (!ImageBuffer)
            {
                Debug.LogError("Error in ProjectLoader: No imageBuffer found in scene");
            }
            webSocketClient = FindObjectOfType<WebSocketClient>();
            if (!webSocketClient)
            {
                Debug.LogError("Error in ProjectLoader: No webSocketClient found in scene");
            }
        }

        public void Initialize(XROrigin xrRig, NetworkController controller)
        {
            networkController = controller;
            ingameUIController = xrRig.GetComponent<IngameUIController>();
            saveController.ingameUIController = ingameUIController;

            FindObjectOfType<VideoController>().ingameUIController = ingameUIController;
            GameObject[] EditModeCanvasObjects = GameObject.FindGameObjectsWithTag("EditModeUI");
            editModeController.ObservedCanvas = new List<Canvas>();
            foreach (var gameObject in EditModeCanvasObjects)
            {
                editModeController.ObservedCanvas.Add(gameObject.GetComponent<Canvas>());
            }
            
            SceneLoader SceneLoader = FindObjectOfType<SceneLoader>();
            SceneScanner SceneScanner = FindObjectOfType<SceneScanner>();
            SceneLoader.ingameUIController = ingameUIController;
            SceneScanner.ingameUIController = ingameUIController;
            SceneLoader.XRRig = xrRig.transform;
            SceneScanner.XRRig = xrRig.transform;
            InteractionSphere360 interactionSphere = FindObjectOfType<InactivityRefHandler>().interactionSphere360;
            Camera camera = xrRig.GetComponentInChildren<Camera>();
            SceneLoader.mainCam = camera;
            interactionSphere.playerCamera = xrRig.GetComponentInChildren<Camera>();
            interactionSphere.GetComponent<UICameraParentor>().playerCamera = camera;
            openProject(networkController.activeHostedProjectSrc);
        }
        
        /// <summary>
        /// loads project data with given src id and opens first scene
        /// </summary>
        /// <param name="src">src id of project to open</param>
        internal void openProject(string src, string atScene = null)
        {
            StartCoroutine(loadAndOpenProject(src, atScene));
        }

        private IEnumerator loadAndOpenProject(string src, string atScene = null)
        {
            activeProject = projectLoader.loadProjectData(src);
            ImageBuffer.Initialize(src);
            
            yield return new WaitUntil(() => !ImageBuffer.isLoadingBuffer);

            activeProjectPath = ProjectLoader.PROJECTS_DIR_PATH + "/" + src;
            if (activeProject.firstSceneName != null && !activeProject.firstSceneName.Equals(""))
            {
                if (atScene != null)
                {
                    if (!saveController.loadScene(atScene))
                    {
                        openProject(activeProject.projectSrc);
                        yield break;
                    }
                }
                else
                {
                    if (!saveController.loadScene(activeProject.firstSceneName))
                    {
                        saveController.loadScene();
                    }
                }
            }
            else
            {
                saveController.loadScene();
            }
            
            isProjectRunning = true;

            if (networkController.authoringMode)
            {
                ingameUIController.toggleSceneEditUI(true);
                webSocketClient.InitEditProjectConnection(this, editModeController);
            }
            else
            {
                ingameUIController.toggleSceneEditUI(false);
                editModeController.toggleEditUI(false);
            }

            while (saveController.isLoadingScene)
            {
                yield return new WaitForSeconds(1);
            }

            onProjectLoaded.Invoke(src);
            yield return null;
        }

        public void changeInitialSceneToCurrent(bool setToCurrent)
        {
            if (setToCurrent)
            {
                activeProject.firstSceneName = activeScene.sceneName;
                projectLoader.updateProjectConfig(activeProject);
            }
            else
            {
                activeProject.firstSceneName = "";
                projectLoader.updateProjectConfig(activeProject);
            }
        }

        public void setNewSaveTimeStamp()
        {
            activeProject.lastLocalSave = TimeUtils.GetUnixTimestamp();
            projectLoader.updateProjectConfig(activeProject);
        }

        internal MediaRefTable getActiveProjectMediaRefs()
        {
            return JsonUtility.FromJson<MediaRefTable>(PlayerPrefs.GetString(activeProject.projectSrc));
        }
    }
}