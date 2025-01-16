using System;
using System.Collections.Generic;

namespace UnityEngine.UI.WebService
{
    [Serializable]
    public class LoginData
    {
        public string username;
        public string userid;
        public string web_auth_token;
        public string refresh_token;
        public string license;
        public string localization;
    }
}