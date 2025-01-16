using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DefaultNamespace;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.SaveSystem;

public class InvestigateInteractable : Base360Interactable
{
    private AssetLoader assetloader;
    private int currentImageIndex = 0;
    protected ImageBuffer ImageBuffer;
    protected Sprite viewImage;

    public Collider highlightCollider;
    
    internal override int type { get; set; }

    protected override void Awake()
    {
        targetScenes = new List<TargetScene>();
        targetScenes.Add(new TargetScene("", "", ""));
        type = 5;
        
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
        base.Start();
    }

    public void hideImage()
    {
        saveController.ingameUIController.closeImageViewPanel();
        highlightCollider.enabled = true;
    }

    public void showImage()
    {
        saveController.ingameUIController.openImageViewPanel(viewImage);
        highlightCollider.enabled = false;
    }


    protected virtual void setImage(string imageSrc)
    {
        if (imageSrc.Any())
        {
            Sprite sp = ImageBuffer.getImageWithSrc(imageSrc);
            viewImage = sp;
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
            viewImage = image;
            setTargetLabel(imageSrc);
        }
        else
        {
            Debug.Log("ERROR in InvstigateInteractable: Image not set");
        }
    }
    
    public override void OnClicked()
    {
        showImage();
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
            //SceneList.GetComponent<SceneList>().targetDisplay.text = targetScenes[0].targetSceneName;
        }
    }
}
