using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveVerticalLine : MonoBehaviour
{

    float timeOfTravel = 10;
    float currentTime = 0;
    float normalizedValue;
    public RectTransform rectTransform;

    public CartesianScanHandler csh;

    private bool isAnimating = false;
    private Coroutine animationCoroutine;
    public int aniCounter = 0;

    public void Start()
    {
        csh = GameObject.Find("CartesianHandler").GetComponent<CartesianScanHandler>();
    }

    public void StartAnimation()
    {
        rectTransform = GetComponent<RectTransform>();

        Vector2 currentPos = rectTransform.anchoredPosition;
        rectTransform.anchoredPosition = new Vector2(-800f, currentPos.y);

        if (isAnimating) { return; }

        isAnimating = true;
        animationCoroutine = StartCoroutine(MoveLine());
    }

    public IEnumerator MoveLine()
    {
        isAnimating = true;
        Vector2 startPosition = new Vector2(-800f, rectTransform.anchoredPosition.y);
        // vorher 920
        Vector2 endPosition = new Vector2(1100f, startPosition.y);

        while (currentTime <= timeOfTravel)
        {
            currentTime += Time.deltaTime;
            normalizedValue = currentTime / timeOfTravel;

            rectTransform.anchoredPosition = Vector3.Lerp(startPosition, endPosition, normalizedValue);
            yield return null;
        }

        //isAnimating = false;
        if (aniCounter < 2)
        {
            yield return new WaitForSeconds(1f);
            resetAnimation();
            animationCoroutine = StartCoroutine(MoveLine());
            aniCounter++;
        }
        else
        {
            yield return new WaitForSeconds(1f);
            Debug.Log("Die vertikale Linie ist 3 Mal durchgelaufen und das Scanning wird abgebrochen.");
            csh.reset();
        }

    }

    public void StopAnimation()
    {
        if (!isAnimating) { return; }

        isAnimating = false;

        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
            animationCoroutine = null;
        }
    }

    public void resetAnimation()
    {
        isAnimating = false;
        if(rectTransform  != null)
        {
            rectTransform.anchoredPosition = new Vector2(-800f, 0f);
        }      
        currentTime = 0;
    }
}
