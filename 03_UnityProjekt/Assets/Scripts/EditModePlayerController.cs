using System;
using UnityEngine;
using UnityEngine.UI.SaveSystem;
using Utilities;

namespace DefaultNamespace
{
    public class EditModePlayerController : MonoBehaviour
    {
        private SaveController saveController;

        private VideoController videoController;
        
        private ProjectController projectController;
        
        private EditModeController editModeController;

        private InteractionSphere360 interactionSphere360;
        
        public XRRigRotator xrRigRotator;

        private void Awake()
        {
            saveController = FindObjectOfType<SaveController>();
            if (!saveController)
            {
                Debug.LogError("Error in EditModeController: Couldn't find SaveController in Scene");
            }
        
            videoController = FindObjectOfType<VideoController>();
            if (!videoController)
            {
                Debug.LogError("Error in EditModeController: Couldn't find VideoController in Scene");
            }
            
            editModeController = FindObjectOfType<EditModeController>();
            if (!editModeController)
            {
                Debug.LogError("Error in EditModeController: Couldn't find EditModeController in Scene");
            }

            interactionSphere360 = FindObjectOfType<InactivityRefHandler>().interactionSphere360;
            if (!interactionSphere360)
            {
                Debug.LogError("Error in EditModeController: Couldn't find InteractionSphere360 in Scene");
            }
        }

        public void RotateXRRigTo(float angle)
        {
            xrRigRotator.rotateTo(angle);
        }
        
        public void setCurrentSceneName(string sceneName)
        {
            saveController.renameCurrentScene(sceneName);
        }

        public void setStateToUnsaved()
        {
            saveController.setEditStateToUnsaved();
        }

        public void LoadScene(string name)
        {
            saveController.loadScene(name);
        }

        public void UpdateCurrentSceneData()
        {
            saveController.updateCurrentSceneData();
        }
        
        public void AdaptInteractionSphereSize(float size)
        {
            interactionSphere360.adaptSphereSize(size);
        }

        public void SetSceneVideoLooping(bool loop)
        {
            videoController.setIsVideoLoopingWithReloadCheck(loop);
        }
        
        public void SetLoadInteractablesDelayed(bool delayed)
        {
            videoController.showInteractablesDelayed = delayed;
        }

        public void SwitchPlayMode(bool isEditOn)
        {
            editModeController.toggleEditUI(isEditOn);
        }
    }
}