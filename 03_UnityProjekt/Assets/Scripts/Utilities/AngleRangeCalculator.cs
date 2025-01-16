using UnityEngine;

namespace Utilities
{
    public class RangeCalculator : MonoBehaviour
    {
        public static bool IsBetween(float number, float min, float max)
        {
            return number >= min && number <= max;
        }
    }
}