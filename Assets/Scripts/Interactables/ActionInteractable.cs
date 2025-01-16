using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI.SaveSystem;

public class ActionInteractable : Base360Interactable
{
    public GameObject SceneList;

    private GameObject Camera;

    private float initialCameraHeight;
    
    internal override int type { get; set; }

    protected override void Awake()
    {
        targetScenes = new List<TargetScene>();
        targetScenes.Add(new TargetScene("", "", ""));
        type = 3;
        base.Awake();
    }

    void Start()
    {
        Camera = FindObjectOfType<Camera>().gameObject;
        initialCameraHeight = Camera.transform.position.y;
        if (!Camera)
        {
            Debug.LogError("error in ActionInteractable: no camera found in scene");
        }
        
        if (!SceneList)
        {
            Debug.Log("no videoFileList object set in WaypointInteractable");
        }
        base.Start();
    }

    void Update()
    {
        if (checkForAction())
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
        base.Update();
    }
    
    private bool checkForAction()
    {
        if (initialCameraHeight - Camera.transform.position.y > 0.2f)
        {
            return true;
        }
        return false;
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
#if UNITY_WEBGL || UNITY_STANDALONE_WIN_DESKTOP
        else
        {
            if (targetScenes[0].targetSceneName.Any())
            {
                bool isSysMessage = checkForSysMessage(targetScenes[0]);
                if (!isSysMessage)
                {
                    saveController.loadScene(targetScenes[0].targetSceneName);
                }
            }
        }
#endif
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