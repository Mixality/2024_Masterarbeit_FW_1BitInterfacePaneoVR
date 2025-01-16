using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.SaveSystem;
using Random = System.Random;

public class RndWaypointInteractable : Base360Interactable
{
    public MultipleChoiceOption[] rndOptions; //TODO: So far there are 4 hard implemented options in GUI, this should better be dynamic

    private MultipleChoiceOption currentlyEditedOption;

    static Random rnd = new Random();
    
    internal override int type { get; set; }
    
    // added by me 
    private bool optionsVisible;
    public FindAndHighlightButtons findAndHighlightButtons;
    public SwitchHandler switchHandler;
    public GameObject bigHighlight;

    protected override void Awake()
    {
        for (var index = 0; index < rndOptions.Length; index++)
        {
            targetScenes.Add(new TargetScene("","", ""));
            type = 2;
            base.Awake();
        }
    }

    new void Start()
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

        if (!PlayerPrefs.HasKey("LastScene"))
        {
            PlayerPrefs.SetString("LastScene", "");
        }
    }

    private void hideOptions()
    {
        foreach (var rndOption in rndOptions)
        {
            rndOption.labelBtn.gameObject.SetActive(false);
            rndOption.targetBtn.gameObject.SetActive(false);
        }

        //added by me 
        if (findAndHighlightButtons.isDialogScanActive)
        {
            findAndHighlightButtons.isDialogScanActive = false;
        }
        optionsVisible = false;
    }
    
    public override void OnClicked()
    {
        if (editModeController.isEditModeActive)
        {
            foreach (var rndOption in rndOptions)
            {
                if (rndOption.targetBtn.gameObject.activeInHierarchy)
                {
                    rndOption.targetBtn.gameObject.SetActive(false);
                }
                else
                {
                    //TODO: this string check is based on the hard written text in the rndButton prefab -> that's to improve
                    if (rndOption.targetBtnText.text.Any() && !rndOption.targetBtnText.text.Equals("Zielszene"))
                    {
                        rndOption.targetBtn.gameObject.SetActive(true);
                    }
                }
            }
        }
        else
        {
            List<TargetScene> filteredTargets = new List<TargetScene>();
            foreach (var target in targetScenes)
            {
                if (target.targetSceneName.Any())
                {
                    filteredTargets.Add(target);
                }
            }

            //TODO: Remove this in upcoming versions; just to make sure that the project participants play both versions
            for (int i = 0; i < filteredTargets.Count; i++)
            {
                if (filteredTargets[i].targetSceneName.Equals(PlayerPrefs.GetString("LastScene")))
                {
                    filteredTargets.Remove(filteredTargets[i]);
                    break;
                }
            }

            int r = rnd.Next(filteredTargets.Count);
            
            bool isSysMessage = checkForSysMessage(filteredTargets[r]);
            if (!isSysMessage)
            {
                PlayerPrefs.SetString("LastScene", filteredTargets[r].targetSceneName);
                saveController.loadScene(filteredTargets[r].targetSceneName);
            }
        }
    }

    internal override void updateGUI()
    {
        for (var index = 0; index < targetScenes.Count; index++)
        {
            rndOptions[index].labelBtnText.text = targetScenes[index].label;
            rndOptions[index].targetBtnText.text = targetScenes[index].targetSceneName;
            rndOptions[index].hiddenDataText.text = targetScenes[index].hiddenData;
        }
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

    public void OnLabelSelected(MultipleChoiceOption rndOption)
    {
        if (editModeController.isEditModeActive)
        {
            currentlyEditedOption = rndOption;
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
