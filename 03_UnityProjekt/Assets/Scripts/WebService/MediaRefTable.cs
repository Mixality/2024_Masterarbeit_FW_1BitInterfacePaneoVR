using System;
using System.Collections.Generic;

namespace UnityEngine.UI.WebService
{
    [Serializable]
    public class MediaRefTable
    {
        public List<MediaReference> imageRefs = new List<MediaReference>();
        public List<MediaReference> audioRefs = new List<MediaReference>();
        public List<MediaReference> videoRefs = new List<MediaReference>();

        public string getNameFromAudioSrc(string src)
        {
            foreach (var audioRef in audioRefs)
            {
                if (audioRef.src.Equals(src))
                {
                    return audioRef.name;
                }
            }
            return "undefined";
        }
        
        public string getNameFromVideoSrc(string src)
        {
            foreach (var videoRef in videoRefs)
            {
                if (videoRef.src.Equals(src))
                {
                    return videoRef.name;
                }
            }
            return "undefined";
        }
        
        public string getNameFromImageSrc(string src)
        {
            foreach (var imageRef in imageRefs)
            {
                if (imageRef.src.Equals(src))
                {
                    return imageRef.name;
                }
            }
            return "undefined";
        }
    }

    [Serializable]
    public class MediaReference : IEquatable<MediaReference>
    {
        public string src;
        public string name;
        
        public override bool Equals(object obj)
        {
            return Equals(obj as MediaReference);
        }

        public bool Equals(MediaReference other)
        {
            return other != null && src == other.src;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(src);
        }
    }
}