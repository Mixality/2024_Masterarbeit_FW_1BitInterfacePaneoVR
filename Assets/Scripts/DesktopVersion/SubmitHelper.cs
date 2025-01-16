using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using WebService;

namespace DesktopVersion
{
    public class SubmitHelper : MonoBehaviour
    {
        public UnityEvent OnSubmit;
        public VirtualKeyboardInputField PasswordInput;
        
        private void Update()
        {
            //use EventSystem, because isFocused doesn't work
            if (Keyboard.current != null && EventSystem.current.currentSelectedGameObject == PasswordInput.gameObject && (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame))
            {
                OnSubmit.Invoke();
            }
        }
    }
}