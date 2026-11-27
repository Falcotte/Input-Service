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
        [Range(0f, MaximumJoystickDeadZone)]
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

        internal const float MaximumJoystickDeadZone = 0.9f;

        private void OnValidate()
        {
            _minimum = new Vector2(Mathf.Clamp01(_minimum.x), Mathf.Clamp01(_minimum.y));
            _maximum = new Vector2(Mathf.Clamp(_maximum.x, _minimum.x, 1f), Mathf.Clamp(_maximum.y, _minimum.y, 1f));
            _joystickRadius = Mathf.Max(0f, _joystickRadius);
            _joystickDeadZone = Mathf.Clamp(_joystickDeadZone, 0f, MaximumJoystickDeadZone);
        }

        internal Rect GetPixelRect(Rect screenRect, Rect safeArea)
        {
            var baseRect = _space == InputAreaSpace.SafeArea ? safeArea : screenRect;

            var minimumX = Mathf.Clamp01(_minimum.x);
            var minimumY = Mathf.Clamp01(_minimum.y);
            var maximumX = Mathf.Clamp01(_maximum.x);
            var maximumY = Mathf.Clamp01(_maximum.y);

            var width = Mathf.Max(0f, maximumX - minimumX) * baseRect.width;
            var height = Mathf.Max(0f, maximumY - minimumY) * baseRect.height;

            return new Rect(baseRect.x + minimumX * baseRect.width, baseRect.y + minimumY * baseRect.height, width,
                height);
        }
    }
}
