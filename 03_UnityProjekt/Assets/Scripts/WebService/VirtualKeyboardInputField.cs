using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.UI.SaveSystem;


namespace WebService
{
    public class VirtualKeyboardInputField : InputField
    {
        private UIController uiController;

        protected override void Start()
        {
            uiController = FindObjectOfType<UIController>();
            base.Start();
        }

        public override void OnSelect(BaseEventData eventData)
        {
            uiController.openGenericInputField(SetText);
            base.OnSelect(eventData);
        }

        private void SetText(string newText)
        {
            text = newText;
        }
    }
}