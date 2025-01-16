using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.UI.SaveSystem;
using UnityEngine.Video;

public class SceneList : MonoBehaviour
{
    public Base360Interactable interactable;
    
    public Button templateButton;
    public Button scenarioFailedButton;
    public Button scenarioSuccessButton;

    public Image listGrid;

    private SceneLoader sceneLoader;

    public Text targetDisplay;

    public bool IsSceneJumper;

    private static string ADD_NEW_SCENE_SIGN = "+";
    private static string SCENARIO_FAILED_SIGN = "Szenario Fehlgeschlagen";
    private static string SCENARIO_SUCCESS_SIGN = "Szenario Erfolgreich";

    public UnityEvent<string> onSceneJump = new UnityEvent<string>(); 
    
    private void Awake()
    {
        sceneLoader = FindObjectOfType<SceneLoader>();
        if (!sceneLoader)
        {
            Debug.LogError("Error while creating scene list element: couldn't find sceneLoader in scene");
        }
    }

    internal void reloadScenes()
    {
        if (IsSceneJumper)
        {
            StartCoroutine(waitForUpdateAndReloadScenes());
        }
    }
    
    IEnumerator waitForUpdateAndReloadScenes()
    {
        yield return new WaitUntil(() => !sceneLoader.isUpdatingFile);
        string[] sceneNames = sceneLoader.getSceneList();
        
        foreach (Transform child in listGrid.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (var sceneName in sceneNames)
        {
            if (sceneName.Any())
            {
                Button newEntry = Instantiate(templateButton, listGrid.transform, false);
                if (!IsSceneJumper)
                {
                    newEntry.onClick.AddListener(delegate
                    {
                        interactable.setTargetScene(sceneName);
                        targetDisplay.text = sceneName;
                    });
                }
                else
                {
                    newEntry.onClick.AddListener(delegate { onSceneJump.Invoke(sceneName); });
                }

                newEntry.GetComponentInChildren<Text>().text = sceneName;
            }
        }
    }
}
