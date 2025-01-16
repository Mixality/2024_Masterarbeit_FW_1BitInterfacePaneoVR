using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Networking;
using UnityEngine.UI.SaveSystem;

namespace WebService
{
    public class NetworkStateUpdater : MonoBehaviour
    {
        internal bool internetConnection;

        public UIController uiController;
        
        private void Start()
        {
            uiController.switchServerReachability(-1);
            checkServerConnection();
        }
        
        public void checkServerConnection()
        {
            StartCoroutine(checkServerConnection((isConnected)=>{
                if (isConnected)
                {
                    internetConnection = true;
                    uiController.switchServerReachability(1);
                }
                else
                {
                    internetConnection = false;
                    uiController.switchServerReachability(0);
                }
            }));
        }

        private IEnumerator checkServerConnection(Action<bool> action){
            string url = PlayerPrefs.GetString("serverRoot");

            yield return new WaitForSecondsRealtime (2f);
            while (true)
            {
                using (UnityWebRequest www = UnityWebRequest.Get(url))
                {
                    yield return www.SendWebRequest();
                
                    if (www.isNetworkError || www.isHttpError)
                    {
                        action (false);
                    }
                    else
                    {
                        action (true);
                    }
                }
                yield return new WaitForSecondsRealtime (10f);
            }
        }
    }
}