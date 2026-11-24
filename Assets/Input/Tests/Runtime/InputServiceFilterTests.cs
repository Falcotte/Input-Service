using System;
using System.Collections;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;
using Object = UnityEngine.Object;

namespace AngryKoala.Input.Tests
{
    public sealed class InputServiceFilterTests : InputServiceTestBase
    {
        private static readonly Vector2EqualityComparer _positionComparer = Vector2EqualityComparer.Instance;

        [UnityTest]
        public IEnumerator Tap_WhereTheFilterTakesThePointer_IsNotGivenToTheScreenSurface()
        {
            var recorder = Record(ScreenSurface);
            Service.AddPointerFilter(new TestPointerFilter(FilterArea));

            yield return Touch.Tap(PointInsideFilterArea);

            Assert.That(recorder.TouchBegan.Count, Is.EqualTo(0));
            Assert.That(recorder.Taps.Count, Is.EqualTo(0));
            Assert.That(ScreenSurface.TouchCount, Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator Tap_WhereTheFilterDoesNotTakeThePointer_BelongsToTheScreenSurface()
        {
            var recorder = Record(ScreenSurface);
            Service.AddPointerFilter(new TestPointerFilter(FilterArea));

            yield return Touch.Tap(PointOutsideFilterArea);

            Assert.That(recorder.TouchBegan.Count, Is.EqualTo(1));
            Assert.That(recorder.Taps.Count, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator Move_OutOfTheFilterAreaAfterBeingTaken_StaysAwayFromTheScreenSurface()
        {
            var recorder = Record(ScreenSurface);
            Service.AddPointerFilter(new TestPointerFilter(FilterArea));

            Touch.Begin(FirstFingerId, PointInsideFilterArea);
            yield return null;

            Touch.Move(FirstFingerId, PointOutsideFilterArea);
            yield return null;

            Assert.That(ScreenSurface.TouchCount, Is.EqualTo(0));
            Assert.That(recorder.TouchBegan.Count, Is.EqualTo(0));

            Touch.End(FirstFingerId);
            yield return WaitFrames(2);

            Assert.That(recorder.TouchBegan.Count, Is.EqualTo(0));
            Assert.That(recorder.TouchEnded.Count, Is.EqualTo(0));
            Assert.That(recorder.Taps.Count, Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator Move_IntoTheFilterAreaAfterGoingDownOutside_StaysWithTheScreenSurface()
        {
            var recorder = Record(ScreenSurface);
            Service.AddPointerFilter(new TestPointerFilter(FilterArea));

            Touch.Begin(FirstFingerId, PointOutsideFilterArea);
            yield return null;

            Touch.Move(FirstFingerId, PointInsideFilterArea);
            yield return null;

            Assert.That(ScreenSurface.TouchCount, Is.EqualTo(1));
            Assert.That(ScreenSurface.PrimaryTouch.Position, Is.EqualTo(PointInsideFilterArea).Using(_positionComparer));

            Touch.End(FirstFingerId);
            yield return WaitFrames(2);

            Assert.That(recorder.TouchEnded.Count, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator Filter_ForOneFinger_IsAskedExactlyOnceWithThePositionOfTouchDown()
        {
            var filter = new TestPointerFilter(FilterArea);
            Service.AddPointerFilter(filter);

            Touch.Begin(FirstFingerId, PointOutsideFilterArea);
            yield return null;

            Touch.Move(FirstFingerId, ScreenPoint(0.8f, 0.6f));
            yield return null;

            Touch.Move(FirstFingerId, PointInsideFilterArea);
            yield return null;

            Touch.End(FirstFingerId);
            yield return WaitFrames(2);

            Assert.That(filter.CallCount, Is.EqualTo(1));
            Assert.That(filter.AskedPositions[0], Is.EqualTo(PointOutsideFilterArea).Using(_positionComparer));
        }

        [UnityTest]
        public IEnumerator Filter_ForTwoFingers_IsAskedOncePerFinger()
        {
            var filter = new TestPointerFilter(FilterArea);
            Service.AddPointerFilter(filter);

            Touch.Begin(FirstFingerId, PointOutsideFilterArea);
            Touch.Begin(SecondFingerId, ScreenPoint(0.9f, 0.5f));
            yield return WaitFrames(3);

            Assert.That(filter.CallCount, Is.EqualTo(2));
        }

        [UnityTest]
        public IEnumerator RemovePointerFilter_AfterRemoval_FilterIsNoLongerAsked()
        {
            var recorder = Record(ScreenSurface);
            var filter = new TestPointerFilter(FilterArea);
            Service.AddPointerFilter(filter);
            Service.RemovePointerFilter(filter);

            yield return Touch.Tap(PointInsideFilterArea);

            Assert.That(filter.CallCount, Is.EqualTo(0));
            Assert.That(recorder.Taps.Count, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator AddPointerFilter_SameFilterTwice_AsksItOnce()
        {
            var filter = new TestPointerFilter(FilterArea);
            Service.AddPointerFilter(filter);
            Service.AddPointerFilter(filter);

            yield return Touch.Tap(PointOutsideFilterArea);

            Assert.That(filter.CallCount, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator AddPointerFilter_Null_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Service.AddPointerFilter(null));

            yield return null;
        }

        [UnityTest]
        public IEnumerator RemovePointerFilter_Null_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Service.RemovePointerFilter(null));

            yield return null;
        }

        [UnityTest]
        public IEnumerator RemovePointerFilter_UnknownFilter_DoesNothing()
        {
            var recorder = Record(ScreenSurface);
            var knownFilter = new TestPointerFilter(FilterArea);
            var unknownFilter = new TestPointerFilter(FilterArea);
            Service.AddPointerFilter(knownFilter);

            Assert.DoesNotThrow(() => Service.RemovePointerFilter(unknownFilter));

            yield return Touch.Tap(PointInsideFilterArea);

            Assert.That(knownFilter.CallCount, Is.EqualTo(1));
            Assert.That(recorder.Taps.Count, Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator Filter_ThatThrows_IsLoggedAndDoesNotTakeThePointer()
        {
            var recorder = Record(ScreenSurface);
            var filter = new TestPointerFilter(FilterArea) { ShouldThrow = true };
            Service.AddPointerFilter(filter);

            LogAssert.Expect(LogType.Error, new Regex(nameof(IPointerFilter)));
            LogAssert.Expect(LogType.Exception, new Regex(FilterFailureMessage));

            yield return Touch.Tap(PointInsideFilterArea);

            Assert.That(filter.CallCount, Is.EqualTo(1));
            Assert.That(recorder.TouchBegan.Count, Is.EqualTo(1));
            Assert.That(recorder.Taps.Count, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator Filter_ThatIsADestroyedObject_IsDroppedAndNoLongerAsked()
        {
            var recorder = Record(ScreenSurface);
            var filterObject = CreateObject("Filter");
            var filter = filterObject.AddComponent<TestPointerFilterBehaviour>();
            Service.AddPointerFilter(filter);

            Object.Destroy(filterObject);
            yield return null;

            yield return Touch.Tap(ScreenCenter);

            Assert.That(recorder.TouchBegan.Count, Is.EqualTo(1));
            Assert.That(recorder.Taps.Count, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator Filter_ThatIsALiveObject_TakesThePointer()
        {
            var recorder = Record(ScreenSurface);
            var filter = CreateObject("Filter").AddComponent<TestPointerFilterBehaviour>();
            Service.AddPointerFilter(filter);

            yield return Touch.Tap(ScreenCenter);

            Assert.That(recorder.TouchBegan.Count, Is.EqualTo(0));
        }
    }
}
