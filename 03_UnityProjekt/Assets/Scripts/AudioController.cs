using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI.SaveSystem;
using UnityEngine.UI.WebService;

namespace DefaultNamespace
{
    public class AudioController : MonoBehaviour
    {
        private ProjectController projectController;
        private void Start()
        {
            projectController = FindObjectOfType<ProjectController>();
            if (!projectController)
            {
                Debug.LogError("Error in AudioController: No project controller found in scene");
            }
        }

        internal void LoadAudioClip(AudioSource audioSource, string audioSrc, string volume)
        {
            StartCoroutine(GetStreamingAccess(audioSource, audioSrc, volume));
        }
        
        internal IEnumerator GetStreamingAccess(AudioSource audioSource, string audioSrc, string volume)
        {
            string userid = PlayerPrefs.GetString("userid");
            string jwt = PlayerPrefs.GetString("web_auth_token");
            string url = PlayerPrefs.GetString("serverRoot") + "/api/getstreamingaccess.php";

            // Erstelle ein WWWForm-Objekt und füge die POST-Daten hinzu
            WWWForm form = new WWWForm();
            form.AddField("web_auth_token", jwt);
            form.AddField("userid", userid);
            form.AddField("mediaSrc", audioSrc);
            form.AddField("projectSrc", projectController.activeProject.projectSrc);
            form.AddField("type", "audio");
            
            using (UnityWebRequest www = UnityWebRequest.Post(url, form))
            {
                yield return www.SendWebRequest();
                if (www.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.Log("No access granted: " + www.error);
                }
                else
                {
                    StartCoroutine(LoadClip(www.downloadHandler.text, audioSource, audioSrc, volume));
                }
            }
        }
        
        IEnumerator LoadClip(string streamingtoken, AudioSource audioSource, string audioSrc, string volume)
        {
            string path;
            UnityWebRequest www;
            
            string audioAccessType = PlayerPrefs.GetString("mediaAccessType");

            if (audioAccessType.Equals("streaming"))
            {
                string userid = PlayerPrefs.GetString("userid");
                path = PlayerPrefs.GetString("serverRoot") + "/api/getstreamingmedia?src=" + audioSrc +
                       "&type=audio&token=" + streamingtoken + "&userid=" + userid;
            }
            else
            {
                path = "file:///" + AssetLoader.getAudioFileFromSrc(audioSrc);
            }

            www = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.UNKNOWN);
            List<MediaReference> audioRefs = JsonUtility.FromJson<MediaRefTable>(PlayerPrefs.GetString(projectController.activeProject.projectSrc)).audioRefs;
            foreach (var audioRef in audioRefs)
            {
                if (audioRef.src.Equals(audioSrc))
                {
                    switch (Path.GetExtension(audioRef.name).ToLower())
                    {
                        case ".mp3":
                            www = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.MPEG);
                            break;
                        case ".wav":
                            www = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.WAV);
                            break;
                        case ".ogg":
                            www = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.OGGVORBIS);
                            break;
                        default:
                            www = UnityWebRequestMultimedia.GetAudioClip(path, AudioType.UNKNOWN);
                            break;
                    }

                    break;
                }
            }

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log(www.error);
            }
            else
            {
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                audioSource.clip = clip;
                try
                {
                    audioSource.volume = Single.Parse(volume);
                }
                catch (FormatException e)
                {
                    audioSource.volume = 1.0f;
                }
                audioSource.Play();
            }
        }
    }
}