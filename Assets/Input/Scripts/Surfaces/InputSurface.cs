using System;
using System.Collections.Generic;
using UnityEngine;

namespace AngryKoala.Input
{
    /// <summary>
    /// One surface: fingers and times go in, state and gestures come out. It does not read input itself.
    /// </summary>
    internal sealed class InputSurface : IInputSurface
    {
        private sealed class Finger
        {
            public int FingerId;
            public Vector2 Position;
            public Vector2 StartPosition;
            public Vector2 Delta;
            public double StartTime;
            public double LastTime;
            public bool IsDown;
            public bool IsHandover;
            public bool HasOtherFinger;
            public bool HasStrayed;
            public bool HasDragBegun;
            public bool HasHeld;
            public bool IsSwipeResolved;

            public void Reset(int fingerId, Vector2 position, double time)
            {
                FingerId = fingerId;
                Position = position;
                StartPosition = position;
                Delta = Vector2.zero;
                StartTime = time;
                LastTime = time;
                IsDown = true;
                IsHandover = false;
                HasOtherFinger = false;
                HasStrayed = false;
                HasDragBegun = false;
                HasHeld = false;
                IsSwipeResolved = false;
            }

            public void Move(Vector2 position, double time)
            {
                Delta = position - Position;
                Position = position;
                LastTime = Math.Max(LastTime, time);
            }

            public void BecomePrimaryByHandover(double time)
            {
                StartPosition = Position;
                LastTime = Math.Max(LastTime, time);
                StartTime = LastTime;
                Delta = Vector2.zero;
                IsHandover = true;
            }
        }

        public string Name { get; }

        public InputArea Area { get; }

        public Rect PixelRect { get; private set; }

        public string LastGesture { get; private set; }

        public bool IsTouching => TouchCount > 0;

        public int TouchCount => _fingers.Count;

        public TouchData PrimaryTouch
        {
            get
            {
                if (_fingers.Count == 0)
                {
                    return default;
                }

                var finger = _fingers[0];

                return CreateTouch(finger, finger.LastTime, false);
            }
        }

        public JoystickData Joystick => GetJoystick();

        public JoystickMode JoystickMode
        {
            get => _joystickMode;
            set { }
        }

        public event Action<TouchData> OnTouchBegan
        {
            add => _touchBeganHandlers.Add(value);
            remove => _touchBeganHandlers.Remove(value);
        }

        public event Action<TouchData> OnTouchEnded
        {
            add => _touchEndedHandlers.Add(value);
            remove => _touchEndedHandlers.Remove(value);
        }

        public event Action<TouchData> OnTap
        {
            add => _tapHandlers.Add(value);
            remove => _tapHandlers.Remove(value);
        }

        public event Action<TouchData> OnHold
        {
            add => _holdHandlers.Add(value);
            remove => _holdHandlers.Remove(value);
        }

        public event Action<TouchData> OnDragBegan
        {
            add => _dragBeganHandlers.Add(value);
            remove => _dragBeganHandlers.Remove(value);
        }

        public event Action<TouchData> OnDrag
        {
            add => _dragHandlers.Add(value);
            remove => _dragHandlers.Remove(value);
        }

        public event Action<TouchData> OnDragEnded
        {
            add => _dragEndedHandlers.Add(value);
            remove => _dragEndedHandlers.Remove(value);
        }

        public event Action<SwipeData> OnSwipe
        {
            add => _swipeHandlers.Add(value);
            remove => _swipeHandlers.Remove(value);
        }

        public event Action<PinchData> OnPinch
        {
            add => _pinchHandlers.Add(value);
            remove => _pinchHandlers.Remove(value);
        }

        private readonly EventHandlersWithArgument<TouchData> _touchBeganHandlers;
        private readonly EventHandlersWithArgument<TouchData> _touchEndedHandlers;
        private readonly EventHandlersWithArgument<TouchData> _tapHandlers;
        private readonly EventHandlersWithArgument<TouchData> _holdHandlers;
        private readonly EventHandlersWithArgument<TouchData> _dragBeganHandlers;
        private readonly EventHandlersWithArgument<TouchData> _dragHandlers;
        private readonly EventHandlersWithArgument<TouchData> _dragEndedHandlers;
        private readonly EventHandlersWithArgument<SwipeData> _swipeHandlers;
        private readonly EventHandlersWithArgument<PinchData> _pinchHandlers;

        private readonly List<Finger> _fingers = new();
        private readonly List<Finger> _pool = new();

        private GestureRules _rules = GestureRules.Default;
        private float _pixelsPerCentimeter = 1f;

        private JoystickMode _joystickMode;
        private float _joystickRadiusCentimeters;
        private float _joystickDeadZone;
        private Vector2 _joystickRestPosition;
        private Vector2 _joystickCenter;

        private bool _isPinching;
        private int _pinchFirstFingerId;
        private int _pinchSecondFingerId;
        private float _pinchStartDistance;
        private float _pinchPreviousDistance;

        private const float _pinchCenterWeight = 0.5f;
        private const float _directionSectorDegrees = 45f;
        private const int _directionSectorCount = 8;

        private const string _tapGestureName = "Tap";
        private const string _holdGestureName = "Hold";
        private const string _dragGestureName = "Drag";
        private const string _pinchGestureName = "Pinch";
        private const string _swipeUpGestureName = "Swipe Up";
        private const string _swipeDownGestureName = "Swipe Down";
        private const string _swipeLeftGestureName = "Swipe Left";
        private const string _swipeRightGestureName = "Swipe Right";

        public InputSurface(string name, InputArea area)
        {
            Name = name;
            Area = area;

            _touchBeganHandlers = new EventHandlersWithArgument<TouchData>(nameof(OnTouchBegan), name);
            _touchEndedHandlers = new EventHandlersWithArgument<TouchData>(nameof(OnTouchEnded), name);
            _tapHandlers = new EventHandlersWithArgument<TouchData>(nameof(OnTap), name);
            _holdHandlers = new EventHandlersWithArgument<TouchData>(nameof(OnHold), name);
            _dragBeganHandlers = new EventHandlersWithArgument<TouchData>(nameof(OnDragBegan), name);
            _dragHandlers = new EventHandlersWithArgument<TouchData>(nameof(OnDrag), name);
            _dragEndedHandlers = new EventHandlersWithArgument<TouchData>(nameof(OnDragEnded), name);
            _swipeHandlers = new EventHandlersWithArgument<SwipeData>(nameof(OnSwipe), name);
            _pinchHandlers = new EventHandlersWithArgument<PinchData>(nameof(OnPinch), name);
        }

        /// <summary>
        /// Sets what the surface measures with. Called before the fingers of a frame are fed in.
        /// </summary>
        public void SetContext(GestureRules rules, float pixelsPerCentimeter, Rect pixelRect)
        {
            _rules = rules;
            _pixelsPerCentimeter = pixelsPerCentimeter;
            PixelRect = pixelRect;
        }

        /// <summary>
        /// Sets the joystick of the surface. May be called every frame. The rest position is in pixels.
        /// </summary>
        public void SetJoystick(JoystickMode mode, float radiusCentimeters, float deadZone, Vector2 restPosition)
        {
            _joystickMode = mode;
            _joystickRadiusCentimeters = Mathf.Max(0f, radiusCentimeters);
            _joystickDeadZone = Mathf.Clamp(deadZone, 0f, InputArea.MaximumJoystickDeadZone);
            _joystickRestPosition = restPosition;
        }

        public void BeginTouch(int fingerId, Vector2 position, double time)
        {
            if (FindFinger(fingerId) != null)
            {
                return;
            }

            var finger = AcquireFinger();
            finger.Reset(fingerId, position, time);

            if (_fingers.Count > 0)
            {
                finger.HasOtherFinger = true;

                foreach (var otherFinger in _fingers)
                {
                    otherFinger.HasOtherFinger = true;
                }
            }

            if (_fingers.Count == 0)
            {
                _joystickCenter = position;
            }

            _fingers.Add(finger);
            RefreshPinchPair();

            _touchBeganHandlers.Raise(CreateTouch(finger, time, false));
        }

        /// <summary>
        /// Called once per frame for every finger that is down, whether it moved or not.
        /// </summary>
        public void UpdateTouch(int fingerId, Vector2 position, double time)
        {
            var finger = FindFinger(fingerId);

            if (finger == null)
            {
                return;
            }

            finger.Move(position, time);

            if (IsPrimary(finger))
            {
                PullJoystickCenter(finger);
                ProcessPrimaryUpdate(finger, time);
            }
        }

        public void EndTouch(int fingerId, Vector2 position, double time)
        {
            var finger = FindFinger(fingerId);

            if (finger == null)
            {
                return;
            }

            finger.Move(position, time);

            if (IsPrimary(finger))
            {
                var distance = MarkStrayed(finger);

                ProcessPrimaryMovement(finger, CreateTouch(finger, time, false), distance);

                if (!finger.IsDown)
                {
                    return;
                }
            }

            var touch = CreateTouch(finger, time, false);
            var shouldRaiseTap = false;
            var shouldRaiseSwipe = false;
            var shouldRaiseDragEnded = false;

            if (IsPrimary(finger))
            {
                shouldRaiseTap = IsTapRecognised(finger, touch);
                shouldRaiseSwipe = _rules.SwipeTiming == SwipeTiming.OnRelease && IsSwipeRecognised(finger, touch);
                shouldRaiseDragEnded = finger.HasDragBegun;
            }

            RemoveFinger(finger, time);

            if (shouldRaiseTap)
            {
                RaiseTap(touch);
            }

            if (shouldRaiseSwipe)
            {
                RaiseSwipe(touch);
            }

            if (shouldRaiseDragEnded)
            {
                _dragEndedHandlers.Raise(touch);
            }

            _touchEndedHandlers.Raise(touch);
        }

        public void CancelTouch(int fingerId, double time)
        {
            var finger = FindFinger(fingerId);

            if (finger == null)
            {
                return;
            }

            CancelFinger(finger, time);
        }

        public void CancelAllTouches(double time)
        {
            for (var remaining = _fingers.Count; remaining > 0 && _fingers.Count > 0; remaining--)
            {
                CancelFinger(_fingers[0], time);
            }
        }

        /// <summary>
        /// Called once per frame after every finger has been fed in. Gestures that need two fingers are decided here.
        /// </summary>
        public void CompleteFrame(double time)
        {
            if (!_isPinching || _fingers.Count < 2)
            {
                return;
            }

            var firstPosition = _fingers[0].Position;
            var secondPosition = _fingers[1].Position;
            var distance = Vector2.Distance(firstPosition, secondPosition);

            if (Mathf.Approximately(distance, _pinchPreviousDistance))
            {
                return;
            }

            var scaleDelta = _pinchPreviousDistance > 0f ? distance / _pinchPreviousDistance : 1f;
            var totalScale = _pinchStartDistance > 0f ? distance / _pinchStartDistance : 1f;
            var center = (firstPosition + secondPosition) * _pinchCenterWeight;

            _pinchPreviousDistance = distance;
            LastGesture = _pinchGestureName;

            _pinchHandlers.Raise(new PinchData(center, scaleDelta, totalScale));
        }

        private void ProcessPrimaryUpdate(Finger finger, double time)
        {
            var distance = MarkStrayed(finger);
            var touch = CreateTouch(finger, time, false);

            UpdateHold(finger, touch);

            if (!finger.IsDown)
            {
                return;
            }

            ProcessPrimaryMovement(finger, touch, distance);
        }

        private void ProcessPrimaryMovement(Finger finger, TouchData touch, float distance)
        {
            UpdateDrag(finger, touch, distance);

            if (!finger.IsDown)
            {
                return;
            }

            UpdateSwipeWhenCrossed(finger, touch);
        }

        private float MarkStrayed(Finger finger)
        {
            var distance = (finger.Position - finger.StartPosition).magnitude;

            if (distance > ToPixels(_rules.TapMaximumDistance))
            {
                finger.HasStrayed = true;
            }

            return distance;
        }

        private JoystickData GetJoystick()
        {
            if (_joystickMode == JoystickMode.None)
            {
                return default;
            }

            var radius = _joystickRadiusCentimeters * Mathf.Max(0f, _pixelsPerCentimeter);

            if (_fingers.Count == 0)
            {
                return new JoystickData(false, _joystickRestPosition, _joystickRestPosition, radius, Vector2.zero);
            }

            var center = _joystickMode == JoystickMode.Fixed ? _joystickRestPosition : _joystickCenter;
            var offset = _fingers[0].Position - center;
            var distance = offset.magnitude;

            var handlePosition = _fingers[0].Position;
            var direction = Vector2.zero;

            if (distance > radius)
            {
                handlePosition = distance > 0f ? center + offset / distance * radius : center;
            }

            if (radius > 0f && distance > 0f)
            {
                var magnitude = Mathf.Min(distance / radius, 1f);

                if (magnitude > _joystickDeadZone)
                {
                    var length = (magnitude - _joystickDeadZone) / (1f - _joystickDeadZone);
                    direction = offset / distance * length;
                }
            }

            return new JoystickData(true, center, handlePosition, radius, direction);
        }

        private void PullJoystickCenter(Finger finger)
        {
            if (_joystickMode != JoystickMode.Following)
            {
                return;
            }

            var radius = _joystickRadiusCentimeters * Mathf.Max(0f, _pixelsPerCentimeter);
            var offset = finger.Position - _joystickCenter;
            var distance = offset.magnitude;

            if (distance <= radius)
            {
                return;
            }

            _joystickCenter = finger.Position - offset / distance * radius;
        }

        private void UpdateHold(Finger finger, TouchData touch)
        {
            if (finger.HasHeld || finger.HasDragBegun || finger.IsHandover || finger.HasOtherFinger || finger.HasStrayed)
            {
                return;
            }

            if (touch.Duration < _rules.HoldDuration)
            {
                return;
            }

            finger.HasHeld = true;
            LastGesture = _holdGestureName;

            _holdHandlers.Raise(touch);
        }

        private void UpdateDrag(Finger finger, TouchData touch, float distance)
        {
            if (!finger.HasDragBegun)
            {
                if (distance < ToPixels(_rules.DragStartDistance))
                {
                    return;
                }

                finger.HasDragBegun = true;
                LastGesture = _dragGestureName;

                _dragBeganHandlers.Raise(touch);

                if (!finger.IsDown)
                {
                    return;
                }

                _dragHandlers.Raise(touch);

                return;
            }

            if (touch.Delta != Vector2.zero)
            {
                _dragHandlers.Raise(touch);
            }
        }

        private void UpdateSwipeWhenCrossed(Finger finger, TouchData touch)
        {
            if (_rules.SwipeTiming != SwipeTiming.WhenCrossed || finger.IsSwipeResolved)
            {
                return;
            }

            if (touch.TotalDelta.magnitude < ToPixels(_rules.SwipeDistance))
            {
                return;
            }

            finger.IsSwipeResolved = true;

            if (IsSwipeRecognised(finger, touch))
            {
                RaiseSwipe(touch);
            }
        }

        private bool IsTapRecognised(Finger finger, TouchData touch)
        {
            if (finger.IsHandover || finger.HasOtherFinger || finger.HasHeld || finger.HasStrayed || finger.HasDragBegun)
            {
                return false;
            }

            if (touch.TotalDelta.magnitude > ToPixels(_rules.TapMaximumDistance))
            {
                return false;
            }

            return touch.Duration <= _rules.TapMaximumDuration;
        }

        private bool IsSwipeRecognised(Finger finger, TouchData touch)
        {
            if (finger.IsHandover || finger.HasOtherFinger)
            {
                return false;
            }

            if (touch.TotalDelta.magnitude < ToPixels(_rules.SwipeDistance))
            {
                return false;
            }

            return _rules.SwipeMaximumDuration <= 0f || touch.Duration <= _rules.SwipeMaximumDuration;
        }

        private void RaiseTap(TouchData touch)
        {
            LastGesture = _tapGestureName;

            _tapHandlers.Raise(touch);
        }

        private void RaiseSwipe(TouchData touch)
        {
            var vector = touch.TotalDelta;
            var direction = GetDirection(vector);
            var distanceCentimeters = _pixelsPerCentimeter > 0f ? vector.magnitude / _pixelsPerCentimeter : 0f;
            var speed = touch.Duration > 0f ? distanceCentimeters / touch.Duration : 0f;

            LastGesture = direction switch
            {
                SwipeDirection.Up => _swipeUpGestureName,
                SwipeDirection.Down => _swipeDownGestureName,
                SwipeDirection.Left => _swipeLeftGestureName,
                _ => _swipeRightGestureName
            };

            _swipeHandlers.Raise(new SwipeData(touch, direction, GetCompassDirection(vector), speed));
        }

        private void CancelFinger(Finger finger, double time)
        {
            finger.Delta = Vector2.zero;
            finger.LastTime = Math.Max(finger.LastTime, time);

            var touch = CreateTouch(finger, time, true);
            var shouldRaiseDragEnded = IsPrimary(finger) && finger.HasDragBegun;

            RemoveFinger(finger, time);

            if (shouldRaiseDragEnded)
            {
                _dragEndedHandlers.Raise(touch);
            }

            _touchEndedHandlers.Raise(touch);
        }

        private void RemoveFinger(Finger finger, double time)
        {
            var wasPrimary = IsPrimary(finger);

            _fingers.Remove(finger);
            finger.IsDown = false;
            _pool.Add(finger);

            if (wasPrimary && _fingers.Count > 0)
            {
                _fingers[0].BecomePrimaryByHandover(time);
                _joystickCenter = _fingers[0].Position;
            }

            RefreshPinchPair();
        }

        private void RefreshPinchPair()
        {
            if (_fingers.Count < 2)
            {
                _isPinching = false;
                return;
            }

            var first = _fingers[0];
            var second = _fingers[1];

            if (_isPinching && first.FingerId == _pinchFirstFingerId && second.FingerId == _pinchSecondFingerId)
            {
                return;
            }

            _isPinching = true;
            _pinchFirstFingerId = first.FingerId;
            _pinchSecondFingerId = second.FingerId;
            _pinchStartDistance = Vector2.Distance(first.Position, second.Position);
            _pinchPreviousDistance = _pinchStartDistance;
        }

        private Finger AcquireFinger()
        {
            if (_pool.Count == 0)
            {
                return new Finger();
            }

            var finger = _pool[_pool.Count - 1];
            _pool.RemoveAt(_pool.Count - 1);

            return finger;
        }

        private Finger FindFinger(int fingerId)
        {
            for (var index = 0; index < _fingers.Count; index++)
            {
                if (_fingers[index].FingerId == fingerId)
                {
                    return _fingers[index];
                }
            }

            return null;
        }

        private bool IsPrimary(Finger finger) => _fingers.Count > 0 && _fingers[0] == finger;

        private float ToPixels(float centimeters) => centimeters * _pixelsPerCentimeter;

        private TouchData CreateTouch(Finger finger, double time, bool wasCanceled)
        {
            var duration = (float)Math.Max(0.0, Math.Max(time, finger.LastTime) - finger.StartTime);

            return new TouchData(finger.FingerId, finger.Position, finger.StartPosition,
                Normalize(finger.Position), finger.Delta, duration, wasCanceled, _pixelsPerCentimeter);
        }

        private Vector2 Normalize(Vector2 position)
        {
            var pixelRect = PixelRect;

            if (pixelRect.width <= 0f || pixelRect.height <= 0f)
            {
                return Vector2.zero;
            }

            return new Vector2(Mathf.Clamp01((position.x - pixelRect.x) / pixelRect.width),
                Mathf.Clamp01((position.y - pixelRect.y) / pixelRect.height));
        }

        private static SwipeDirection GetDirection(Vector2 vector)
        {
            if (Mathf.Abs(vector.x) > Mathf.Abs(vector.y))
            {
                return vector.x >= 0f ? SwipeDirection.Right : SwipeDirection.Left;
            }

            return vector.y >= 0f ? SwipeDirection.Up : SwipeDirection.Down;
        }

        private static SwipeCompassDirection GetCompassDirection(Vector2 vector)
        {
            var angle = Mathf.Atan2(vector.y, vector.x) * Mathf.Rad2Deg;
            var sector = (Mathf.RoundToInt(angle / _directionSectorDegrees) + _directionSectorCount) % _directionSectorCount;

            return sector switch
            {
                0 => SwipeCompassDirection.Right,
                1 => SwipeCompassDirection.UpRight,
                2 => SwipeCompassDirection.Up,
                3 => SwipeCompassDirection.UpLeft,
                4 => SwipeCompassDirection.Left,
                5 => SwipeCompassDirection.DownLeft,
                6 => SwipeCompassDirection.Down,
                _ => SwipeCompassDirection.DownRight
            };
        }
    }
}
