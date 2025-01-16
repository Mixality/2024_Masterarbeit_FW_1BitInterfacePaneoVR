using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuNavigation : MonoBehaviour
{

    public GameObject xrRig;

    public void turnLeft()
    {
        xrRig.transform.Rotate(0, -40, 0);
    }

    public void turnRight() 
    {
        xrRig.transform.Rotate(0, 40, 0);
    }


    // Up und Down funktioniert noch nicht so richtig - neu überlegen! 
    public void turnUp() 
    {
        xrRig.transform.Rotate(0, 0, 0);
    }

    public void turnDown()
    {
        xrRig.transform.Rotate(0, 0, 0);
    }
}
