﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.UI.SaveSystem;

public class Audio : Base360Interactable
{
    public GameObject AudioList;
    public AudioSource audioSource;
    private AudioClip clip;
    public Slider VolumeSlider;
    private AudioController audioController;

    public Text VolumeLabel;
    
    internal override int type { get; set; }

    protected override void Awake()
    {
        targetScenes = new List<TargetScene>();
        targetScenes.Add(new TargetScene("", "", ""));
        type = 10;
        base.Awake();
    }

    void Start()
    {
        if (!VolumeSlider)
        {
            Debug.Log("no VolumeSlider object set in AudioInteractable");
        }
        
        if (!AudioList)
        {
            Debug.Log("no audioFileList object set in AudioInteractable");
        }
        base.Start();
        audioController = FindObjectOfType<AudioController>();
        if (!audioController)
        {
            Debug.LogError("no AudioController in scene");
        }
        editModeController.OnStateChanged.AddListener(restartPlaying);
        audioSource.loop = true;
        if (targetScenes[0].targetSceneName.Any())
        {
            if (!audioSource.isPlaying)
            {
                bool isSysMessage = checkForSysMessage(targetScenes[0]);
                if (!isSysMessage)
                {
                    audioController.LoadAudioClip(audioSource, targetScenes[0].targetSceneName, targetScenes[0].hiddenData);
                }
            }
        }
    }
    
    public void changeVolumeValue(Single value)
    {
        VolumeLabel.text = value.ToString();
        if (targetScenes.Count > 0 && value >= 0)
        {
            targetScenes[0].hiddenData = value.ToString();
        }
        audioSource.volume = value;
    }

    public void setLoop(bool isLooping)
    {
        audioSource.loop = isLooping;
    }
    
    public override void OnClicked()
    {
        if (editModeController.isEditModeActive)
        {
            if (AudioList.activeInHierarchy)
            {
                VolumeSlider.gameObject.SetActive(false);
                AudioList.SetActive(false);
            } 
            else
            {
                VolumeSlider.gameObject.SetActive(true);
                AudioList.SetActive(true);
            }
        }
    }

    private void restartPlaying()
    {
        audioSource.Stop();
        if (targetScenes[0].targetSceneName.Any())
        {
            if (!audioSource.isPlaying)
            {
                bool isSysMessage = checkForSysMessage(targetScenes[0]);
                if (!isSysMessage)
                {
                    audioController.LoadAudioClip(audioSource, targetScenes[0].targetSceneName, targetScenes[0].hiddenData);
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
            AudioList.GetComponent<AudioFileList>().currentAudioDisplay.text = saveController.getActiveProjectMediaRefs().getNameFromAudioSrc(targetScenes[0].targetSceneName);
            try
            {
                VolumeSlider.value = Single.Parse(targetScenes[0].hiddenData);
            }
            catch (FormatException e)
            {
            
            }
        }
    }
}