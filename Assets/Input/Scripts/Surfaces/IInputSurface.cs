using System;

namespace AngryKoala.Input
{
    /// <summary>
    /// The whole screen or one area of it. State is polled; events are raised when something happens.
    /// </summary>
    public interface IInputSurface
    {
        bool IsTouching { get; }

        int TouchCount { get; }

        /// <summary>
        /// The first finger that went down on this surface. Default while nothing touches it.
        /// </summary>
        TouchData PrimaryTouch { get; }

        JoystickData Joystick { get; }

        event Action<TouchData> OnTouchBegan;
        event Action<TouchData> OnTouchEnded;
        event Action<TouchData> OnTap;
        event Action<TouchData> OnHold;
        event Action<TouchData> OnDragBegan;
        event Action<TouchData> OnDrag;
        event Action<TouchData> OnDragEnded;
        event Action<SwipeData> OnSwipe;
        event Action<PinchData> OnPinch;
    }
}
