using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DefaultNamespace;
using Network;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.Networking;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.UI.SaveSystem;
using UnityEngine.UI.WebService;
using UnityEngine.Video;
using Utilities;
using WebService;

enum VersionState
{
    NotCloned,
    UpToDate,
    BehindRemote,
    BeforeRemote,
    LocalOnly,
    ChangesLocalAndRemote,
    OnlyStreaming
}

public class ProjectList : MonoBehaviour
{
    private string SURE_TO_DELETE;
    private string LOAD_SCENARIO;
    private string UPDATE_SCENARIO;
    private string DOWNLOAD_SCENARIO;
    private string LOCAL_USER;
    private string ADDITIONAL_DATA_LOW;
    private string ADDITIONAL_DATA_MEDIUM;
    private string ADDITIONAL_DATA_HIGH;
    private string UNKNOWN;
    
    public Button templateButton;
    public Button templateButtonPulled;
    public Button templateButtonPullAvailable;
    public Button templateButtonPullNeccessary;
    public Button templateButtonOnlyStreaming;
    public Button templateButtonLocalOnly;
    
    public Button openButton;
    public Button editButton;
    public Button downloadButton;
    public Button uploadButton;

    public Text btnDownloadLabel;

    public Text selProjectAuthor;
    public Text selProjectTitle;
    public Text selProjectSize;
    public Text selProjectSceneCount;

    public TMP_Dropdown videoQualityDropdown;
    public TMP_Dropdown mediaAccessTypeDropdown;
    
    private string selectedProjectName;
    private string selectedProjectSrc;

    public Image listGrid;

    private ProjectManager projectManager;

    private UIController UIController;

    private NetworkController networkController;

    private bool isProjectListLoading;
    private void Awake()
    {
        SURE_TO_DELETE = LocalizationSettings.StringDatabase.GetLocalizedString("SURE_TO_DELETE");
        LOAD_SCENARIO = LocalizationSettings.StringDatabase.GetLocalizedString("LOAD_SCENARIO");
        UPDATE_SCENARIO = LocalizationSettings.StringDatabase.GetLocalizedString("UPDATE_SCENARIO");
        DOWNLOAD_SCENARIO = LocalizationSettings.StringDatabase.GetLocalizedString("DOWNLOAD_SCENARIO");
        LOCAL_USER = LocalizationSettings.StringDatabase.GetLocalizedString("LOCAL_USER");
        ADDITIONAL_DATA_LOW = LocalizationSettings.StringDatabase.GetLocalizedString("ADDITIONAL_DATA_LOW");
        ADDITIONAL_DATA_MEDIUM = LocalizationSettings.StringDatabase.GetLocalizedString("ADDITIONAL_DATA_MEDIUM");
        ADDITIONAL_DATA_HIGH = LocalizationSettings.StringDatabase.GetLocalizedString("ADDITIONAL_DATA_HIGH");
        UNKNOWN = LocalizationSettings.StringDatabase.GetLocalizedString("UNKNOWN");
        projectManager = FindObjectOfType<ProjectManager>();
        if (!projectManager)
        {
            Debug.LogError("Error in Project List: Couldn't find ProjectManager in Scene");
        }
        projectManager.OnProjectsChanged.AddListener(reloadProjects);
    }

    private void Start()
    {
        openButton.interactable = false;
        
        UIController = FindObjectOfType<UIController>();
        if (!UIController)
        {
            Debug.LogError("Error in Project List: Couldn't find UIController in Scene");
        }

        networkController = FindObjectOfType<NetworkController>();
        if (!networkController)
        {
            Debug.LogError("Error in Project List: Couldn't find NetworkController in Scene");
        }
        
        openButton.interactable = false;
        uploadButton.interactable = false;
        downloadButton.interactable = false;
        editButton.interactable = false;

        string currentVideoQuality = PlayerPrefs.GetString("videoQuality");
        switch (currentVideoQuality)
        {
            case "preview":
                videoQualityDropdown.value = 0;
                break;
            case "low":
                videoQualityDropdown.value = 1;
                break;
            case "high":
                videoQualityDropdown.value = 2;
                break;
            default:
                PlayerPrefs.SetString("videoQuality", "high");
                videoQualityDropdown.value = 2;
                break;
        }

#if UNITY_WEBGL
        //no video download possibility in webgl
        PlayerPrefs.SetString("mediaAccessType", "streaming");
        mediaAccessTypeDropdown.value = 0;
        mediaAccessTypeDropdown.options.RemoveAt(1);
        mediaAccessTypeDropdown.interactable = false;
#else
        string currentMediaAcessType = PlayerPrefs.GetString("mediaAccessType");
        switch (currentMediaAcessType)
        {
            case "streaming":
                mediaAccessTypeDropdown.value = 0;
                break;
            case "download":
                mediaAccessTypeDropdown.value = 1;
                break;
            default:
                PlayerPrefs.SetString("mediaAccessType", "download");
                mediaAccessTypeDropdown.value = 1;
                break;
        }
#endif


        /* ONLY FOR MULTIPLAYER DEBUG!
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        StartCoroutine(hostAutomatic());
#else

#endif */
    }

    private IEnumerator hostAutomatic()
    {
        yield return new WaitForSeconds(4);
        selectedProjectSrc = "Umgang mit Unsicherheit u. Handlungsdruck";
        openSelectedProjectAsHost();
        yield return null;
    }
    public void reloadProjects()
    {
        if (gameObject.activeSelf)
        {
            if (!isProjectListLoading)
            {
                isProjectListLoading = true;
                StartCoroutine(reloadProjectsAsync());
            }
        }
    }

    private IEnumerator reloadProjectsAsync()
    {
        foreach (Transform child in listGrid.transform)
        {
            Destroy(child.gameObject);
        }

        string mediaAccessType = PlayerPrefs.GetString("mediaAccessType");

        if (mediaAccessType.Equals("streaming"))
        {
            btnDownloadLabel.text = LOAD_SCENARIO;
        }
        else
        {
            btnDownloadLabel.text = DOWNLOAD_SCENARIO;
        }

        string currentUsername;
        if (PlayerPrefs.GetInt("loggedIn") >= 1)
        {
            currentUsername = PlayerPrefs.GetString("username");
            //just locals
            List<ProjectData> localProjectConfigs = new List<ProjectData>();
            foreach (var dir in Directory.GetDirectories(ProjectLoader.PROJECTS_DIR_PATH))
            {
                try
                {
                    ProjectData currentProjectData;
                    if (File.Exists(dir + "/" + "project_config.json"))
                    {
                        string projectConfig = File.ReadAllText(dir + "/" + "project_config.json");
                        localProjectConfigs.Add(JsonUtility.FromJson<ProjectData>(projectConfig));
                    }
                }
                catch (DirectoryNotFoundException e)
                {
                    Debug.Log(e.Message);
                }
            }

            //add remotes
            yield return projectManager.updateRemoteProjectList();
            ProjectData[] remoteData = getRemoteProjectList();

            Button newEntry;

            string url = PlayerPrefs.GetString("serverRoot");
            using (UnityWebRequest www = UnityWebRequest.Get(url))
            {
                yield return www.SendWebRequest();

                if (www.isNetworkError || www.isHttpError)
                {
                    foreach (var project in localProjectConfigs)
                    {
                        //check if there is only streaming data and no videos/audios
                        MediaRefTable mediaRefs =
                            JsonUtility.FromJson<MediaRefTable>(PlayerPrefs.GetString(project.projectSrc));
                        List<MediaReference> videoRefs = mediaRefs.videoRefs;
                        List<MediaReference> audioRefs = mediaRefs.audioRefs;
                        if ((videoRefs.Count > 0 && AssetLoader.getVideoFileFromSrc(videoRefs[0].src) == null) || (audioRefs.Count > 0 && AssetLoader.getAudioFileFromSrc(audioRefs[0].src) == null))
                        {
                            int projectSize = getProjectSize(project, true);
                            newEntry = Instantiate(templateButtonOnlyStreaming);
                            newEntry.onClick.AddListener(delegate
                            {
                                setSelectedProject(project, 4, VersionState.OnlyStreaming, projectSize);
                            });
                            newEntry.GetComponentInChildren<Text>().text = project.projectName;
                            newEntry.transform.SetParent(listGrid.transform, false);
                        } 
                        else 
                        {
                            int projectSize = getProjectSize(project, true);
                            newEntry = Instantiate(templateButtonPulled);
                            newEntry.onClick.AddListener(delegate
                            {
                                setSelectedProject(project, 4, VersionState.UpToDate, projectSize);
                            });
                            newEntry.GetComponentInChildren<Text>().text = project.projectName;
                            newEntry.transform.SetParent(listGrid.transform, false);
                        }
                    }
                }
                else
                {
                    foreach (var project in localProjectConfigs)
                    {
                        bool remoteFound = false;
                        foreach (var remoteProject in remoteData)
                        {
                            if (project.projectSrc.Equals(remoteProject.projectSrc))
                            {
                                remoteFound = true;
                                int projectSize = getProjectSize(project, true);

                                if (project.lastRemoteSave == remoteProject.lastRemoteSave &&
                                    project.lastLocalSave <= remoteProject.lastRemoteSave)
                                {
                                    //check if there is only streaming data and no videos/audios in download mode
                                    MediaRefTable mediaRefs =
                                        JsonUtility.FromJson<MediaRefTable>(PlayerPrefs.GetString(project.projectSrc));
                                    List<MediaReference> videoRefs = mediaRefs.videoRefs;
                                    List<MediaReference> audioRefs = mediaRefs.audioRefs;
                                    if (mediaAccessType.Equals("download") && videoRefs.Count > 0 && (AssetLoader.getVideoFileFromSrc(videoRefs[0].src) == null || audioRefs.Count > 0 && AssetLoader.getAudioFileFromSrc(audioRefs[0].src) == null))
                                    {
                                        newEntry = Instantiate(templateButtonOnlyStreaming);
                                        newEntry.onClick.AddListener(delegate
                                        {
                                            setSelectedProject(project, remoteProject.projectAccessLevel, VersionState.OnlyStreaming, projectSize);
                                        });
                                        newEntry.GetComponentInChildren<Text>().text = project.projectName;
                                        newEntry.transform.SetParent(listGrid.transform, false);
                                    }
                                    else
                                    {
                                        newEntry = Instantiate(templateButtonPulled);
                                        newEntry.onClick.AddListener(delegate
                                        {
                                            setSelectedProject(project, remoteProject.projectAccessLevel,
                                                VersionState.UpToDate, projectSize);
                                        });
                                        ToggleDisplayIcon sharedIcon =
                                            newEntry.GetComponentInChildren<ToggleDisplayIcon>();
                                        if (sharedIcon && remoteProject.projectOwner.Equals(currentUsername))
                                        {
                                            sharedIcon.gameObject.SetActive(false);
                                        }
                                    }
                                }
                                else
                                {
                                    newEntry = Instantiate(templateButtonPullAvailable);
                                    newEntry.onClick.AddListener(delegate
                                    {
                                        setSelectedProject(remoteProject, remoteProject.projectAccessLevel,
                                            VersionState.ChangesLocalAndRemote, projectSize);
                                    });
                                    ToggleDisplayIcon sharedIcon = newEntry.GetComponentInChildren<ToggleDisplayIcon>();
                                    if (sharedIcon && remoteProject.projectOwner.Equals(currentUsername))
                                    {
                                        sharedIcon.gameObject.SetActive(false);
                                    }
                                }

                                newEntry.GetComponentInChildren<Text>().text = remoteProject.projectName;
                                newEntry.transform.SetParent(listGrid.transform, false);

                                //fire click again if a project has been selected to update displayed project data
                                if (project.projectName.Equals(selProjectTitle.text))
                                {
                                    newEntry.onClick.Invoke();
                                }
                            }
                        }

                        if (!remoteFound)
                        {
                            int projectSize = getProjectSize(project, true);
                            newEntry = Instantiate(templateButtonLocalOnly);
                            newEntry.onClick.AddListener(delegate
                            {
                                setSelectedProject(project, 4, VersionState.LocalOnly, projectSize);
                            });

                            newEntry.GetComponentInChildren<Text>().text = project.projectName;
                            newEntry.transform.SetParent(listGrid.transform, false);

                            //fire click again if a project has been selected to update displayed project data
                            if (project.projectName.Equals(selProjectTitle.text))
                            {
                                newEntry.onClick.Invoke();
                            }
                        }
                    }

                    foreach (var remoteProject in remoteData)
                    {
                        bool localFound = false;
                        foreach (var localProject in localProjectConfigs)
                        {
                            if (localProject.projectSrc.Equals(remoteProject.projectSrc))
                            {
                                localFound = true;
                            }
                        }

                        if (!localFound)
                        {
                            int projectSize = getProjectSize(remoteProject);

                            newEntry = Instantiate(templateButton);
                            newEntry.onClick.AddListener(delegate
                            {
                                setSelectedProject(remoteProject, remoteProject.projectAccessLevel,
                                    VersionState.NotCloned, projectSize);
                            });
                            newEntry.GetComponentInChildren<Text>().text = remoteProject.projectName;
                            newEntry.transform.SetParent(listGrid.transform, false);

                            //fire click again if a project has been selected to update displayed project data
                            if (remoteProject.projectName.Equals(selProjectTitle.text))
                            {
                                newEntry.onClick.Invoke();
                            }

                            ToggleDisplayIcon sharedIcon = newEntry.GetComponentInChildren<ToggleDisplayIcon>();
                            if (sharedIcon && remoteProject.projectOwner.Equals(currentUsername))
                            {
                                sharedIcon.gameObject.SetActive(false);
                            }
                        }
                    }
                }
            }
        }
        isProjectListLoading = false;
    }
    
    private int getProjectSize(ProjectData projectData, bool localProject = false)
    {
        int projectSize;
        string videoQuality = PlayerPrefs.GetString("videoQuality");
        if (localProject)
        {
            videoQuality = projectData.currentVideoQuality;
        }
#if UNITY_WEBGL || UNITY_STANDALONE_WIN
        switch (videoQuality)
        {
            case "preview":
                projectSize = projectData.projectSize_h264_preview;
                break;
            case "low":
                projectSize = projectData.projectSize_h264_low;
                break;
            case "high":
                projectSize = projectData.projectSize_h264_high;
                break;
            default:
                projectSize = projectData.projectSize_h264_high;
                break;
        }
#else
        switch (videoQuality)
        {
            case "preview":
                projectSize = projectData.projectSize_h264_preview;
                break;
            case "low":
                projectSize = projectData.projectSize_h265_low;
                break;
            case "high":
                projectSize = projectData.projectSize_h265_high;
                break;
            default:
                projectSize = projectData.projectSize_h265_high;
                break;
        }
#endif
        return projectSize;
    }
    
    private ProjectData[] getRemoteProjectList()
    {
        RemoteProjectList remoteProjectList = null;
        
        if(Directory.Exists(ProjectLoader.PROJECTS_DIR_PATH + "/"))
        {
            try
            {
                string remoteProjectListString = File.ReadAllText(ProjectLoader.PROJECTS_DIR_PATH + "/" + ProjectManager.REMOTE_PROJECT_LIST_FILE);
                remoteProjectList = JsonUtility.FromJson<RemoteProjectList>(remoteProjectListString);
                return remoteProjectList.remoteData.ToArray();
            }
            catch (FileNotFoundException e)
            {
                Debug.Log("The " + ProjectManager.REMOTE_PROJECT_LIST_FILE + " could not be read:");
                Debug.Log(e.Message);
            }
        }
        else
        {
            Debug.LogError("Folder not found: " + ProjectLoader.PROJECTS_DIR_PATH);
        }

        return Array.Empty<ProjectData>();
    }
    
    void setSelectedProject(ProjectData projectData, int accessLevel, VersionState versionState, int size = 0)
    {
        selectedProjectSrc = projectData.projectSrc;
        selectedProjectName = projectData.projectName;
        if (selectedProjectSrc.Any() && accessLevel <= 4)
        {
            selProjectTitle.text = projectData.projectName;
            selProjectAuthor.text = projectData.projectOwner;
            if (projectData.sceneCount > 0)
            {
                selProjectSceneCount.text = projectData.sceneCount.ToString();
            }
            else
            {
                selProjectSceneCount.text = UNKNOWN;
            }

            string additionalSizeData = "";
            if (projectData.currentVideoQuality != null)
            {
                if (!projectData.currentVideoQuality.Equals(PlayerPrefs.GetString("videoQuality")))
                {
                    switch (projectData.currentVideoQuality)
                    {
                        case "preview":
                            additionalSizeData = ADDITIONAL_DATA_LOW;
                            break;
                        case "low":
                            additionalSizeData = ADDITIONAL_DATA_MEDIUM;
                            break;
                        case "high":
                            additionalSizeData = ADDITIONAL_DATA_HIGH;
                            break;
                    }
                }
            }
            
            if (size >= 0 && size / 1000000 == 0)
            {
                selProjectSize.text = "<1mb";
            }
            else if (size > 0)
            {
                selProjectSize.text = size / 1000000 + "mb";
            }
            else
            {
                selProjectSize.text = UNKNOWN;
            }

            selProjectSize.text = selProjectSize.text + additionalSizeData;

            if (accessLevel >= 3)
            {
                editButton.gameObject.SetActive(false);
            }
            else
            {
                editButton.gameObject.SetActive(true);
            }
            
            if (versionState == VersionState.UpToDate || versionState == VersionState.BehindRemote || versionState == VersionState.LocalOnly)
            {
                openButton.interactable = true;
                editButton.interactable = true;
            }
            else
            {
                openButton.interactable = false;
                editButton.interactable = false;
            }

            if (versionState == VersionState.UpToDate || versionState == VersionState.LocalOnly)
            {
                downloadButton.interactable = false;
            }
            else
            {
                uploadButton.interactable = false;
            }
            if (versionState == VersionState.BehindRemote || versionState == VersionState.ChangesLocalAndRemote || versionState == VersionState.NotCloned)
            {
                downloadButton.interactable = true;
            }
            else
            {
                downloadButton.interactable = false;
            }

            if (mediaAccessTypeDropdown.value != 0 && versionState == VersionState.OnlyStreaming)
            {
                downloadButton.interactable = true;
                openButton.interactable = false;
                editButton.interactable = false;
            }

            if (versionState == VersionState.BehindRemote)
            {
                btnDownloadLabel.text = UPDATE_SCENARIO;
            }
            else
            {
                if (mediaAccessTypeDropdown.value == 0)
                {
                    btnDownloadLabel.text = LOAD_SCENARIO;
                }
                else
                {
                    btnDownloadLabel.text = DOWNLOAD_SCENARIO;
                }
            }
            
            if (PlayerPrefs.GetInt("loggedIn") == 0)
            {
                downloadButton.interactable = false;
                uploadButton.interactable = false;
                editButton.interactable = false;
            }
        }
        else
        {
            openButton.interactable = false;
            uploadButton.interactable = false;
            downloadButton.interactable = false;
        }
    }

    public void openSelectedProjectAsHost()
    {
        if (selectedProjectSrc.Any())
        {
            //projectController.authoringMode = false;
            string username;
            if (PlayerPrefs.GetInt("loggedIn") != 0)
            {
                username = PlayerPrefs.GetString("username");
            }
            else
            {
                username = LOCAL_USER;
            }
            networkController.HostProject(selectedProjectSrc, selectedProjectName, username);
        }
    }

    public void editSelectedProject(bool complexMode)
    {
        if (selectedProjectSrc.Any())
        {
            networkController.EditProject(selectedProjectSrc, selectedProjectName);
        }
    }

    public void uploadSelectedProject()
    {
        if (selectedProjectSrc.Any())
        {
            projectManager.uploadProject(selectedProjectSrc);
        }
    }

    public void downloadSelectedProject()
    {
        if (selectedProjectSrc.Any())
        {
            projectManager.pullRemoteProject(selectedProjectSrc, selectedProjectName);
        }
    }

    public void deleteSelectedProject()
    {
        if (selectedProjectSrc.Any())
        {
            UIController.openGenericVerifyField(deleteSelectedProject, SURE_TO_DELETE);
        }
    }
    
    private void deleteSelectedProject(bool userIsSure)
    {
        if (userIsSure)
        {
            projectManager.deleteProject(selectedProjectSrc);
        }
    }

    public void changeVideoQuality(int qualityOption)
    {
        switch (qualityOption)
        {
            case 0:
                PlayerPrefs.SetString("videoQuality", "preview");
                break;
            case 1:
                PlayerPrefs.SetString("videoQuality", "low");
                break;
            case 2:
                PlayerPrefs.SetString("videoQuality", "high");
                break;
            default:
                PlayerPrefs.SetString("videoQuality", "high");
                break;
        }
        reloadProjects();
    }
    
    public void changeMediaAccessType(int accessOption)
    {
        switch (accessOption)
        {
            case 0:
                PlayerPrefs.SetString("mediaAccessType", "streaming");
                break;
            case 1:
                PlayerPrefs.SetString("mediaAccessType", "download");
                break;
            default:
                PlayerPrefs.SetString("mediaAccessType", "download");
                break;
        }
        reloadProjects();
    }
}
