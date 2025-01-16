using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.SaveSystem;

public class MultipleChoiceInteractable : Base360Interactable
{
    public MultipleChoiceOption[] mcOptions; //TODO: So far there are 4 hard implemented options in GUI, this should better be dynamic

    private MultipleChoiceOption currentlyEditedOption;

    public Toggle showOptionsOnEnabledToggle;

    //added by me 
    private bool optionsVisible;
    public FindAndHighlightButtons findAndHighlightButtons;
    public SwitchHandler switchHandler;
    public GameObject bigHighlight;

    internal override int type { get; set; }

    protected override void Awake()
    {
        for (var index = 0; index < mcOptions.Length; index++)
        {
            targetScenes.Add(new TargetScene("","", ""));
            type = 1;
            base.Awake();
        }
    }

    void Start()
    {
        //added by me 
        findAndHighlightButtons = GameObject.Find("FindInteractablesScript").GetComponent<FindAndHighlightButtons>();
        switchHandler = GameObject.Find("SwitchHandler").GetComponent<SwitchHandler>();

        base.Start();
        editModeController.OnStateChanged.AddListener(hideOptions);
        if (showOptionsOnEnable)
        {
            OnClicked();
        }
    }

    private void hideOptions()
    {
        foreach (var mcOption in mcOptions)
        {
            mcOption.labelBtn.gameObject.SetActive(false);
            mcOption.targetBtn.gameObject.SetActive(false);
        }
        showOptionsOnEnabledToggle.gameObject.SetActive(false);

        if (findAndHighlightButtons.isDialogScanActive)
        {
            findAndHighlightButtons.isDialogScanActive = false;
        }

    }

    public override void OnClicked()
    {

        foreach (var mcOption in mcOptions)
        {
            if (mcOption.labelBtn.gameObject.activeInHierarchy)
            {
                mcOption.labelBtn.gameObject.SetActive(false);
                optionsVisible = false;
                if(switchHandler.ScanningMethod == 1){
                    bigHighlight.SetActive(true);
                }

                switchHandler.InitialInteraction = false;
                

                if (editModeController.isEditModeActive)
                {
                    mcOption.targetBtn.gameObject.SetActive(false);
                }

            }
            else
            {
                //TODO: this string check is based on the hard written text in the mcButton prefab -> that's to improve
                if (mcOption.labelBtnText.text.Any() && !mcOption.labelBtnText.text.Equals("angezeigter Text"))
                {
                    mcOption.labelBtn.gameObject.SetActive(true);
                    optionsVisible = true;
                    if(switchHandler.ScanningMethod == 1)
                    {
                        bigHighlight.SetActive(false);
                    }

                    if (editModeController.isEditModeActive)
                    {
                        mcOption.targetBtn.gameObject.SetActive(true);
                    }
                }
            }
        }
        if (editModeController.isEditModeActive)
        {
            if (showOptionsOnEnabledToggle.gameObject.activeInHierarchy)
            {
                showOptionsOnEnabledToggle.gameObject.SetActive(false);
            }
            else
            {
                showOptionsOnEnabledToggle.gameObject.SetActive(true);
            }
        }
        else
        {
            showOptionsOnEnabledToggle.gameObject.SetActive(false);
        }

        //added by me 
        if(switchHandler.ScanningMethod == 0)
        {
            if (optionsVisible)
            {
                findAndHighlightButtons.DialogScan(mcOptions);
            }
            else
            {
                findAndHighlightButtons.EndDialogScan();
            }
        }

    }

    internal override void updateGUI()
    {
        for (var index = 0; index < targetScenes.Count; index++)
        {
            mcOptions[index].labelBtnText.text = targetScenes[index].label;
            mcOptions[index].targetBtnText.text = targetScenes[index].targetSceneName;
            mcOptions[index].hiddenDataText.text = targetScenes[index].hiddenData;
        }
        showOptionsOnEnabledToggle.isOn = showOptionsOnEnable;
    }
    
    internal override void setTargetScene(string sceneName)
    {
        if (!sceneName.Equals("TargetSceneName") && currentlyEditedOption)
        {
            targetScenes[currentlyEditedOption.optionIndex].targetSceneName = sceneName;
            updateGUI();
        }
    }

    internal override void setTargetLabel(string text)
    {
        targetScenes[currentlyEditedOption.optionIndex].label = text;
        updateGUI();
    }
    
    internal override void setTargetHiddenData(string stringData)
    {
        targetScenes[currentlyEditedOption.optionIndex].hiddenData = stringData;
        updateGUI();
    }

    public void OnLabelSelected(MultipleChoiceOption mcOption)
    {
        if (editModeController.isEditModeActive)
        {
            currentlyEditedOption = mcOption;
        }
        else
        {
            if (mcOption.targetBtnText.text.Any() && !mcOption.targetBtnText.text.Equals("TargetSceneName"))
            {
                TargetScene sysMessage = new TargetScene(mcOption.labelBtnText.text, mcOption.targetBtnText.text, mcOption.hiddenDataText.text);
                bool isSysMessage = checkForSysMessage(sysMessage);
                if (!isSysMessage)
                {
                    saveController.loadScene(mcOption.targetBtnText.text);
                }
            }
        }
    }

    public void OnTargetButtonSelected(MultipleChoiceOption mcOption)
    {
        if (editModeController.isEditModeActive)
        {
            currentlyEditedOption = mcOption;
        }
    }
}
