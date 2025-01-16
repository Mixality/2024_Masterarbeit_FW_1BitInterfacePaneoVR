using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using UnityEngine;
using UnityEngine.UI.SaveSystem;
using UnityEngine.UI.WebService;
using UnityEngine.Video;

public class AssetLoader : MonoBehaviour
{
    internal string[] videoFiles;

    internal string[] getAllVideoFiles()
    {
        string[] files = Directory.GetFiles(ProjectLoader.VIDEOPOOL_DIR_PATH, "*.mp4");
        return files.Select(file => Path.GetFileName(file)).ToArray();
    }
    
    internal string[] getAllImageFilesForProject(string projectSrc)
    {
        List<MediaReference> imageRefs =
            JsonUtility.FromJson<MediaRefTable>(PlayerPrefs.GetString(projectSrc)).imageRefs;
        List<string> fileList = new List<string>();
        foreach (var imageRef in imageRefs)
        {
            fileList.Add(ProjectLoader.IMAGEPOOL_DIR_PATH + "/" + imageRef.src + "/" + imageRef.name);
        }
        return fileList.ToArray();
    }
    
    internal string[] getAllAudioFilesForProject(string projectSrc)
    {
        List<MediaReference> audioRefs =
            JsonUtility.FromJson<MediaRefTable>(PlayerPrefs.GetString(projectSrc)).imageRefs;
        List<string> fileList = new List<string>();
        foreach (var audioRef in audioRefs)
        {
            fileList.Add(ProjectLoader.AUDIOPOOL_DIR_PATH + "/" + audioRef.src + "/" + audioRef.name);
        }
        return fileList.ToArray();
    }

    internal byte[] getBytesFromImageFile(string filepath)
    {
        return File.ReadAllBytes(filepath);
    }

    internal static string getAudioFileFromSrc(string audioSrc)
    {
        string basePath = ProjectLoader.AUDIOPOOL_DIR_PATH + "/" + audioSrc;
        try
        {
            string[] files = Directory.GetFiles(basePath);
            if (files.Length > 0)
            {
                return files[0];
            }
        }
        catch (DirectoryNotFoundException e)
        {
            return null;
        }
        return null;
    }
    
    internal static string getImageFileFromSrc(string imageSrc)
    {
        string basePath = ProjectLoader.IMAGEPOOL_DIR_PATH + "/" + imageSrc;
        try
        {
            string[] files = Directory.GetFiles(basePath);
            if (files.Length > 0)
            {
                return files[0];
            }
        }
        catch (DirectoryNotFoundException e)
        {
            return null;
        }
        return null;
    }

    internal static string getVideoFileFromSrc(string videoSrc)
    {
        string basePath = ProjectLoader.VIDEOPOOL_DIR_PATH + "/" + videoSrc;
        try
        {
            string[] files = Directory.GetFiles(basePath);
            if (files.Length > 0)
            {
                return files[0];
            }
        }
        catch (DirectoryNotFoundException e)
        {
            return null;
        }
        return null;
    }
}
