using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NavMenuNeeded : MonoBehaviour
{
    public GameObject NavMode;
    // Start is called before the first frame update
    void Start()
    {
        SwitchHandler switchHandler = GameObject.Find("SwitchHandler").GetComponent<SwitchHandler>();
        if(switchHandler.ScanningMethod == 1)
        {
            gameObject.SetActive(false);
        }
    }
}
