using UnityEngine;

namespace Assets.GameLogic
{
    public sealed class Utils
    {
        /// <summary>
        /// Map value from one range to another.
        /// </summary>
        public static float Map(float newMin, float newMax, float origMin, float origMax, float value) 
            => Mathf.Lerp(newMin, newMax, Mathf.InverseLerp(origMin, origMax, value));
    }
}
