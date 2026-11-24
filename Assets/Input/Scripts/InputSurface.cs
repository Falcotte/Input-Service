using System;
using UnityEngine;

namespace AngryKoala.Input
{
    /// <summary>
    /// One surface: fingers and times go in, state and gestures come out. It does not read input itself.
    /// </summary>
    internal sealed class InputSurface : IInputSurface
    {
        public string Name { get; }

        public InputArea Area { get; }

        public Rect PixelRect { get; private set; }

        public string LastGesture { get; private set; }

        public bool IsTouching => TouchCount > 0;

        public int TouchCount => 0;

        public TouchData PrimaryTouch => default;

        public JoystickData Joystick => default;

        public event Action<TouchData> OnTouchBegan;
        public event Action<TouchData> OnTouchEnded;
        public event Action<TouchData> OnTap;
        public event Action<TouchData> OnHold;
        public event Action<TouchData> OnDragBegan;
        public event Action<TouchData> OnDrag;
        public event Action<TouchData> OnDragEnded;
        public event Action<SwipeData> OnSwipe;
        public event Action<PinchData> OnPinch;

        public InputSurface(string name, InputArea area)
        {
            Name = name;
            Area = area;
        }

        /// <summary>
        /// Sets what the surface measures with. Called before the fingers of a frame are fed in.
        /// </summary>
        public void SetContext(GestureRules rules, float pixelsPerCentimeter, Rect pixelRect)
        {
        }

        public void BeginTouch(int fingerId, Vector2 position, double time)
        {
        }

        /// <summary>
        /// Called once per frame for every finger that is down, whether it moved or not.
        /// </summary>
        public void UpdateTouch(int fingerId, Vector2 position, double time)
        {
        }

        public void EndTouch(int fingerId, Vector2 position, double time)
        {
        }

        public void CancelTouch(int fingerId, double time)
        {
        }

        public void CancelAllTouches(double time)
        {
        }

        /// <summary>
        /// Called once per frame after every finger has been fed in. Gestures that need two fingers are decided here.
        /// </summary>
        public void CompleteFrame(double time)
        {
        }
    }
}
