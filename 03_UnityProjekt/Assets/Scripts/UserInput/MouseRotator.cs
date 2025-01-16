using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UserInput
{
    public class MouseRotator : MonoBehaviour
    {
        public float speedH = 0.7f;
        public float speedV = 0.7f;

        private float yaw = 0.0f;
        private float pitch = 0.0f;
        
        private void Update()
        {
            if (Mouse.current.rightButton.isPressed)
            {
                yaw += speedH * Mouse.current.delta.x.ReadValue();
                pitch -= speedV * Mouse.current.delta.y.ReadValue();

                transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
            }
        }

        public void resetView()
        {
            transform.localRotation = new Quaternion(0, 0, 0, 0);
        }
    }
}