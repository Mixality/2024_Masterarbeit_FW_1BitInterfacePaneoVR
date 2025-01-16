using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class HintPanel : MonoBehaviour
{
    public Text HintText;

    internal void setHint(string hint)
    {
        HintText.text = hint;
    }

    public void CloseHint()
    {
        gameObject.transform.parent.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }
}
