using UnityEngine;

namespace AngryKoala.Input
{
    public readonly struct JoystickData
    {
        public bool IsActive { get; }

        /// <summary>
        /// Where the joystick sits on the screen, in pixels.
        /// </summary>
        public Vector2 Center { get; }

        /// <summary>
        /// Where the handle sits on the screen, in pixels. Never further from the centre than the radius.
        /// </summary>
        public Vector2 HandlePosition { get; }

        /// <summary>
        /// In pixels.
        /// </summary>
        public float Radius { get; }

        /// <summary>
        /// Length 0 to 1. Zero inside the dead zone.
        /// </summary>
        public Vector2 Direction { get; }

        internal JoystickData(bool isActive, Vector2 center, Vector2 handlePosition, float radius, Vector2 direction)
        {
            IsActive = isActive;
            Center = center;
            HandlePosition = handlePosition;
            Radius = radius;
            Direction = direction;
        }
    }
}
