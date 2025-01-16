using System;
using UnityEngine;

namespace DefaultNamespace
{
    public class ShaderChangeTrigger : MonoBehaviour
    {
        private VideoController videoController;

        private void Start()
        {
            videoController = FindObjectOfType<VideoController>();
            if (!videoController)
            {
                Debug.LogError("Error in SceneLoader: Couldn't find videoController in scene");
            }
        }

        public void TriggerShaderChange(bool turnOn)
        {
            videoController.toggleStereoMode(turnOn);
        }
    }
}