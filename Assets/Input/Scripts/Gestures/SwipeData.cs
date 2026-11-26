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

        public SwipeDirection8 Direction8 { get; }

        /// <summary>
        /// Centimetres per second, over the whole swipe.
        /// </summary>
        public float Speed { get; }

        internal SwipeData(TouchData touch, SwipeDirection direction, SwipeDirection8 direction8, float speed)
        {
            Touch = touch;
            Vector = touch.TotalDelta;
            Direction = direction;
            Direction8 = direction8;
            Speed = speed;
        }
    }
}
