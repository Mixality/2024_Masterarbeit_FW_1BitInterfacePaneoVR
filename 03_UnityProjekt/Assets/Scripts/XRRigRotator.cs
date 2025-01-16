using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XRRigRotator : MonoBehaviour
{
    public void rotateTo(float angle)
    {
        gameObject.transform.rotation = Quaternion.AngleAxis(angle, Vector3.up);
    }
}
