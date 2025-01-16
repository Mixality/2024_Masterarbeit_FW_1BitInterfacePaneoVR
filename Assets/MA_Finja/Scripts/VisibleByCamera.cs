using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisibleByCamera : MonoBehaviour
{

    private Camera cam;

    public bool IsVisibleByCam(Camera cam, Collider targetObjCollider)
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(cam);
        
        if(GeometryUtility.TestPlanesAABB(planes, targetObjCollider.bounds))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

}
