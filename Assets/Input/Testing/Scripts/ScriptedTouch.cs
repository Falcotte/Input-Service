using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace AngryKoala.Input
{
    /// <summary>
    /// A made-up touchscreen for tests. Positions are pixels from the bottom left. The routines yield frames so that
    /// the Input System has processed everything they queued when they end.
    /// </summary>
    public sealed class ScriptedTouch : IDisposable
    {
        private readonly Touchscreen _touchscreen;
        private readonly Dictionary<int, Vector2> _fingerPositions = new();
        private readonly List<int> _fingerIds = new();
        private readonly InputSettings.BackgroundBehavior _savedBackgroundBehavior;
#if UNITY_EDITOR
        private readonly InputSettings.EditorInputBehaviorInPlayMode _savedEditorBehavior;
#endif

        private bool _isDisposed;

        private const string _deviceName = "Scripted Touchscreen";
        private const int _firstFingerId = 1;
        private const int _secondFingerId = 2;
        private const int _framesAfterRelease = 2;

        private ScriptedTouch()
        {
            _savedBackgroundBehavior = InputSystem.settings.backgroundBehavior;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;

#if UNITY_EDITOR
            _savedEditorBehavior = InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.editorInputBehaviorInPlayMode =
                InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
#endif

            _touchscreen = InputSystem.AddDevice<Touchscreen>(_deviceName);
        }

        public static ScriptedTouch Create()
        {
            return new ScriptedTouch();
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;

            _fingerIds.Clear();
            _fingerIds.AddRange(_fingerPositions.Keys);

            foreach (var fingerId in _fingerIds)
            {
                Cancel(fingerId);
            }

            if (_touchscreen.added)
            {
                InputSystem.RemoveDevice(_touchscreen);
            }

            InputSystem.settings.backgroundBehavior = _savedBackgroundBehavior;
#if UNITY_EDITOR
            InputSystem.settings.editorInputBehaviorInPlayMode = _savedEditorBehavior;
#endif
        }

        public void Begin(int fingerId, Vector2 screenPosition)
        {
            if (fingerId < _firstFingerId)
            {
                throw new ArgumentOutOfRangeException(nameof(fingerId), fingerId,
                    "Finger ids start at 1. The Input System treats touch id 0 as invalid.");
            }

            if (_fingerPositions.ContainsKey(fingerId))
            {
                return;
            }

            _fingerPositions[fingerId] = screenPosition;
            QueueTouch(fingerId, TouchPhase.Began, screenPosition);
        }

        public void Move(int fingerId, Vector2 screenPosition)
        {
            if (!_fingerPositions.ContainsKey(fingerId))
            {
                return;
            }

            _fingerPositions[fingerId] = screenPosition;
            QueueTouch(fingerId, TouchPhase.Moved, screenPosition);
        }

        public void End(int fingerId)
        {
            Finish(fingerId, TouchPhase.Ended);
        }

        public void Cancel(int fingerId)
        {
            Finish(fingerId, TouchPhase.Canceled);
        }

        public IEnumerator Tap(Vector2 screenPosition)
        {
            Begin(_firstFingerId, screenPosition);
            yield return null;

            End(_firstFingerId);

            yield return WaitFrames(_framesAfterRelease);
        }

        public IEnumerator Hold(Vector2 screenPosition, float seconds)
        {
            Begin(_firstFingerId, screenPosition);
            yield return null;

            var endTime = Time.unscaledTime + seconds;

            while (Time.unscaledTime < endTime)
            {
                yield return null;
            }

            End(_firstFingerId);

            yield return WaitFrames(_framesAfterRelease);
        }

        public IEnumerator Drag(Vector2 from, Vector2 to, float seconds)
        {
            Begin(_firstFingerId, from);
            yield return null;

            yield return RunMoves(seconds, progress =>
            {
                var position = progress >= 1f ? to : Vector2.Lerp(from, to, progress);
                Move(_firstFingerId, position);
            });

            End(_firstFingerId);

            yield return WaitFrames(_framesAfterRelease);
        }

        public IEnumerator Pinch(Vector2 center, float fromDistance, float toDistance, float seconds)
        {
            Begin(_firstFingerId, GetPinchPosition(center, fromDistance, -1f));
            Begin(_secondFingerId, GetPinchPosition(center, fromDistance, 1f));
            yield return null;

            yield return RunMoves(seconds, progress =>
            {
                var distance = progress >= 1f ? toDistance : Mathf.Lerp(fromDistance, toDistance, progress);
                Move(_firstFingerId, GetPinchPosition(center, distance, -1f));
                Move(_secondFingerId, GetPinchPosition(center, distance, 1f));
            });

            End(_firstFingerId);
            End(_secondFingerId);

            yield return WaitFrames(_framesAfterRelease);
        }

        private void Finish(int fingerId, TouchPhase phase)
        {
            if (!_fingerPositions.TryGetValue(fingerId, out var position))
            {
                return;
            }

            _fingerPositions.Remove(fingerId);
            QueueTouch(fingerId, phase, position);
        }

        private void QueueTouch(int fingerId, TouchPhase phase, Vector2 position)
        {
            var isLifting = phase == TouchPhase.Ended || phase == TouchPhase.Canceled;

            var state = new TouchState
            {
                touchId = fingerId,
                phase = phase,
                position = position,
                pressure = isLifting ? 0f : 1f
            };

            InputSystem.QueueStateEvent(_touchscreen, state);
        }

        private static Vector2 GetPinchPosition(Vector2 center, float distance, float side)
        {
            return center + new Vector2(side * distance * 0.5f, 0f);
        }

        /// <summary>
        /// Calls the move once per frame with the progress from 0 to 1 over the unscaled seconds. It moves at least
        /// twice, and the last move has a progress of exactly 1.
        /// </summary>
        private static IEnumerator RunMoves(float seconds, Action<float> move)
        {
            var startTime = Time.unscaledTime;
            var moveCount = 0;

            while (true)
            {
                yield return null;

                var progress = seconds > 0f ? Mathf.Clamp01((Time.unscaledTime - startTime) / seconds) : 1f;

                if (moveCount == 0 && progress >= 1f)
                {
                    progress = 0.5f;
                }

                moveCount++;
                move(progress);

                if (progress >= 1f)
                {
                    yield break;
                }
            }
        }

        private static IEnumerator WaitFrames(int frameCount)
        {
            for (var frame = 0; frame < frameCount; frame++)
            {
                yield return null;
            }
        }
    }
}
