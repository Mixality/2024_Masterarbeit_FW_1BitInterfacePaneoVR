using System;
using UnityEngine;
using UnityEngine.XR;

namespace UnityEngine.UI
{
    public class ConsoleToGUI : MonoBehaviour
    {
        public Text GUIConsole;
        //#if !UNITY_EDITOR
        static string myLog = "";
        private string output;
        private string stack;
        bool doShow = true;

        void OnEnable()
        {
            Application.logMessageReceived += Log;
        }
     
        void OnDisable()
        {
            Application.logMessageReceived -= Log;
        }
     
        public void Log(string logString, string stackTrace, LogType type)
        {
            output = logString;
            stack = stackTrace;
            myLog = output + "\n" + myLog;
            if (myLog.Length > 5000)
            {
                myLog = myLog.Substring(0, 4000);
            }
            GUIConsole.text = myLog;
        }
        //#endif
    }
}