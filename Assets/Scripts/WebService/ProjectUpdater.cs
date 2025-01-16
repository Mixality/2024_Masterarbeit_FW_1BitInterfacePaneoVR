using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using DefaultNamespace;
using LightBuzz.Archiver;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI.WebService;
using WebService;

namespace UnityEngine.UI.SaveSystem
{
    public class ProjectUpdater : MonoBehaviour
    {
        private ProjectManager projectManager;
        private AutomaticRelogin accessTokenUpdater;
        
        private string connectionErrorMsg =
            "Keine Verbindung möglich - Projekte können nicht aktualisiert werden. Passiert dies unerwartet, prüfen Sie die Internetverbindung und versuchen Sie sich neu einzuloggen.";

        private string Error405Msg = "Upload nicht möglich. Projekt ist schreibgeschützt.";
        
        internal UnityEvent<string> OnUpdateDownloadMediaType = new UnityEvent<string>();
        internal UnityEvent<string> OnUpdateDownloadMediaAmount = new UnityEvent<string>();
        internal UnityEvent<string> OnUpdateProcessType = new UnityEvent<string>();
        internal UnityEvent<string> OnUpdateLoadingProjectName = new UnityEvent<string>();
        internal UnityEvent<string> OnDownloadError = new UnityEvent<string>();
        internal UnityEvent OnDownloadStarted = new UnityEvent();
        internal UnityEvent OnDownloadFinished = new UnityEvent();

        private void Awake()
        {
            accessTokenUpdater = FindObjectOfType<AutomaticRelogin>();
            if (!accessTokenUpdater)
            {
                Debug.LogError("ProjectUpdater: no access token manager found");
            }
        }

        private void Start()
        {
            projectManager = FindObjectOfType<ProjectManager>();
        }

        internal void remoteUploadProject(string projectSrc)
        {
            StartCoroutine(UploadProject(projectSrc));
        }

        internal void pullRemoteProject(string projectSrc, string projectName)
        {
            StartCoroutine(DownloadRemoteProject(projectSrc, projectName));
        }

        private IEnumerator UploadProject(string projectName)
        {
            projectManager.uiController.toggleDownloadStatePanel(true);
            projectManager.uiController.labelProjectName.text = projectName.Replace("\n", "");
            projectManager.uiController.labelProcessType.text = "Upload";
            
            projectManager.uiController.labelType.text = "Sende Projektdateien";
            projectManager.uiController.labelAmount.text = "";
            
            string projectPath = ProjectLoader.PROJECTS_DIR_PATH+"/"+projectName;

            yield return projectManager.setLastSaveToCurrentTime(projectName);

            //REMOVED "ZIP AND UPLOAD" FUNCTION DUE TO BETTER SOLUTION WITH SEPARATE SCENE UPLOAD
            //Implement new upload project function if neccessary
            
            projectManager.uiController.toggleDownloadStatePanel(false);
            projectManager.OnProjectsChanged.Invoke();
        }

        internal IEnumerator GetRemoteProjectList()
        {
            string userid = PlayerPrefs.GetString("userid");
            string jwt = PlayerPrefs.GetString("web_auth_token");
            string url = PlayerPrefs.GetString("serverRoot") + "/api/readprojectdir.php";

            // Erstelle ein WWWForm-Objekt und füge die POST-Daten hinzu
            WWWForm form = new WWWForm();
            form.AddField("web_auth_token", jwt);
            form.AddField("userid", userid);

            yield return accessTokenUpdater.RefreshToken();
            
            using (UnityWebRequest www = UnityWebRequest.Post(url, form))
            {
                yield return www.Send();
                if (www.isNetworkError || www.isHttpError)
                {
                    Debug.Log(www.error);
                    Debug.Log("projectList not found on server url: " + url);
                    OnDownloadError.Invoke(connectionErrorMsg);
                }
                else
                {
                    if (www.responseCode == 404)
                    {
                        Debug.Log("404/Not Found");
                    }
                    else if (www.responseCode == 401)
                    {
                        Debug.Log("401/Unauthorized");
                    }
                    else
                    {
                        RemoteProjectList projectList =
                            JsonUtility.FromJson<RemoteProjectList>(www.downloadHandler.text);
                        string filePath = ProjectLoader.PROJECTS_DIR_PATH + "/" +
                                          ProjectManager.REMOTE_PROJECT_LIST_FILE;

                        if (!File.Exists(filePath))
                        {
                            using (File.Create(filePath))
                            {
                            }
                        }

                        using (var stream = new FileStream(filePath, FileMode.Truncate))
                        {
                            using (var writer = new StreamWriter(stream))
                            {
                                if (projectList != null)
                                {
                                    writer.WriteLine(www.downloadHandler.text);
                                }
                            }
                        }
                    }
                }
            }
        }
        
        private IEnumerator DownloadRemoteProject(string projectSrc, string projectName)
        {
            string fileName = projectName.Replace("\n", "") + ".zip";
            string userid = PlayerPrefs.GetString("userid");
            string authtoken = PlayerPrefs.GetString("web_auth_token");
            string getProjectApiCall;
            string savePath;
            
            if (fileName.Any())
            {
                OnDownloadStarted.Invoke();
                OnUpdateLoadingProjectName.Invoke(projectName.Replace("\n", ""));
                OnUpdateProcessType.Invoke("Download");

                //download project json data
                savePath = string.Format("{0}/{1}/{2}", ProjectLoader.PROJECTS_DIR_PATH, projectSrc, fileName);
                
                if(true)
                {
                    string newFolder = projectSrc;

                    if (Directory.Exists(ProjectLoader.PROJECTS_DIR_PATH + "/" + newFolder))
                    {
                        Directory.Delete(ProjectLoader.PROJECTS_DIR_PATH + "/" + newFolder, true);
                    }

                    Directory.CreateDirectory(ProjectLoader.PROJECTS_DIR_PATH + "/" + newFolder);

                    OnUpdateDownloadMediaType.Invoke("Lade Szenen");
                    OnUpdateDownloadMediaAmount.Invoke("");

                    Debug.Log("now extracting!");

                    // Now download a zip file to a native pointer of an lzip.inMemory class
                    bool downloadDone = false;
                    
                    // An inMemory lzip struct class.
                    lzip.inMemory inMemZip = null;

                    // Here we are calling the coroutine for an inMemory class.
                    // Replace the url with one that will comply with CORS headers: https://dev.to/alexandlazaris/why-isnt-my-unity-web-request-running-in-webgl-build-1lfm
                    StartCoroutine(downloadProjectZipFileWithAuth(projectSrc, userid, authtoken, r => downloadDone = r, result => inMemZip = result));

                    while (!downloadDone) yield return true;
                    
                    lzip.getFileInfoMem(inMemZip);

                    int progress = 0;
                    byte[][] zipFilesData = lzip.entries2Buffers(inMemZip.size().ToString(), lzip.ninfo.ToArray(), ref progress, inMemZip.memoryPointer());

                    for (int i = 0; i < zipFilesData.Length; i++)
                    {
                        using (var stream = new FileStream(ProjectLoader.PROJECTS_DIR_PATH + "/" + newFolder + "/" + lzip.ninfo[i], FileMode.CreateNew))
                        {
                            using (var writer = new StreamWriter(stream))
                            {
                                writer.WriteLine(Encoding.UTF8.GetString(zipFilesData[i]));
                            }
                        }
                    }

                    // free the struct and the native memory it occupies!!!
                    lzip.free_inmemory(inMemZip);

                    //download neccessary media files
                    SceneData scene;

                    yield return UpdateMediaRefTable(projectSrc);

                    string refTableString = PlayerPrefs.GetString(projectSrc);
                    MediaRefTable mediaRefTable;
                    mediaRefTable = JsonUtility.FromJson<MediaRefTable>(refTableString);

                    Directory.CreateDirectory(ProjectLoader.VIDEOPOOL_DIR_PATH);
                    Directory.CreateDirectory(ProjectLoader.IMAGEPOOL_DIR_PATH);
                    Directory.CreateDirectory(ProjectLoader.AUDIOPOOL_DIR_PATH);
                    
                    OnUpdateDownloadMediaType.Invoke("Lade Bilder");

                    int counter = 0;
                    foreach (var imageRef in mediaRefTable.imageRefs)
                    {
                        counter++;
                        OnUpdateDownloadMediaAmount.Invoke(counter + "/" + mediaRefTable.imageRefs.Count);
                        Directory.CreateDirectory(ProjectLoader.IMAGEPOOL_DIR_PATH + "/" + imageRef.src);
                        savePath = string.Format("{0}/{1}/{2}", ProjectLoader.IMAGEPOOL_DIR_PATH, imageRef.src, imageRef.name);
                        yield return StartCoroutine(DownloadFile(imageRef.src, userid, authtoken, "image", projectSrc, savePath, false));
                    }

                    string mediaAccessType = PlayerPrefs.GetString("mediaAccessType");
                    if (mediaAccessType != "streaming")
                    {
                        counter = 0;
                        OnUpdateDownloadMediaType.Invoke("Lade Audiodateien");
                        foreach (var audioRef in mediaRefTable.audioRefs)
                        {
                            counter++;
                            OnUpdateDownloadMediaAmount.Invoke(counter + "/" + mediaRefTable.audioRefs.Count);
                            Directory.CreateDirectory(ProjectLoader.AUDIOPOOL_DIR_PATH + "/" + audioRef.src);
                            savePath = string.Format("{0}/{1}/{2}", ProjectLoader.AUDIOPOOL_DIR_PATH, audioRef.src,
                                audioRef.name);
                            yield return StartCoroutine(DownloadFile(audioRef.src, userid, authtoken, "audio",
                                projectSrc, savePath, false));
                        }

                        counter = 0;
                        OnUpdateDownloadMediaType.Invoke("Lade Videos");
                        foreach (var videoRef in mediaRefTable.videoRefs)
                        {
                            counter++;
                            OnUpdateDownloadMediaAmount.Invoke(counter + "/" + mediaRefTable.videoRefs.Count);
                            Directory.CreateDirectory(ProjectLoader.VIDEOPOOL_DIR_PATH + "/" + videoRef.src);
                            savePath = string.Format("{0}/{1}/{2}", ProjectLoader.VIDEOPOOL_DIR_PATH, videoRef.src,
                                videoRef.name);
                            yield return StartCoroutine(DownloadFile(videoRef.src, userid, authtoken, "video",
                                projectSrc, savePath, false));
                        }

                        if (projectManager)
                        {
                            projectManager.setProjectVideoQuality(projectSrc);
                        }
                    }
                }
            }

            OnDownloadFinished.Invoke();
            deleteTempProjectArchives();
        }

        private IEnumerator UpdateMediaRefTable(string projectSrc)
        {
            string url = PlayerPrefs.GetString("serverRoot") + "/api/getmediarefsforproject";
            string userid = PlayerPrefs.GetString("userid");
            string jwt = PlayerPrefs.GetString("web_auth_token");
            
            WWWForm form = new WWWForm();

            form.AddField("userid", userid);
            form.AddField("web_auth_token", jwt);
            form.AddField("src", projectSrc);

            yield return accessTokenUpdater.RefreshToken();
            
            using (UnityWebRequest www = UnityWebRequest.Post(url, form))
            {
                yield return www.SendWebRequest();
                
                if (www.isNetworkError || www.isHttpError)
                {
                    Debug.Log(www.error);
                    Debug.Log("Login failed");
                }
                else
                {
                    string refTableData = www.downloadHandler.text;
                    PlayerPrefs.SetString(projectSrc, refTableData);
                }
            }
        }
        
        private IEnumerator DownloadFile(string src, string userid, string authtoken, string type, string projectSrc, string savePath, bool overwrite)
        {
            if (File.Exists(savePath) && !overwrite)
            {
                
            }
            else
            {
                if (File.Exists(savePath))
                {
                    File.Delete(savePath);
                }

                string mediaApiURL;
                if (type.Equals("video"))
                {
                    string videoQuality = PlayerPrefs.HasKey("videoQuality") ? PlayerPrefs.GetString("videoQuality") : "high";
#if UNITY_ANDROID
                    mediaApiURL = PlayerPrefs.GetString("serverRoot") + "/api/getmedia?src=" + src + "&type=" + type + "&quality=" + videoQuality + "&platform=android";
#else
                    mediaApiURL = PlayerPrefs.GetString("serverRoot") + "/api/getmedia?src=" + src + "&type=" + type + "&quality=" + videoQuality;
#endif
                }
                else
                {
                    mediaApiURL = PlayerPrefs.GetString("serverRoot") + "/api/getmedia?src=" + src + "&type=" + type;
                } 

                //post body for auth
                WWWForm form = new WWWForm();
                form.AddField("userid", userid);
                form.AddField("web_auth_token", authtoken);
                form.AddField("projectSrc", projectSrc);

                yield return accessTokenUpdater.RefreshToken();
                
                using (UnityWebRequest www = UnityWebRequest.Post(mediaApiURL, form))
                {
                    yield return www.Send();

                    if (www.isNetworkError || www.isHttpError)
                    {
                        Debug.Log(www.error);
                        Debug.Log("image not found on server url: " + mediaApiURL);
                        projectManager.uiController.showError(connectionErrorMsg);
                    }
                    else
                    {
                        File.WriteAllBytes(savePath, www.downloadHandler.data);
                    }
                }
            }
        }

        private void deleteTempProjectArchives()
        {
            DirectoryInfo di = new DirectoryInfo(ProjectLoader.PROJECTS_DIR_PATH);
            FileInfo[] files = di.GetFiles("*.zip").Where(p => p.Extension == ".zip").ToArray();
            foreach (FileInfo file in files)
            {
                try
                {
                    file.Attributes = FileAttributes.Normal;
                    File.Delete(file.FullName);
                }
                catch (FileNotFoundException e)
                {
                    Debug.Log(e);
                }
            }
        }

        // lzip extension with post body for jwt authentication
        // Parameters:
        //
        // url:             The url of the file you want to download to a native memory buffer.
        // downloadDone:    Informs a bool that the download of the file to memory is done.
        // inmem:           An lzip.inMemory class to get the data.
        // pointer:         An IntPtr for a native memory buffer
        // fileSize:        The size of the downloaded file will be returned here.
        // 
        public static IEnumerator downloadProjectZipFileWithAuth(string projectSrc, string userid, string authtoken, Action<bool> downloadDone, Action<lzip.inMemory> inmem, Action<IntPtr> pointer = null, Action<int> fileSize = null)
        {
            // Get the file lenght first, so we create a correct size native memory buffer.
            using (UnityWebRequest webRequest = UnityWebRequest.Get(PlayerPrefs.GetString("serverRoot") + "/api/getprojectfilesize?src=" + projectSrc))
            {
                // Request and wait for the desired page.
                yield return webRequest.SendWebRequest();


                if (webRequest.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.Log("Not Received: " + webRequest.error);
                }
                else
                {
                    if (!lzip.nativeBufferIsBeingUsed)
                    {

                        //get the size of the zip
                        int zipSize = Convert.ToInt32(webRequest.downloadHandler.text);

                        // If the zip size is larger then 0
                        if (zipSize > 0)
                        {

                            lzip.nativeBuffer = lzip.createBuffer(zipSize);
                            lzip.nativeBufferIsBeingUsed = true;

                            // buffer for the download
                            byte[] bytes = new byte[2048];
                            lzip.nativeOffset = 0;

                            //post body for auth
                            WWWForm form = new WWWForm();
                            form.AddField("userid", userid);
                            form.AddField("web_auth_token", authtoken);

                            string mediaApiURL = PlayerPrefs.GetString("serverRoot") + "/api/getmedia?src=" + projectSrc + "&type=project";

                            using (UnityWebRequest wwwSK = UnityWebRequest.Post(mediaApiURL, form))
                            {

                                // Here we call our custom webrequest function to download our archive to a native memory buffer.
                                wwwSK.downloadHandler = new lzip.CustomWebRequest(bytes);

                                yield return wwwSK.SendWebRequest();

                                if (wwwSK.error != null)
                                {
                                    Debug.Log(wwwSK.error);
                                }
                                else
                                {
                                    downloadDone(true);

                                    if (inmem != null)
                                    {
                                        // create a new inMemory struct to pass as output to the Action param.
                                        lzip.inMemory t = new lzip.inMemory();

                                        t.pointer = lzip.nativeBuffer;
                                        t.info[0] = zipSize;
                                        inmem(t);
                                    }

                                    if (pointer != null) { pointer(lzip.nativeBuffer); fileSize(zipSize); }

                                    //reset lzip intermediate buffer params.
                                    lzip.nativeBufferIsBeingUsed = false;
                                    lzip.nativeOffset = 0;
                                    lzip.nativeBuffer = IntPtr.Zero;

                                    //Debug.Log("Custom download done");
                                }
                            }

                        }

                    }
                    else { Debug.LogError("Native buffer is being used, or not yet freed!"); }
                }
            }
        }
    }
}