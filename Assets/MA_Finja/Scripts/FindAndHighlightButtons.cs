using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

public class FindAndHighlightButtons : MonoBehaviour

{
    public VisibleByCamera visibleByCamCheck;

    // TO DO: Prüfen, ob alles noch funktioniert, wenn die protected oder private gestellt sind 

    public List<GameObject> allButtons;
    public List<GameObject> options;
    public bool isScanActive = false;
    public bool isDialogScanActive = false;
    public bool isInGameMenuScanActive = false;
    public bool isNavMenuScanActive = false;
    public bool isHintPanelActive = false;
    public bool isImageViewActive = false;
    public bool isVerifyScanActive = false;
 
    public float ScanRate;

    private HintPanel hintPanel;
    private GameObject Canvas_image_btn;

    private GameObject highlightedObject;

    private Camera cam;

    [SerializeField] private AudioClip startScanning;
    [SerializeField] private AudioClip itemChange;
    [SerializeField] private AudioClip itemClicked;

    public NextElementLine scriptNextElement;

    private float DebugStartTime;
    private int countCycle;

    public void Start()
     {

        GameObject hP = GameObject.Find("HintPanel");
        if (hP != null)
        {
            hintPanel = hP.GetComponent<HintPanel>();

            if (hintPanel != null)
            {
                hP.SetActive(false);                
            }
        }

        Canvas_image_btn = GameObject.Find("Canvas_image_btn");

        if(Canvas_image_btn != null)
        {
            Canvas_image_btn.SetActive(false);
        }


        GameObject MPCam = GameObject.Find("Main MP Camera");
        cam = MPCam.GetComponent<Camera>();

    }

    // Zur Initialisierung des Scannings 
    public void SetScanActive()
    {
        isScanActive = true;
        StartItemScanning();
    }

    // Sucht alle Interaktivien Elemente in der Szene - wird aktuell nicht benötigt! 
    public void SearchInteractablesByTag()
    {
        List<GameObject> foundButtons = GameObject.FindGameObjectsWithTag("InteractableElement").ToList();

        foreach (GameObject btn in foundButtons)
        {
            allButtons.Add(btn);
        }
    }

    public void StartItemScanning()
    {
        StartCoroutine(ItemScanning());
    }

    IEnumerator ItemScanning()
    {
        SoundHandler.instance.playSound(startScanning, transform, 1f);
        DebugStartTime = Time.time;

        countCycle = 1;

        while (isScanActive)
        {

            foreach (GameObject btn in allButtons)
            {

                if (!isScanActive) { break; }

                if (btn.active)
                {

                    bool highlightableBnt = false;
                    GameObject highlight = null;

                    // for the interactables
                    try
                    {

                        highlight = btn.transform.Find("Highlight").gameObject;
                        highlightableBnt = true;
                        
                    }

                    catch (NullReferenceException ex)
                    {
                        highlightableBnt = false;
                    }

                    // for the hud 
                    if (highlight == null)
                    {
                        try
                        {
                            GameObject canvas = btn.transform.Find("Canvas_trigger").gameObject;
                            highlight = canvas.transform.Find("Highlight").gameObject;
                            highlightableBnt = true;
                        }

                        catch (NullReferenceException ex2)
                        {
                            highlightableBnt = false;
                        }
                    }

                    if (highlightableBnt && (btn != null))
                    {
                        Collider highlightCollider = highlight.GetComponent<Collider>();
                        if (highlightCollider != null) 
                        {

                            if(visibleByCamCheck.IsVisibleByCam(cam, highlightCollider))
                            {
                                highlight.GetComponent<MeshRenderer>().enabled = true;
                                highlightedObject = btn;
                                SoundHandler.instance.playSound(itemChange, transform, 1f);

                                int currentPos = allButtons.IndexOf(btn);
                                int nextPos = (currentPos + 1) % allButtons.Count; 

                                GameObject nextObject = null;
                                int iterations = 0; 

                                // Zyklische Suche nach dem nächsten sichtbaren Element
                                while (iterations < allButtons.Count)
                                {
                                    GameObject potentialNextObject = allButtons[nextPos];
                                   

                                    if(potentialNextObject.tag == "InteractableElement")
                                    {
                                        nextObject = potentialNextObject;
                                        break;
                                    }

                                    GameObject nextHighlight = null;

                                    try
                                    {
                                        nextHighlight = potentialNextObject.transform.Find("Highlight").gameObject;
                                    }
                                    catch(NullReferenceException ex)
                                    {
                                        nextHighlight = null;
                                    }
                                    
                                    if(nextHighlight != null)
                                    {
                                        Collider nextCollider = nextHighlight.GetComponent<Collider>();
                                        // Prüfe, ob das Objekt sichtbar ist
                                        if (visibleByCamCheck.IsVisibleByCam(cam, nextCollider))
                                        {
                                            nextObject = potentialNextObject;
                                            break;
                                        }
                                    }
                                   
                                    // Gehe zum nächsten Element (zyklisch)
                                    nextPos = (nextPos + 1) % allButtons.Count;
                                    iterations++;
                                }

                                if (nextObject != null)
                                {
                                    scriptNextElement.DrawLineBetweenElements(cam, btn, nextObject);
                                }

                                
                                yield return new WaitForSeconds(ScanRate);
                                if (highlight != null)
                                {
                                    highlight.GetComponent<MeshRenderer>().enabled = false;
                                }
                            }
                        }
                    }
                }

            }

            countCycle++;
        }

        scriptNextElement.drawLine = false;

    }

    public void SwitchSelection()
    {

        isScanActive = false;
        scriptNextElement.drawLine = false;
        SoundHandler.instance.playSound(itemClicked, transform, 1f);

        float DebugEndTime = Time.time;
        Debug.Log($"Selektionsgeschwindigkeit: {DebugEndTime - DebugStartTime} Sekunden");
        DebugStartTime = Time.time;

        Debug.Log("Anzahl der nötigen Scanningzyklen:" + countCycle);
        countCycle = 1;

        if (isInGameMenuScanActive)
        {
            InGameMenuSwitchSelection();
        }
        else if (isNavMenuScanActive)
        {
            NavMenuSwitchSelection();
        }
        else if (isVerifyScanActive)
        {
            VerifySwitchSelection();
        }
        else if (!isDialogScanActive || highlightedObject == options[0]) 
        {
            DefaultSwitchSelection();
        }
        else if (isDialogScanActive)
        {        
            DialogSwitchSelection();
        } 
        
    }

    public void DefaultSwitchSelection()
    {

        if (highlightedObject != null)
        {
           Debug.Log(highlightedObject.name);
           GameObject canvas = highlightedObject.transform.Find("Canvas_trigger").gameObject;
           GameObject but = canvas.transform.Find("Btn_Trigger").gameObject;

           Button buttonToClick = but.GetComponent<Button>();
           
           ExecuteEvents.Execute(buttonToClick.gameObject, new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);

        }

    }


    public void DialogSwitchSelection()
    {
        GameObject but = highlightedObject;
        
        Button buttonToClick = but.GetComponent<Button>();
        ExecuteEvents.Execute(buttonToClick.gameObject, new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);

        isDialogScanActive = false;
        isScanActive = false;

    }

    public void InGameMenuSwitchSelection()
    {
        GameObject but = highlightedObject;

        Button buttonToClick = but.GetComponent<Button>();
        ExecuteEvents.Execute(buttonToClick.gameObject, new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);  

        isInGameMenuScanActive = false;
    }

    public void NavMenuSwitchSelection()
    {
        try
        {
            GameObject but = highlightedObject;

            Button buttonToClick = but.GetComponent<Button>();
            ExecuteEvents.Execute(buttonToClick.gameObject, new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);

        }
        catch (NullReferenceException ex)
        {
            Debug.Log("Hier ist irgendwas schief gelaufen in der Nav Menu Switch Selection.");
        }
    }

    public void VerifySwitchSelection()
    {
        GameObject but = highlightedObject;
        Button buttonToClick = but.GetComponent<Button>();
        ExecuteEvents.Execute(buttonToClick.gameObject, new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);
        // but.GetComponent<Button>().onClick.Invoke();
    }

    public void EndDialogScan()
    {
        isScanActive = false;
        isDialogScanActive = false;
    }


    public void DialogScan(MultipleChoiceOption[] mcOptions)
    {
        options.Clear();
        
        options.Add(highlightedObject);

        foreach (MultipleChoiceOption mcOption in mcOptions)
        {
            options.Add(mcOption.labelBtn.gameObject);
        }


        isDialogScanActive = true;
        StartCoroutine(ItemScanningDialog());

    }

    IEnumerator ItemScanningDialog()
    {
       
        SoundHandler.instance.playSound(startScanning, transform, 1f);
        DebugStartTime = Time.time;

        countCycle = 1;

        while (isDialogScanActive)
        {

            foreach (GameObject btn in options)
             {
                if (isDialogScanActive)
                {
                    if (btn != null && btn.active)
                    {
                        GameObject highlight = btn.transform.Find("Highlight").gameObject;
                        highlight.GetComponent<MeshRenderer>().enabled = true;
                        highlightedObject = btn;
                        SoundHandler.instance.playSound(itemChange, transform, 1f);

                        int currentPos = options.IndexOf(btn);
                        int nextPos = (currentPos + 1) % options.Count;

                        GameObject nextObject = null;
                        int iterations = 0;

                        // Zyklische Suche nach dem nächsten sichtbaren Element
                        while (iterations < options.Count)
                        {
                            GameObject potentialNextObject = options[nextPos];
                            GameObject nextHighlight = null;

                            try
                            {
                                nextHighlight = potentialNextObject.transform.Find("Highlight").gameObject;
                            }
                            catch (NullReferenceException ex)
                            {
                                nextHighlight = null;
                                Debug.Log("Highlight wird gerade im Dialog-Scan nicht gefunden.");
                            }

                            if (nextHighlight != null)
                            {
                                Collider nextCollider = nextHighlight.GetComponent<Collider>();
                                // Prüfe, ob das Objekt sichtbar ist
                                if (visibleByCamCheck.IsVisibleByCam(cam, nextCollider))
                                {
                                    nextObject = potentialNextObject;
                                    break;
                                }
                            }

                            // Gehe zum nächsten Element (zyklisch)
                            nextPos = (nextPos + 1) % options.Count;
                            iterations++;
                        }

                        if (nextObject != null)
                        {
                            scriptNextElement.DrawLineBetweenElements(cam, btn, nextObject);
                        }

                        yield return new WaitForSeconds(ScanRate);

                        if (btn != null)
                        {
                            highlight.GetComponent<MeshRenderer>().enabled = false;
                        }
                    }
                }
                                        
             }
            countCycle++;
        }

    }
    public void StartScanningInGameMenu()
    {
        isInGameMenuScanActive = true;
        isScanActive = false;
        StartCoroutine(inGameMenuScanning());
    }

    public void StopScanningInGameMenu()
    {
        isInGameMenuScanActive = false;
    }

    public void StartScanningNavMenu()
    {
        isNavMenuScanActive = true;
        StartCoroutine(navMenuScanning());
    }

    public void StopScanningNavMenu()
    {
        isNavMenuScanActive = false;
    }

    public void StopScanningVerify()
    {
        isVerifyScanActive = false;
    }

    public void StartScanningVerify()
    {
        isVerifyScanActive = true;

        isDialogScanActive = false;
        isScanActive = false;

        StartCoroutine(VerifyPanelScanning());
    }

    IEnumerator inGameMenuScanning()
    {
        List<GameObject> hudButtons = GameObject.FindGameObjectsWithTag("HUD").ToList();
        SoundHandler.instance.playSound(startScanning, transform, 1f);
        DebugStartTime = Time.time;

        countCycle = 1;

        while (isInGameMenuScanActive)
        {

            foreach (GameObject btn in hudButtons)
            {
                bool highlightableBnt = false;
                GameObject highlight = null;

                try
                {
                    highlight = btn.transform.Find("Highlight").gameObject;
                    highlightableBnt = true;
                }

                catch (NullReferenceException ex)
                {
                    //Debug.Log("Hehe abgefangen");
                }

                if (highlightableBnt && btn != null)
                {
                    highlight.GetComponent<MeshRenderer>().enabled = true;
                    highlightedObject = btn;
                    SoundHandler.instance.playSound(itemChange, transform, 1f);

                    int currentPos = hudButtons.IndexOf(btn);
                    int nextPos = 0;
                    if (currentPos < hudButtons.Count - 1)
                    {
                        nextPos = currentPos + 1;
                    }

                    GameObject nextObject = hudButtons[nextPos];
                    scriptNextElement.DrawLineBetweenElements(cam, btn, nextObject);

                    yield return new WaitForSeconds(ScanRate);
                    if (highlight != null)
                    {
                        highlight.GetComponent<MeshRenderer>().enabled = false;
                    }

                }

            }
            countCycle++;
        }
    }

    IEnumerator navMenuScanning()
    {
        List<GameObject> navButtons = GameObject.FindGameObjectsWithTag("NAV").ToList();

        StopCoroutine("ItemScanning");
        SoundHandler.instance.playSound(startScanning, transform, 1f);

        DebugStartTime = Time.time;

        countCycle = 1;

        while (isNavMenuScanActive)
        {

            foreach (GameObject btn in navButtons)
            {
                if (!isNavMenuScanActive) { break; }
                
                bool highlightableBnt = false;
                GameObject highlight = null;

                try
                {
                    highlight = btn.transform.Find("Highlight").gameObject;
                    highlightableBnt = true;
                }

                catch (NullReferenceException ex)
                {
                    //Debug.Log("Hehe abgefangen im Nav Scanning");
                }

                if (!highlightableBnt)
                {
                    try
                    {
                        GameObject canvas = btn.transform.parent.gameObject;
                        highlight = canvas.transform.Find("Highlight").gameObject;
                        highlightableBnt = true;                                       
                    }
                    catch (NullReferenceException ex2)
                    {
                        //Debug.Log("Abgefangen2 im Nav Scanning..");
                    }
                }

                if (highlightableBnt && btn != null)
                {
                    highlight.GetComponent<MeshRenderer>().enabled = true;
                    highlightedObject = btn;
                    SoundHandler.instance.playSound(itemChange, transform, 1f);

                    int currentPos = navButtons.IndexOf(btn);
                    int nextPos = 0;
                    if (currentPos < navButtons.Count - 1)
                    {
                        nextPos = currentPos + 1;
                    }

                    GameObject nextObject = navButtons[nextPos];
                    scriptNextElement.DrawLineBetweenElements(cam, btn, nextObject);

                    yield return new WaitForSeconds(ScanRate);
                    if (highlight != null)
                    {
                        highlight.GetComponent<MeshRenderer>().enabled = false;
                    }

                }

            }
            countCycle++;
        }

    }


    IEnumerator VerifyPanelScanning()
    {
        List<GameObject> verifyButtons = GameObject.FindGameObjectsWithTag("Verify").ToList();
        SoundHandler.instance.playSound(startScanning, transform, 1f);

        DebugStartTime = Time.time;

        countCycle = 1;

        while (isVerifyScanActive)
        {

            foreach (GameObject btn in verifyButtons)
            {
                if (btn.active)
                {
                    bool highlightableBnt = false;
                    GameObject highlight = null;

                    try
                    {
                        highlight = btn.transform.Find("Highlight").gameObject;
                        highlightableBnt = true;
                    }

                    catch (NullReferenceException ex)
                    {
                        //Debug.Log("Hehe abgefangen im Verify Scanning");
                    }

                    if (highlightableBnt && btn != null)
                    {
                        highlight.GetComponent<MeshRenderer>().enabled = true;
                        highlightedObject = btn;
                        SoundHandler.instance.playSound(itemChange, transform, 1f);
                        yield return new WaitForSeconds(ScanRate);
                        if (highlight != null)
                        {
                            highlight.GetComponent<MeshRenderer>().enabled = false;
                        }

                    }
                }
            }
            countCycle++;
        }

    }

    public void ActivatedHintPanel()
    {
        isHintPanelActive = true;
        StopCoroutine("ItemScanning");
        isScanActive = false;

        DebugStartTime = Time.time;
    }

    public void SelcetHintPanel()
    {
        hintPanel.CloseHint();
        isHintPanelActive = false;
    }

    public void ActivatedImageCanvas()
    {
        isImageViewActive = true;
        StopCoroutine("ItemScanning");
        isScanActive = false;

        DebugStartTime = Time.time;
    }

    public void SelectImageButton()
    {
        isImageViewActive = false;
        GameObject closeButtonObj = Canvas_image_btn.transform.Find("Button").gameObject;
        Button closeButton = closeButtonObj.GetComponent<Button>();
        ExecuteEvents.Execute(closeButton.gameObject, new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);

    }

    // Wird vermutlich nicht mehr gebraucht 
    /*public void InvokeOnClick()
    {
        buttonToClick.onClick.Invoke();      
        EventSystem.current.SetSelectedGameObject(null);   
        buttonToClick = null;
    }*/


    // Debug-Funktion 
    public void printButtons()
    {
        foreach (GameObject but in allButtons)
        {
            Debug.Log(but.name);
        }
    }

}
