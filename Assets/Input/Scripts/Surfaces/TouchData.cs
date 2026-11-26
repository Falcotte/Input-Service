using UnityEngine;

namespace AngryKoala.Input
{
    public readonly struct TouchData
    {
        private readonly float _pixelsPerCentimeter;

        public int FingerId { get; }

        public Vector2 Position { get; }

        public Vector2 StartPosition { get; }

        /// <summary>
        /// The position inside the surface, from (0, 0) at its bottom left to (1, 1) at its top right.
        /// </summary>
        public Vector2 NormalizedPosition { get; }

        /// <summary>
        /// Movement since the last frame, in pixels.
        /// </summary>
        public Vector2 Delta { get; }

        public float Duration { get; }

        public bool WasCanceled { get; }

        public Vector2 TotalDelta => Position - StartPosition;

        public Vector2 DeltaCentimeters => ToCentimeters(Delta);

        public Vector2 TotalDeltaCentimeters => ToCentimeters(TotalDelta);

        internal TouchData(int fingerId, Vector2 position, Vector2 startPosition, Vector2 normalizedPosition,
            Vector2 delta, float duration, bool wasCanceled, float pixelsPerCentimeter)
        {
            FingerId = fingerId;
            Position = position;
            StartPosition = startPosition;
            NormalizedPosition = normalizedPosition;
            Delta = delta;
            Duration = duration;
            WasCanceled = wasCanceled;
            _pixelsPerCentimeter = pixelsPerCentimeter;
        }

        private Vector2 ToCentimeters(Vector2 pixels)
        {
            if (_pixelsPerCentimeter <= 0f)
            {
                return Vector2.zero;
            }

            return pixels / _pixelsPerCentimeter;
        }
    }
}
