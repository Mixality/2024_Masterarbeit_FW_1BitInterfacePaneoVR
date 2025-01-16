using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace DefaultNamespace
{
    public class TriggerContextMenuHandler : MonoBehaviour
    {
        private GameObject selectedInteractable;
        private CanvasGroup canvasGroup;

        private void Start()
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        private void Update()
        {
            if(gameObject.activeSelf && (Mouse.current.rightButton.wasPressedThisFrame || Mouse.current.leftButton.wasPressedThisFrame))
            {
                if(!EventSystem.current.IsPointerOverGameObject())
                {
                    hideMenu();
                }
                
            }
        }

        public void deleteSelectedInteractable()
        {
            Destroy(selectedInteractable);
            hideMenu();
        }

        internal void setSelectedInteractable(GameObject selectedObject)
        {
            selectedInteractable = selectedObject;
        }

        internal void showMenu()
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
            GetComponent<RectTransform>().position = new Vector3(Mouse.current.position.ReadValue().x, Mouse.current.position.ReadValue().y, 0);
        }

        private void hideMenu()
        {
            canvasGroup.alpha = 0;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
    }
}