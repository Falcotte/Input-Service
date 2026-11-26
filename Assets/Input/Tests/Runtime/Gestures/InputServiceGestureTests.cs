using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace AngryKoala.Input.Tests
{
    public sealed class InputServiceGestureTests : InputServiceTestBase
    {
        [UnityTest]
        public IEnumerator Tap_OnTheScreen_IsRaisedOnTheScreenSurface()
        {
            var recorder = Record(ScreenSurface);

            yield return Touch.Tap(ScreenCenter);

            Assert.That(recorder.Taps.Count, Is.EqualTo(1));
            Assert.That(recorder.Holds.Count, Is.EqualTo(0));
            Assert.That(recorder.DragBegan.Count, Is.EqualTo(0));
            Assert.That(recorder.Swipes.Count, Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator Hold_LongerThanTheHoldDuration_RaisesHoldAndNoTap()
        {
            var recorder = Record(ScreenSurface);

            yield return Touch.Hold(ScreenCenter, HoldSeconds);

            Assert.That(recorder.Holds.Count, Is.EqualTo(1));
            Assert.That(recorder.Taps.Count, Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator Drag_AcrossTheScreen_RaisesDragBeganDragAndDragEnded()
        {
            var recorder = Record(ScreenSurface);

            yield return DragAcrossCenter(Vector2.right, DragSeconds);

            Assert.That(recorder.DragBegan.Count, Is.EqualTo(1));
            Assert.That(recorder.Drags.Count, Is.GreaterThan(0));
            Assert.That(recorder.DragEnded.Count, Is.EqualTo(1));
            Assert.That(recorder.Taps.Count, Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator Drag_ToTheRight_RaisesSwipeRight()
        {
            var recorder = Record(ScreenSurface);

            yield return DragAcrossCenter(Vector2.right, DragSeconds);

            Assert.That(recorder.Swipes.Count, Is.EqualTo(1));
            Assert.That(recorder.Swipes[0].Direction, Is.EqualTo(SwipeDirection.Right));
        }

        [UnityTest]
        public IEnumerator Swipe_MoveAndEndQueuedInTheSameFrame_RaisesASwipe()
        {
            var recorder = Record(ScreenSurface);
            var halfVector = Vector2.right * (DragLength * 0.5f);

            Touch.Begin(FirstFingerId, ScreenCenter - halfVector);
            yield return null;

            Touch.Move(FirstFingerId, ScreenCenter + halfVector);
            Touch.End(FirstFingerId);
            yield return WaitFrames(2);

            Assert.That(recorder.Swipes.Count, Is.EqualTo(1));
            Assert.That(recorder.Swipes[0].Direction, Is.EqualTo(SwipeDirection.Right));
            Assert.That(recorder.Taps.Count, Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator Drag_ToTheLeft_RaisesSwipeLeft()
        {
            var recorder = Record(ScreenSurface);

            yield return DragAcrossCenter(Vector2.left, DragSeconds);

            Assert.That(recorder.Swipes.Count, Is.EqualTo(1));
            Assert.That(recorder.Swipes[0].Direction, Is.EqualTo(SwipeDirection.Left));
        }

        [UnityTest]
        public IEnumerator Drag_UpTheScreen_RaisesSwipeUp()
        {
            var recorder = Record(ScreenSurface);

            yield return DragAcrossCenter(Vector2.up, DragSeconds);

            Assert.That(recorder.Swipes.Count, Is.EqualTo(1));
            Assert.That(recorder.Swipes[0].Direction, Is.EqualTo(SwipeDirection.Up));
        }

        [UnityTest]
        public IEnumerator Drag_DownTheScreen_RaisesSwipeDown()
        {
            var recorder = Record(ScreenSurface);

            yield return DragAcrossCenter(Vector2.down, DragSeconds);

            Assert.That(recorder.Swipes.Count, Is.EqualTo(1));
            Assert.That(recorder.Swipes[0].Direction, Is.EqualTo(SwipeDirection.Down));
        }

        [UnityTest]
        public IEnumerator Drag_Diagonally_TotalDeltaCentimetersIsTotalDeltaDividedByOnePositiveNumber()
        {
            var recorder = Record(ScreenSurface);

            yield return DragAcrossCenter(new Vector2(1f, 1f), DragSeconds);

            Assert.That(recorder.DragEnded.Count, Is.EqualTo(1));

            var touch = recorder.DragEnded[0];
            var pixelsPerCentimeterX = touch.TotalDelta.x / touch.TotalDeltaCentimeters.x;
            var pixelsPerCentimeterY = touch.TotalDelta.y / touch.TotalDeltaCentimeters.y;

            Assert.That(pixelsPerCentimeterX, Is.GreaterThan(0f));
            Assert.That(pixelsPerCentimeterY, Is.EqualTo(pixelsPerCentimeterX).Within(0.01f).Percent);
        }

        [UnityTest]
        public IEnumerator Pinch_FingersMoveApart_RaisesPinchWithTotalScaleAboveOne()
        {
            var recorder = Record(ScreenSurface);

            yield return Touch.Pinch(ScreenCenter, Screen.width * 0.2f, Screen.width * 0.6f, PinchSeconds);

            Assert.That(recorder.Pinches.Count, Is.GreaterThan(0));
            Assert.That(recorder.Pinches[^1].TotalScale, Is.GreaterThan(1f));
        }

        [UnityTest]
        public IEnumerator Pinch_FingersMoveTogether_RaisesPinchWithTotalScaleBelowOne()
        {
            var recorder = Record(ScreenSurface);

            yield return Touch.Pinch(ScreenCenter, Screen.width * 0.6f, Screen.width * 0.2f, PinchSeconds);

            Assert.That(recorder.Pinches.Count, Is.GreaterThan(0));
            Assert.That(recorder.Pinches[^1].TotalScale, Is.LessThan(1f));
        }
    }
}
