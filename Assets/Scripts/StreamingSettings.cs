using System;
using Network;
using UnityEngine;
using UnityEngine.UI;

namespace DefaultNamespace
{
    public class StreamingSettings : MonoBehaviour
    {
        private NetworkController networkController;

        public Toggle toggleStreamingQuality;
        
        private void Start()
        {
            networkController = FindObjectOfType<NetworkController>();
            if (!networkController)
            {
                Debug.LogError("no network controller found in scene");
            }

            toggleStreamingQuality.isOn = networkController.highQualityStreaming;
        }

        public void ChangeStreamingQuality(bool highQuality)
        {
            networkController.highQualityStreaming = highQuality;
        }
    }
}