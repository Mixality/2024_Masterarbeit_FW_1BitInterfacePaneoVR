using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoFileList : MonoBehaviour
{
    private AssetLoader assetloader;
    
    private VideoController videoController;

    private SaveController saveController;

    /* Button in scene data canvas, that displays current 360°-video filename in scene */
    public Button currentVideoButton;
    
    /* for instancing new buttons with several filenames */
    public Button templateButton;

    public Image listGrid;
    
    // Start is called before the first frame update
    void Start()
    {
        GameObject[] sceneManagerObjects = GameObject.FindGameObjectsWithTag("SceneManager");
        foreach (var sceneManagerObject in sceneManagerObjects)
        {
            Debug.Log("found");
            if (sceneManagerObject.GetComponent<AssetLoader>())
            {
                assetloader = sceneManagerObject.GetComponent<AssetLoader>();
                loadVideos();
                videoController = sceneManagerObject.GetComponent<VideoController>();
                saveController = sceneManagerObject.GetComponent<SaveController>();
                break;
            }
        }
    }

    private void loadVideos()
    {
        string[] fileNames = assetloader.getAllVideoFiles();

        foreach (Transform child in listGrid.transform)
        {
            Destroy(child.gameObject);
        }
        
        foreach (var fileName in fileNames)
        {
            Button newEntry = Instantiate(templateButton);
            newEntry.onClick.AddListener(delegate
            {
                currentVideoButton.GetComponentInChildren<Text>().text = fileName;
                videoController.PlaySceneVideo(fileName);
            });
            newEntry.GetComponentInChildren<Text>().text = fileName;
            newEntry.transform.SetParent(listGrid.transform, false);
        }
    }
}
