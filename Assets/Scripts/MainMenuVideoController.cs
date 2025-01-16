using System;
using Network;
using RenderHeads.Media.AVProVideo;
using UnityEngine;
using UnityEngine.Video;

namespace DefaultNamespace
{
    public class MainMenuVideoController : MonoBehaviour
    {
        internal bool videoIsLooping;

        public MediaPlayer MediaPlayer;

        public GameObject WebGLStartupCanvas;

        private NetworkController networkController;
        
        private void Awake()
        {
            networkController = FindObjectOfType<NetworkController>();
            if (!networkController)
            {
                Debug.LogError("no network controller found in scene");
            }
            
        }

        private void Start()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            //just on app start
            if (networkController.LastUnityScene == null)
            {
                WebGLStartupCanvas.SetActive(true);
            }
            else
            {
                WebGLStartupCanvas.SetActive(false);
                PlayDefaultVideo();
            }
#else
            if (WebGLStartupCanvas)
            {
                WebGLStartupCanvas.SetActive(false);
            }

            PlayDefaultVideo();
#endif
        }

        //Only for WebGL builds, that need a user interaction in some browsers for playing media
        public void StartManually()
        {
            WebGLStartupCanvas.SetActive(false);
            PlayDefaultVideo();
        }
        
        internal void PlayDefaultVideo()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            string defaultVideo = "base_scene.mp4";
#elif UNITY_WEBGL && !UNITY_EDITOR
            string defaultVideo = "base_scene.webm";
#else
            string defaultVideo = "base_scene_h264.mp4";
#endif
            bool isOpening = MediaPlayer.OpenMedia(new MediaPath(defaultVideo, MediaPathType.RelativeToStreamingAssetsFolder), autoPlay:false);
            Play();
        }

        void Play()
        {
            MediaPlayer.Control.Play();
        }
    }
}