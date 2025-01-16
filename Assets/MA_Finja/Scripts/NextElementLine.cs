using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NextElementLine : MonoBehaviour
{
    public VisibleByCamera vbc;
    private bool visibleCheckNeeded;
    public bool drawLine = false;
    public LineRenderer line;

    private GameObject currentObj;
    private GameObject nextObj;
    private Camera mainCam;

    // aka die ScanRate 
    public float animationDuration = 2f;

    // to do: check ob das zweite Object visible by cam ist 

    private void Update()
    {
        if (drawLine && currentObj != null && nextObj != null)
        {

            Vector3 currentObjectPosition = GetWorldPosition(mainCam, currentObj);
            Vector3 nextObjectPosition = GetWorldPosition(mainCam, nextObj);

            line.SetPosition(0, currentObjectPosition);
            line.SetPosition(1, nextObjectPosition);
        }
        else
        {
            line.enabled = false;
        }
        
    }

    public void DrawLineBetweenElements(Camera cam, GameObject current, GameObject next)
    {
        if (!line.enabled)
        {
            line.enabled = true;
        }

        currentObj = current;
        nextObj = next;
        mainCam = cam;
        drawLine = true;
        StartCoroutine(AnimateAlpha(animationDuration));
    }

    private Vector3 GetWorldPosition(Camera cam, GameObject gm)
    {

        if(gm.tag == "InteractableElement")
        {
            GameObject canvas = gm.transform.GetChild(0).gameObject;

            if (canvas != null)
            {
                RectTransform rectTransform = canvas.GetComponent<RectTransform>();

                if (rectTransform != null)
                {
                    return rectTransform.position;
                }
            }
        }

        return gm.transform.position;
    }


    private IEnumerator AnimateAlpha(float duration)
    {
        float halfDuration = duration / 2f;

        // Initialisiere die GradientAlphaKeys des LineRenderers
        Gradient gradient = line.colorGradient;
        GradientAlphaKey[] alphaKeys = gradient.alphaKeys;

        alphaKeys[0].alpha = 0f;
        alphaKeys[1].alpha = 0f;

        // AlphaKey[0] von 0 auf 1 in der ersten Hälfte
        for (float t = 0; t < halfDuration; t += Time.deltaTime)
        {
            float alpha = t / halfDuration; // Linearer Interpolationswert
            alphaKeys[0].alpha = alpha;

            // Setze die neuen AlphaKeys
            gradient.alphaKeys = alphaKeys;
            line.colorGradient = gradient;

            yield return null;
        }

        alphaKeys[0].alpha = 1f; // Sicherstellen, dass der Endwert erreicht wird
        gradient.alphaKeys = alphaKeys;
        line.colorGradient = gradient;

        // AlphaKey[1] von 0 auf 1 in der zweiten Hälfte
        for (float t = 0; t < halfDuration; t += Time.deltaTime)
        {
            float alpha = t / halfDuration; // Linearer Interpolationswert
            alphaKeys[1].alpha = alpha;

            // Setze die neuen AlphaKeys
            gradient.alphaKeys = alphaKeys;
            line.colorGradient = gradient;

            yield return null;
        }
        alphaKeys[1].alpha = 1f; // Sicherstellen, dass der Endwert erreicht wird
        gradient.alphaKeys = alphaKeys;
        line.colorGradient = gradient;
    }
}
