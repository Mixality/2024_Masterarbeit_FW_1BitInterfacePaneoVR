using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class XRInteractionTransmitter : MonoBehaviour
{
    public void TransmitXRSelectionEnter(SelectEnterEventArgs e)
    {
        e.interactableObject.transform.gameObject.GetComponent<Base360Interactable>().OnGrabbedStart(gameObject);
    }
    
    public void TransmitXRSelectionExit(SelectExitEventArgs e)
    {
        e.interactableObject.transform.gameObject.GetComponent<Base360Interactable>().OnGrabbedEnd(gameObject);
    }
    
    public void TransmitXRHoveringEnter(HoverEnterEventArgs e)
    {
        e.interactableObject.transform.gameObject.GetComponent<Base360Interactable>().OnHoveredStart(gameObject);
    }
    
    public void TransmitXRHoveringExit(HoverExitEventArgs e)
    {
        e.interactableObject.transform.gameObject.GetComponent<Base360Interactable>().OnHoveredEnd(gameObject);
    }
}