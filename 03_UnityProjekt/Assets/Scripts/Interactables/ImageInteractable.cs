using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.SaveSystem;

public class ImageInteractable : Base360Interactable
{
    public GameObject SceneList;
    public Image ButtonImage;
    public GameObject ButtonNext;
    public GameObject ButtonPrev;
    private ImageBuffer ImageBuffer;
    private int currentImageIndex = 0;

    internal override int type { get; set; }

    protected override void Awake()
    {
        targetScenes = new List<TargetScene>();
        targetScenes.Add(new TargetScene("", "", ""));
        type = 8;
        
        GameObject[] sceneManagerObjects = GameObject.FindGameObjectsWithTag("SceneManager");
        foreach (var sceneManagerObject in sceneManagerObjects)
        {
            if (sceneManagerObject.GetComponent<ImageBuffer>())
            {
                ImageBuffer = sceneManagerObject.GetComponent<ImageBuffer>();
                break;
            }
        }
        
        base.Awake();
    }

    void Start()
    {
        if (!SceneList)
        {
            Debug.Log("no videoFileList object set in WaypointInteractable");
        }
        base.Start();
    }
    
    public void loadNextImage()
    {
        if (currentImageIndex < ImageBuffer.getBufferSize() - 1)
        {
            currentImageIndex++;
        }
        else
        {
            currentImageIndex = 0;
        }

        KeyValuePair<string, Sprite> image = ImageBuffer.getImageWithIndex(currentImageIndex);
        setImage(image.Value, image.Key);
    }

    public void loadPreviousImage()
    {
        if (currentImageIndex > 0)
        {
            currentImageIndex--;
        }
        else
        {
            currentImageIndex = ImageBuffer.getBufferSize() - 1;
        }
        
        KeyValuePair<string, Sprite> image = ImageBuffer.getImageWithIndex(currentImageIndex);
        setImage(image.Value, image.Key);
    }

    protected virtual void setImage(string imageSrc)
    {
        if (imageSrc.Any())
        {
            Sprite sp = ImageBuffer.getImageWithSrc(imageSrc);
            ButtonImage.GetComponent<Image>().sprite = sp;
            setTargetLabel(imageSrc);
        }
        else
        {
            Debug.Log("ERROR in InvstigateInteractable: ImageName not set");
        }
    }
    
    protected virtual void setImage(Sprite image, string imageSrc)
    {
        if (image)
        {
            ButtonImage.GetComponent<Image>().sprite = image;
            setTargetLabel(imageSrc);
        }
        else
        {
            Debug.Log("ERROR in ImageInteractable: Image not set");
        }
    }
    
    public override void OnClicked()
    {
        if (editModeController.isEditModeActive)
        {
            if (SceneList.activeInHierarchy)
            {
                SceneList.SetActive(false);
                ButtonNext.SetActive(false);
                ButtonPrev.SetActive(false);
            } 
            else
            {
                SceneList.SetActive(true);
                ButtonNext.SetActive(true);
                ButtonPrev.SetActive(true);
                SceneList.GetComponent<SceneList>().reloadScenes();
            }
        }
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
            setImage(targetScenes[0].label);
            SceneList.GetComponent<SceneList>().targetDisplay.text = saveController.getActiveProjectMediaRefs().getNameFromImageSrc(targetScenes[0].targetSceneName);
        }
    }
}
