using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using DefaultNamespace;
using Network;
using RenderHeads.Media.AVProVideo;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.UI.SaveSystem;
using UnityEngine.Video;



public class VideoController : MonoBehaviour
{
    internal bool videoIsLooping;
    internal bool showInteractablesDelayed;
    
    internal IngameUIController ingameUIController;
    private NetworkController networkController;
    private EditModeController editModeController;

    public UnityEvent onVideoPlaybackStarted;
    public UnityEvent onVideoPlaybackEnded;

    internal bool reachedEndAndStopped;

    public MediaPlayer mediaPlayer1;
    public MediaPlayer mediaPlayer2;

    private MediaPlayer currentMediaPlayer;
    
    private bool media1Visible;

    public Material skyboxMaterial;
    public Material skyboxMaterial2;

    public Shader webGLShader;
    public Shader normalShader;
    
    public bool isStereoView;

#if UNITY_WEBGL
    public bool videoStreaming = true;
#else
    public bool videoStreaming = false;
#endif
    
    private void Start()
    {
        networkController = FindObjectOfType<NetworkController>();
        if (!networkController)
        {
            Debug.LogError("no network controller found in scene");
        }
        editModeController = FindObjectOfType<EditModeController>();
        if (!editModeController)
        {
            Debug.LogError("no edit mode controller found in scene");
        }
        
        mediaPlayer1.Events.AddListener(HandlePlayerEvent);
        mediaPlayer2.Events.AddListener(HandlePlayerEvent);
        currentMediaPlayer = mediaPlayer1;

#if UNITY_WEBGL && !UNITY_EDITOR
        skyboxMaterial.shader = webGLShader;
        skyboxMaterial2.shader = webGLShader;
#elif UNITY_ANDROID && !UNITY_EDITOR
        if (isStereoView)
        {
            skyboxMaterial.shader = normalShader;
            skyboxMaterial2.shader = normalShader;
        }
        else
        {
            skyboxMaterial.shader = webGLShader;
            skyboxMaterial2.shader = webGLShader;
        }
#elif UNITY_STANDALONE_WIN
        //VideoRender.SetupStereoEyeModeMaterial(skyboxMaterial, StereoEye.Right);
        //VideoRender.SetupStereoEyeModeMaterial(skyboxMaterial2, StereoEye.Right);
#else
        skyboxMaterial.shader = normalShader;
        skyboxMaterial2.shader = normalShader;
#endif
    }
    
    private void HandlePlayerEvent(MediaPlayer mp, MediaPlayerEvent.EventType eventType, ErrorCode code)
    {
        if (eventType == MediaPlayerEvent.EventType.ReadyToPlay)
        {
            
        }
        else if (eventType == MediaPlayerEvent.EventType.FirstFrameReady)
        {
            if (media1Visible)
            {
                RenderSettings.skybox = skyboxMaterial;
                mediaPlayer2.Control.CloseMedia();
            }
            else
            {
                RenderSettings.skybox = skyboxMaterial2;
                mediaPlayer1.Control.CloseMedia();
            }
            
            onVideoPlaybackStarted.Invoke();
        }
        else if (eventType == MediaPlayerEvent.EventType.FinishedPlaying)
        {
            EndReached();
        }
    }

    internal bool IsVideoPlayerPlaying()
    {
        return currentMediaPlayer.Control.IsPlaying();
    }

    internal void PlaySceneVideo(string videoSrc, string streamingToken=null)
    {
        switchMediaPlayer();
        string videoAccessType = PlayerPrefs.GetString("mediaAccessType");

        if (videoAccessType.Equals("streaming"))
        {
            string userid = PlayerPrefs.GetString("userid");
            string videoQuality = PlayerPrefs.HasKey("videoQuality") ? PlayerPrefs.GetString("videoQuality") : "high";
            string videoURL = PlayerPrefs.GetString("serverRoot") + "/api/getstreamingmedia?src=" + videoSrc + "&type=video&quality=" + videoQuality+"&token=" + streamingToken + "&userid=" + userid;
            //currentMediaPlayer.Loop = videoIsLooping;
        
            startPlayingFromZero(videoURL);
        }
        else
        {
            //currentMediaPlayer.Loop = videoIsLooping;
            string videoSrcPath = ProjectLoader.VIDEOPOOL_DIR_PATH + "/" + videoSrc;
            string[] files = Directory.GetFiles(videoSrcPath, "*.mp4");
            if (files.Length > 0)
            {
                startPlayingFromZero(files[0]);
            }
            else
            {
                Debug.Log("video not found " + videoSrcPath);
            }
        }
    }

    //TODO: So far pauses on first frame, because last frame couldn't be calculated, yet
    internal void PlayStreamingVideoOnLastFrame(string videoFile)
    {
        PlaySceneVideo(videoFile);
        if (!videoIsLooping)
        {
            onVideoPlaybackStarted.AddListener(pauseVideoOnPlaybackStart);
        }
    }
    
    internal IEnumerator GetStreamingAccess(string mediaSrc, string type, string projectSrc)
    {
        string userid = PlayerPrefs.GetString("userid");
        string jwt = PlayerPrefs.GetString("web_auth_token");
        string url = PlayerPrefs.GetString("serverRoot") + "/api/getstreamingaccess.php";

        // Erstelle ein WWWForm-Objekt und füge die POST-Daten hinzu
        WWWForm form = new WWWForm();
        form.AddField("web_auth_token", jwt);
        form.AddField("userid", userid);
        form.AddField("mediaSrc", mediaSrc);
        form.AddField("projectSrc", projectSrc);
        form.AddField("type", type);
            
        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            yield return www.SendWebRequest();
            if (www.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log("No access granted: " + www.error);
            }
            else
            {
                PlaySceneVideo(mediaSrc, www.downloadHandler.text);
            }
        }
    }
    
    private void pauseVideoOnPlaybackStart()
    {
        currentMediaPlayer.Control.Pause();
        onVideoPlaybackStarted.RemoveListener(pauseVideoOnPlaybackStart);
    }

    internal void PlayDefaultVideo()
    {
        switchMediaPlayer();
#if UNITY_ANDROID && !UNITY_EDITOR
        string defaultVideo = Application.streamingAssetsPath + "/base_scene.mp4";
#else
        string defaultVideo = Application.streamingAssetsPath + "/base_scene_h264.mp4";
#endif
        //currentMediaPlayer.Loop = videoIsLooping;
        startPlayingFromZero(defaultVideo);
    }

    private void startPlayingFromZero(string pathOrURL)
    {
        //hides interactables till video ends on non looping videos TODO: make that variable for users
        if (showInteractablesDelayed && !(networkController.authoringMode && editModeController.isEditModeActive)) 
        {
            ingameUIController.hideInteractionSphere();
        }
        else
        {
            ingameUIController.showInteractionSphere();
        }
        bool isOpen = currentMediaPlayer.OpenMedia(new MediaPath(pathOrURL, MediaPathType.AbsolutePathOrURL), autoPlay: true);
    }

    internal void RepeatCurrentVideo()
    {
        currentMediaPlayer.Control.Rewind();
        Play();
    }

    internal void ReloadCurrentVideo()
    {
        string currentPath = currentMediaPlayer.MediaPath.Path;
        currentMediaPlayer.CloseMedia();
        currentMediaPlayer.OpenMedia(new MediaPath(currentPath, MediaPathType.AbsolutePathOrURL),
            true);
    }
    
    private void Play()
    {
        reachedEndAndStopped = false;
        currentMediaPlayer.Control.Play();
    }
    
    void EndReached()
    {
        if (showInteractablesDelayed && !(networkController.authoringMode && editModeController.isEditModeActive)) {
            ingameUIController.showInteractionSphere(); //important only if interaction sphere was hidden during playback
        }

        if (!videoIsLooping)
        {
            reachedEndAndStopped = true;
        }
        else
        {
            reachedEndAndStopped = false;
            RepeatCurrentVideo();
        }
        
        onVideoPlaybackEnded.Invoke();
    }

    internal void CloseCurrentVideo()
    {
        PlayDefaultVideo();
    }

    public void setIsVideoLoopingWithReloadCheck(bool looping)
    {
        videoIsLooping = looping;
        //currentMediaPlayer.Loop = looping;
        if (!looping && reachedEndAndStopped)
        {
            RepeatCurrentVideo();
        }
    }
    
    public void setIsVideoLooping(bool looping)
    {
        videoIsLooping = looping;
        //currentMediaPlayer.Loop = looping;
    }

    private void switchMediaPlayer()
    {
        if (!media1Visible)
        {
            currentMediaPlayer = mediaPlayer1;
            media1Visible = true;
        }
        else
        {
            media1Visible = false;
            currentMediaPlayer = mediaPlayer2;
        }
    }
    
    public void toggleStereoMode(bool isStereo)
    {

        MediaHints hints;
        if (isStereo && currentMediaPlayer.Info.GetVideoHeight() / currentMediaPlayer.Info.GetVideoWidth() == 1)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            skyboxMaterial.shader = webGLShader;
            skyboxMaterial2.shader = webGLShader;
#else

            hints = mediaPlayer1.FallbackMediaHints;
            hints.stereoPacking = StereoPacking.TopBottom;
            mediaPlayer1.FallbackMediaHints = hints;
            hints = mediaPlayer2.FallbackMediaHints;
            hints.stereoPacking = StereoPacking.TopBottom;
            mediaPlayer2.FallbackMediaHints = hints;
            skyboxMaterial.shader = normalShader;
            skyboxMaterial2.shader = normalShader;
            VideoRender.SetupStereoEyeModeMaterial(skyboxMaterial, StereoEye.Both);
            VideoRender.SetupStereoEyeModeMaterial(skyboxMaterial2, StereoEye.Both);

            ReloadCurrentVideo();
#endif
        }
        else
        {
            hints = mediaPlayer1.FallbackMediaHints;
            hints.stereoPacking = StereoPacking.None;
            mediaPlayer1.FallbackMediaHints = hints;
            hints = mediaPlayer2.FallbackMediaHints;
            hints.stereoPacking = StereoPacking.None;
            mediaPlayer2.FallbackMediaHints = hints;
#if UNITY_ANDROID && !UNITY_EDITOR
            skyboxMaterial.shader = webGLShader;
            skyboxMaterial2.shader = webGLShader;
#elif UNITY_WEBGL && !UNITY_EDITOR
            skyboxMaterial.shader = webGLShader;
            skyboxMaterial2.shader = webGLShader;
#elif UNITY_STANDALONE_WIN
            skyboxMaterial.shader = webGLShader;
            skyboxMaterial2.shader = webGLShader;
#else
            skyboxMaterial.shader = normalShader;
            skyboxMaterial2.shader = normalShader;
            if (currentMediaPlayer.Info.GetVideoHeight() / currentMediaPlayer.Info.GetVideoWidth() == 1)
            {
                VideoRender.SetupStereoEyeModeMaterial(skyboxMaterial, StereoEye.Left);
                VideoRender.SetupStereoEyeModeMaterial(skyboxMaterial2, StereoEye.Left);
            }
#endif
            ReloadCurrentVideo();
        }

        isStereoView = isStereo;
    }
}
