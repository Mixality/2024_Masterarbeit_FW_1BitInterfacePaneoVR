using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class VirtualKeyListener : MonoBehaviour
{
    private VirtualKeyboardTransmitter keyboardTransmitter;

    public Text charOnKey;
    
    public KeyCode key;

    private void Start()
    {
        gameObject.GetComponent<Button>().onClick.AddListener(fireButton);
        keyboardTransmitter = GetComponentInParent<VirtualKeyboardTransmitter>();
        if (!keyboardTransmitter)
        {
            Debug.Log("Error in VirtualKeyListener: Couldn't find VirtualKeyboardTransmitter component in parent GameObject");
        }

        if (Regex.Matches(charOnKey.text, @"[a-zA-ZÄÖÜ]").Count == 1)
        {
            keyboardTransmitter.onHitCaps.AddListener(switchCase);
            switchCase(false);
        }
        else if (Regex.Matches(charOnKey.text.Replace("Alpha", ""), @"\d").Count == 1)
        {
            keyboardTransmitter.onHitCaps.AddListener(switchDigitCase);
            switchDigitCase(false);
        }
        switchCase(false);
    }

    internal void fireButton()
    {
        keyboardTransmitter.HandleKeyPress(key);
    }

    private void switchDigitCase(bool toUpperCase)
    {
        if (toUpperCase)
        {
            charOnKey.text = keyboardTransmitter.digitToSymbol(key.ToString().Replace("Alpha", ""));
        }
        else
        {
            charOnKey.text = key.ToString().Replace("Alpha", "");
        }
    }
    
    private void switchCase(bool toUpperCase)
    {
        if (toUpperCase)
        {
            charOnKey.text = charOnKey.text.ToUpper();
        }
        else
        {
            charOnKey.text = charOnKey.text.ToLower();
        }
    }
}
