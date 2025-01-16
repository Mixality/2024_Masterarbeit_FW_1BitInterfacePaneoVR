using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using DefaultNamespace;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI.WebService;

namespace UnityEngine.UI.SaveSystem
{
    public class ProjectLoader : MonoBehaviour
    {
        private static string PROJECTS_SUBDIR = "Projects";
        private static string VIDEOPOOL_SUBDIR = "Videopool";
        private static string IMAGEPOOL_SUBDIR = "Imagepool";
        private static string AUDIOPOOL_SUBDIR = "Audiopool";
        
        private static string USER_DIR;
        
        public static string PROJECTS_DIR_PATH;
        public static string VIDEOPOOL_DIR_PATH;
        public static string IMAGEPOOL_DIR_PATH;
        public static string AUDIOPOOL_DIR_PATH;
        
        public static string PROJECT_CONFIG_FILE = "project_config.json";

        private void Awake()
        {
            InitializeDirectories();
        }

        //TODO: unterstand why that works, although it's in a static context
        internal void InitializeDirectories()
        {
            if (PlayerPrefs.GetInt("loggedIn") != 0)
            {
                USER_DIR = Application.persistentDataPath + "/users/" + PlayerPrefs.GetString("userid") + "/";
            }
            else
            {
                USER_DIR = Application.persistentDataPath + "/users/localUser/";
            }

            PROJECTS_DIR_PATH = USER_DIR + PROJECTS_SUBDIR;
            VIDEOPOOL_DIR_PATH = USER_DIR + VIDEOPOOL_SUBDIR;
            IMAGEPOOL_DIR_PATH = USER_DIR + IMAGEPOOL_SUBDIR;
            AUDIOPOOL_DIR_PATH = USER_DIR + AUDIOPOOL_SUBDIR;
            
            //does not override existing ones
            Directory.CreateDirectory(PROJECTS_DIR_PATH);
            Directory.CreateDirectory(VIDEOPOOL_DIR_PATH);
            Directory.CreateDirectory(IMAGEPOOL_DIR_PATH);
            Directory.CreateDirectory(AUDIOPOOL_DIR_PATH);
        }

        internal static string[] getProjectList()
        {
            return Directory.GetDirectories(PROJECTS_DIR_PATH)
                .Select(d => new DirectoryInfo(d).Name).ToArray();
        }

        internal void updateProjectConfig(ProjectData projectData)
        {
            SaveIntoJson(projectData);
        }

        internal ProjectData loadProjectData(string projectSrc)
        {
            ProjectData projectData = LoadFromJson(projectSrc);
            return projectData;
        }

        internal void deleteProject(string src, bool deleteUnusedMedia = true)
        {
            string compDirName = "root: "+src;
            try
            {
                DirectoryInfo di = new DirectoryInfo(PROJECTS_DIR_PATH + "/" + src);
                
                if (deleteUnusedMedia)
                {
                    List<MediaReference> videoFilesToKeep = new List<MediaReference>();
                    List<MediaReference> imageFilesToKeep = new List<MediaReference>();
                    List<MediaReference> audioFilesToKeep = new List<MediaReference>();

                    MediaRefTable otherMediaRefs;
                
                    foreach (var dir in Directory.GetDirectories(PROJECTS_DIR_PATH))
                    {
                        compDirName = new DirectoryInfo(dir).Name;
                        if (!compDirName.Equals(src))
                        {
                            otherMediaRefs = JsonUtility.FromJson<MediaRefTable>(PlayerPrefs.GetString(compDirName));

                            videoFilesToKeep = videoFilesToKeep.Union(otherMediaRefs.videoRefs).ToList();
                            imageFilesToKeep = imageFilesToKeep.Union(otherMediaRefs.imageRefs).ToList();
                            audioFilesToKeep = audioFilesToKeep.Union(otherMediaRefs.audioRefs).ToList();
                        }
                    }
                    
                    //Delete all unused files (including files that aren't in any projects at all)
                    
                    string[] allVideos = Directory.GetDirectories(VIDEOPOOL_DIR_PATH);
                    string[] allImages = Directory.GetDirectories(IMAGEPOOL_DIR_PATH);
                    string[] allAudios = Directory.GetDirectories(AUDIOPOOL_DIR_PATH);
                    
                    foreach (var videoFolder in allVideos)
                    {
                        string directoryName = Path.GetFileName(videoFolder);
                        MediaReference tempRef = new MediaReference();
                        tempRef.src = directoryName;
                        if (!videoFilesToKeep.Contains(tempRef))
                        {
                            Directory.Delete(videoFolder, true);
                        }
                    }

                    foreach (var imageFolder in allImages)
                    {
                        string directoryName = Path.GetFileName(imageFolder);
                        MediaReference tempRef = new MediaReference();
                        tempRef.src = directoryName;
                        if (!imageFilesToKeep.Contains(tempRef))
                        {
                            Directory.Delete(imageFolder, true);
                        }
                    }

                    foreach (var audioFolder in allAudios)
                    {
                        string directoryName = Path.GetFileName(audioFolder);
                        MediaReference tempRef = new MediaReference();
                        tempRef.src = directoryName;
                        if (!audioFilesToKeep.Contains(tempRef))
                        {
                            Directory.Delete(audioFolder, true);
                        }
                    }
                }

                if (Directory.Exists(PROJECTS_DIR_PATH + "/" + src))
                {
                    di.Delete(true);
                }

                PlayerPrefs.DeleteKey(src);
            }
            catch (DirectoryNotFoundException e)
            {
                Debug.LogWarning(e);
            }
        }

        private void SaveIntoJson(ProjectData projectData){
            string projectString = JsonUtility.ToJson(projectData);
            File.WriteAllText(PROJECTS_DIR_PATH + "/" + projectData.projectSrc + "/" + PROJECT_CONFIG_FILE, projectString);
        }

        private ProjectData LoadFromJson(string projectSrc)
        {
            ProjectData project = null;
        
            if(Directory.Exists(PROJECTS_DIR_PATH + "/" + projectSrc))
            {
                try
                {
                    string projectString = File.ReadAllText(PROJECTS_DIR_PATH + "/" + projectSrc + "/" + PROJECT_CONFIG_FILE);
                    project = JsonUtility.FromJson<ProjectData>(projectString);
                }
                catch (FileNotFoundException)
                {
                    Debug.LogError("Error while loading project: Directory exists, but there's no project_config.json");
                }
            }
            else
            {
                Debug.LogError("Error while loading project: No project folder called '" + projectSrc + "'");
            }
            return project;
        }
    }
}