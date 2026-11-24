using System;
using System.Collections.Generic;
using AngryKoala.Services;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Object = UnityEngine.Object;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace AngryKoala.Input
{
    public sealed class InputService : BaseService<IInputService>, IInputService
    {
        private struct TrackedFinger
        {
            public int Id;
            public InputSurface Owner;
            public bool IsSeen;
        }

        [SerializeField]
        [Tooltip("Optional. Empty means the default thresholds.")]
        private GestureSettings _gestureSettings;

        [SerializeField]
        [Tooltip("The areas of the screen, in order. Where two overlap, the earlier one gets the finger.")]
        private List<InputArea> _areas = new();

        public IInputSurface Screen => _screen;

        public bool IsBlocked
        {
            get
            {
                for (var index = 0; index < _blockers.Count; index++)
                {
                    if (!IsDestroyed(_blockers[index]))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public event Action OnBackPressed
        {
            add => _backHandlers.Add(value);
            remove => _backHandlers.Remove(value);
        }

        private readonly InputSurface _screen = new("Screen", null);
        private readonly EventHandlers _backHandlers = new(nameof(OnBackPressed), null);
        private readonly List<object> _blockers = new();
        private readonly List<IPointerFilter> _pointerFilters = new();
        private readonly List<TrackedFinger> _fingers = new();
        private readonly List<InputSurface> _areaSurfaces = new();

        private bool _isTouchSupportEnabled;

        private const int _mouseFingerId = -1;
        private const float _centimetersPerInch = 2.54f;

        protected override void Awake()
        {
            base.Awake();

            CreateConfiguredAreaSurfaces();
            UpdateContext();

            if (ServiceLocator.TryGet<IInputService>(out var registered) && !ReferenceEquals(registered, this))
            {
                Debug.LogError($"{nameof(InputService)} on '{name}' is disabled: another {nameof(InputService)} is registered.", this);
                enabled = false;
            }
        }

        private void OnEnable()
        {
            if (_isTouchSupportEnabled)
            {
                return;
            }

            EnhancedTouchSupport.Enable();
            _isTouchSupportEnabled = true;
        }

        private void OnDisable()
        {
            CancelAllTouches();
            _fingers.Clear();

            if (!_isTouchSupportEnabled)
            {
                return;
            }

            EnhancedTouchSupport.Disable();
            _isTouchSupportEnabled = false;
        }

        public IInputSurface GetArea(InputArea area)
        {
            if (area == null)
            {
                throw new ArgumentNullException(nameof(area));
            }

            var surface = FindAreaSurface(area);

            if (surface == null)
            {
                surface = CreateAreaSurface(area);
            }

            return surface;
        }

        public void Block(object source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (ContainsBlocker(source))
            {
                return;
            }

            var wasBlocked = IsBlocked;
            _blockers.Add(source);

            if (wasBlocked)
            {
                return;
            }

            CancelAllTouches();
            IgnoreTrackedFingers();
        }

        public void Unblock(object source)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            for (var index = 0; index < _blockers.Count; index++)
            {
                if (ReferenceEquals(_blockers[index], source))
                {
                    _blockers.RemoveAt(index);
                    return;
                }
            }
        }

        public void AddPointerFilter(IPointerFilter pointerFilter)
        {
            if (pointerFilter == null)
            {
                throw new ArgumentNullException(nameof(pointerFilter));
            }

            if (_pointerFilters.Contains(pointerFilter))
            {
                return;
            }

            _pointerFilters.Add(pointerFilter);
        }

        public void RemovePointerFilter(IPointerFilter pointerFilter)
        {
            if (pointerFilter == null)
            {
                throw new ArgumentNullException(nameof(pointerFilter));
            }

            _pointerFilters.Remove(pointerFilter);
        }

        public void GetData(List<InputSurfaceData> results)
        {
            if (results == null)
            {
                throw new ArgumentNullException(nameof(results));
            }

            results.Clear();
            results.Add(new InputSurfaceData(_screen.Name, _screen.Area, _screen.PixelRect, _screen.TouchCount,
                _screen.PrimaryTouch, _screen.Joystick, _screen.LastGesture));

            for (var index = 0; index < _areaSurfaces.Count; index++)
            {
                var surface = _areaSurfaces[index];

                results.Add(new InputSurfaceData(surface.Name, surface.Area, surface.PixelRect, surface.TouchCount,
                    surface.PrimaryTouch, surface.Joystick, surface.LastGesture));
            }
        }

        #region Internal Methods

        internal void ProcessFrame()
        {
            var time = Time.unscaledTimeAsDouble;

            PruneDestroyed();
            UpdateContext();
            ProcessTouches(time);
            ProcessMouse(time);
            CompleteFrame(time);
            ProcessBack();
        }

        internal void GetTrackedFingers(List<InputFingerData> results)
        {
            if (results == null)
            {
                throw new ArgumentNullException(nameof(results));
            }

            results.Clear();

            for (var index = 0; index < _fingers.Count; index++)
            {
                var finger = _fingers[index];
                var ownerName = finger.Owner != null ? finger.Owner.Name : null;

                results.Add(new InputFingerData(finger.Id, finger.Id == _mouseFingerId, ownerName));
            }
        }

        internal void GetBlockers(List<object> results)
        {
            if (results == null)
            {
                throw new ArgumentNullException(nameof(results));
            }

            results.Clear();
            results.AddRange(_blockers);
        }

        internal void GetPointerFilters(List<IPointerFilter> results)
        {
            if (results == null)
            {
                throw new ArgumentNullException(nameof(results));
            }

            results.Clear();
            results.AddRange(_pointerFilters);
        }

        #endregion

        private void PruneDestroyed()
        {
            for (var index = _blockers.Count - 1; index >= 0; index--)
            {
                if (IsDestroyed(_blockers[index]))
                {
                    _blockers.RemoveAt(index);
                }
            }

            for (var index = _pointerFilters.Count - 1; index >= 0; index--)
            {
                if (IsDestroyed(_pointerFilters[index]))
                {
                    _pointerFilters.RemoveAt(index);
                }
            }
        }

        private void CreateConfiguredAreaSurfaces()
        {
            for (var index = 0; index < _areas.Count; index++)
            {
                var area = _areas[index];

                if (area == null || FindAreaSurface(area) != null)
                {
                    continue;
                }

                CreateAreaSurface(area);
            }
        }

        private InputSurface CreateAreaSurface(InputArea area)
        {
            var surface = new InputSurface(area.name, area);
            _areaSurfaces.Add(surface);

            UpdateAreaContext(surface, GetScreenRect(), UnityEngine.Screen.safeArea);

            return surface;
        }

        private InputSurface FindAreaSurface(InputArea area)
        {
            for (var index = 0; index < _areaSurfaces.Count; index++)
            {
                if (ReferenceEquals(_areaSurfaces[index].Area, area))
                {
                    return _areaSurfaces[index];
                }
            }

            return null;
        }

        private void UpdateContext()
        {
            var screenRect = GetScreenRect();
            var safeArea = UnityEngine.Screen.safeArea;

            _screen.SetContext(GetRules(_gestureSettings), GetPixelsPerCentimeter(_gestureSettings), screenRect);

            for (var index = 0; index < _areaSurfaces.Count; index++)
            {
                UpdateAreaContext(_areaSurfaces[index], screenRect, safeArea);
            }
        }

        private void UpdateAreaContext(InputSurface surface, Rect screenRect, Rect safeArea)
        {
            var area = surface.Area;

            if (area == null)
            {
                surface.SetContext(GetRules(_gestureSettings), GetPixelsPerCentimeter(_gestureSettings), default);
                return;
            }

            var gestureSettings = area.GestureSettings != null ? area.GestureSettings : _gestureSettings;
            var pixelRect = area.GetPixelRect(screenRect, safeArea);
            var restPosition = pixelRect.min + Vector2.Scale(area.FixedJoystickPosition, pixelRect.size);

            surface.SetContext(GetRules(gestureSettings), GetPixelsPerCentimeter(gestureSettings), pixelRect);
            surface.SetJoystick(area.JoystickMode, area.JoystickRadius, area.JoystickDeadZone, restPosition);
        }

        private static Rect GetScreenRect()
        {
            return new Rect(0f, 0f, UnityEngine.Screen.width, UnityEngine.Screen.height);
        }

        private static GestureRules GetRules(GestureSettings gestureSettings)
        {
            return gestureSettings != null ? gestureSettings.ToRules() : GestureRules.Default;
        }

        private static float GetPixelsPerCentimeter(GestureSettings gestureSettings)
        {
            return GetDpi(gestureSettings) / _centimetersPerInch;
        }

        private static float GetDpi(GestureSettings gestureSettings)
        {
            var dpi = UnityEngine.Screen.dpi;

            if (dpi > 0f)
            {
                return dpi;
            }

            if (gestureSettings != null && gestureSettings.FallbackDpi > 0f)
            {
                return gestureSettings.FallbackDpi;
            }

            return GestureSettings.DefaultFallbackDpi;
        }

        private void CompleteFrame(double time)
        {
            _screen.CompleteFrame(time);

            for (var index = 0; index < _areaSurfaces.Count; index++)
            {
                _areaSurfaces[index].CompleteFrame(time);
            }
        }

        private void ProcessTouches(double time)
        {
            if (!_isTouchSupportEnabled)
            {
                return;
            }

            for (var index = 0; index < _fingers.Count; index++)
            {
                var finger = _fingers[index];
                finger.IsSeen = false;
                _fingers[index] = finger;
            }

            var touches = Touch.activeTouches;

            for (var index = 0; index < touches.Count; index++)
            {
                ProcessTouch(touches[index], time);
            }

            for (var index = _fingers.Count - 1; index >= 0; index--)
            {
                var finger = _fingers[index];

                if (finger.Id == _mouseFingerId || finger.IsSeen)
                {
                    continue;
                }

                if (finger.Owner != null)
                {
                    finger.Owner.CancelTouch(finger.Id, time);
                }

                _fingers.RemoveAt(index);
            }
        }

        private void ProcessTouch(Touch touch, double time)
        {
            var fingerId = touch.touchId;
            var index = FindFinger(fingerId);

            if (index < 0)
            {
                ProcessUntrackedTouch(touch, time);
                return;
            }

            var owner = _fingers[index].Owner;

            switch (touch.phase)
            {
                case TouchPhase.Ended:
                    _fingers.RemoveAt(index);

                    if (owner != null)
                    {
                        owner.EndTouch(fingerId, touch.screenPosition, time);
                    }

                    break;
                case TouchPhase.Canceled:
                    _fingers.RemoveAt(index);

                    if (owner != null)
                    {
                        owner.CancelTouch(fingerId, time);
                    }

                    break;
                default:
                    MarkSeen(index);

                    if (owner != null)
                    {
                        owner.UpdateTouch(fingerId, touch.screenPosition, time);
                    }

                    break;
            }
        }

        private void ProcessUntrackedTouch(Touch touch, double time)
        {
            switch (touch.phase)
            {
                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    return;
                case TouchPhase.Began:
                    CancelMouseFinger(time);
                    BeginFinger(touch.touchId, touch.screenPosition, time);
                    return;
                default:
                    _fingers.Add(new TrackedFinger { Id = touch.touchId, Owner = null, IsSeen = true });
                    return;
            }
        }

        private void BeginFinger(int fingerId, Vector2 position, double time)
        {
            var owner = DecideOwner(position);

            _fingers.Add(new TrackedFinger { Id = fingerId, Owner = owner, IsSeen = true });

            if (owner != null)
            {
                owner.BeginTouch(fingerId, position, time);
            }
        }

        private InputSurface DecideOwner(Vector2 position)
        {
            if (IsBlocked)
            {
                return null;
            }

            for (var index = 0; index < _pointerFilters.Count; index++)
            {
                if (TakesPointer(_pointerFilters[index], position))
                {
                    return null;
                }
            }

            for (var index = 0; index < _areaSurfaces.Count; index++)
            {
                if (_areaSurfaces[index].PixelRect.Contains(position))
                {
                    return _areaSurfaces[index];
                }
            }

            return _screen;
        }

        private static bool TakesPointer(IPointerFilter pointerFilter, Vector2 position)
        {
            try
            {
                return pointerFilter.TakesPointer(position);
            }
            catch (Exception exception)
            {
                var context = pointerFilter as Object;
                Debug.LogError($"A {nameof(IPointerFilter)} threw while asked about a pointer.", context);
                Debug.LogException(exception, context);

                return false;
            }
        }

        private void ProcessMouse(double time)
        {
            var mouse = Mouse.current;

            if (mouse == null || !mouse.enabled)
            {
                CancelMouseFinger(time);
                return;
            }

            var mouseIndex = FindFinger(_mouseFingerId);

            if (mouseIndex < 0)
            {
                if (HasTouchFinger() || !mouse.leftButton.wasPressedThisFrame)
                {
                    return;
                }

                BeginFinger(_mouseFingerId, mouse.position.ReadValue(), time);
                mouseIndex = FindFinger(_mouseFingerId);
            }

            var owner = _fingers[mouseIndex].Owner;
            var isHeld = mouse.leftButton.isPressed && !mouse.leftButton.wasReleasedThisFrame;

            if (isHeld)
            {
                if (owner != null)
                {
                    owner.UpdateTouch(_mouseFingerId, mouse.position.ReadValue(), time);
                }

                return;
            }

            _fingers.RemoveAt(mouseIndex);

            if (owner != null)
            {
                owner.EndTouch(_mouseFingerId, mouse.position.ReadValue(), time);
            }
        }

        private bool HasTouchFinger()
        {
            if (_isTouchSupportEnabled && Touch.activeTouches.Count > 0)
            {
                return true;
            }

            for (var index = 0; index < _fingers.Count; index++)
            {
                if (_fingers[index].Id != _mouseFingerId)
                {
                    return true;
                }
            }

            return false;
        }

        private void CancelMouseFinger(double time)
        {
            var index = FindFinger(_mouseFingerId);

            if (index < 0)
            {
                return;
            }

            var owner = _fingers[index].Owner;
            _fingers.RemoveAt(index);

            if (owner != null)
            {
                owner.CancelTouch(_mouseFingerId, time);
            }
        }

        private void ProcessBack()
        {
            var keyboard = Keyboard.current;

            if (keyboard == null || !keyboard.escapeKey.wasPressedThisFrame)
            {
                return;
            }

            _backHandlers.Raise();
        }

        private void CancelAllTouches()
        {
            var time = Time.unscaledTimeAsDouble;

            _screen.CancelAllTouches(time);

            for (var index = 0; index < _areaSurfaces.Count; index++)
            {
                _areaSurfaces[index].CancelAllTouches(time);
            }
        }

        private void IgnoreTrackedFingers()
        {
            for (var index = 0; index < _fingers.Count; index++)
            {
                var finger = _fingers[index];
                finger.Owner = null;
                _fingers[index] = finger;
            }
        }

        private int FindFinger(int fingerId)
        {
            for (var index = 0; index < _fingers.Count; index++)
            {
                if (_fingers[index].Id == fingerId)
                {
                    return index;
                }
            }

            return -1;
        }

        private void MarkSeen(int index)
        {
            var finger = _fingers[index];
            finger.IsSeen = true;
            _fingers[index] = finger;
        }

        private bool ContainsBlocker(object source)
        {
            for (var index = 0; index < _blockers.Count; index++)
            {
                if (ReferenceEquals(_blockers[index], source))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsDestroyed(object source)
        {
            return source is Object unityObject && unityObject == null;
        }

        private void Update()
        {
            ProcessFrame();
        }
    }
}
