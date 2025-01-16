using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModeNavigation : MonoBehaviour
{
    public Transform xrRig;


    public bool turn(Vector3 intersectionPoint)
    {
        Vector3 targetDirection = intersectionPoint - xrRig.position;

        // Nur Y-Achse berücksichtigen
        targetDirection.y = 0;

        // Prüfen, ob der Zielvektor gültig ist (z.B. Schnittpunkt nicht direkt unter der Kamera)
        if (targetDirection == Vector3.zero)
        {
            Debug.LogWarning("Ungültiger Zielpunkt für Navigation!");
            return false;
        }

        // Berechnet neue Rotation, sodass das XRRig auf den Schnittpunkt zeigt
        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

        xrRig.rotation = Quaternion.Euler(0, targetRotation.eulerAngles.y, 0);
        return true;

    }
}
