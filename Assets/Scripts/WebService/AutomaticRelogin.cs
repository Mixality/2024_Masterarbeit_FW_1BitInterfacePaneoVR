using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI.WebService;

namespace WebService
{
    public class AutomaticRelogin : MonoBehaviour
    {
        public UniqueIdentifierManager identifier;

        internal UnityEvent OnTokenRefreshed = new UnityEvent();
        internal UnityEvent OnTokenRefreshFailed  = new UnityEvent();
        
        internal IEnumerator RefreshToken()
        {
            string url = PlayerPrefs.GetString("serverRoot") + "/api/login_unity.php?refresh=1";
            WWWForm form = new WWWForm();
            form.AddField("refresh_token", PlayerPrefs.GetString("refresh_token"));
            form.AddField("client_id", identifier.GetClientID());
            
            using (UnityWebRequest www = UnityWebRequest.Post(url, form))
            {
                yield return www.SendWebRequest();
        
                if (www.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.Log("Token refresh failed");
                    OnTokenRefreshFailed.Invoke();
                }
                else
                {
                    if (www.responseCode == 401)
                    {
                        Debug.Log("token expired");
                        OnTokenRefreshFailed.Invoke();
                    }
                    else if (www.responseCode == 404)
                    {
                        OnTokenRefreshFailed.Invoke();
                    }
                    else
                    {
                        string jsonResponse = www.downloadHandler.text;
                        PlayerPrefs.SetString("web_auth_token", JsonUtility.FromJson<LoginData>(jsonResponse).web_auth_token);
                        OnTokenRefreshed.Invoke();
                    }
                }
            }
        }
    }
}