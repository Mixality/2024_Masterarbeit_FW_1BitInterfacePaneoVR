using System;
using System.Collections;
using TMPro;
using Unity.Netcode;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UI.SaveSystem;
using UnityEngine.XR.Interaction.Toolkit;
using Utilities;

namespace Network
{
    public class SceneTransitionHandler : MonoBehaviour
    {
        [SerializeField]
        public string SceneNameMenu;
        [SerializeField]
        public string SceneNameMain;
        [SerializeField]
        public string SceneNameMain_2D;
        [SerializeField]
        public string SceneNameMenu_2D;
        
        public bool InitializeAsHost;

        public bool InitAsMutliplayer { get; private set; }
        
        public CanvasGroup LoadingScreen;

        public Text LabelProjectNameToLoad;
        public Text LabelLoadingOperation;

        public XRBaseController LeftHandLoadingController;
        public XRBaseController RightHandLoadingController;

        private static string LOADING_PROJECT_MESSAGE = "Lade Szenario";
        private static string LOADING_MENU_MESSAGE = "Schließe Szenario";

        public void StartScenarioAsHost(string projectSrc, string projectName, bool multiplayer)
        {
            InitAsMutliplayer = multiplayer;
            InitializeAsHost = true;
            LabelProjectNameToLoad.text = projectName;
            LabelLoadingOperation.text = LOADING_PROJECT_MESSAGE;
#if UNITY_WEBGL || UNITY_STANDALONE_WIN_DESKTOP
            StartCoroutine(LoadSceneAsync(SceneNameMain_2D, false));
#else
            StartCoroutine(LoadSceneAsync(SceneNameMain, false));
#endif
        }

        public void StopScenarioAsHost()
        {
            LabelLoadingOperation.text = LOADING_MENU_MESSAGE;
#if UNITY_WEBGL || UNITY_STANDALONE_WIN_DESKTOP
            StartCoroutine(LoadSceneAsync(SceneNameMenu_2D, true));
#else
            StartCoroutine(LoadSceneAsync(SceneNameMenu, true));
#endif
        }
        
        public void StartScenarioAsClient(string projectSrc, string projectName)
        {
            InitializeAsHost = false;
            InitAsMutliplayer = true;
            LabelProjectNameToLoad.text = projectSrc;
            LabelLoadingOperation.text = LOADING_PROJECT_MESSAGE;
#if UNITY_WEBGL || UNITY_STANDALONE_WIN_DESKTOP
            StartCoroutine(LoadSceneAsync(SceneNameMain_2D, false));
#else
            StartCoroutine(LoadSceneAsync(SceneNameMain, false));
#endif
        }

        IEnumerator LoadSceneAsync(string sceneName, bool shutdownNetwork)
        {
            GetComponent<NetworkController>().LastUnityScene = SceneManager.GetActiveScene().name;
            
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

            yield return new WaitForSeconds(0.5f);
            
            ShowLoadingScreen();

#if !UNITY_WEBGL || UNITY_STANDALONE_WIN_DESKTOP
            if (shutdownNetwork)
            {
                NetworkManager.Singleton.Shutdown();
            }
#endif
            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        }
        
        private void ShowLoadingScreen()
        {
            LoadingScreen.alpha = 1;
            LoadingScreen.gameObject.SetActive(true);
            GameObject.FindWithTag("MainXRRig").SetActive(false);
        }

        internal void FinishSceneTransition()
        {
            HideLoadingScreen();
        }
        
        private void HideLoadingScreen()
        {
            StartCoroutine(fadeOut());
        }

        private IEnumerator fadeOut()
        {
            InteractionSphere360 interactionSphere360 = FindObjectOfType<InactivityRefHandler>().interactionSphere360;
            Camera camera = GameObject.FindWithTag("MainXRRig").GetComponentInChildren<Camera>();
            camera.enabled = true;
            GameObject sceneCurtain = null;
            foreach (var transform in camera.GetComponentsInChildren<Transform>())
            {
                if (transform.gameObject.tag.Equals("SceneCurtain"))
                {
                    sceneCurtain = transform.gameObject;
                }
            }

            LoadingScreen.gameObject.SetActive(false);

            if (interactionSphere360)
            {
                interactionSphere360.gameObject.SetActive(true);
            }
            
            if (sceneCurtain)
            {
                CanvasGroup curtainCanvas = sceneCurtain.GetComponent<CanvasGroup>();
                if (curtainCanvas)
                {
                    if (curtainCanvas)
                    {
                        curtainCanvas.alpha = 1;
                        while (curtainCanvas.alpha > 0)
                        {
                            yield return new WaitForFixedUpdate();
                            curtainCanvas.alpha = curtainCanvas.alpha - 0.1f;
                        }
                    }
                }
            }
            
            
            //Camera loadScreenCam = LoadingScreen.GetComponentInChildren<Camera>();
            
            /*LoadingScreen.alpha = 1;
            while (LoadingScreen.alpha > 0)
            {
                yield return new WaitForFixedUpdate();
                LoadingScreen.alpha = LoadingScreen.alpha - 0.1f;
            }*/
            
            //loadScreenCam.gameObject.SetActive(true);
            //LoadingScreen.gameObject.SetActive(false);
            
            yield return null;
        }
    }
}