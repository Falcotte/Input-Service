using UnityEngine;

namespace AngryKoala.Input
{
    /// <summary>
    /// A rectangle of the screen that gets its own surface. The asset is also the key to ask the input service with.
    /// </summary>
    [CreateAssetMenu(fileName = "InputArea", menuName = "Angry Koala/Input/Input Area")]
    public sealed class InputArea : ScriptableObject
    {
        [SerializeField]
        [Tooltip("What the rectangle is a fraction of.")]
        private InputAreaSpace _space = InputAreaSpace.SafeArea;
        public InputAreaSpace Space => _space;

        [SerializeField]
        [Tooltip("The bottom left corner, from (0, 0) to (1, 1).")]
        private Vector2 _minimum = Vector2.zero;
        public Vector2 Minimum => _minimum;

        [SerializeField]
        [Tooltip("The top right corner, from (0, 0) to (1, 1).")]
        private Vector2 _maximum = Vector2.one;
        public Vector2 Maximum => _maximum;

        [SerializeField]
        [Tooltip("The joystick this area drives, if any.")]
        private JoystickMode _joystickMode = JoystickMode.None;
        public JoystickMode JoystickMode => _joystickMode;

        [SerializeField]
        [Tooltip("How far the handle can move from the centre, in centimetres.")]
        private float _joystickRadius = 1f;
        public float JoystickRadius => _joystickRadius;

        [SerializeField]
        [Range(0f, 0.9f)]
        [Tooltip("The part of the radius around the centre that gives no direction.")]
        private float _joystickDeadZone = 0.1f;
        public float JoystickDeadZone => _joystickDeadZone;

        [SerializeField]
        [Tooltip("Where a fixed joystick sits inside the area, from (0, 0) to (1, 1).")]
        private Vector2 _fixedJoystickPosition = new(0.5f, 0.5f);
        public Vector2 FixedJoystickPosition => _fixedJoystickPosition;

        [SerializeField]
        [Tooltip("Optional. Empty means the input service's gesture settings.")]
        private GestureSettings _gestureSettings;
        public GestureSettings GestureSettings => _gestureSettings;

        internal Rect GetPixelRect(Rect screenRect, Rect safeArea)
        {
            return default;
        }
    }
}
