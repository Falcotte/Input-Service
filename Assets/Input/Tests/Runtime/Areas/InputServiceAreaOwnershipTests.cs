using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace AngryKoala.Input.Tests
{
    public sealed class InputServiceAreaOwnershipTests : InputServiceAreaTestBase
    {
        [UnityTest]
        public IEnumerator Begin_InsideAnArea_BelongsToTheAreaSurface()
        {
            var areaSurface = Service.GetArea(CreateLeftHalfArea());
            var areaRecorder = Record(areaSurface);
            var screenRecorder = Record(ScreenSurface);
            yield return null;

            Touch.Begin(FirstFingerId, PointInLeftHalf);
            yield return null;

            Assert.That(areaSurface.TouchCount, Is.EqualTo(1));
            Assert.That(areaSurface.IsTouching, Is.True);
            Assert.That(areaRecorder.TouchBegan.Count, Is.EqualTo(1));
            Assert.That(areaSurface.PrimaryTouch.FingerId, Is.EqualTo(FirstFingerId));
            Assert.That(ScreenSurface.TouchCount, Is.EqualTo(0));
            Assert.That(screenRecorder.TouchBegan.Count, Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator Begin_OutsideEveryArea_BelongsToTheScreenSurface()
        {
            var areaSurface = Service.GetArea(CreateLeftHalfArea());
            var areaRecorder = Record(areaSurface);
            var screenRecorder = Record(ScreenSurface);
            yield return null;

            Touch.Begin(FirstFingerId, PointInRightHalf);
            yield return null;

            Assert.That(ScreenSurface.TouchCount, Is.EqualTo(1));
            Assert.That(screenRecorder.TouchBegan.Count, Is.EqualTo(1));
            Assert.That(areaSurface.TouchCount, Is.EqualTo(0));
            Assert.That(areaRecorder.TouchBegan.Count, Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator Move_OutOfTheAreaAfterBeginningInside_StaysWithTheAreaSurfaceForTheWholePress()
        {
            var areaSurface = Service.GetArea(CreateLeftHalfArea());
            var areaRecorder = Record(areaSurface);
            var screenRecorder = Record(ScreenSurface);
            yield return null;

            Touch.Begin(FirstFingerId, PointInLeftHalf);
            yield return null;

            Touch.Move(FirstFingerId, PointInRightHalf);
            yield return null;

            Assert.That(areaSurface.TouchCount, Is.EqualTo(1));
            Assert.That(areaSurface.PrimaryTouch.Position.x, Is.EqualTo(PointInRightHalf.x).Within(1f));
            Assert.That(ScreenSurface.TouchCount, Is.EqualTo(0));

            Touch.End(FirstFingerId);
            yield return WaitFrames(2);

            Assert.That(areaRecorder.TouchEnded.Count, Is.EqualTo(1));
            Assert.That(areaRecorder.TouchEnded[0].WasCanceled, Is.False);
            Assert.That(screenRecorder.TouchBegan.Count, Is.EqualTo(0));
            Assert.That(screenRecorder.TouchEnded.Count, Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator Move_IntoAnAreaAfterBeginningOutside_StaysWithTheScreenSurfaceForTheWholePress()
        {
            var areaSurface = Service.GetArea(CreateLeftHalfArea());
            var areaRecorder = Record(areaSurface);
            var screenRecorder = Record(ScreenSurface);
            yield return null;

            Touch.Begin(FirstFingerId, PointInRightHalf);
            yield return null;

            Touch.Move(FirstFingerId, PointInLeftHalf);
            yield return null;

            Assert.That(ScreenSurface.TouchCount, Is.EqualTo(1));
            Assert.That(areaSurface.TouchCount, Is.EqualTo(0));

            Touch.End(FirstFingerId);
            yield return WaitFrames(2);

            Assert.That(screenRecorder.TouchEnded.Count, Is.EqualTo(1));
            Assert.That(areaRecorder.TouchBegan.Count, Is.EqualTo(0));
            Assert.That(areaRecorder.TouchEnded.Count, Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator Begin_WhereTwoAreasOverlap_GoesToTheAreaWhoseSurfaceWasCreatedFirst()
        {
            var firstSurface = Service.GetArea(CreateArea("First", Vector2.zero, new Vector2(0.6f, 1f)));
            var secondSurface = Service.GetArea(CreateArea("Second", new Vector2(0.4f, 0f), Vector2.one));
            yield return null;

            Touch.Begin(FirstFingerId, ScreenPoint(0.5f, 0.5f));
            yield return null;

            Assert.That(firstSurface.TouchCount, Is.EqualTo(1));
            Assert.That(secondSurface.TouchCount, Is.EqualTo(0));
            Assert.That(ScreenSurface.TouchCount, Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator Begin_WhereTwoAreasOverlap_FollowsTheOrderOfCreationNotTheOrderOfTheCoordinates()
        {
            var secondSurface = Service.GetArea(CreateArea("Right Side First", new Vector2(0.4f, 0f), Vector2.one));
            var firstSurface = Service.GetArea(CreateArea("Left Side Second", Vector2.zero, new Vector2(0.6f, 1f)));
            yield return null;

            Touch.Begin(FirstFingerId, ScreenPoint(0.5f, 0.5f));
            yield return null;

            Assert.That(secondSurface.TouchCount, Is.EqualTo(1));
            Assert.That(firstSurface.TouchCount, Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator Begin_InTheOverlappingPartOnly_TheOtherAreaStillGetsItsOwnPart()
        {
            var firstSurface = Service.GetArea(CreateArea("First", Vector2.zero, new Vector2(0.6f, 1f)));
            var secondSurface = Service.GetArea(CreateArea("Second", new Vector2(0.4f, 0f), Vector2.one));
            yield return null;

            Touch.Begin(FirstFingerId, ScreenPoint(0.8f, 0.5f));
            yield return null;

            Assert.That(secondSurface.TouchCount, Is.EqualTo(1));
            Assert.That(firstSurface.TouchCount, Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator Begin_WhereTheFilterTakesThePointer_IsGivenToNoArea()
        {
            var areaSurface = Service.GetArea(CreateLeftHalfArea());
            var areaRecorder = Record(areaSurface);
            var screenRecorder = Record(ScreenSurface);
            var filter = new TestPointerFilter(FilterArea);
            Service.AddPointerFilter(filter);
            yield return null;

            Touch.Begin(FirstFingerId, PointInsideFilterArea);
            yield return null;

            Assert.That(filter.CallCount, Is.EqualTo(1));
            Assert.That(areaSurface.TouchCount, Is.EqualTo(0));
            Assert.That(areaRecorder.TouchBegan.Count, Is.EqualTo(0));
            Assert.That(ScreenSurface.TouchCount, Is.EqualTo(0));
            Assert.That(screenRecorder.TouchBegan.Count, Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator Begin_WhereTheFilterDoesNotTakeThePointer_StillGoesToTheArea()
        {
            var areaSurface = Service.GetArea(CreateRightHalfArea());
            Service.AddPointerFilter(new TestPointerFilter(FilterArea));
            yield return null;

            Touch.Begin(FirstFingerId, PointOutsideFilterArea);
            yield return null;

            Assert.That(areaSurface.TouchCount, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator Move_OutOfTheFilterAreaAfterBeingTaken_StaysAwayFromTheArea()
        {
            var areaSurface = Service.GetArea(CreateArea("Everything", Vector2.zero, Vector2.one));
            var areaRecorder = Record(areaSurface);
            Service.AddPointerFilter(new TestPointerFilter(FilterArea));
            yield return null;

            Touch.Begin(FirstFingerId, PointInsideFilterArea);
            yield return null;

            Touch.Move(FirstFingerId, PointOutsideFilterArea);
            yield return null;

            Touch.End(FirstFingerId);
            yield return WaitFrames(2);

            Assert.That(areaSurface.TouchCount, Is.EqualTo(0));
            Assert.That(areaRecorder.TouchBegan.Count, Is.EqualTo(0));
            Assert.That(areaRecorder.TouchEnded.Count, Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator Tap_InsideAnArea_RaisesTheTapOnTheAreaSurfaceOnly()
        {
            var areaSurface = Service.GetArea(CreateLeftHalfArea());
            var areaRecorder = Record(areaSurface);
            var screenRecorder = Record(ScreenSurface);
            yield return null;

            yield return Touch.Tap(PointInLeftHalf);

            Assert.That(areaRecorder.TouchBegan.Count, Is.EqualTo(1));
            Assert.That(areaRecorder.Taps.Count, Is.EqualTo(1));
            Assert.That(areaRecorder.TouchEnded.Count, Is.EqualTo(1));
            Assert.That(screenRecorder.TouchBegan.Count, Is.EqualTo(0));
            Assert.That(screenRecorder.Taps.Count, Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator Tap_InTheMiddleOfALeftHalfArea_HasANormalizedPositionOfAboutTheMiddle()
        {
            var areaRecorder = Record(Service.GetArea(CreateLeftHalfArea()));
            yield return null;

            yield return Touch.Tap(PointInLeftHalf);

            Assert.That(areaRecorder.Taps.Count, Is.EqualTo(1));
            Assert.That(areaRecorder.Taps[0].NormalizedPosition.x, Is.EqualTo(0.5f).Within(0.02f));
            Assert.That(areaRecorder.Taps[0].NormalizedPosition.y, Is.EqualTo(0.5f).Within(0.02f));
        }

        [UnityTest]
        public IEnumerator Tap_InAnAreaThatDoesNotStartAtTheOrigin_IsNormalizedToTheAreasRectangle()
        {
            var area = CreateArea("Middle", new Vector2(0.5f, 0.25f), new Vector2(1f, 0.75f));
            var areaRecorder = Record(Service.GetArea(area));
            yield return null;

            yield return Touch.Tap(ScreenPoint(0.625f, 0.375f));

            Assert.That(areaRecorder.Taps.Count, Is.EqualTo(1));
            Assert.That(areaRecorder.Taps[0].NormalizedPosition.x, Is.EqualTo(0.25f).Within(0.02f));
            Assert.That(areaRecorder.Taps[0].NormalizedPosition.y, Is.EqualTo(0.25f).Within(0.02f));
        }

        [UnityTest]
        public IEnumerator Drag_InsideAnArea_RaisesTheDragOnTheAreaSurfaceOnly()
        {
            var areaRecorder = Record(Service.GetArea(CreateLeftHalfArea()));
            var screenRecorder = Record(ScreenSurface);
            yield return null;

            yield return Touch.Drag(ScreenPoint(0.25f, 0.1f), ScreenPoint(0.25f, 0.9f), DragSeconds);

            Assert.That(areaRecorder.DragBegan.Count, Is.EqualTo(1));
            Assert.That(areaRecorder.Drags.Count, Is.GreaterThan(0));
            Assert.That(areaRecorder.DragEnded.Count, Is.EqualTo(1));
            Assert.That(areaRecorder.Taps.Count, Is.EqualTo(0));
            Assert.That(screenRecorder.DragBegan.Count, Is.EqualTo(0));
            Assert.That(screenRecorder.Drags.Count, Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator Drag_FromInsideAnAreaToOutsideIt_KeepsRaisingOnTheAreaSurface()
        {
            var areaRecorder = Record(Service.GetArea(CreateLeftHalfArea()));
            var screenRecorder = Record(ScreenSurface);
            yield return null;

            yield return Touch.Drag(ScreenPoint(0.25f, 0.5f), ScreenPoint(0.9f, 0.5f), DragSeconds);

            Assert.That(areaRecorder.DragBegan.Count, Is.EqualTo(1));
            Assert.That(areaRecorder.DragEnded.Count, Is.EqualTo(1));
            Assert.That(areaRecorder.TouchEnded.Count, Is.EqualTo(1));
            Assert.That(areaRecorder.TouchEnded[0].Position.x, Is.EqualTo(Screen.width * 0.9f).Within(1f));
            Assert.That(screenRecorder.DragBegan.Count, Is.EqualTo(0));
            Assert.That(screenRecorder.TouchBegan.Count, Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator Drag_FromOutsideEveryAreaIntoOne_StaysOnTheScreenSurface()
        {
            var areaRecorder = Record(Service.GetArea(CreateLeftHalfArea()));
            var screenRecorder = Record(ScreenSurface);
            yield return null;

            yield return Touch.Drag(ScreenPoint(0.9f, 0.5f), ScreenPoint(0.25f, 0.5f), DragSeconds);

            Assert.That(screenRecorder.DragBegan.Count, Is.EqualTo(1));
            Assert.That(areaRecorder.TouchBegan.Count, Is.EqualTo(0));
            Assert.That(areaRecorder.DragBegan.Count, Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator TwoFingersInTwoAreas_AreTwoPrimaryTouchesOnePerSurface()
        {
            var leftSurface = Service.GetArea(CreateLeftHalfArea());
            var rightSurface = Service.GetArea(CreateRightHalfArea());
            yield return null;

            Touch.Begin(FirstFingerId, PointInLeftHalf);
            Touch.Begin(SecondFingerId, PointInRightHalf);
            yield return null;

            Assert.That(leftSurface.TouchCount, Is.EqualTo(1));
            Assert.That(rightSurface.TouchCount, Is.EqualTo(1));
            Assert.That(leftSurface.PrimaryTouch.FingerId, Is.EqualTo(FirstFingerId));
            Assert.That(rightSurface.PrimaryTouch.FingerId, Is.EqualTo(SecondFingerId));
            Assert.That(ScreenSurface.TouchCount, Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator TwoFingersInTwoAreas_MovingTowardsEachOther_RaiseNoPinchAnywhere()
        {
            var leftSurface = Service.GetArea(CreateLeftHalfArea());
            var rightSurface = Service.GetArea(CreateRightHalfArea());
            var leftRecorder = Record(leftSurface);
            var rightRecorder = Record(rightSurface);
            var screenRecorder = Record(ScreenSurface);
            yield return null;

            Touch.Begin(FirstFingerId, ScreenPoint(0.1f, 0.5f));
            Touch.Begin(SecondFingerId, ScreenPoint(0.9f, 0.5f));
            yield return null;

            for (var step = 1; step <= 6; step++)
            {
                var progress = step / 6f;

                Touch.Move(FirstFingerId, ScreenPoint(Mathf.Lerp(0.1f, 0.4f, progress), 0.5f));
                Touch.Move(SecondFingerId, ScreenPoint(Mathf.Lerp(0.9f, 0.6f, progress), 0.5f));
                yield return null;
            }

            Assert.That(leftSurface.TouchCount, Is.EqualTo(1));
            Assert.That(rightSurface.TouchCount, Is.EqualTo(1));
            Assert.That(leftRecorder.Pinches.Count, Is.EqualTo(0));
            Assert.That(rightRecorder.Pinches.Count, Is.EqualTo(0));
            Assert.That(screenRecorder.Pinches.Count, Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator TwoFingersInTwoAreas_BothTap_RaiseOneTapEachOnTheirOwnSurface()
        {
            var leftRecorder = Record(Service.GetArea(CreateLeftHalfArea()));
            var rightRecorder = Record(Service.GetArea(CreateRightHalfArea()));
            yield return null;

            Touch.Begin(FirstFingerId, PointInLeftHalf);
            Touch.Begin(SecondFingerId, PointInRightHalf);
            yield return null;

            Touch.End(FirstFingerId);
            Touch.End(SecondFingerId);
            yield return WaitFrames(2);

            Assert.That(leftRecorder.Taps.Count, Is.EqualTo(1));
            Assert.That(rightRecorder.Taps.Count, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator TwoFingersInOneArea_RaiseAPinchOnThatAreaSurface()
        {
            var areaRecorder = Record(Service.GetArea(CreateArea("Everything", Vector2.zero, Vector2.one)));
            var screenRecorder = Record(ScreenSurface);
            yield return null;

            yield return Touch.Pinch(ScreenCenter, 100f, 300f, PinchSeconds);

            Assert.That(areaRecorder.Pinches.Count, Is.GreaterThan(0));
            Assert.That(screenRecorder.Pinches.Count, Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator Block_WhileTouchingAnArea_CancelsTheTouchOnTheAreaSurface()
        {
            var areaSurface = Service.GetArea(CreateLeftHalfArea());
            var areaRecorder = Record(areaSurface);
            yield return null;

            Touch.Begin(FirstFingerId, PointInLeftHalf);
            yield return null;

            Service.Block(this);
            yield return null;

            Assert.That(areaRecorder.TouchEnded.Count, Is.EqualTo(1));
            Assert.That(areaRecorder.TouchEnded[0].WasCanceled, Is.True);
            Assert.That(areaSurface.TouchCount, Is.EqualTo(0));
            Assert.That(areaRecorder.Taps.Count, Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator Begin_InsideAnAreaWhileBlocked_IsNeverGivenToTheAreaSurface()
        {
            var areaSurface = Service.GetArea(CreateLeftHalfArea());
            var areaRecorder = Record(areaSurface);
            Service.Block(this);
            yield return null;

            Touch.Begin(FirstFingerId, PointInLeftHalf);
            yield return null;

            Assert.That(areaRecorder.TouchBegan.Count, Is.EqualTo(0));
            Assert.That(areaSurface.TouchCount, Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator Disable_WhileTouchingAnArea_CancelsTheTouchOnTheAreaSurface()
        {
            var areaSurface = Service.GetArea(CreateLeftHalfArea());
            var areaRecorder = Record(areaSurface);
            yield return null;

            Touch.Begin(FirstFingerId, PointInLeftHalf);
            yield return null;

            Service.enabled = false;
            yield return null;

            Assert.That(areaRecorder.TouchEnded.Count, Is.EqualTo(1));
            Assert.That(areaRecorder.TouchEnded[0].WasCanceled, Is.True);
            Assert.That(areaSurface.TouchCount, Is.EqualTo(0));
        }
    }
}
