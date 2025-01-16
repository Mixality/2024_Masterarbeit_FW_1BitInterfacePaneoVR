using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICameraParentor : MonoBehaviour
{
    public Camera playerCamera;
    public bool parentYAxisOnly;
    
    void Update()
    {
        if (playerCamera != null)
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
        }
    }
}
