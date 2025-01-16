using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionTestLogger : MonoBehaviour
{
    public void DebugLogMessage(string message = "hello")
    {
        Debug.Log(message);
    }
}
