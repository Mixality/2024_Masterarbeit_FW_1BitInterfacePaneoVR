using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using DefaultNamespace;
using LightBuzz.Archiver;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI.WebService;
using WebService;

namespace UnityEngine.UI.SaveSystem
{
    public class SceneUpdater : MonoBehaviour
    {
        internal UnityEvent OnSceneUploadSuccess = new UnityEvent();
        internal UnityEvent<string> OnSceneUploadError = new UnityEvent<string>();

        private AutomaticRelogin accessTokenUpdater;
        private void Awake()
        {
            accessTokenUpdater = FindObjectOfType<AutomaticRelogin>();
            if (!accessTokenUpdater)
            {
                Debug.LogError("SceneUpdater: no access token manager found");
            }
        }

        internal void remoteUploadSceneData(SceneData scene, ProjectData project)
        {
            StartCoroutine(UploadSceneData(scene, project));
        }

        private IEnumerator UploadSceneData(SceneData scene, ProjectData project)
        {
            string userid = PlayerPrefs.GetString("userid");
            string jwt = PlayerPrefs.GetString("web_auth_token");
            string url = PlayerPrefs.GetString("serverRoot") + "/api/uploadscenedata.php";

            // Erstelle ein WWWForm-Objekt und füge die POST-Daten hinzu
            WWWForm form = new WWWForm();
            form.AddField("web_auth_token", jwt);
            form.AddField("clientId", PlayerPrefs.GetString("client_id"));
            form.AddField("userid", userid);
            form.AddField("scenedata", JsonUtility.ToJson(scene));
            form.AddField("projectSrc", project.projectSrc);
            form.AddField("timestamp", project.lastLocalSave.ToString());
            
            yield return accessTokenUpdater.RefreshToken();
            
            using (UnityWebRequest www = UnityWebRequest.Post(url, form))
            {
                yield return www.Send();
                if (www.isNetworkError || www.isHttpError)
                {
                    Debug.Log(www.error);
                    Debug.Log("scene upload not possible on: " + url);
                    OnSceneUploadError.Invoke(www.error);
                }
                else
                {
                    Debug.Log(www.downloadHandler.text);
                    OnSceneUploadSuccess.Invoke();
                }
            }
        }
    }
}