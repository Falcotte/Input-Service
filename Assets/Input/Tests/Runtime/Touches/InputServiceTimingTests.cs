using System.Collections;
using System.Threading;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace AngryKoala.Input.Tests
{
    public sealed class InputServiceTimingTests : InputServiceTestBase
    {
        private const double _releaseDelaySeconds = 0.1;
        private const int _hitchMilliseconds = 600;
        private const float _durationTolerance = 0.03f;

        [UnityTest]
        public IEnumerator Tap_ReleasedBeforeAFrameHitch_IsATapWithTheDurationOfTheTouch()
        {
            var recorder = Record(ScreenSurface);
            var touchscreen = AddDevice<Touchscreen>();
            var startTime = InputState.currentTime;

            QueueTouch(touchscreen, TouchPhase.Began, startTime);
            yield return null;

            QueueTouch(touchscreen, TouchPhase.Ended, startTime + _releaseDelaySeconds);
            Thread.Sleep(_hitchMilliseconds);
            yield return null;

            Assert.That(recorder.TouchBegan.Count, Is.EqualTo(1));
            Assert.That(recorder.Taps.Count, Is.EqualTo(1));
            Assert.That(recorder.Holds.Count, Is.EqualTo(0));
            Assert.That(recorder.TouchEnded.Count, Is.EqualTo(1));
            Assert.That(recorder.TouchEnded[0].WasCanceled, Is.False);
            Assert.That(recorder.TouchEnded[0].Duration, Is.EqualTo((float)_releaseDelaySeconds).Within(_durationTolerance));
        }

        [UnityTest]
        public IEnumerator Hold_FingerRestingLongerThanTheHoldDuration_RaisesOneHold()
        {
            var recorder = Record(ScreenSurface);

            Touch.Begin(FirstFingerId, ScreenCenter);
            yield return new WaitForSecondsRealtime(HoldSeconds);

            Assert.That(recorder.Holds.Count, Is.EqualTo(1));
            Assert.That(recorder.Holds[0].Duration, Is.GreaterThanOrEqualTo(GestureRules.DefaultHoldDuration));

            yield return WaitFrames(3);

            Touch.End(FirstFingerId);
            yield return WaitFrames(2);

            Assert.That(recorder.Holds.Count, Is.EqualTo(1));
            Assert.That(recorder.Taps.Count, Is.EqualTo(0));
        }

        private static void QueueTouch(Touchscreen touchscreen, TouchPhase phase, double time)
        {
            var isLifting = phase == TouchPhase.Ended || phase == TouchPhase.Canceled;

            var state = new TouchState
            {
                touchId = FirstFingerId,
                phase = phase,
                position = ScreenCenter,
                pressure = isLifting ? 0f : 1f
            };

            InputSystem.QueueStateEvent(touchscreen, state, time);
        }
    }
}
