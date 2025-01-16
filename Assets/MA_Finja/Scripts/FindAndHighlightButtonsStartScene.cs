using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FindAndHIghlightButtonsStartScene : MonoBehaviour
{
    public List<GameObject> allButtons;
    // Start is called before the first frame update
    void Start()
    {
        List<GameObject> foundButtons = GameObject.FindGameObjectsWithTag("InteractableElement").ToList();

        foreach (GameObject btn in foundButtons)
        {
            allButtons.Add(btn);
        }

        foreach (GameObject but in allButtons)
        {
            Debug.Log(but.name);
        }
    }
}
