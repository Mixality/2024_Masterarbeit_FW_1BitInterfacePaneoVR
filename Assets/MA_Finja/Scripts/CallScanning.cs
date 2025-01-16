using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CallScanning : MonoBehaviour
{

    public FindAndHighlightButtons findAndHighlightButtons;
    public GameObject DirectionBtnGroup;
    public SwitchHandler switchHandler;
    public CartesianScanHandler csHandler;

    // Start is called before the first frame update
    void Start()
    {
        GameObject gm = GameObject.Find("FindInteractablesScript");
        if (gm != null)
        {
            findAndHighlightButtons = gm.GetComponent<FindAndHighlightButtons>();
        }

        switchHandler = GameObject.Find("SwitchHandler").GetComponent<SwitchHandler>();
        csHandler = GameObject.Find("CartesianHandler").GetComponent<CartesianScanHandler>();

    }

    public void StartIngameMenuScanning()
    {
        if(switchHandler.ScanningMethod == 0)
        {
            findAndHighlightButtons.StartScanningInGameMenu();
        }
    }

    public void NavMenuScanning()
    {
        
        if (!DirectionBtnGroup.active)
        {
            DirectionBtnGroup.SetActive(true);
            findAndHighlightButtons.StartScanningNavMenu();
        }
        else
        {
            DirectionBtnGroup.SetActive(false);
            findAndHighlightButtons.StopScanningNavMenu();
            // hier könnte der Item Scanning Prozess wieder gestartet werden? 
        }
    }

    public void changeNavMode()
    {
        csHandler.changeMode();
    }


    public void DebugRight()
    {
        Debug.Log("Rechts herum!");
    }

    public void DebugLeft()
    {
        Debug.Log("Links herum!");
    }
}

