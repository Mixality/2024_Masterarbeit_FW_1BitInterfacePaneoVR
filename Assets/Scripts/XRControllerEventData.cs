using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.Interaction.Toolkit;

namespace DefaultNamespace
{
    public class XRControllerEventData : PointerEventData
    {
        /// <summary>
        /// Custom Event data 
        /// </summary>
        public XRControllerEventData(EventSystem eventSystem) : base(eventSystem)
        {
        }
        
        /// <summary>
        /// The Interactor that triggered this event, or <see langword="null"/> if no interactor was found.
        /// </summary>
        public XRBaseControllerInteractor pointingController { get; set; }
    }
}