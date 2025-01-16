using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DesktopVersion
{
    public class ScreenCameraRotator : MonoBehaviour
    {
        private float rotationSpeed = 0.08f; 
        private float verticalRotationLimit = 80.0f; 
        private float rotationDecay = 0.96f; // Decay factor for rotation
        
        private float currentRotationX = 0.0f; // Current vertical rotation
        private Vector2 rotationVelocity = Vector2.zero; // Velocity vector for the rotation
        private bool isDragging;

        internal bool isRotationControlActive = true;
        
        
        void Update()
        {
            Tuple<float, float> inputPosition = GetInputPosition();

            if (isRotationControlActive && (Mouse.current.leftButton.isPressed || (Touchscreen.current != null && Touchscreen.current.touches[0].press.isPressed)))
            {
                isDragging = true;
                rotationVelocity = new Vector2(-inputPosition.Item1, inputPosition.Item2) * rotationSpeed;
            }
            else if (isDragging)
            {
                // Start decaying the rotation when the input ends
                isDragging = false;
            }

            // Apply the decay rate to the rotation velocity
            rotationVelocity *= rotationDecay;

            // Calculate new rotations
            transform.Rotate(Vector3.up, rotationVelocity.x, Space.World);
            currentRotationX += rotationVelocity.y;
            currentRotationX = Mathf.Clamp(currentRotationX, -verticalRotationLimit, verticalRotationLimit);

            // Apply the rotation
            transform.rotation = Quaternion.Euler(currentRotationX, transform.rotation.eulerAngles.y, 0.0f);

            // Completely stop the rotation when the velocity is very low
            if (rotationVelocity.magnitude < 0.01f)
            {
                rotationVelocity = Vector2.zero;
            }
        }
        
        private Tuple<float, float> GetInputPosition()
        {
            Tuple<float, float> inputPosition = new Tuple<float, float>(0, 0);

            if (Mouse.current != null)
            {
                float mouseX = Mouse.current.delta.x.ReadValue();
                float mouseY = Mouse.current.delta.y.ReadValue();
                inputPosition = new Tuple<float, float>(mouseX, mouseY);
            }

            if (Touchscreen.current != null)
            {
                float touchX = Touchscreen.current.touches[0].delta.x.ReadValue();
                float touchY = Touchscreen.current.touches[0].delta.y.ReadValue();
                if (touchX != 0 || touchY != 0)
                {
                    inputPosition = new Tuple<float, float>(touchX, touchY);
                }
            }

            return inputPosition;
        }
    }
}