using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionSphere360 : MonoBehaviour
{
    public Camera playerCamera;
    //public bool parentYAxisOnly;
    internal float currentSphereSize = 0;
    
    public void adaptSphereSize(float size)
    {
        gameObject.transform.localScale = Vector3.one * size;
        currentSphereSize = size;
    }
    
    /*void Update()
    {
        if (parentYAxisOnly)
        {
            var position = transform.position;
            position = new Vector3(position.x, playerCamera.transform.position.y, position.z);
            transform.position = position;
        }
        else
        {
            transform.position = playerCamera.transform.position;
        }
    }*/
}