using System;
using System.Collections;
using System.Collections.Generic;
using Network;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.UI.SaveSystem;
using Utilities;

namespace DefaultNamespace
{
    public class IngameUIController : MonoBehaviour
    {
        public GameObject IngameMenu;
        public Text IngameMenuTitle;
        
        public GameObject sceneEditUI;

        public GameObject EditModeSelectionPanel;
        public Button EditModeSaveButton;
        public GameObject EditModeSaveBtnIconSave;
        public GameObject EditModeSaveBtnIconProgress;
        
        public SceneList SceneJumper;

        internal NetworkController networkController;

        internal ProjectController projectController;

        public GameObject directionArrow;
        
        public Text sceneNameInput;
        public Text videoFileNameInput;
        public Toggle isVideoLooping;
        public Toggle showInteractablesDelayed;
        
        public Slider interactSphereSizeSlider;
        public Slider xrRigRotationSlider;
        public GameObject initialSceneTag;
        
        private InteractionSphere360 interactionSphere;
        
        public GameObject warningFold;
        public GameObject successFold;

        public Canvas MessagesCanvas;
        public HintPanel HintPanel;
        public VerifyPanel VerifyPanel;
        
        public Canvas ImageBoxCanvas;
        public Canvas ImageBoxBtnCanvas;
        public Image ImageBox;

        //added by me 
        public FindAndHighlightButtons findAndHighlightButtons;
        public SwitchHandler switchHandler;

        private void Start()
        {
            projectController = FindObjectOfType<ProjectController>();
            networkController = FindObjectOfType<NetworkController>();
            interactionSphere = FindObjectOfType<InactivityRefHandler>().interactionSphere360;
            IngameMenuTitle.text = networkController.activeHostedProjectName;
            //added by me 
            GameObject fAHBGM = GameObject.Find("FindInteractablesScript");
            findAndHighlightButtons = fAHBGM.GetComponent<FindAndHighlightButtons>();
            switchHandler = GameObject.Find("SwitchHandler").GetComponent<SwitchHandler>();
        }

        public void toggleIngameMenu()
        {
            //original line 
            //IngameMenu.SetActive(!IngameMenu.gameObject.activeSelf);

            //edited by me 
            if (!IngameMenu.gameObject.activeSelf)
            {
                IngameMenu.gameObject.SetActive(true);
                if(switchHandler.ScanningMethod == 0)
                {
                    findAndHighlightButtons.StartScanningInGameMenu();
                }             
            }
            else
            {
                IngameMenu.gameObject.SetActive(false);
            }
            
        }
        
        /// <summary>
        /// Opens a Yes/No Input that shows a questions and calls a given method [like createNewProject(string name)] on clicking "Yes"
        /// If theres a control sigh for noIsTrue, then clicking on "No" will submit with true instead of clicking "Yes"
        /// </summary>
        internal void openGenericVerifyField(UnityAction<bool> methodToCallOnSubmit, string question)
        {
            bool showReturnBtn;
            bool showMenuBtn;
            
            string[] parts = question.Split(new char[] { '$' }, 2); //split only on first $ sign
            string pureQuestion = parts[1];
            string buttonOptions = parts[0];

            switch (buttonOptions)
            {
                case "x":
                    showReturnBtn = false;
                    showMenuBtn = false;
                    break;
                case "r":
                    showReturnBtn = true;
                    showMenuBtn = false;
                    break;
                case "m":
                    showReturnBtn = false;
                    showMenuBtn = true;
                    break;
                case "mr":
                    showReturnBtn = true;
                    showMenuBtn = true;
                    break;
                case "rm":
                    showReturnBtn = true;
                    showMenuBtn = true;
                    break;
                default:
                    showReturnBtn = true;
                    showMenuBtn = true;
                    break;
            }
            
            MessagesCanvas.gameObject.SetActive(true);
            VerifyPanel.gameObject.SetActive(true);
            VerifyPanel.question.text = pureQuestion;
            VerifyPanel.buttonBlue.text = "Zurück";
            VerifyPanel.buttonRed.text = "Beenden";
            VerifyPanel.toggleNoButton(showMenuBtn);
            VerifyPanel.toggleYesButton(showReturnBtn);
            
            VerifyPanel.OnSubmit.RemoveAllListeners();
            VerifyPanel.OnSubmit.AddListener(methodToCallOnSubmit);
        }
        
        /// <summary>
        /// Toggles Scene Edit UI for creating/editing projects
        /// </summary>
        /// <param name="toggle">show/hide</param>
        public void toggleSceneEditUI(bool toggle)
        {
            if (projectController.networkController.authoringMode)
            {
                sceneEditUI.SetActive(toggle);
                if (directionArrow)
                {
                    directionArrow.SetActive(toggle);
                }

                if (toggle)
                {
                    SceneJumper.reloadScenes();
                }
            }
            else
            {
                sceneEditUI.SetActive(false);
            }
        }

        internal void setSaveBtnToProgressState()
        {
            EditModeSaveButton.interactable = true;
            EditModeSaveBtnIconProgress.SetActive(true);
            EditModeSaveBtnIconSave.SetActive(false);
        }
        
        internal void setSaveBtnToSavedState()
        {
            EditModeSaveButton.interactable = false;
            EditModeSaveBtnIconProgress.SetActive(false);
            EditModeSaveBtnIconSave.SetActive(true);
        }
        
        internal void setSaveBtnToUnsavedState()
        {
            EditModeSaveButton.interactable = true;
            EditModeSaveBtnIconProgress.SetActive(false);
            EditModeSaveBtnIconSave.SetActive(true);
        }

        public void EndProject()
        {
            interactionSphere.gameObject.SetActive(false);
            networkController.LeaveSession();
        }
        
        internal void hideInteractionSphere()
        {
            //doing it like this keeps neccessary events from interactables enabled but makes them invisible 
            interactionSphere.gameObject.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        }

        internal void showInteractionSphere()
        {
            interactionSphere.gameObject.transform.localScale = Vector3.one * interactionSphere.currentSphereSize;
        }
        
        internal void showWarningFold()
        {
            warningFold.gameObject.SetActive(true);
        }

        internal void hideWarningFold()
        {
            warningFold.gameObject.SetActive(false);
        }
        
        internal void showSuccessFold()
        {
            successFold.gameObject.SetActive(true);
        }

        internal void hideSuccessFold()
        {
            successFold.gameObject.SetActive(false);
        }

        internal void openHintPanel(string hint)
        {
            MessagesCanvas.gameObject.SetActive(true);
            HintPanel.gameObject.SetActive(true);
            HintPanel.HintText.text = hint;

            // added by me 
            if(switchHandler.ScanningMethod == 1)
            {
                GameObject acceptBut = HintPanel.gameObject.transform.Find("AcceptBtn").gameObject;
                GameObject highlight = acceptBut.transform.Find("Highlight").gameObject;
                highlight.GetComponent<MeshRenderer>().enabled = false;
            }
        }

        public void openImageViewPanel(Sprite image)
        {
            UIViewRotator rotator = ImageBoxCanvas.transform.parent.GetComponent<UIViewRotator>();
            if (rotator)
            {
                rotator.hasNewFixedPosition = false;
            }
            if (image)
            {
                ImageBoxCanvas.gameObject.SetActive(true);
                ImageBoxBtnCanvas.gameObject.SetActive(true);
                ImageBox.sprite = image;

                if (switchHandler.ScanningMethod == 1)
                {
                    GameObject backButton = ImageBoxBtnCanvas.gameObject.transform.Find("Button").gameObject;
                    GameObject highlight = backButton.transform.Find("Highlight").gameObject;
                    highlight.GetComponent<MeshRenderer>().enabled = false;
                }
                else
                {
                    findAndHighlightButtons.isImageViewActive = true;
                    findAndHighlightButtons.isScanActive = false;
                }            
            }
            else
            {
                Debug.Log("No image given for opening in ImageViewPanel");
            }
        }

        public void closeImageViewPanel()
        {
            ImageBoxCanvas.gameObject.SetActive(false);
            ImageBoxBtnCanvas.gameObject.SetActive(false);
            UIViewRotator rotator = ImageBoxCanvas.transform.parent.GetComponent<UIViewRotator>();
            findAndHighlightButtons.isImageViewActive = false;
            
            if (rotator)
            {
                rotator.hasNewFixedPosition = false;
            }
        }

        public void ShowSceneCurtain()
        {
            StartCoroutine(showSceneCurtain());
        }
        private IEnumerator showSceneCurtain()
        {
            GameObject sceneCurtain = GameObject.FindWithTag("SceneCurtain");
            if (sceneCurtain)
            {
                CanvasGroup curtainCanvas = sceneCurtain.GetComponent<CanvasGroup>();
                if (curtainCanvas)
                {
                    if (curtainCanvas)
                    {
                        curtainCanvas.alpha = 0;
                        while (curtainCanvas.alpha < 1)
                        {
                            yield return new WaitForFixedUpdate();
                            curtainCanvas.alpha = curtainCanvas.alpha + 0.1f;
                        }
                    }
                }
            }
        }
        
        public void HideSceneCurtain()
        {
            StartCoroutine(hideSceneCurtain());
        }
        private IEnumerator hideSceneCurtain()
        {
            GameObject sceneCurtain = GameObject.FindWithTag("SceneCurtain");
            if (sceneCurtain)
            {
                CanvasGroup curtainCanvas = sceneCurtain.GetComponent<CanvasGroup>();
                if (curtainCanvas && curtainCanvas.alpha > 0)
                {
                    if (curtainCanvas)
                    {
                        curtainCanvas.alpha = 1;
                        while (curtainCanvas.alpha > 0)
                        {
                            yield return new WaitForFixedUpdate();
                            curtainCanvas.alpha = curtainCanvas.alpha - 0.1f;
                        }
                        curtainCanvas.alpha = 0;
                    }
                }
            }
        }
    }
}