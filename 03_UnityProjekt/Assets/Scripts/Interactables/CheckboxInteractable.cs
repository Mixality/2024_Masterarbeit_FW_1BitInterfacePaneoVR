using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI.SaveSystem;

public class CheckboxInteractable : Base360Interactable
{
    internal override int type { get; set; }

    void Awake()
    {
        targetScenes = new List<TargetScene>();
        targetScenes.Add(new TargetScene("", "", ""));
        type = 14;
        questionOnFail = "false";
        base.Awake();
    }
    public override void OnClicked()
    {
        
    }

    internal override void setTargetScene(string sceneName)
    {

    }

    internal override void setTargetLabel(string sceneName)
    {
        
    }
    
    internal override void setTargetHiddenData(string stringData)
    {
        
    }

    internal override void updateGUI()
    {
        
    }
}
