using System.Collections.Generic;
using Unity.Netcode;

namespace UnityEngine.UI.SaveSystem
{
    [System.Serializable]
    public class SceneData
    {
        public string sceneName;
        public string videoFileName;
        public int videoIsLooping;
        public int showInteractablesDelayed = 0;
        public float interactionSphereSize;
        public float xrRigRotation;
        public List<Interactable> interactables = new List<Interactable>();
        public float chartPosX, chartPosY; //node position in webApp flow chart
    }
    
    [System.Serializable]
    public class Interactable
    {
        public string id = System.Guid.NewGuid().ToString();
        public int type;
        public float locX, locY, locZ, rotX, rotY, rotZ, scaX, scaY, scaZ;
        public int showOptionsOnEnable; //only used for multiple choice interactables
        public string questionOnFailed; //only used if an interactable leads to scenario failing
        public List<TargetScene> targetScenes = new List<TargetScene>();
        public float chartPosX, chartPosY; //node position in webApp flow chart
    }

    [System.Serializable]
    public class TargetScene
    {
        public TargetScene(string label, string targetSceneName, string hiddenData)
        {
            id = System.Guid.NewGuid().ToString();
            this.hiddenData = hiddenData;
            this.label = label;
            this.targetSceneName = targetSceneName;
        }

        public string id;
        public string label;
        public string targetSceneName;
        public string hiddenData;
        public float chartPosX, chartPosY; //node position in webApp flow chart
    }
}