using UnityEngine;

namespace AngryKoala.Input
{
    public readonly struct SwipeData
    {
        public TouchData Touch { get; }

        /// <summary>
        /// From where the touch began to where the swipe was recognised, in pixels.
        /// </summary>
        public Vector2 Vector { get; }

        public SwipeDirection Direction { get; }

        public SwipeCompassDirection CompassDirection { get; }

        /// <summary>
        /// Centimetres per second, over the whole swipe.
        /// </summary>
        public float Speed { get; }

        internal SwipeData(TouchData touch, SwipeDirection direction, SwipeCompassDirection compassDirection, float speed)
        {
            Touch = touch;
            Vector = touch.TotalDelta;
            Direction = direction;
            CompassDirection = compassDirection;
            Speed = speed;
        }
    }
}
