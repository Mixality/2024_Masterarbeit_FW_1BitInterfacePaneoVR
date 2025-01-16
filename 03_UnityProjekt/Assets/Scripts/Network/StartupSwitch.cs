using System;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;
using WebService;

namespace Network
{
    public class StartupSwitch : MonoBehaviour
    {
        public AutomaticRelogin automaticRelogin;
        private void Start()
        {
            if (PlayerPrefs.HasKey("keepMeLoggedIn") && PlayerPrefs.HasKey("refresh_token"))
            {
                automaticRelogin.OnTokenRefreshed.AddListener(SwitchScenes);
                automaticRelogin.OnTokenRefreshFailed.AddListener(resetUserAndSwitch);
                StartCoroutine(automaticRelogin.RefreshToken());
            }
            else
            {
                resetUserAndSwitch();
            }
        }

        private void resetUserAndSwitch()
        {
            PlayerPrefs.SetInt("loggedIn", 0);
            PlayerPrefs.SetString("username", "");
            PlayerPrefs.SetString("userid", "");
            PlayerPrefs.DeleteKey("localization");
            PlayerPrefs.DeleteKey("web_auth_token");
            PlayerPrefs.DeleteKey("refresh_token");
            PlayerPrefs.DeleteKey("keepMeLoggedIn");
            SwitchScenes();
        }
        private void SwitchScenes()
        {
            SetLanguageFromPreferences();
#if UNITY_WEBGL || UNITY_STANDALONE_WIN_DESKTOP
            SceneManager.LoadSceneAsync("MainMenu_2D");
#else
            SceneManager.LoadSceneAsync("MainMenu");
#endif
        }
        
        private void SetLanguageFromPreferences()
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
                    localeString = "de";
                    break;
                case SystemLanguage.English:
                default:
                    localeString = "en";
                    break;
            }
            
            Locale selectedLocale;
            selectedLocale = LocalizationSettings.AvailableLocales.GetLocale(localeString);
            LocalizationSettings.SelectedLocale = selectedLocale;
        }
    }
}