using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.Events;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Networking;
using UnityEngine.UI.WebService;
using WebService;

namespace UnityEngine.UI.SaveSystem
{
    public class Authenticator : MonoBehaviour
    {
        public InputField inputMail;
        public InputField inputPassword;

        public UnityEvent<string> onLoggedIn;
        public UnityEvent onLoggedOut;
        public UnityEvent onLoginFailed;

        public NetworkStateUpdater networkStateUpdater;
        private WebSocketClient webSocketClient;
        public UIController uiController;
        private UniqueIdentifierManager identifier;
        
        public Toggle storeMailToggle;
        
        public Toggle keepLoggedInToggle;

        private static string STORED_MAIL_KEY = "storedMail";

        private void Awake()
        {
            identifier = FindObjectOfType<UniqueIdentifierManager>();
            if (!identifier)
            {
                Debug.LogError("Authenticator: no UUID manager found");
            }
        }

        private void Start()
        {
            uiController.DisableSpectatorTab();
            if (PlayerPrefs.GetInt("loggedIn") != 0)
            {
                onLoggedIn.Invoke(PlayerPrefs.GetString("username"));
                uiController.SetLicenseLabel(PlayerPrefs.GetString("app_license"));
                StartCoroutine(GetMultiplayerAccess());
            }

            if (PlayerPrefs.HasKey(STORED_MAIL_KEY))
            {
                inputMail.text = PlayerPrefs.GetString(STORED_MAIL_KEY);
                storeMailToggle.isOn = true;
            }

            webSocketClient = FindObjectOfType<WebSocketClient>();
            if (!webSocketClient)
            {
                Debug.LogError("Authenticator: WebSocketClient not found");
            }
        }
        
        internal IEnumerator GetMultiplayerAccess()
        {
#if !UNITY_WEBGL
            string userid = PlayerPrefs.GetString("userid");
            string jwt = PlayerPrefs.GetString("web_auth_token");
            string url = PlayerPrefs.GetString("serverRoot") + "/api/getmultiplayeraccess.php";

            // Erstelle ein WWWForm-Objekt und füge die POST-Daten hinzu
            WWWForm form = new WWWForm();
            form.AddField("web_auth_token", jwt);
            form.AddField("userid", userid);

            using (UnityWebRequest www = UnityWebRequest.Post(url, form))
            {
                yield return www.SendWebRequest();
                if (www.result == UnityWebRequest.Result.ProtocolError)
                {
                    uiController.DisableSpectatorTab();
                }
                else
                {
                    uiController.EnableSpectatorTab();
                }
            }
#else
            uiController.DisableSpectatorTab();
            yield return null;
#endif
        }
        
        public void toggleStoredMail(bool store)
        {
            if (!store)
            {
                PlayerPrefs.DeleteKey(STORED_MAIL_KEY);
            }
        }
        
        private void setStoredMail(string mail)
        {
            PlayerPrefs.SetString(STORED_MAIL_KEY, mail);
        }
        
        public void LoginFired()
        {
            if (storeMailToggle.isOn)
            {
                setStoredMail(inputMail.text);
            }
            StartCoroutine(LogIn(inputMail.text, inputPassword.text));
        }

        private IEnumerator LogIn(string email, string password)
        {
            string url = PlayerPrefs.GetString("serverRoot") + "/api/login_unity.php?login=1";
            bool keepMeLoggedIn = keepLoggedInToggle.isOn;
            
            WWWForm form = new WWWForm();

            form.AddField("email", email);
            form.AddField("password", password);
            form.AddField("client_id", identifier.GetClientID());
            
            using (UnityWebRequest www = UnityWebRequest.Post(url, form))
            {
                yield return www.SendWebRequest();
                
                if (www.isNetworkError || www.isHttpError)
                {
                    Debug.Log(www.error);
                    Debug.Log("Login failed");
                    onLoginFailed.Invoke();
                }
                else
                {
                    string jsonLoginData = www.downloadHandler.text;
                    LoginData loginData;
                    loginData = JsonUtility.FromJson<LoginData>(jsonLoginData);
                    PlayerPrefs.SetInt("loggedIn", 1);
                    PlayerPrefs.SetString("username", loginData.username);
                    PlayerPrefs.SetString("userid", loginData.userid);
                    PlayerPrefs.SetString("web_auth_token", loginData.web_auth_token);
                    PlayerPrefs.SetString("refresh_token", loginData.refresh_token);
                    PlayerPrefs.SetString("app_license", loginData.license);
                    PlayerPrefs.SetString("localization", loginData.localization);
                    uiController.SetLicenseLabel(loginData.license);
                    if (keepMeLoggedIn)
                    {
                        PlayerPrefs.SetInt("keepMeLoggedIn", 1);
                    }
                    else
                    {
                        PlayerPrefs.DeleteKey("keepMeLoggedIn");
                    }

                    onLoggedIn.Invoke(loginData.username);
                    StartCoroutine(GetMultiplayerAccess());
                    webSocketClient.TryConnectToServer();
                    SetLanguageFromPreferences();
                }
            }
        }
        
        public void LogOut()
        {
            StartCoroutine(TryLogout());
        }

        private IEnumerator TryLogout()
        {
            string url = PlayerPrefs.GetString("serverRoot");
            WWWForm form = new WWWForm();
            using (UnityWebRequest www = UnityWebRequest.Post(url, form))
            {
                yield return www.SendWebRequest();
            
                if (www.isNetworkError || www.isHttpError)
                {
                    uiController.showLogoutWarning();
                }
                else
                {
                    webSocketClient.CloseConnection();
                    LogOutConfirmed();
                }
            }
        }

        public void LogOutConfirmed()
        {
            PlayerPrefs.SetInt("loggedIn", 0);
            PlayerPrefs.SetString("username", "");
            PlayerPrefs.SetString("userid", "");
            PlayerPrefs.DeleteKey("web_auth_token");
            PlayerPrefs.DeleteKey("refresh_token");
            PlayerPrefs.DeleteKey("keepMeLoggedIn");
            PlayerPrefs.DeleteKey("app_license");
            PlayerPrefs.DeleteKey("localization");
            if (PlayerPrefs.HasKey(STORED_MAIL_KEY))
            {
                inputMail.text = PlayerPrefs.GetString(STORED_MAIL_KEY);
            }
            uiController.SetLicenseLabel("");
            uiController.DisableSpectatorTab();
            inputPassword.text = "";
            SetLanguageFromPreferences();
            onLoggedOut.Invoke();
        }
        
        private async void SetLanguageFromPreferences()
        {
            string localeString = null;
            /*if (PlayerPrefs.HasKey("localization"))
            {
                string localeKey = PlayerPrefs.GetString("localization", "en_EN");
                switch (localeKey)
                {
                    case "de_DE":
                        localeString = "de";
                        break;
                    case "en_EN":
                        localeString = "en";
                        break;
                }
            }

            if (localeString == null)
            {
                
            }*/

            SystemLanguage systemLanguage = Application.systemLanguage;
            switch (systemLanguage)
            {
                case SystemLanguage.German:
                    localeString = "en";
                    break;
                case SystemLanguage.English:
                default:
                    localeString = "en";
                    break;
            }
            
            while (LocalizationSettings.InitializationOperation.IsDone == false)
            {
                await Task.Delay(100);
            }
            
            Locale selectedLocale;
            selectedLocale = LocalizationSettings.AvailableLocales.GetLocale(localeString);
            LocalizationSettings.SelectedLocale = selectedLocale;
        }
    }
}