using System;
using UnityEngine;
using Utilities;

namespace DefaultNamespace
{
    public class UIViewRotator : MonoBehaviour
    {
        public GameObject Camera2Follow;
        public bool smoothTransition;
        public float smoothTime = 0.3F;
        public bool spawnFixedAtCurrentView;
        private float CameraDistance;
        private Vector3 velocity = Vector3.zero;
        private Vector3 initialCamTransformPoint;
        
        /// <summary>
        /// Has to be set from an external source, since there's no "inactive" check for owning gameObject to set it
        /// false again on deactivate
        /// </summary>
        internal bool hasNewFixedPosition;
        
        private void Start()
        {
            CameraDistance = Vector3.Distance(Camera2Follow.transform.position, transform.position);
            initialCamTransformPoint = Camera2Follow.transform.TransformPoint(new Vector3(0, 0, CameraDistance));
            initialCamTransformPoint = new Vector3(initialCamTransformPoint.x, 0, initialCamTransformPoint.z);
        }

        private void Update()
        {
            float currentAngle = Vector3.SignedAngle(initialCamTransformPoint, Camera2Follow.transform.forward, Vector3.up);
            Vector3 targetPosition;
            if (!spawnFixedAtCurrentView)
            {
                if (currentAngle < 0)
                {
                    currentAngle = 360 + currentAngle;
                }
                
                if (RangeCalculator.IsBetween(currentAngle, 45, 135))
                {
                    targetPosition = Quaternion.AngleAxis(90, Vector3.up) * initialCamTransformPoint;
                    targetPosition = new Vector3(targetPosition.x, Camera2Follow.transform.position.y,
                        targetPosition.z);
                }
                else if (RangeCalculator.IsBetween(currentAngle, 135, 225))
                {
                    targetPosition = Quaternion.AngleAxis(180, Vector3.up) * initialCamTransformPoint;
                    targetPosition = new Vector3(targetPosition.x, Camera2Follow.transform.position.y,
                        targetPosition.z);
                }
                else if (RangeCalculator.IsBetween(currentAngle, 225, 315))
                {
                    targetPosition = Quaternion.AngleAxis(-90, Vector3.up) * initialCamTransformPoint;
                    targetPosition = new Vector3(targetPosition.x, Camera2Follow.transform.position.y,
                        targetPosition.z);
                }
                else //default and between -45(225) and 45
                {
                    targetPosition = initialCamTransformPoint;
                    targetPosition = new Vector3(targetPosition.x, Camera2Follow.transform.position.y,
                        targetPosition.z);
                }
                if (smoothTransition)
                {
                    transform.position =
                        Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
                }
                else
                {
                    transform.position = targetPosition;
                }
                var lookAtPos = new Vector3(Camera2Follow.transform.position.x, transform.position.y, Camera2Follow.transform.position.z);
                transform.LookAt(2*transform.position - lookAtPos);  
            }
            else
            {
                if(!hasNewFixedPosition)
                {
                    targetPosition = Camera2Follow.transform.forward * CameraDistance;
                    targetPosition = new Vector3(targetPosition.x, transform.position.y, targetPosition.z);
                    transform.position = targetPosition;
                    var lookAtPos = new Vector3(Camera2Follow.transform.position.x, transform.position.y, Camera2Follow.transform.position.z);
                    transform.LookAt(2*transform.position - lookAtPos);
                    hasNewFixedPosition = true;
                }
            }
        }
    }
}