using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class VirtualKeyboardTransmitter : MonoBehaviour
{
    public InputField inputField;
    private bool hitShift;
    private bool hitCaps;

    internal UnityEvent<bool> onHitCaps = new Toggle.ToggleEvent();
    
    public void HandleCapsPress()
    {
        hitCaps = !hitCaps;
        if (!hitCaps)
        {
            hitShift = false;
        }
        onHitCaps.Invoke(hitCaps);
    }
    
    internal void HandleKeyPress(KeyCode key)
    {
        if (Regex.Matches(key.ToString(),@"[a-zA-ZÄÖÜ]").Count == 1 && key.ToString().Length == 1)
        {
            if (hitShift)
            {
                inputField.text += key.ToString().ToUpper();
                hitShift = false;
                onHitCaps.Invoke(hitShift);
            }
            else if (hitCaps)
            {
                inputField.text += key.ToString().ToUpper();
            }
            else
            {
                inputField.text += key.ToString().ToLower();
            }
        }
        else if (Regex.Matches(key.ToString(),@"\d").Count == 1 && key.ToString().Replace("Alpha", "").Length == 1)
        {
            if (hitShift)
            {
                inputField.text += digitToSymbol(key.ToString().Replace("Alpha", ""));
                hitShift = false;
                onHitCaps.Invoke(hitShift);
            }
            else if (hitCaps)
            {
                inputField.text += digitToSymbol(key.ToString().Replace("Alpha", ""));
            }
            else
            {
                inputField.text += key.ToString().Replace("Alpha", "");
            }
        }
        else if (key == KeyCode.Underscore)
        {
            inputField.text += "_";
        }
        else if (key == KeyCode.Minus)
        {
            inputField.text += "-";
        }
        else if (key == KeyCode.At)
        {
            inputField.text += "@";
        }
        else if (key == KeyCode.Space)
        {
            inputField.text += " ";
        }
        else if (key == KeyCode.Comma)
        {
            inputField.text += ",";
        }
        else if (key == KeyCode.Period)
        {
            inputField.text += ".";
        }
        else if (key == KeyCode.Exclaim)
        {
            inputField.text += "!";
        }
        else if (key == KeyCode.Question)
        {
            inputField.text += "?";
        }
        else if (key == KeyCode.F1)
        {
            inputField.text += "Ä";
        }
        else if (key == KeyCode.F2)
        {
            inputField.text += "Ö";
        }
        else if (key == KeyCode.F3)
        {
            inputField.text += "Ü";
        }
        else if (key == KeyCode.F4)
        {
            inputField.text += "ß";
        }
        else if (key == KeyCode.Less)
        {
            inputField.text += "<";
        }
        else if (key == KeyCode.Greater)
        {
            inputField.text += ">";
        }
        else if (key == KeyCode.Backslash)
        {
            inputField.text += "\\";
        }
        else if (key == KeyCode.LeftBracket)
        {
            inputField.text += "[";
        }
        else if (key == KeyCode.RightBracket)
        {
            inputField.text += "]";
        }
        else if (key == KeyCode.Hash)
        {
            inputField.text += "#";
        }
        else if (key == KeyCode.Colon)
        {
            inputField.text += ":";
        }
        else if (key == KeyCode.Semicolon)
        {
            inputField.text += ";";
        }
        else if (key == KeyCode.Asterisk)
        {
            inputField.text += "*";
        }
        else if (key == KeyCode.Plus)
        {
            inputField.text += "+";
        }
        else if (key == KeyCode.Quote)
        {
            inputField.text += "'";
        }
        else if (key == KeyCode.Backspace && inputField.text.Length >= 1)
        {
            inputField.text = inputField.text.Remove(inputField.text.Length - 1);
        }
        else if (key == KeyCode.Return)
        {
            inputField.GetComponentInParent<InputFieldPanel>().Submit();
            gameObject.SetActive(false);
        }
        else if (key == KeyCode.RightShift || key == KeyCode.LeftShift)
        {
            hitShift = true;
            onHitCaps.Invoke(hitShift);
        }
        else if (key == KeyCode.Escape)
        {
            inputField.transform.parent.gameObject.SetActive(false);
            gameObject.SetActive(false);
        }
    }

    internal string digitToSymbol(string digit)
    {
        switch (digit)
        {
            case "1":
                return "!";
            case "2":
                return "\"";
            case "3":
                return "§";
            case "4":
                return "$";
            case "5":
                return "%";
            case "6":
                return "&";
            case "7":
                return "/";
            case "8":
                return "(";
            case "9":
                return ")";
            case "0":
                return "=";
            default:
                return "N/A";
        }
    }
    
    internal string symbolToDigit(string digit)
    {
        switch (digit)
        {
            case "!":
                return "1";
            case "\"":
                return "2";
            case "§":
                return "3";
            case "$":
                return "4";
            case "%":
                return "5";
            case "&":
                return "6";
            case "/":
                return "7";
            case "(":
                return "8";
            case ")":
                return "9";
            case "=":
                return "0";
            default:
                return "N/A";
        }
    }
}
