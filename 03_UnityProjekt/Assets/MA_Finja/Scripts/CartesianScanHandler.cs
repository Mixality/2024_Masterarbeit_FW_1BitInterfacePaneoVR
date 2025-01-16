using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

public class CartesianScanHandler : MonoBehaviour
{
    
    private bool firstStarted = false;
    private bool secondStarted = false;
    private bool timeForSelection = false;

    private GameObject cartesainHorizontal;
    private LineRenderer lineRendererHorizontal;
    private MoveHorizontalLine mhl;

    private GameObject cartesainVertikal;
    private LineRenderer lineRendererVertikal;
    private MoveVerticalLine mvl;

    private Vector3 intersectionPoint; 
    private Camera cam;
    private GameObject interactiveElement;

    public GameObject markerPrefab;

    private RectTransform horizontalLine;
    private RectTransform verticalLine;

    private bool navModeActive = false;
    private ModeNavigation modeNav;

    [SerializeField] private AudioClip startScanning;
    [SerializeField] private AudioClip itemClicked;
    [SerializeField] private AudioClip noItemHitted;
    [SerializeField] private AudioClip noSelection;
    [SerializeField] private AudioClip changeInteractionMode;

    private Color navColor = new Color(0f, 31f, 140f);
    private Color selectColor = new Color(255f, 0f, 246f);

    private float DebugStartTime;


    void Start()
    {
        cartesainHorizontal = GameObject.Find("LineHorizontal");


        if (cartesainHorizontal != null)
        {
            lineRendererHorizontal = cartesainHorizontal.GetComponent<LineRenderer>();
            mhl = cartesainHorizontal.GetComponent<MoveHorizontalLine>();
            horizontalLine = cartesainHorizontal.GetComponent<RectTransform>();
            cartesainHorizontal.SetActive(false);
        }

        cartesainVertikal = GameObject.Find("LineVertikal");

        if (cartesainVertikal != null)
        {
            lineRendererVertikal = cartesainVertikal.GetComponent<LineRenderer>();
            mvl = cartesainVertikal.GetComponent<MoveVerticalLine>();
            verticalLine = cartesainVertikal.GetComponent<RectTransform>();
            cartesainVertikal.SetActive(false);
        }

        GameObject MPCam = GameObject.Find("Main MP Camera");
        cam = MPCam.GetComponent<Camera>();

        modeNav = GameObject.Find("Navigation").GetComponent<ModeNavigation>();
    }

    public void changeMode()
    {
        if (navModeActive)
        {
            navModeActive = false;
            lineRendererHorizontal.startColor = selectColor;
            lineRendererHorizontal.endColor = selectColor;
            lineRendererVertikal.startColor = selectColor;
            lineRendererVertikal.endColor = selectColor;
        }
        else
        {
            navModeActive = true;
            lineRendererHorizontal.startColor = navColor;
            lineRendererHorizontal.endColor = navColor;
            lineRendererVertikal.startColor = navColor;
            lineRendererVertikal.endColor = navColor;
        }

        //SoundHandler.instance.playSound(changeInteractionMode, transform, 1f);

    }


    public void Scanning()
    {

        if (!firstStarted)
        {
            ScanningFirstLine();
            DebugStartTime = Time.time;
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
                float DebugEndTime = Time.time;
                Debug.Log($"Selektionsgeschwindigkeit: {DebugEndTime - DebugStartTime} Sekunden");
            }
        }
    }

    public void ScanningFirstLine()
    {
        firstStarted = true;
        cartesainHorizontal.SetActive(true);
        mhl.StartAnimation();
        SoundHandler.instance.playSound(startScanning, transform, 1f);
    }


    public void StopFirstLine()
    {
        StartSecondLine();
        mhl.StopAnimation();
    }

    public void StartSecondLine()
    {
        secondStarted = true;
        cartesainVertikal.SetActive(true);
        mvl.StartAnimation();
        SoundHandler.instance.playSound(startScanning, transform, 1f);
    }

    public void StopSecondLine()
    {
        mvl.StopAnimation();
        timeForSelection = true;
    }

    public void Selection()
    {
        mvl.StopAnimation();
        FindIntersection();

        Debug.Log($"Horizontale Linie ist: {mhl.aniCounter} Mal durchgelaufen.");
        Debug.Log($"Vertikale Linie ist: {mvl.aniCounter} Mal durchgelaufen.");

        mhl.resetAnimation();
        mhl.aniCounter = 0;
        mvl.resetAnimation();
        cartesainHorizontal.SetActive(false);
        cartesainVertikal.SetActive(false);
        firstStarted = false;
        secondStarted = false;
        timeForSelection = false;
    }

    public void reset()
    {
        firstStarted = false;
        secondStarted = false;
        timeForSelection = false;
        mhl.resetAnimation();
        mhl.aniCounter = 0;
        mvl.resetAnimation();
        mvl.aniCounter = 0;
        cartesainHorizontal.SetActive(false);
        cartesainVertikal.SetActive(false);

        SoundHandler.instance.playSound(noSelection, transform, 1f);
    }

    // Berechnung des Schnittpunkts der Linien
    void FindIntersection()
    {

        // Positionen der Linien im Canvas Space
        Vector2 horizontalLinePosition = horizontalLine.anchoredPosition;
        Vector2 verticalLinePosition = verticalLine.anchoredPosition;

        // Berechnung Schnittpunkts Canvas Space
        Vector2 intersectionInCanvasSpace = new Vector2(
            verticalLinePosition.x, 
            horizontalLinePosition.y 
        );

        // Umwandlung in Weltkoordinaten
        Vector3 intersectionWorldPosition = horizontalLine.transform.parent.TransformPoint(intersectionInCanvasSpace);


        // Selection-Mode 
        if (!navModeActive)
        {
            if (CheckForInteractiveElement(intersectionWorldPosition))
            {
                Debug.Log("Ein Interaktives Element wurde gefunden!");
                Debug.Log($"Intersection in Canvas Space: {intersectionInCanvasSpace}");
                SoundHandler.instance.playSound(itemClicked, transform, 1f);
            }
            else
            {
                Debug.Log("Leere Eingabe - kein Interaktives Objekt getroffen.");
                SoundHandler.instance.playSound(noItemHitted, transform, 1f);
            }
        }
        // Navigations-Mode
        else
        {
            Debug.Log("Navigation");
            if (modeNav.turn(intersectionWorldPosition))
            {
                SoundHandler.instance.playSound(itemClicked, transform, 1f);
            }
            else
            {
                SoundHandler.instance.playSound(noItemHitted, transform, 1f);
            }
        }
    }

    // Prüfen, ob mit dem Schnittpunkt ein interaktives Element getroffen wurde 
    bool CheckForInteractiveElement(Vector3 intersection)
    {

        interactiveElement = null;
        
        // Bildschirmposition des Schnittpunkts 
        Vector3 screenPoint = cam.WorldToScreenPoint(intersection);

        // Ray von der Kamera durch die Bildschirmposition des Schnittpunkts
        Ray ray = cam.ScreenPointToRay(screenPoint);
        
        float radius = 0.5f;

        //Debug.DrawLine(ray.origin, ray.origin + ray.direction * 1000, Color.green, 20f);
        //Debug.DrawRay(ray.origin, Vector3.up * radius, Color.blue, 20f);

        //  Raycast durchführen
        if (Physics.SphereCast(ray, radius, out RaycastHit hit, Mathf.Infinity))
        {

            GameObject hittedObject = hit.collider.gameObject;

            if (hittedObject.name == "InteractionSphere")
            {
                return false;
            }

            GameObject hittedParent = hittedObject.transform.parent.gameObject;
            Debug.Log($"Getroffenes Element: {hittedParent.name}");

            if (hittedParent != null)
            {
                // Prüfen, ob das getroffene Objekt interaktiv ist
                if (hittedParent.CompareTag("Interactive"))
                {
                    interactiveElement = hittedParent;
                    DefaultActiveObject();
                    return true;
                }

                if (hittedParent.CompareTag("HUD") || hittedParent.CompareTag("Hint") || hittedParent.CompareTag("Verify") || hittedParent.CompareTag("ImageBack"))
                {
                    interactiveElement = hittedParent;
                    HUDActiveObject();
                    return true;
                }

                if (hittedParent.CompareTag("Menu"))
                {
                    interactiveElement = hittedParent;
                    MenuActiveObject();
                    return true;
                }
            }

        }
        else
        {

            return false;
        }
        return false;
    }

    public void DefaultActiveObject()
    {

        if (interactiveElement != null)
        {
            GameObject canvas = interactiveElement.transform.Find("Canvas_trigger").gameObject;
            GameObject but = canvas.transform.Find("Btn_Trigger").gameObject;
            Button buttonToClick = but.GetComponent<Button>();
            ExecuteEvents.Execute(buttonToClick.gameObject, new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);
            //but.GetComponent<Button>().onClick.Invoke();
            interactiveElement = null;
        }
    }

    public void HUDActiveObject()
    {
        Button buttonToClick = interactiveElement.GetComponent<Button>();
        ExecuteEvents.Execute(buttonToClick.gameObject, new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);
        //interactiveElement.GetComponent<Button>().onClick.Invoke();
        interactiveElement = null;
    }

    public void MenuActiveObject()
    {
        GameObject but = interactiveElement.transform.Find("Btn_Trigger").gameObject;
        Button buttonToClick = but.GetComponent<Button>();
        ExecuteEvents.Execute(buttonToClick.gameObject, new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);
        //but.GetComponent<Button>().onClick.Invoke();
        interactiveElement = null;
    }

}
