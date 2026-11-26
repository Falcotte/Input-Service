using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;

namespace AngryKoala.Input.Tests
{
    public sealed class InputServiceTouchTests : InputServiceTestBase
    {
        private static readonly Vector2EqualityComparer _positionComparer = Vector2EqualityComparer.Instance;
        private static readonly Vector2AbsoluteEqualityComparer _absolutePositionComparer = new(0.5f);

        [UnityTest]
        public IEnumerator Begin_OnTheScreen_RaisesTouchBeganOnTheScreenSurface()
        {
            var recorder = Record(ScreenSurface);

            Touch.Begin(FirstFingerId, ScreenCenter);
            yield return null;

            Assert.That(recorder.TouchBegan.Count, Is.EqualTo(1));
            Assert.That(recorder.TouchBegan[0].FingerId, Is.EqualTo(FirstFingerId));
            Assert.That(recorder.TouchBegan[0].Position, Is.EqualTo(ScreenCenter).Using(_positionComparer));
        }

        [UnityTest]
        public IEnumerator Begin_OnTheScreen_ScreenSurfaceIsTouching()
        {
            Touch.Begin(FirstFingerId, ScreenCenter);
            yield return null;

            Assert.That(ScreenSurface.IsTouching, Is.True);
            Assert.That(ScreenSurface.TouchCount, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator Begin_OnTheScreen_PrimaryTouchHasTheScriptedPosition()
        {
            Touch.Begin(FirstFingerId, ScreenCenter);
            yield return null;

            Assert.That(ScreenSurface.PrimaryTouch.FingerId, Is.EqualTo(FirstFingerId));
            Assert.That(ScreenSurface.PrimaryTouch.Position, Is.EqualTo(ScreenCenter).Using(_positionComparer));
            Assert.That(ScreenSurface.PrimaryTouch.StartPosition, Is.EqualTo(ScreenCenter).Using(_positionComparer));
        }

        [UnityTest]
        public IEnumerator Begin_TwoFingers_TouchCountIsTwo()
        {
            Touch.Begin(FirstFingerId, ScreenPoint(0.3f, 0.5f));
            Touch.Begin(SecondFingerId, ScreenPoint(0.7f, 0.5f));
            yield return null;

            Assert.That(ScreenSurface.TouchCount, Is.EqualTo(2));
        }

        [UnityTest]
        public IEnumerator Move_WhileDown_UpdatesThePrimaryTouchPosition()
        {
            var target = ScreenPoint(0.6f, 0.4f);

            Touch.Begin(FirstFingerId, ScreenCenter);
            yield return null;

            Touch.Move(FirstFingerId, target);
            yield return null;

            Assert.That(ScreenSurface.PrimaryTouch.Position, Is.EqualTo(target).Using(_positionComparer));
            Assert.That(ScreenSurface.PrimaryTouch.StartPosition, Is.EqualTo(ScreenCenter).Using(_positionComparer));
        }

        [UnityTest]
        public IEnumerator End_AfterBegin_RaisesTouchEndedWithoutCancel()
        {
            var recorder = Record(ScreenSurface);

            Touch.Begin(FirstFingerId, ScreenCenter);
            yield return null;

            Touch.End(FirstFingerId);
            yield return null;

            Assert.That(recorder.TouchEnded.Count, Is.EqualTo(1));
            Assert.That(recorder.TouchEnded[0].WasCanceled, Is.False);
            Assert.That(ScreenSurface.IsTouching, Is.False);
            Assert.That(ScreenSurface.TouchCount, Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator Cancel_WhileDown_RaisesTouchEndedAsCanceled()
        {
            var recorder = Record(ScreenSurface);

            Touch.Begin(FirstFingerId, ScreenCenter);
            yield return null;

            Touch.Cancel(FirstFingerId);
            yield return null;

            Assert.That(recorder.TouchEnded.Count, Is.EqualTo(1));
            Assert.That(recorder.TouchEnded[0].WasCanceled, Is.True);
            Assert.That(ScreenSurface.TouchCount, Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator Cancel_WhileDown_RaisesNoTap()
        {
            var recorder = Record(ScreenSurface);

            Touch.Begin(FirstFingerId, ScreenCenter);
            yield return null;

            Touch.Cancel(FirstFingerId);
            yield return WaitFrames(3);

            Assert.That(recorder.Taps.Count, Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator Begin_WithTheIdOfAFingerThatLiftedInTheUpdateBefore_IsANewTouch()
        {
            var recorder = Record(ScreenSurface);
            var secondPosition = ScreenPoint(0.7f, 0.4f);

            Touch.Begin(FirstFingerId, ScreenCenter);
            yield return null;

            Touch.End(FirstFingerId);
            yield return null;

            Touch.Begin(FirstFingerId, secondPosition);
            yield return null;

            Assert.That(recorder.TouchBegan.Count, Is.EqualTo(2));
            Assert.That(recorder.TouchEnded.Count, Is.EqualTo(1));
            Assert.That(ScreenSurface.TouchCount, Is.EqualTo(1));
            Assert.That(ScreenSurface.PrimaryTouch.Position, Is.EqualTo(secondPosition).Using(_absolutePositionComparer));
            Assert.That(ScreenSurface.PrimaryTouch.StartPosition,
                Is.EqualTo(secondPosition).Using(_absolutePositionComparer));
        }

        [UnityTest]
        public IEnumerator Tap_WithTheIdOfAFingerThatLiftedInTheUpdateBefore_IsRaisedAgain()
        {
            var recorder = Record(ScreenSurface);

            Touch.Begin(FirstFingerId, ScreenCenter);
            yield return null;

            Touch.End(FirstFingerId);
            yield return null;

            yield return Touch.Tap(ScreenPoint(0.7f, 0.4f));

            Assert.That(recorder.Taps.Count, Is.EqualTo(2));
            Assert.That(ScreenSurface.TouchCount, Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator PrimaryTouch_WhenNothingTouches_IsDefault()
        {
            yield return null;

            Assert.That(ScreenSurface.PrimaryTouch.FingerId, Is.EqualTo(0));
            Assert.That(ScreenSurface.PrimaryTouch.Position, Is.EqualTo(Vector2.zero).Using(_positionComparer));
        }
    }
}
