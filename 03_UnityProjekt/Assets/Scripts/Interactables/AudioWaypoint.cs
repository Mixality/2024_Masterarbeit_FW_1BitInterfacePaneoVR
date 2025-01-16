using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.SaveSystem;

public class AudioWaypoint : Base360Interactable
{
    public GameObject AudioList;
    public AudioSource audioSource;
    private AudioClip clip;
    public Slider VolumeSlider;
    public GameObject ImagePlay;
    public GameObject ImageSkip;
    private bool userFiredPlayback;
    private bool startedPlayback;
    private AudioController audioController;
    public Text VolumeLabel;
    
    internal override int type { get; set; }

    protected override void Awake()
    {
        targetScenes = new List<TargetScene>();
        targetScenes.Add(new TargetScene("", "", ""));
        type = 12;
        base.Awake();
    }

    void Start()
    {
        if (!VolumeSlider)
        {
            Debug.Log("no VolumeSlider object set in AudioWaypoint");
        }
        
        if (!AudioList)
        {
            Debug.Log("no audioFileList object set in AudioWaypoint");
        }
        base.Start();
        audioController = FindObjectOfType<AudioController>();
        if (!audioController)
        {
            Debug.LogError("no AudioController in scene");
        }
        editModeController.OnStateChanged.AddListener(stopPlaying);
    }

    private void LateUpdate()
    {
        //Switch scene on playback stopped
        if (userFiredPlayback)
        {
            if (audioSource.isPlaying && !startedPlayback)
            {
                startedPlayback = true;
            }
            
            if (startedPlayback && !audioSource.isPlaying) //finished playing
            {
                userFiredPlayback = false;
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
    }

    public void changeVolumeValue(Single value)
    {
        VolumeLabel.text = value.ToString();
        if (targetScenes.Count > 0 && value >= 0)
        {
            targetScenes[0].hiddenData = value.ToString();
        }
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
        else
        {
            if (targetScenes[0].label.Any())
            {
                if (!audioSource.isPlaying)
                {
                    bool isSysMessage = checkForSysMessage(targetScenes[0]);
                    if (!isSysMessage)
                    {
                        audioController.LoadAudioClip(audioSource, targetScenes[0].label, targetScenes[0].hiddenData);
                        userFiredPlayback = true;
                    }
                    ImagePlay.SetActive(false);
                    ImageSkip.SetActive(true);
                }
                else
                {
                    stopPlaying();
                }
            }
        }
    }

    private void stopPlaying()
    {
        audioSource.Stop();
        ImagePlay.SetActive(true);
        ImageSkip.SetActive(false);
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
            string displayName = saveController.getActiveProjectMediaRefs().getNameFromAudioSrc(targetScenes[0].label);
            AudioList.GetComponent<AudioFileList>().currentAudioDisplay.text = displayName;
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