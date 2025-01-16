using System;
//using Unity.XR.PXR;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace DefaultNamespace
{
    public class XRUIButtonHapticFeedback : MonoBehaviour, IPointerEnterHandler
    {
        public bool hapticFeedbackOnDisabled;

        private Button button;
        private InputField input;
        private Toggle toggle;
        
        private void Start()
        {
            button = GetComponent<Button>();
            input = GetComponent<InputField>();
            toggle = GetComponent<Toggle>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!button && !input && !toggle) return;
            
            //Use custom class XRControllerEventData 
            if (eventData is TrackedDeviceEventData trackedDeviceEventData)
            {
                if (button)
                {
                    if (button.interactable || hapticFeedbackOnDisabled)
                    {
                        if (trackedDeviceEventData.interactor is XRBaseControllerInteractor xrInteractor)
                        {
                            xrInteractor.SendHapticImpulse(0.4f, 0.05f);
                           /* if (xrInteractor.gameObject.tag.Equals("RightHand"))
                            {
                                PXR_Input.SendHapticImpulse(PXR_Input.VibrateType.RightController, 0.4f, 50, 100);
                            }
                            else
                            {
                                PXR_Input.SendHapticImpulse(PXR_Input.VibrateType.LeftController, 0.4f, 50, 100);
                            }*/
                        }
                    }
                }

                if (input)
                {
                    if (input.interactable || hapticFeedbackOnDisabled)
                    {
                        if (trackedDeviceEventData.interactor is XRBaseControllerInteractor xrInteractor)
                        {
                            xrInteractor.SendHapticImpulse(0.3f, 0.05f);
                            /*if (xrInteractor.gameObject.tag.Equals("RightHand"))
                            {
                                PXR_Input.SendHapticImpulse(PXR_Input.VibrateType.RightController, 0.3f, 50, 100);
                            }
                            else
                            {
                                PXR_Input.SendHapticImpulse(PXR_Input.VibrateType.LeftController, 0.3f, 50, 100);
                            }*/
                        }
                    }
                }

                if (toggle)
                {
                    if (toggle.interactable || hapticFeedbackOnDisabled)
                    {
                        if (trackedDeviceEventData.interactor is XRBaseControllerInteractor xrInteractor)
                        {
                            xrInteractor.SendHapticImpulse(0.3f, 0.05f);
                            /*if (xrInteractor.gameObject.tag.Equals("RightHand"))
                            {
                                PXR_Input.SendHapticImpulse(PXR_Input.VibrateType.RightController, 0.3f, 50, 100);
                            }
                            else
                            {
                                PXR_Input.SendHapticImpulse(PXR_Input.VibrateType.LeftController, 0.3f, 50, 100);
                            }*/
                        }
                    }
                }
            }
        }
    }
}