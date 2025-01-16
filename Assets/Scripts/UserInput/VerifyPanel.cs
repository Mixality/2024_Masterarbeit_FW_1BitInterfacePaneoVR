using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class VerifyPanel : MonoBehaviour
{
    public Text question;

    internal UnityEvent<bool> OnSubmit = new Toggle.ToggleEvent();

    public Text buttonBlue;
    public Text buttonRed;

    private string defaultBlueText = "Ja";
    private string defaultRedText = "Nein";

    public FindAndHighlightButtons findAndHighlightButtons;

    private void Submit(bool choice)
    {
        OnSubmit.Invoke(choice);
        
        //reset for next question
        OnSubmit.RemoveAllListeners();
        gameObject.SetActive(false);
        toggleYesButton(true);
        toggleNoButton(true);
        buttonBlue.text = defaultBlueText;
        buttonRed.text = defaultRedText;
    }

    internal void toggleYesButton(bool toggle)
    {
        buttonBlue.transform.parent.gameObject.SetActive(toggle);
    }

    internal void toggleNoButton(bool toggle)
    {
        buttonRed.transform.parent.gameObject.SetActive(toggle);
    }
    
    public void selectYes()
    {
        Submit(true);
    }

    public void selectNo()
    {
        Submit(false);
    }

    // added by me - Idee: start Scanning, sobald das Object aktiviert wird  
    void OnEnable()
    {
        SwitchHandler switchHandler = GameObject.Find("SwitchHandler").GetComponent<SwitchHandler>();

        if(switchHandler.ScanningMethod == 0)
        {
            GameObject gm = GameObject.Find("FindInteractablesScript");
            if (gm != null)
            {
                findAndHighlightButtons = gm.GetComponent<FindAndHighlightButtons>();
            }
            findAndHighlightButtons.StartScanningVerify();
        }
    }

    void OnDisable()
    {
        SwitchHandler switchHandler = GameObject.Find("SwitchHandler").GetComponent<SwitchHandler>();

        if (switchHandler.ScanningMethod == 0)
        {
            GameObject gm = GameObject.Find("FindInteractablesScript");
            if (gm != null)
            {
                findAndHighlightButtons = gm.GetComponent<FindAndHighlightButtons>();
            }
            findAndHighlightButtons.StopScanningVerify();
        }
    }

}
