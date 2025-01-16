using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.SaveSystem;

public class HintInteractable : Base360Interactable
{
    public Slider DelayTimer;

    public Text DelayLabel;

    public MultipleChoiceOption HintTextButton;

    public SwitchHandler switchHandler;


    internal override int type { get; set; }

    protected override void Awake()
    {
        targetScenes = new List<TargetScene>();
        targetScenes.Add(new TargetScene("", "", ""));
        type = 9;
        base.Awake();
    }

    void Start()
    {
        if (!DelayTimer)
        {
            Debug.Log("no delayTimer object set in HintInteractable");
        }

        base.Start();

        Single delay = Convert.ToSingle(targetScenes[0].hiddenData);
        if (!editModeController.isEditModeActive)
        {
            StartCoroutine(callActionDelay(delay));
        }

        switchHandler = GameObject.Find("SwitchHandler").GetComponent<SwitchHandler>();
    }

    IEnumerator callActionDelay(Single delay)
    {
        yield return new WaitForSeconds(delay + 1); //add another second for average orientation/loading time
        CallAction();
    }
    
    public void changeDelayValue(Single value)
    {
        DelayLabel.text = value.ToString();
        targetScenes[0].hiddenData = value.ToString();
    }

    public void setHintLabelText(string text)
    {
        setTargetLabel(text);
        updateGUI();
    }
    
    public override void OnClicked()
    {
        if (editModeController.isEditModeActive)
        {
            if (DelayTimer.gameObject.activeInHierarchy)
            {
                DelayTimer.gameObject.SetActive(false);
                HintTextButton.gameObject.SetActive(false);
            } 
            else
            {
                DelayTimer.gameObject.SetActive(true);
                HintTextButton.gameObject.SetActive(true);
            }
        }
        else
        {
            //saveController.uiController.openGenericInputField();
        }
    }

    private void CallAction()
    {
        if (!editModeController.isEditModeActive)
        {
            if (targetScenes[0].label.Any())
            {
                saveController.ingameUIController.openHintPanel(targetScenes[0].label);

                if(switchHandler.ScanningMethod == 0)
                {
                    GameObject ScriptfAHB = GameObject.Find("FindInteractablesScript");
                    ScriptfAHB.GetComponent<FindAndHighlightButtons>().ActivatedHintPanel();
                }
            }
        }
    }
    
    internal override void setTargetScene(string sceneName)
    {
        targetScenes[0].targetSceneName = sceneName;
    }

    internal override void setTargetLabel(string labelName)
    {
        targetScenes[0].label = labelName;
    }
    
    internal override void setTargetHiddenData(string stringData)
    {
        targetScenes[0].hiddenData = stringData;
    }

    internal override void updateGUI()
    {
        HintTextButton.labelBtnText.text = targetScenes[0].label;
        DelayTimer.value = Convert.ToSingle(targetScenes[0].hiddenData);
    }
}
