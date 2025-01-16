using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System;

public class CartesainHandler : MonoBehaviour
{

    public bool firstStarted = false;
    public bool secondStarted = false;

    private GameObject cartesainHorizontal;
    private LineRenderer lineHorizontal;
    private GameObject cartesainVertikal;
    private LineRenderer lineVertikal;

    // Start is called before the first frame update
    public void Start()
    {

        cartesainHorizontal = GameObject.Find("LineHorizontal");
        Debug.Log(cartesainHorizontal);

        if (cartesainHorizontal != null)
        { 
            lineHorizontal = cartesainHorizontal.GetComponent<LineRenderer>();
            cartesainHorizontal.SetActive(false);
        }

        cartesainVertikal = GameObject.Find("LineVertikal");

        if(cartesainVertikal != null)
        {
            lineVertikal = cartesainVertikal.GetComponent<LineRenderer>();
            cartesainVertikal.SetActive(false);
        }
        
    }

    // Hier Logs einfügen für die Evaluation!!!
    public void Scanning()
    {
        if (!firstStarted)
        {
            ScanningFirstLine();
        }
        else
        {
            if (!secondStarted)
            {
                StopFirstLine();
            }
            else
            {
                Selection();
            }
        }
    }

    public void ScanningFirstLine()
    {
        firstStarted = true;
        cartesainHorizontal.SetActive(true);
        Debug.Log("Hier findet die Animation der Linie statt");
    }


    public void StopFirstLine()
        {
            Debug.Log("Hier wird die Animation der ersten Linie angehalten, die Position gemerkt & die zweite Linie in Gang gebracht");
            StartSecondLine();
        }

    public void StartSecondLine()
    {
        secondStarted = true;
        cartesainVertikal.SetActive(true);
        Debug.Log("Hier findet die Animation der Linie statt");
    }

    public void Selection()
    {
        Debug.Log("Hier wird die zweite Linie gestoppt und die Auswahl getroffen. Am Ende müssen die Bools wieder auf false gesetzt werden");
        cartesainHorizontal.SetActive(false);
        cartesainVertikal.SetActive(false);
        firstStarted = false;
        secondStarted = false;
    }


}
