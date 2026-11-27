using UnityEngine;

namespace AngryKoala.Input
{
    public readonly struct InputSurfaceData
    {
        public string Name { get; }

        /// <summary>
        /// Null for the screen surface.
        /// </summary>
        public InputArea Area { get; }

        /// <summary>
        /// In pixels, from the bottom left of the screen.
        /// </summary>
        public Rect PixelRect { get; }

        public int TouchCount { get; }

        public TouchData PrimaryTouch { get; }

        public JoystickData Joystick { get; }

        public string LastGesture { get; }

        internal InputSurfaceData(string name, InputArea area, Rect pixelRect, int touchCount, TouchData primaryTouch,
            JoystickData joystick, string lastGesture)
        {
            Name = name;
            Area = area;
            PixelRect = pixelRect;
            TouchCount = touchCount;
            PrimaryTouch = primaryTouch;
            Joystick = joystick;
            LastGesture = lastGesture;
        }
    }
}
