using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI.SaveSystem;
using UnityEngine.Video;
using UnityEngine.XR.Interaction.Toolkit;

public abstract class Base360Interactable : XRBaseInteractable, IPointerClickHandler
{
    internal string id;
    internal string questionOnFail { get; set; }
    private GameObject attachingController = null;
    private GameObject TriggerContextMenu;
    private bool isAttached = false;
    private int layerSphereRaycast = 1 << 8;

    private Coroutine timer = null;
    private bool isAttachedOnPointer = false;
    private bool isPointerClicking;
    private bool pointerDown;
    
    internal bool isAtSpawnPoint = false;
    internal EditModeController editModeController;
    internal SaveController saveController;
    internal List<TargetScene> targetScenes = new List<TargetScene>();

    internal abstract int type { get; set; }

    //only for multiple choice interactables
    public bool showOptionsOnEnable { get; set; }

    //needed for parenting and reparenting for resizing with the interaction sphere
    internal GameObject InteractionSphere;

    //webapp only data stored here, since sceneScanner creates new SceneData Interactables when saving
    internal float chartPosX, chartPosY;

    protected override void Awake()
    {
        editModeController = GameObject.FindGameObjectWithTag("EditModeController").GetComponent<EditModeController>();
        saveController = GameObject.FindGameObjectWithTag("SceneManager").GetComponent<SaveController>();
        base.Awake();
    }

    // Start is called before the first frame update
    protected void Start()
    {
        if (questionOnFail == null || !questionOnFail.Any())
        {
            questionOnFail = "Keine konfigurierte Frage gefunden. Zum Szenario zurückkehren?";
        }
        GetComponent<Renderer>().material.color = Color.white;
        if (!editModeController)
        {
            Debug.LogError("no editModeController in scene");
        }
        if (!saveController)
        {
            Debug.LogError("no SaveController in scene");
        }
        TriggerContextMenu = GameObject.FindGameObjectWithTag("TriggerContextMenu");
    }

    // Update is called once per frame
    protected void Update()
    {
        if (isAttached)
        {
            snapToInteractionSphere(attachingController);
        }
        else if (isAttachedOnPointer)
        {
            snapToInteractionSphereByMouse();
        }
    }

    private void snapToInteractionSphere(GameObject Controller)
    {
        RaycastHit raycastHit;
        if (Physics.Raycast(Controller.transform.position, Controller.transform.forward, out raycastHit, Mathf.Infinity, layerSphereRaycast))
        {
            gameObject.transform.position = raycastHit.point;
            GameObject o;
            (o = gameObject).transform.LookAt(2* transform.position - InteractionSphere.transform.position);
            float scale = InteractionSphere.transform.localScale.x * 0.0008f;
            o.transform.localScale = new Vector3(scale,scale,scale);
        }
    }

    private void snapToInteractionSphereByMouse()
    {
        RaycastHit raycastHit;
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out raycastHit, Mathf.Infinity, layerSphereRaycast))
        {
            gameObject.transform.position = raycastHit.point;
            GameObject o;
            (o = gameObject).transform.LookAt(2* transform.position - InteractionSphere.transform.position);
            float scale = InteractionSphere.transform.localScale.x * 0.0008f;
            o.transform.localScale = new Vector3(scale,scale,scale);
        }
    }

    internal void OnGrabbedStart(GameObject Controller)
    {
        if (editModeController.isEditModeActive)
        {
            attachToController(Controller);
        }
        else
        {
            Debug.Log("do something in play mode");
        }
    }

    internal void OnGrabbedEnd(GameObject Controller)
    {
        detachFromController();
    }

    public void OnPointerDown(BaseEventData eventData)
    {
#if UNITY_WEBGL || UNITY_STANDALONE_WIN_DESKTOP
        if (!pointerDown)
        {
            pointerDown = true;
            editModeController.toggleScreenCameraRotation(false);
            timer = StartCoroutine(ClickTimer());
        }
#endif
    }

    IEnumerator ClickTimer()
    {
#if UNITY_WEBGL || UNITY_STANDALONE_WIN_DESKTOP
        isPointerClicking = true;
        
        if (editModeController.isEditModeActive)
        {
            yield return new WaitForSeconds(0.5f);
            isPointerClicking = false;
            attachToPointer();
        }
#else
        yield return null;
#endif
    }
    
    public void OnPointerUp(BaseEventData eventData)
    {
        PointerEventData pointerEventData = eventData as PointerEventData;
        pointerDown = false;
#if UNITY_WEBGL || UNITY_STANDALONE_WIN_DESKTOP
        if (timer != null)
        {
            StopCoroutine(timer);
        }
#else
        isPointerClicking = true;
#endif
        if (isPointerClicking)
        {
            if (pointerEventData.button == PointerEventData.InputButton.Right)
            {
                
            }
            else
            {
                isAttachedOnPointer = false;
                OnClicked();
            }
        }
        else
        {
            isAttachedOnPointer = false;
            detachFromController();
        }
#if UNITY_WEBGL || UNITY_STANDALONE_WIN_DESKTOP
        editModeController.toggleScreenCameraRotation(true);
#endif
    }
    
    public virtual void OnClicked()
    {
        //do specific operations in sub classes
    }

    /// <summary>
    /// a target scene name can be set as a sys trigger message for triggering events like "scenario failed"
    /// </summary>
    protected bool checkForSysMessage(TargetScene scene)
    {
        if (scene.targetSceneName == null)
        {
            return false;
        }

        if (scene.targetSceneName.Equals(SaveController.FAIL_SCENE_TRIGGER) || scene.targetSceneName.Equals(SaveController.SUCCESS_SCENE_TRIGGER))
        {
            saveController.handleSysTrigger(scene, this);
            return true;
        }

        return false;
    }
    
    internal void OnHoveredStart(GameObject Controller)
    {
        //just for testing purposes in editor
        /*OnGrabbedStart(Controller);
        Invoke("detachFromController", 2);*/

        GetComponent<Renderer>().material.color = Color.red;
    }

    internal void OnHoveredEnd(GameObject Controller)
    {
        GetComponent<Renderer>().material.color = Color.white;
    }
    
    /// <summary>
    /// Attaches Base360Interactable to given GameObject (usually a controller).
    /// </summary>
    internal void attachToController(GameObject Controller)
    {
        if (Controller.GetComponentsInChildren<Base360Interactable>().Length <= 0)
        {
            editModeController.setStateToUnsaved();
            gameObject.transform.parent = Controller.transform;
            attachingController = Controller;
            isAttached = true;
        }
    }

    /// <summary>
    /// Attaches Base360Interactable to scene root and activates translation by 2D pointer (usually mouse).
    /// </summary>
    internal void attachToPointer()
    {
        editModeController.setStateToUnsaved();
        gameObject.transform.parent = null;
        attachingController = null;
        isAttachedOnPointer = true;
    }

    internal void detachFromController()
    {
        gameObject.transform.parent = InteractionSphere.transform;
        attachingController = null;
        isAttached = false;
    }

    internal void setQuestionOnFail(string question)
    {
        questionOnFail = question;
    }
    
    internal abstract void setTargetScene(string sceneName);
    
    internal abstract void setTargetLabel(string labelName);

    internal abstract void setTargetHiddenData(string data);
    
    internal abstract void updateGUI();
    public void OnPointerClick(PointerEventData eventData)
    {
        
    }

    public void showInteractable()
    {
        gameObject.GetComponentInChildren<Canvas>().enabled = true;
    }

    public void hideInteractable()
    {
        gameObject.GetComponentInChildren<Canvas>().enabled = false;
    }
}
