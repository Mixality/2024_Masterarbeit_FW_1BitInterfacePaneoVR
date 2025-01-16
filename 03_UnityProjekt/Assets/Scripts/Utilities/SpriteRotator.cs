using UnityEngine;

namespace Utilities
{
    public class SpriteRotator : MonoBehaviour
    {
        float rotationsPerMinute = 30.0f;
        
        void Update()
        {
            transform.Rotate(0,0,6.0f*rotationsPerMinute*Time.deltaTime);
        }
    }
}