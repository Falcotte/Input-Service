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

        /// <summary>
        /// The mode of the joystick on this surface: the one set here, otherwise the one of the area asset. Setting it
        /// overrides the asset for this surface only and never changes the asset. The most recent change wins: when
        /// the mode of the asset changes afterwards, the surface follows the asset again. The screen surface has no
        /// joystick: it stays None and setting another mode logs a warning.
        /// </summary>
        JoystickMode JoystickMode { get; set; }

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
