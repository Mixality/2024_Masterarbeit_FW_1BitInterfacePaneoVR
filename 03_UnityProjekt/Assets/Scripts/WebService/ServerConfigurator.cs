using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace WebService
{
    public class ServerConfigurator : MonoBehaviour
    {
        public static string defaultServerRoot = "https://app.paneovr.net";
        //public static string defaultServerRoot = "http://localhost:8090"; //for local development
        public static string defaultWebsocketRoot = "wss://socket.paneovr.net";
        //public static string defaultWebsocketRoot = "ws://localhost:8070"; //for local development
        public InputField InputField;

        private void Awake()
        {
            //Just always set serverRoot as system set
            /*if (PlayerPrefs.HasKey("serverRoot") && PlayerPrefs.GetString("serverRoot").Any())
            {
                InputField.text = PlayerPrefs.GetString("serverRoot");
            }*/
            InputField.text = defaultServerRoot;
            PlayerPrefs.SetString("serverRoot", defaultServerRoot);
        }

        public void UpdateServerRoot()
        {
            string stringCheck = InputField.text;
            stringCheck = stringCheck.Replace('\\', '/');
            if (!stringCheck.Substring(stringCheck.Length - 1).Equals("/"))
            {
                stringCheck = stringCheck + "/";
            }
            PlayerPrefs.SetString("serverRoot", stringCheck);
            gameObject.SetActive(false);
        }
    }
}