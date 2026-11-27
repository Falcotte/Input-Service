using System;
using AngryKoala.Services;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace AngryKoala.Input
{
    /// <summary>
    /// Draws the joystick of an input area on a UI Toolkit document.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class VirtualJoystickView : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The area whose joystick is drawn. Empty draws nothing.")]
        private InputArea _area;
        public InputArea Area => _area;

        [SerializeField]
        [Tooltip("The colour of the circle the handle moves in.")]
        private Color _backgroundColor = new(1f, 1f, 1f, 0.25f);
        public Color BackgroundColor => _backgroundColor;

        [SerializeField]
        [Tooltip("The colour of the handle.")]
        private Color _handleColor = new(1f, 1f, 1f, 0.6f);
        public Color HandleColor => _handleColor;

        [SerializeField]
        [Tooltip("The colour of the line around the background and the handle.")]
        private Color _outlineColor = new(0f, 0f, 0f, 0.35f);
        public Color OutlineColor => _outlineColor;

        [SerializeField]
        [Min(0f)]
        [Tooltip("The width of that line, in panel pixels. 0 draws no line.")]
        private float _outlineWidth = 2f;
        public float OutlineWidth => _outlineWidth;

        [SerializeField]
        [Range(0.1f, 1f)]
        [Tooltip("The diameter of the handle, as a fraction of the diameter of the background.")]
        private float _handleSize = 0.5f;
        public float HandleSize => _handleSize;

        [SerializeField]
        [Tooltip("Fade the joystick out while no finger drives it. Applies to floating and following joysticks. A fixed joystick is always shown.")]
        private bool _hideWhenIdle = true;
        public bool HideWhenIdle => _hideWhenIdle;

        [SerializeField]
        [Min(0f)]
        [Tooltip("How long the joystick takes to fade in or out, in seconds. Not affected by the time scale.")]
        private float _fadeDuration = 0.15f;
        public float FadeDuration => _fadeDuration;

        [SerializeField]
        [Min(0f)]
        [Tooltip("How long the handle, and a joystick that does not hide, take to move back after the finger lifts, in seconds. Not affected by the time scale.")]
        private float _returnDuration = 0.1f;
        public float ReturnDuration => _returnDuration;

        internal VisualElement Background => _background;

        internal VisualElement Handle => _handle;

        private UIDocument _document;
        private VisualElement _background;
        private VisualElement _handle;
        private IInputService _service;
        private IInputSurface _surface;
        private float _opacity;

        private Vector2 _shownCenter;
        private Vector2 _shownHandleOffset;
        private Vector2 _returnStartCenter;
        private Vector2 _returnStartHandleOffset;
        private float _returnElapsed;
        private bool _wasActive;
        private bool _isReturning;
        private bool _isReturnHiding;
        private bool _hasShownCenter;

        private const float _fullOpacity = 1f;
        private const float _hiddenOpacity = 0f;
        private const float _completedProgress = 1f;

        private const string _backgroundClassName = "angry-koala-joystick__background";
        private const string _handleClassName = "angry-koala-joystick__handle";

        private void Awake()
        {
            _document = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            _background = CreateCircle(_backgroundClassName, _backgroundColor, _outlineColor, _outlineWidth);
            _handle = CreateCircle(_handleClassName, _handleColor, _outlineColor, _outlineWidth);
            _opacity = _hideWhenIdle ? _hiddenOpacity : _fullOpacity;
            ResetShownState();

            AttachElements();

            ServiceLocator.OnServiceRegistered += HandleServiceRegistered;
            ServiceLocator.OnServiceDeregistered += HandleServiceDeregistered;

            if (ServiceLocator.TryGet<IInputService>(out var service))
            {
                _service = service;
                ResolveSurface();
            }
        }

        private void OnDisable()
        {
            ServiceLocator.OnServiceRegistered -= HandleServiceRegistered;
            ServiceLocator.OnServiceDeregistered -= HandleServiceDeregistered;

            _background.RemoveFromHierarchy();
            _handle.RemoveFromHierarchy();

            _background = null;
            _handle = null;
            _service = null;
            _surface = null;
        }

        private void HandleServiceRegistered(Type type, IService service)
        {
            if (type != typeof(IInputService) || service is not IInputService inputService)
            {
                return;
            }

            _service = inputService;
            ResolveSurface();
        }

        private void HandleServiceDeregistered(Type type, IService service)
        {
            if (type != typeof(IInputService) || !ReferenceEquals(_service, service))
            {
                return;
            }

            _service = null;
            _surface = null;
        }

        private void ResolveSurface()
        {
            _surface = null;

            if (_area == null || _service == null || IsDestroyed(_service))
            {
                return;
            }

            _surface = _service.GetArea(_area);
        }

        #region Internal Methods

        internal void SetArea(InputArea area)
        {
            _area = area;

            ResetShownState();
            ResolveSurface();
        }

        #endregion

        private void LateUpdate()
        {
            if (!AttachElements())
            {
                return;
            }

            if (_service != null && IsDestroyed(_service))
            {
                _service = null;
                _surface = null;
            }

            var mode = _surface != null ? _surface.JoystickMode : JoystickMode.None;
            var hasJoystick = mode != JoystickMode.None;
            var display = hasJoystick ? DisplayStyle.Flex : DisplayStyle.None;

            _background.style.display = display;
            _handle.style.display = display;

            if (!hasJoystick)
            {
                _opacity = _hideWhenIdle ? _hiddenOpacity : _fullOpacity;
                ResetShownState();

                return;
            }

            var joystick = _surface.Joystick;

            UpdateOpacity(joystick.IsActive, mode);
            UpdateShownState(joystick, mode);
            UpdatePlacement(joystick.Radius);
        }

        private bool AttachElements()
        {
            var root = _document.rootVisualElement;

            if (root == null || root.panel == null)
            {
                return false;
            }

            if (_background.parent != root)
            {
                root.Add(_background);
            }

            if (_handle.parent != root)
            {
                root.Add(_handle);
            }

            return true;
        }

        private void UpdateOpacity(bool isActive, JoystickMode mode)
        {
            var target = !_hideWhenIdle || isActive || mode == JoystickMode.Fixed ? _fullOpacity : _hiddenOpacity;

            if (_fadeDuration <= 0f || mode == JoystickMode.Fixed)
            {
                _opacity = target;
            }
            else
            {
                _opacity = Mathf.MoveTowards(_opacity, target, Time.unscaledDeltaTime / _fadeDuration);
            }

            _background.style.opacity = _opacity;
            _handle.style.opacity = _opacity;
        }

        private void UpdateShownState(JoystickData joystick, JoystickMode mode)
        {
            if (joystick.IsActive)
            {
                _shownCenter = joystick.Center;
                _shownHandleOffset = joystick.HandlePosition - joystick.Center;
                _wasActive = true;
                _isReturning = false;
                _hasShownCenter = true;

                return;
            }

            if (_wasActive)
            {
                _wasActive = false;
                _isReturning = true;
                _returnElapsed = 0f;
                _returnStartCenter = _shownCenter;
                _returnStartHandleOffset = _shownHandleOffset;
                _isReturnHiding = IsHiding(mode);
            }

            if (_isReturning)
            {
                AdvanceReturn(joystick);

                return;
            }

            _shownHandleOffset = Vector2.zero;

            if (IsHiding(mode) && _hasShownCenter)
            {
                return;
            }

            _shownCenter = joystick.Center;
            _hasShownCenter = false;
        }

        private void AdvanceReturn(JoystickData joystick)
        {
            _returnElapsed += Time.unscaledDeltaTime;

            var progress = _returnDuration <= 0f ? _completedProgress : Mathf.Clamp01(_returnElapsed / _returnDuration);
            var remaining = 1f - progress;
            var eased = 1f - remaining * remaining;

            _shownHandleOffset = Vector2.Lerp(_returnStartHandleOffset, Vector2.zero, eased);
            _shownCenter = _isReturnHiding ? _returnStartCenter : Vector2.Lerp(_returnStartCenter, joystick.Center, eased);

            if (progress >= _completedProgress)
            {
                _isReturning = false;
                _hasShownCenter = _isReturnHiding;
            }
        }

        private void ResetShownState()
        {
            _wasActive = false;
            _isReturning = false;
            _hasShownCenter = false;
        }

        private bool IsHiding(JoystickMode mode) => _hideWhenIdle && mode != JoystickMode.Fixed;

        private void UpdatePlacement(float radius)
        {
            var panel = _background.panel;

            var center = ScreenToPanel(panel, _shownCenter);
            var radiusEdge = ScreenToPanel(panel, _shownCenter + new Vector2(radius, 0f));
            var diameter = 2f * Mathf.Abs(radiusEdge.x - center.x);
            var handleCenter = ScreenToPanel(panel, _shownCenter + _shownHandleOffset);

            Place(_background, center, diameter);
            Place(_handle, handleCenter, diameter * _handleSize);
        }

        private static void Place(VisualElement element, Vector2 center, float diameter)
        {
            element.style.left = center.x - diameter * 0.5f;
            element.style.top = center.y - diameter * 0.5f;
            element.style.width = diameter;
            element.style.height = diameter;
        }

        #region Utility

        private static VisualElement CreateCircle(string className, Color color, Color outlineColor, float outlineWidth)
        {
            var element = new VisualElement();
            element.AddToClassList(className);
            element.pickingMode = PickingMode.Ignore;

            var halfLength = new Length(50f, LengthUnit.Percent);

            element.style.position = Position.Absolute;
            element.style.display = DisplayStyle.None;
            element.style.borderTopLeftRadius = halfLength;
            element.style.borderTopRightRadius = halfLength;
            element.style.borderBottomLeftRadius = halfLength;
            element.style.borderBottomRightRadius = halfLength;
            element.style.backgroundColor = color;
            element.style.borderTopWidth = outlineWidth;
            element.style.borderRightWidth = outlineWidth;
            element.style.borderBottomWidth = outlineWidth;
            element.style.borderLeftWidth = outlineWidth;
            element.style.borderTopColor = outlineColor;
            element.style.borderRightColor = outlineColor;
            element.style.borderBottomColor = outlineColor;
            element.style.borderLeftColor = outlineColor;

            return element;
        }

        private static Vector2 ScreenToPanel(IPanel panel, Vector2 screenPosition)
        {
            return RuntimePanelUtils.ScreenToPanel(panel, new Vector2(screenPosition.x, Screen.height - screenPosition.y));
        }

        private static bool IsDestroyed(IInputService service)
        {
            return service is Object unityObject && unityObject == null;
        }

        #endregion
    }
}
