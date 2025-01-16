using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI.SaveSystem;

/// <summary>
/// A little tricky name on the current name conventions, because it's exactly not a player interaction that triggers this,
/// it's a simple "switch scene on video end"
/// </summary>
public class PlaythroughInteractable : Base360Interactable
{
    public GameObject SceneList;

    private GameObject Camera;
    
    internal override int type { get; set; }

    protected override void Awake()
    {
        targetScenes = new List<TargetScene>();
        targetScenes.Add(new TargetScene("", "", ""));
        type = 4;
        base.Awake();
    }

    void Start()
    {
        Camera = FindObjectOfType<Camera>().gameObject;
        if (!Camera)
        {
            Debug.LogError("error in PlaythroughInteractable: no camera found in scene");
        }
        
        if (!SceneList)
        {
            Debug.Log("no videoFileList object set in PlaythroughInteractable");
        }
        base.Start();
        saveController.OnSceneVideoEndReached.AddListener(CallAction);
    }

    private void CallAction()
    {
        if (!editModeController.isEditModeActive)
        {
            if (targetScenes[0].targetSceneName.Any())
            {
                if (targetScenes[0].targetSceneName.Any())
                {
                    bool isSysMessage = checkForSysMessage(targetScenes[0]);
                    if (!isSysMessage)
                    {
                        gameObject.SetActive(false);
                        saveController.loadScene(targetScenes[0].targetSceneName);
                    }
                }
            }
        }
    }

    public override void OnClicked()
    {
        if (editModeController.isEditModeActive)
        {
            if (SceneList.activeInHierarchy)
            {
                SceneList.SetActive(false);
            } 
            else
            {
                SceneList.SetActive(true);
                SceneList.GetComponent<SceneList>().reloadScenes();
            }
        }
    }

    internal override void setTargetScene(string sceneName)
    {
        if (targetScenes?.Any() == true)
        {
            targetScenes[0].targetSceneName = sceneName;
        }
    }

    internal override void setTargetLabel(string labelName)
    {
        if (targetScenes?.Any() == true)
        {
            targetScenes[0].label = labelName;
        }
    }
    
    internal override void setTargetHiddenData(string stringData)
    {
        if (targetScenes?.Any() == true)
        {
            targetScenes[0].hiddenData = stringData;
        }
    }

    internal override void updateGUI()
    {
        if (targetScenes?.Any() == true)
        {
            SceneList.GetComponent<SceneList>().targetDisplay.text = targetScenes[0].targetSceneName;
        }
    }
}