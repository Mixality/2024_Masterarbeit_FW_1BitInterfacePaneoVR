using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.SaveSystem;
using UnityEngine.Video;

public class TalkTextList : MonoBehaviour
{
    public Button templateButton;

    public Button emptyInput;
    
    public InputField inputField;
    
    private string selectedProject;
    
    public Image listGrid;
    
    private UIController UIController;

    private void OnEnable()
    {
        reloadProjects();
    }
    
    internal void reloadProjects()
    {
        BetterStreamingAssets.Initialize();
        string[] texts = BetterStreamingAssets.ReadAllLines("/textStrings.txt");
        foreach (Transform child in listGrid.transform)
        {
            Destroy(child.gameObject);
        }
        
        foreach (var text in texts)
        {
            Button newEntry = Instantiate(templateButton);
            newEntry.onClick.AddListener(delegate
            {
                putTextIntoInput(text);
            });
            newEntry.GetComponentInChildren<Text>().text = text;
            newEntry.transform.SetParent(listGrid.transform, false);
        }
    }

    private void putTextIntoInput(string text)
    {
        inputField.text = text;
    }

    public void emptyInputField()
    {
        inputField.text = "";
    }
}
