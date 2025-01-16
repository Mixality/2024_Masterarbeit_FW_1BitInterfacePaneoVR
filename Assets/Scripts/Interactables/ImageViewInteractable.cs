using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace Interactables
{
    /// <summary>
    /// Like the investigate interactable, but with the image as trigger sprite, not the magnifier
    /// </summary>
    public class ImageViewInteractable : InvestigateInteractable
    {
        public Button triggerButton;


        protected override void Awake()
        {
            base.Awake();
            type = 7;
        }

        protected override void setImage(string imageSrc)
        {
            Sprite sp = ImageBuffer.getImageWithSrc(imageSrc);
            viewImage = sp;
            setTargetLabel(imageSrc);
            triggerButton.GetComponent<Image>().sprite = sp;
        }
        
        protected override void setImage(Sprite image, string imageSrc)
        {
            if (image)
            {
                viewImage = image;
                triggerButton.GetComponent<Image>().sprite = image;
                setTargetLabel(imageSrc);
            }
            else
            {
                Debug.Log("ERROR in ImageViewInteractable: Image not set");
            }
        }
    }
}