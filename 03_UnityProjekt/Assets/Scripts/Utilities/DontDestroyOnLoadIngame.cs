using UnityEngine;

namespace Network
{
    public class DontDestroyOnLoadIngame : MonoBehaviour
    {
        private static DontDestroyOnLoadIngame instance;
        void Awake()
        {
            DontDestroyOnLoad(this);
            
            if (instance == null) {
                instance = this;
            } else {
                DestroyObject(gameObject);
            }
        }
    }
}