using System;
using System.Collections;
using System.Reflection;
using Network;
using RenderHeads.Media.AVProVideo;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Video;
using WebService;

namespace UnityEngine.UI.SaveSystem
{
    public class UIController : MonoBehaviour
    {
        public GameObject ProjectMenu;
        public ProjectList ProjectList;
        public GameObject UserMenu;
        public ServerConfigurator ServerConfigMenu;
        public GameObject ErrorPanel;
        public Text ErrorMsg;
        public TextMeshProUGUI LicenseLabel;
        
        //Tab Bar TODO: with increasing btn amount switch to an array solution for better activation handling
        public Button btnScnenarioTab;
        public Button btnSessionTab;
        public Button btnSessionTabDisabled;
        public GameObject panelScenarioTab;
        public GameObject panelSpectatorTab;

        public InputFieldPanel inputFieldPanel;
        public VerifyPanel VerifyPanel;

        public InputField inputField;
        public GameObject virtualKeyboard;

        //download information fields
        public Text labelProcessType;
        public Text labelProjectName;
        public Text labelType;
        public Text labelAmount;
        public GameObject ActiveDownloadPanel;
        
        //wifistates
        public GameObject wifiAndInternet;
        public GameObject wifiNoInternet;
        public GameObject noWifi;
        public GameObject wifiNoInfo;
        public GameObject logoutWarningPanel;

        public Button authLogoutButton;
        
        public Toggle switchStreamingQualityBtn;
        public Button closeAppBtn;
        
        private NetworkController networkController;

        private ProjectUpdater projectUpdater;
        
        private void Awake()
        {
            networkController = FindObjectOfType<NetworkController>();
        }

        private void Start()
        {
            StartCoroutine(WaitForVideoPlayback());
#if UNITY_STANDALONE_WIN_DESKTOP && !UNITY_EDITOR
            closeAppBtn.gameObject.SetActive(true);
#elif UNITY_WEBGL
            
#endif
            projectUpdater = FindObjectOfType<ProjectUpdater>();
            if (!projectUpdater)
            {
                Debug.LogError("UI Controller: no projectUpdater found");
            }
            projectUpdater.OnUpdateDownloadMediaType.AddListener(setLabelMediaType);
            projectUpdater.OnUpdateDownloadMediaAmount.AddListener(setLabelMediaAmount);
            projectUpdater.OnUpdateProcessType.AddListener(setLabelProcessType);
            projectUpdater.OnUpdateLoadingProjectName.AddListener(setLabelProjectName);
            projectUpdater.OnDownloadError.AddListener(showError);
            projectUpdater.OnDownloadStarted.AddListener(showDownloadStatePanel);
            projectUpdater.OnDownloadFinished.AddListener(hideDownloadStatePanel);
        }

        IEnumerator WaitForVideoPlayback()
        {
            MediaPlayer mediaPlayer = FindObjectOfType<MediaPlayer>();
            if (mediaPlayer)
            {
                while (!mediaPlayer.Control.IsPlaying())
                {
                    yield return new WaitForSeconds(0.5f);
                }
                yield return new WaitForSeconds(0.5f);
                networkController.FinishSceneTransition();
            }
            else
            {
                //networkController.FinishSceneTransition();
            }

            yield return null;
        }

        internal void SetLicenseLabel(string label)
        {
            if (LicenseLabel != null)
            {
                LicenseLabel.text = label;
            }
        }
        
        public void EnableSpectatorTab()
        {
            btnSessionTabDisabled.gameObject.SetActive(false);
            btnSessionTab.gameObject.SetActive(true);
        }
        
        public void DisableSpectatorTab()
        {
            btnSessionTabDisabled.gameObject.SetActive(true);
            btnSessionTab.gameObject.SetActive(false);
        }
        
        public void SwitchToScenarioExplorerTab()
        {
            btnScnenarioTab.interactable = false;
            btnSessionTab.interactable = true;
            panelScenarioTab.SetActive(true);
            panelSpectatorTab.SetActive(false);
        }

        public void SwitchToOpenSessionsTab()
        {
            btnScnenarioTab.interactable = true;
            btnSessionTab.interactable = false;
            panelScenarioTab.SetActive(false);
            panelSpectatorTab.SetActive(true);
        }

        public void setProjectMenuVisibility(bool toggle)
        {
            ProjectMenu.SetActive(toggle);
            UserMenu.SetActive(toggle);
        }

        public void toggleProjectMenu()
        {
            ProjectMenu.SetActive(!ProjectMenu.activeSelf);
            UserMenu.SetActive(!UserMenu.activeSelf);
        }
        
        public void toggleServerConfigMenu()
        {
            if (PlayerPrefs.HasKey("serverRoot"))
            {
                ServerConfigMenu.InputField.text = PlayerPrefs.GetString("serverRoot");
            }
            ServerConfigMenu.gameObject.SetActive(!ServerConfigMenu.gameObject.activeSelf);
        }

        public void showError(string msg)
        {
            ErrorPanel.SetActive(true);
            ErrorMsg.text = msg;
        }
        
        public void showLogoutWarning()
        {
            logoutWarningPanel.SetActive(true);
        }

        private void setLabelProjectName(string text)
        {
            labelProjectName.text = text;
        }

        private void setLabelProcessType(string text)
        {
            labelProcessType.text = text;
        }
        
        private void setLabelMediaType(string text)
        {
            labelType.text = text;
        }

        private void setLabelMediaAmount(string text)
        {
            labelAmount.text = text;
        }
        
        private void showDownloadStatePanel()
        {
            toggleDownloadStatePanel(true);
        }
        private void hideDownloadStatePanel()
        {
            toggleDownloadStatePanel(false);
        }
        public void toggleDownloadStatePanel(bool toggle)
        {
            authLogoutButton.interactable = !toggle;
            ActiveDownloadPanel.SetActive(toggle);
        }

        /// <summary>
        /// Opens an Input that calls a given method [like createNewProject(string name)] on submit and passes input text
        /// Will be hidden automatically after submitting
        /// </summary>
        internal void openGenericInputField(UnityAction<string> methodToCallOnSubmit, bool isInputForQuestionText = false)
        {
            virtualKeyboard.SetActive(true);
            if (isInputForQuestionText)
            {
                inputFieldPanel.prepareForSetQuestion();
            }
            else
            {
                inputFieldPanel.prepareForSetGeneric();
            }
            inputField.text = "";
            inputFieldPanel.gameObject.SetActive(true);
            inputFieldPanel.OnSubmitInput.RemoveAllListeners();
            inputFieldPanel.OnSubmitInput.AddListener(methodToCallOnSubmit);
        }


        /// <summary>
        /// Opens a Yes/No Input that shows a questions and calls a given method [like createNewProject(string name)] on clicking "Yes"
        /// If theres a control sigh for noIsTrue, then clicking on "No" will submit with true instead of clicking "Yes"
        /// </summary>
        internal void openGenericVerifyField(UnityAction<bool> methodToCallOnSubmit, string question)
        {
            VerifyPanel.gameObject.SetActive(true);

            VerifyPanel.question.text = question; 
            
            VerifyPanel.OnSubmit.RemoveAllListeners();
            VerifyPanel.OnSubmit.AddListener(methodToCallOnSubmit);
        }
        
        private void OnDisable()
        {
            inputFieldPanel.OnSubmitInput.RemoveAllListeners();
        }

        internal void switchServerReachability(int state)
        {
            switch (state)
            {
                case 0:
                    wifiAndInternet.SetActive(false);
                    noWifi.SetActive(true);
                    wifiNoInfo.SetActive(false);
                    break;
                case 1:
                    wifiAndInternet.SetActive(true);
                    noWifi.SetActive(false);
                    wifiNoInfo.SetActive(false);
                    break;
                default:
                    wifiAndInternet.SetActive(false);
                    noWifi.SetActive(false);
                    wifiNoInfo.SetActive(true);
                    break;
            }
        }

        public void CloseApp()
        {
            Application.Quit();
        }
    }
}