using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DefaultNamespace;
using Newtonsoft.Json;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.UI.SaveSystem;
using UnityEngine.UI.WebService;
using Utilities;

public class SaveController : MonoBehaviour
{
    internal static string FAIL_SCENE_TRIGGER = "sys_sc_failed";
    internal static string SUCCESS_SCENE_TRIGGER = "sys_sc_success";
    private SceneScanner sceneScanner;
    private SceneLoader sceneLoader;
    private ProjectController projectController;
    private VideoController videoController;
    internal IngameUIController ingameUIController;
    internal ProjectUpdater remoteProjectUpdater;
    internal UnityEvent OnSceneVideoEndReached = new UnityEvent();
    internal bool isLoadingScene = false;
    internal bool isReloadingScene = false;
    internal bool unsavedChanges = false;

    private SceneUpdater remoteSceneUpdater;
        
    void Start()
    {
        sceneScanner = FindObjectOfType<SceneScanner>();
        if (!sceneScanner)
        {
            Debug.LogError("no scene scanner found in scene");
        }
        sceneLoader = FindObjectOfType<SceneLoader>();
        if (!sceneLoader)
        {
            Debug.LogError("no scene loader found in scene");
        }
        projectController = FindObjectOfType<ProjectController>();
        if (!projectController)
        {
            Debug.LogError("no project controller found in scene");
        }
        videoController = FindObjectOfType<VideoController>();
        if (!videoController)
        {
            Debug.LogError("Error in SceneLoader: Couldn't find videoController in scene");
        }
        videoController.onVideoPlaybackEnded.AddListener(delegate
        {
            OnSceneVideoEndReached.Invoke();
        });
        videoController.onVideoPlaybackStarted.AddListener(unlockSceneLoading);
        
        remoteSceneUpdater = gameObject.AddComponent<SceneUpdater>();
        remoteSceneUpdater.OnSceneUploadSuccess.AddListener(setEditStateToSaved);
        
        remoteProjectUpdater = gameObject.AddComponent<ProjectUpdater>();
        
        remoteProjectUpdater.OnDownloadFinished.AddListener(delegate
        {
            if (isReloadingScene)
            {
                projectController.openProject(projectController.activeProject.projectSrc, projectController.activeScene.sceneName);
                isReloadingScene = false;
            }
        });
    }

    private void deleteSaveDataInActiveProject()
    {
        DirectoryInfo di = new DirectoryInfo(projectController.activeProjectPath);
        FileInfo[] files = di.GetFiles("*.json").Where(p => p.Extension == ".json").ToArray();
        foreach (FileInfo file in files)
        {
            try
            {
                file.Attributes = FileAttributes.Normal;
                File.Delete(file.FullName);
            }
            catch
            {
            }
        }
    }

    public void updateProjectFromRemoteWithReload()
    {
        isReloadingScene = true;
        ingameUIController.ShowSceneCurtain();
        remoteProjectUpdater.pullRemoteProject(projectController.activeProject.projectSrc, projectController.activeProject.projectSrc);
    }
    
    //update in background without notification or reloading current scene
    public void updateProjectFromRemote()
    {
        isReloadingScene = false;
        remoteProjectUpdater.pullRemoteProject(projectController.activeProject.projectSrc, projectController.activeProject.projectSrc);
    }

    public void updateCurrentSceneData()
    {
        projectController.ingameUIController.setSaveBtnToProgressState();
        SceneData sceneData = sceneScanner.scanCurrentScene(projectController.activeScene);
        SaveIntoJson(sceneData);
        projectController.setNewSaveTimeStamp();
        remoteSceneUpdater.remoteUploadSceneData(sceneData, projectController.activeProject);
    }

    internal void setEditStateToUnsaved()
    {
        if (!unsavedChanges)
        {
            unsavedChanges = true;
            projectController.ingameUIController.setSaveBtnToUnsavedState();
        }
    }

    internal void setEditStateToSaved()
    {
        unsavedChanges = false;
        projectController.ingameUIController.setSaveBtnToSavedState();
        updateProjectFromRemote(); //always update from remote after push
    }
    
    public void renameCurrentScene(string sceneName)
    {
        sceneLoader.renameSceneListFile(sceneScanner.getCurrentSceneName(), sceneName);
        RemoveJsonSceneData(sceneScanner.getCurrentSceneName());
        sceneLoader.setSceneName(sceneName);
        updateCurrentSceneData();
    }

    /// <summary>
    /// If sceneName is a sys_trigger string, interactable handles that instead of loading a new scene.
    /// /// </summary>
    public void handleSysTrigger(TargetScene scene, Base360Interactable triggeringInteractable)
    {
        if(scene.targetSceneName.Equals(FAIL_SCENE_TRIGGER))
        {
            ingameUIController.showWarningFold();
            ingameUIController.openGenericVerifyField(resumeScenarioAfterEnd, scene.hiddenData);
        }
        else if(scene.targetSceneName.Equals(SUCCESS_SCENE_TRIGGER))
        {
            ingameUIController.showSuccessFold();
            ingameUIController.openGenericVerifyField(resumeScenarioAfterEnd, scene.hiddenData);
        }
    }

    /// <summary>
    /// Loads the scene with given name in current project folder.
    /// Creates and loads an new scene with this name, if no scene could be found.
    /// Seperate method overload for working with dynamic unity events.
    /// </summary>
    public bool loadScene(string name)
    {
        return loadScene(name, false);
    }
    
    /// <summary>
    /// Loads the scene with given name in current project folder.
    /// Creates and loads an new scene with this name, if no scene could be found.
    /// Returns false, if there is already a scene load active.
    /// </summary>
    public bool loadScene(string name, bool isInitialScene = false)
    {
        if (!isLoadingScene)
        {
            videoController.reachedEndAndStopped = false; //TODO: Not a good solution to set it like this in terms of architecture
            isLoadingScene = true;
            string sceneDataString = GetJsonSceneData(name);
            if (sceneDataString == null)
            {
                isLoadingScene = false;
                return false;
            }
            SceneData sceneData = LoadFromJsonString(sceneDataString);
            if (sceneData != null)
            {
#if UNITY_WEBGL || UNITY_STANDALONE_WIN_DESKTOP
                sceneLoader.LoadSceneWebGL(sceneDataString, isInitialScene);
#else
                if (NetworkManager.Singleton.IsHost)
                {
                    sceneLoader.LoadSceneClientRpc(sceneDataString, isInitialScene);
                }
                else
                {
                    sceneLoader.LoadSceneServerRpc(sceneDataString, isInitialScene);
                }
#endif
                
                projectController.activeScene = sceneData;
            }
            else
            {
                isLoadingScene = false;
                Debug.Log("no sceneData found for name: " + name + " - reloading project");
                return false;
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// Loads the first scene found regardless of name or firstScene config in project data.
    /// </summary>
    public bool loadScene()
    {
        string[] sceneList = sceneLoader.getSceneList();
        if (sceneList.Any())
        {
            loadScene(sceneList[0]);
            return true;
        }
        return false;
    }

    private void unlockSceneLoading()
    {
        isLoadingScene = false;
    }

    public void returnToMainMenu()
    {
        ingameUIController.hideInteractionSphere();
        projectController.networkController.LeaveSession();
    }
    
    private void resumeScenarioAfterEnd(bool userConfirmed = true)
    {
        ingameUIController.hideWarningFold();
        ingameUIController.hideSuccessFold();
        if (!userConfirmed)
        {
            returnToMainMenu();
        }
    }
    
    private void SaveIntoJson(SceneData sceneData){
        string sceneString = JsonUtility.ToJson(sceneData);
        File.WriteAllText(projectController.activeProjectPath + "/" + sceneData.sceneName + "Data.json", sceneString);
    }

    private void RemoveJsonSceneData(string sceneName)
    {
        File.Delete(projectController.activeProjectPath + "/" + sceneName + "Data.json");
    }

    private string GetJsonSceneData(string sceneName)
    {
        string sceneString = null;
        if(File.Exists(projectController.activeProjectPath + "/" + sceneName + "Data.json"))
        {
            sceneString = File.ReadAllText(projectController.activeProjectPath + "/" + sceneName + "Data.json");
        }
        else
        {
            Debug.Log("No JSON file called " + sceneName + "Data.json in project folder");
        }

        return sceneString;
    }
    
    private SceneData LoadFromJsonString(string sceneDataString)
    {
        SceneData scene;
        scene = JsonUtility.FromJson<SceneData>(sceneDataString);
        return scene;
    }

    internal MediaRefTable getActiveProjectMediaRefs()
    {
        return projectController.getActiveProjectMediaRefs();
    }
}
