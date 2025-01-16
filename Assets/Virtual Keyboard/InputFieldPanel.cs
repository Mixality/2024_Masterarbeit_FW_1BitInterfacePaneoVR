using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class InputFieldPanel : MonoBehaviour
{
    private static string GENERIC_WINDOW_TITLE = "Neuen Namen setzen";
    private static string GENERIC_QUESTION_TITLE = "Neue Frage setzen";
    
    public Text WindowTitle;
    public Toggle NoIsTrueToggle;
    
    private InputField inputField;
    
    internal UnityEvent<string> OnSubmitInput = new InputField.SubmitEvent();

    private bool isPreparedForQuestion = false;
    
    private void Start()
    {
        inputField = gameObject.GetComponentInChildren<InputField>();
        if (!inputField)
        {
            Debug.LogError("Error in InputFieldPanel: cannot find Unity InputField in gameObject");
        }
        NoIsTrueToggle.gameObject.SetActive(false);
    }

    internal void prepareForSetGeneric()
    {
        NoIsTrueToggle.gameObject.SetActive(false);
        isPreparedForQuestion = false;
        WindowTitle.text = GENERIC_WINDOW_TITLE;
    }

    internal void prepareForSetQuestion()
    {
        NoIsTrueToggle.gameObject.SetActive(true);
        NoIsTrueToggle.isOn = false;
        isPreparedForQuestion = true;
        WindowTitle.text = GENERIC_QUESTION_TITLE;
    }

    /// <summary>
    /// If Input field is for setting a question, invoke method with the current input and a control sign for checking which answer
    /// is the right one for submitting on VerifyPanel
    /// </summary>
    internal void Submit()
    {
        if (isPreparedForQuestion)
        {
            if (NoIsTrueToggle.isOn)
            {
                OnSubmitInput.Invoke("t$" + inputField.text);
            }
            else
            {
                OnSubmitInput.Invoke("f$" + inputField.text);
            }
        }
        else
        {
            OnSubmitInput.Invoke(inputField.text);
        }

        OnSubmitInput.RemoveAllListeners();
        prepareForSetGeneric();
        gameObject.SetActive(false);
    }
}
