using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NullInteractable : Base360Interactable
{
    internal override int type { get; set; }

    void Start()
    {
        type = 99;
        base.Start();
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
