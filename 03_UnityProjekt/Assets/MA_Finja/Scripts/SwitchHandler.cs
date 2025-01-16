using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class SwitchHandler : MonoBehaviour
{
    public InputActionReference switchInputReference = null;

    public float pressThreshold = 2f;
    private float pressStartTime = 0f;
    private bool isPressing = false;
    private bool playedSound = false;

    [Tooltip("Item Scanning = 0, Cartesain = 1")]
    public int ScanningMethod;

    public FindAndHighlightButtons findAndHighlightButtons;
    public CartesianScanHandler cartesianHandler;
    [SerializeField] private AudioClip changeInteractionMode;

    public bool InitialInteraction = false;


    private void Awake()
    {
        //switchInputReference.action.started += SwitchInput;
        switchInputReference.action.started += OnPressStarted;
        switchInputReference.action.canceled += OnPressEnded;

        Debug.Log($"Eingestellte Scanning Methode: {ScanningMethod}");
    }

    private void OnDestroy()
    {
        //switchInputReference.action.started -= SwitchInput;
        switchInputReference.action.started -= OnPressStarted;
        switchInputReference.action.canceled -= OnPressEnded;
    }


    private void Update()
    {
        if (isPressing && !playedSound)
        {
            float pressDuration = Time.time - pressStartTime;

            if (pressDuration >= pressThreshold)
            {
                // Spiel den Ton ab
                SoundHandler.instance.playSound(changeInteractionMode, transform, 2f);
                playedSound = true;
            }
        }
    }

    private void OnPressStarted(InputAction.CallbackContext context)
    {
        // Beginn des Tastendrucks registrieren - nur wirklich wichtig bei Cartesian 
        if (ScanningMethod == 1)
        {
            pressStartTime = Time.time;
            isPressing = true;
            playedSound = false;
        }

    }

    private void OnPressEnded(InputAction.CallbackContext context)
    {

        // O = Item Scanning 
        if (ScanningMethod == 0)
        {

            if (findAndHighlightButtons.isHintPanelActive)
            {
                findAndHighlightButtons.SelcetHintPanel();
                
                InitialInteraction = false;
            }
            else
            {
                if (findAndHighlightButtons.isImageViewActive)
                {
                    findAndHighlightButtons.SelectImageButton();
                }
                else
                {
                    if (findAndHighlightButtons.isVerifyScanActive)
                    {
                        findAndHighlightButtons.SwitchSelection();
                        InitialInteraction = false;
                    }
                    else
                    {
                        if (!InitialInteraction)
                        {
                            findAndHighlightButtons.SetScanActive();
                            InitialInteraction = true;
                        }
                        else
                        {
                            findAndHighlightButtons.SwitchSelection();
                            if (!findAndHighlightButtons.isDialogScanActive && !findAndHighlightButtons.isInGameMenuScanActive && !findAndHighlightButtons.isNavMenuScanActive && !findAndHighlightButtons.isVerifyScanActive)
                            {
                                InitialInteraction = false;
                            }

                        }
                    }
                }
                
            }
        }

        // 1 = Cartesain Scanning - hier ist die Dauer des Drückens wichtig 
        if (ScanningMethod == 1)
        {        
            // Prüfen, wie lange die Taste gedrückt wurde
            float pressDuration = Time.time - pressStartTime;

            if (pressDuration >= pressThreshold)
            {
                cartesianHandler.changeMode();
            }
            else
            {
                cartesianHandler.Scanning();
            }

            isPressing = false;
        }
    }


}
