using System;
using System.Collections.Generic;

namespace UnityEngine.UI.SaveSystem
{
    [System.Serializable]
    public class ProjectData
    {
        public ProjectData(string projectName, string projectSrc)
        {
            this.projectName = projectName;
            this.projectSrc = projectSrc;
        }

        public string projectSrc;
        public string projectName;
        public string projectOwner;
        public string firstSceneName;
        public long lastRemoteSave;
        public long lastLocalSave;
        public int projectAccessLevel;
        public int projectSize_h265_high;
        public int projectSize_h265_low;
        public int projectSize_h264_high;
        public int projectSize_h264_low;
        public int projectSize_h264_preview;
        public int sceneCount;
        public string currentVideoQuality; //only important for clients, not set by server
    }
}