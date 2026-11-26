using UnityEngine;

namespace AngryKoala.Input
{
    public readonly struct PinchData
    {
        /// <summary>
        /// The point halfway between the two fingers, in pixels.
        /// </summary>
        public Vector2 Center { get; }

        /// <summary>
        /// The distance between the fingers now, divided by the distance in the last frame. 1 means no change.
        /// </summary>
        public float ScaleDelta { get; }

        /// <summary>
        /// The distance between the fingers now, divided by the distance when the pinch began.
        /// </summary>
        public float TotalScale { get; }

        internal PinchData(Vector2 center, float scaleDelta, float totalScale)
        {
            Center = center;
            ScaleDelta = scaleDelta;
            TotalScale = totalScale;
        }
    }
}
