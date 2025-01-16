using System;
using DefaultNamespace;
using Unity.Netcode;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.UI;
using Random = UnityEngine.Random;
using XRController = UnityEngine.InputSystem.XR.XRController;

namespace Network
{
    public class NetworkPlayer : NetworkBehaviour
    {
        [SerializeField] private Vector2 placementArea = new Vector2(-10.0f, 10.0f);

        public XRBaseController LeftController;
        public XRBaseController RightController;

        public GameObject UIAnchor;
        public GameObject CameraCurtain;
        
        public override void OnNetworkSpawn()
        {
            DisableClientInput();
            if (IsLocalPlayer)
            {
                var clientCamera = GetComponentInChildren<Camera>();
                for (int i = 0; i < clientCamera.transform.childCount; i++)
                {
                    if (clientCamera.transform.GetChild(i).tag.Equals("PlayerAvatar"))
                    {
                        clientCamera.transform.GetChild(i).gameObject.transform.Translate(0,0,-0.15f);
                    }
                }

                //important to get the right XRRig for good transitions to the loading sphere XRRig on scene changes;
                gameObject.tag = "MainXRRig";
            }
            else
            {
                UIAnchor.SetActive(false);
                CameraCurtain.SetActive(false);
            }
        }

        public void DisableClientInput() {
            if (IsClient && !IsOwner)
            {
                var clientMoveProvider = GetComponent<NetworkMoveProvider>();
                var clientControllers = GetComponentsInChildren<ActionBasedController>();
                var clientTurnProvider = GetComponent<ActionBasedSnapTurnProvider>();
                var clientHead = GetComponentInChildren<TrackedPoseDriver>();
                var clientCamera = GetComponentInChildren<Camera>();

                clientCamera.enabled = false;
                clientMoveProvider.enableInputActions = false;
                clientTurnProvider.enableTurnAround = false;
                clientTurnProvider.enableTurnAround = false;
                clientHead.enabled = false;

                foreach (var controller in clientControllers)
                {
                    controller.enableInputActions = false;
                    controller.enableInputTracking = false;
                }
            }
        }

        private void Start()
        {
            /*if (IsClient && IsOwner)
            {
                transform.position = new Vector3(Random.Range(placementArea.x, placementArea.y), transform.position.y,
                    Random.Range(placementArea.x, placementArea.y));
            }*/
        }

        public void OnSelectGrabbable(SelectEnterEventArgs eventArgs)
        {
            if (IsClient && IsOwner)
            {
                NetworkObject networkObjectSelected =
                    eventArgs.interactableObject.transform.GetComponent<NetworkObject>();
                if (networkObjectSelected != null)
                {
                    // request owenership from the server
                    RequestGrabbableOwnershipServerRpc(OwnerClientId, networkObjectSelected);
                }
            }
        }

        [ServerRpc]
        public void RequestGrabbableOwnershipServerRpc(ulong newOwnerClientId,
            NetworkObjectReference networkObjectReference)
        {
            if (networkObjectReference.TryGet(out NetworkObject networkObject))
            {
                networkObject.ChangeOwnership(newOwnerClientId);
            }
            else
            {
                Debug.LogWarning($"Unable to change ownership for clientId {newOwnerClientId}");
            }
        }
    }
}