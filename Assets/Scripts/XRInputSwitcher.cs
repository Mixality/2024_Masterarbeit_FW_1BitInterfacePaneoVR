using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

public class XRInputSwitcher : MonoBehaviour
{
    void Start()
    {
        /*TODO: Not a perfect solution cause the conditions are not perfectly distinguish between VR and Desktop-Version*/
        #if UNITY_ANDROID
            GetComponent<XRUIInputModule>().enabled = true;
            GetComponent<InputSystemUIInputModule>().enabled = false;
        #elif UNITY_STANDALONE_WIN
            GetComponent<XRUIInputModule>().enabled = false;
            GetComponent<InputSystemUIInputModule>().enabled = true;
        #elif UNITY_WEBGL || UNITY_STANDALONE_WIN_DESKTOP
            GetComponent<XRUIInputModule>().enabled = false;
            GetComponent<InputSystemUIInputModule>().enabled = true;
        #endif
    }
}
