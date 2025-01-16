using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace DefaultNamespace
{
    public class ImageBuffer : MonoBehaviour
    {
        private AssetLoader assetloader;
        private List<KeyValuePair<string, Sprite>> imageBuffer = new List<KeyValuePair<string, Sprite>>();

        internal bool isLoadingBuffer { get; private set; }
        
        private void Start()
        {
            isLoadingBuffer = true;
            GameObject[] sceneManagerObjects = GameObject.FindGameObjectsWithTag("SceneManager");
            foreach (var sceneManagerObject in sceneManagerObjects)
            {
                if (sceneManagerObject.GetComponent<AssetLoader>())
                {
                    assetloader = sceneManagerObject.GetComponent<AssetLoader>();
                    break;
                }
            }

            if (assetloader)
            {
                
            }
            else
            {
                Debug.Log("Error in ImageBuffer: No AssetLoader found");
            }
        }

        internal void Initialize(string projectSrc)
        {
            StartCoroutine(LoadImageBuffer(projectSrc));
        }
        
        private IEnumerator LoadImageBuffer(string projectSrc)
        {
            isLoadingBuffer = true;
            string[] imageFiles = assetloader.getAllImageFilesForProject(projectSrc);
            foreach (var imageFile in imageFiles)
            {
                byte[] imgData;
                Texture2D tex = new Texture2D(2, 2);
                imgData = assetloader.getBytesFromImageFile(imageFile);
                tex.LoadImage(imgData);
                Sprite sp = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
                imageBuffer.Add(new KeyValuePair<string, Sprite>(Path.GetFileName(Path.GetDirectoryName(imageFile)), sp));
            }
            yield return isLoadingBuffer = false;
        }

        public Sprite getImageWithSrc(string imageSrc)
        {
            foreach (var keyValuePair in imageBuffer)
            {
                if (keyValuePair.Key.Equals(imageSrc))
                {
                    return keyValuePair.Value;
                }
            }
            Debug.Log("Image not found in Buffer: " + imageSrc);
            return null;
        }

        internal int getBufferSize()
        {
            return imageBuffer.Count;
        }
        
        public KeyValuePair<string, Sprite> getImageWithIndex(int index)
        {
            if (index < imageBuffer.Count && index >= 0)
            {
                return imageBuffer[index];
            }
            Debug.Log("ImageBuffer index not valid: " + index);
            return new KeyValuePair<string, Sprite>();
        }
    }
}