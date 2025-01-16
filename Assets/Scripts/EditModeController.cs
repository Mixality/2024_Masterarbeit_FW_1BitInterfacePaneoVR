using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using DesktopVersion;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.UI.SaveSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class EditModeController : MonoBehaviour
{
    public bool isEditModeActive;

    internal List<Canvas> ObservedCanvas;

    private SaveController saveController;

    private VideoController videoController;
    
    public UnityEvent OnStateChanged;

#if UNITY_WEBGL || UNITY_STANDALONE_WIN_DESKTOP
    private ScreenCameraRotator ScreenCameraRotator;
#endif

    private void Awake()
    {
        saveController = FindObjectOfType<SaveController>();
        if (!saveController)
        {
            Debug.LogError("Error in EditModeController: Couldn't find SaveController in Scene");
        }
        
        videoController = FindObjectOfType<VideoController>();
        if (!videoController)
        {
            Debug.LogError("Error in EditModeController: Couldn't find VideoController in Scene");
        }

#if UNITY_WEBGL || UNITY_STANDALONE_WIN_DESKTOP
        ScreenCameraRotator = FindObjectOfType<ScreenCameraRotator>();
        if (!ScreenCameraRotator)
        {
            Debug.LogError("Error in EditModeController: Couldn't find ScreenCameraRotator in Scene");
        }
#endif
    }

    public void setStateToUnsaved()
    {
        saveController.setEditStateToUnsaved();
    }
    
    public void toggleEditUI(bool active)
    {
        foreach (var canvas in ObservedCanvas)
        {
            TapButton[] buttons = canvas.GetComponentsInChildren<TapButton>();
            foreach (var button in buttons)
            {
                button.interactable = active;
            }
            
            Slider[] sliders = canvas.GetComponentsInChildren<Slider>();
            foreach (var slider in sliders)
            {
                slider.interactable = active;
            }
        }
        isEditModeActive = active;
        OnStateChanged.Invoke();
        videoController.RepeatCurrentVideo();
    }

#if UNITY_WEBGL || UNITY_STANDALONE_WIN_DESKTOP
    public void toggleScreenCameraRotation(bool isActive)
    {
        ScreenCameraRotator.isRotationControlActive = isActive;
    }
#endif
}
