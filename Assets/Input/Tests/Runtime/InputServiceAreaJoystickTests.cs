using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;

namespace AngryKoala.Input.Tests
{
    public sealed class InputServiceAreaJoystickTests : InputServiceAreaTestBase
    {
        private static readonly Vector2AbsoluteEqualityComparer _positionComparer = new(1f);

        [UnityTest]
        public IEnumerator FloatingJoystick_DraggedRightBeyondTheRadius_PointsRight()
        {
            var area = CreateArea("Floating", Vector2.zero, Vector2.one, JoystickMode.Floating);
            var surface = Service.GetArea(area);
            yield return null;

            Touch.Begin(FirstFingerId, ScreenCenter);
            yield return null;

            var radius = surface.Joystick.Radius;

            Assert.That(radius, Is.GreaterThan(0f));

            Touch.Move(FirstFingerId, ScreenCenter + new Vector2(radius * 2f, 0f));
            yield return WaitFrames(2);

            var joystick = surface.Joystick;

            Assert.That(joystick.IsActive, Is.True);
            Assert.That(joystick.Direction.x, Is.EqualTo(1f).Within(0.01f));
            Assert.That(joystick.Direction.y, Is.EqualTo(0f).Within(0.01f));
            Assert.That(joystick.Center, Is.EqualTo(ScreenCenter).Using(_positionComparer));
            Assert.That(joystick.HandlePosition, Is.EqualTo(ScreenCenter + new Vector2(radius, 0f)).Using(_positionComparer));
        }

        [UnityTest]
        public IEnumerator FloatingJoystick_AfterTheLift_IsInactiveWithZeroDirection()
        {
            var area = CreateArea("Floating", Vector2.zero, Vector2.one, JoystickMode.Floating);
            var surface = Service.GetArea(area);
            yield return null;

            Touch.Begin(FirstFingerId, ScreenCenter);
            yield return null;

            var radius = surface.Joystick.Radius;

            Touch.Move(FirstFingerId, ScreenCenter + new Vector2(radius * 2f, 0f));
            yield return WaitFrames(2);

            Touch.End(FirstFingerId);
            yield return WaitFrames(2);

            Assert.That(surface.Joystick.IsActive, Is.False);
            Assert.That(surface.Joystick.Direction, Is.EqualTo(Vector2.zero));
        }

        [UnityTest]
        public IEnumerator FloatingJoystick_InAnArea_LeavesTheScreenSurfaceJoystickAtDefault()
        {
            var area = CreateArea("Floating", Vector2.zero, Vector2.one, JoystickMode.Floating);
            var surface = Service.GetArea(area);
            yield return null;

            Touch.Begin(FirstFingerId, ScreenCenter);
            yield return null;

            var radius = surface.Joystick.Radius;

            Touch.Move(FirstFingerId, ScreenCenter + new Vector2(radius * 2f, 0f));
            yield return WaitFrames(2);

            Assert.That(ScreenSurface.Joystick, Is.EqualTo(default(JoystickData)));

            Touch.End(FirstFingerId);
            yield return WaitFrames(2);

            Assert.That(ScreenSurface.Joystick, Is.EqualTo(default(JoystickData)));
        }

        [UnityTest]
        public IEnumerator FloatingJoystick_WhenTheFingerGoesDownOutsideTheArea_StaysInactive()
        {
            var area = CreateArea("Floating Left", LeftHalfMinimum, LeftHalfMaximum, JoystickMode.Floating);
            var surface = Service.GetArea(area);
            yield return null;

            Touch.Begin(FirstFingerId, PointInRightHalf);
            yield return null;

            Touch.Move(FirstFingerId, PointInRightHalf + new Vector2(60f, 0f));
            yield return null;

            Assert.That(surface.Joystick.IsActive, Is.False);
            Assert.That(surface.Joystick.Direction, Is.EqualTo(Vector2.zero));
            Assert.That(ScreenSurface.Joystick, Is.EqualTo(default(JoystickData)));
        }

        [UnityTest]
        public IEnumerator FloatingJoystick_CentersOnWhereTheFingerWentDown()
        {
            var area = CreateArea("Floating", Vector2.zero, Vector2.one, JoystickMode.Floating);
            var surface = Service.GetArea(area);
            var touchDown = ScreenPoint(0.3f, 0.6f);
            yield return null;

            Touch.Begin(FirstFingerId, touchDown);
            yield return null;

            Assert.That(surface.Joystick.IsActive, Is.True);
            Assert.That(surface.Joystick.Center, Is.EqualTo(touchDown).Using(_positionComparer));
            Assert.That(surface.Joystick.Direction, Is.EqualTo(Vector2.zero));
        }

        [UnityTest]
        public IEnumerator FollowingJoystick_DraggedFarBeyondTheRadius_KeepsTheFingerOnTheCircle()
        {
            var area = CreateArea("Following", Vector2.zero, Vector2.one, JoystickMode.Following);
            var surface = Service.GetArea(area);
            yield return null;

            Touch.Begin(FirstFingerId, ScreenPoint(0.3f, 0.5f));
            yield return null;

            var radius = surface.Joystick.Radius;
            var finger = ScreenPoint(0.3f, 0.5f) + new Vector2(radius * 3f, 0f);

            Touch.Move(FirstFingerId, finger);
            yield return WaitFrames(2);

            Assert.That(surface.Joystick.Center, Is.EqualTo(finger - new Vector2(radius, 0f)).Using(_positionComparer));
            Assert.That(surface.Joystick.Direction.x, Is.EqualTo(1f).Within(0.01f));
        }

        [UnityTest]
        public IEnumerator Joystick_DeadZoneOfTheAsset_IsUsed()
        {
            var area = CreateArea("Dead Zone", Vector2.zero, Vector2.one, JoystickMode.Floating, 1f, 0.5f);
            var surface = Service.GetArea(area);
            yield return null;

            Touch.Begin(FirstFingerId, ScreenCenter);
            yield return null;

            var radius = surface.Joystick.Radius;

            Touch.Move(FirstFingerId, ScreenCenter + new Vector2(radius * 0.25f, 0f));
            yield return WaitFrames(2);

            Assert.That(surface.Joystick.Direction, Is.EqualTo(Vector2.zero));

            Touch.Move(FirstFingerId, ScreenCenter + new Vector2(radius * 0.75f, 0f));
            yield return WaitFrames(2);

            Assert.That(surface.Joystick.Direction.x, Is.EqualTo(0.5f).Within(0.02f));
        }

        [UnityTest]
        public IEnumerator Joystick_RadiusOfTheAsset_IsInCentimetres()
        {
            var smallArea = CreateArea("Small", Vector2.zero, LeftHalfMaximum, JoystickMode.Floating, 1f);
            var largeArea = CreateArea("Large", RightHalfMinimum, Vector2.one, JoystickMode.Floating, 2f);
            var smallSurface = Service.GetArea(smallArea);
            var largeSurface = Service.GetArea(largeArea);
            yield return null;

            Assert.That(smallSurface.Joystick.Radius, Is.GreaterThan(0f));
            Assert.That(largeSurface.Joystick.Radius, Is.EqualTo(smallSurface.Joystick.Radius * 2f).Within(0.01f));
        }

        [UnityTest]
        public IEnumerator Joystick_WithModeNoneInTheArea_StaysAtDefaultWhileTouching()
        {
            var area = CreateArea("No Joystick", Vector2.zero, Vector2.one);
            var surface = Service.GetArea(area);
            yield return null;

            Touch.Begin(FirstFingerId, ScreenCenter);
            yield return null;

            Touch.Move(FirstFingerId, ScreenCenter + new Vector2(80f, 0f));
            yield return null;

            Assert.That(surface.Joystick, Is.EqualTo(default(JoystickData)));
        }

        [UnityTest]
        public IEnumerator FixedJoystick_AtRest_SitsAtItsPlaceInTheAreasRectangle()
        {
            var area = CreateArea("Fixed", LeftHalfMinimum, LeftHalfMaximum, JoystickMode.Fixed, 1f, 0.1f,
                new Vector2(0.25f, 0.5f));
            var surface = Service.GetArea(area);
            yield return null;

            var expectedCenter = new Vector2(Screen.width * 0.125f, Screen.height * 0.5f);
            var joystick = surface.Joystick;

            Assert.That(joystick.IsActive, Is.False);
            Assert.That(joystick.Center, Is.EqualTo(expectedCenter).Using(_positionComparer));
            Assert.That(joystick.HandlePosition, Is.EqualTo(expectedCenter).Using(_positionComparer));
            Assert.That(joystick.Radius, Is.GreaterThan(0f));
            Assert.That(joystick.Direction, Is.EqualTo(Vector2.zero));
        }

        [UnityTest]
        public IEnumerator FixedJoystick_WhileTouching_KeepsItsCenterAndPointsFromItToTheFinger()
        {
            var area = CreateArea("Fixed", LeftHalfMinimum, LeftHalfMaximum, JoystickMode.Fixed, 1f, 0f,
                new Vector2(0.25f, 0.5f));
            var surface = Service.GetArea(area);
            yield return null;

            var center = new Vector2(Screen.width * 0.125f, Screen.height * 0.5f);
            var radius = surface.Joystick.Radius;

            Touch.Begin(FirstFingerId, center + new Vector2(0f, radius * 2f));
            yield return WaitFrames(2);

            var joystick = surface.Joystick;

            Assert.That(joystick.IsActive, Is.True);
            Assert.That(joystick.Center, Is.EqualTo(center).Using(_positionComparer));
            Assert.That(joystick.Direction.x, Is.EqualTo(0f).Within(0.01f));
            Assert.That(joystick.Direction.y, Is.EqualTo(1f).Within(0.01f));
        }

        [UnityTest]
        public IEnumerator Joysticks_InTwoAreas_AreDrivenByTheirOwnFingers()
        {
            var leftArea = CreateArea("Left", LeftHalfMinimum, LeftHalfMaximum, JoystickMode.Floating);
            var rightArea = CreateArea("Right", RightHalfMinimum, RightHalfMaximum, JoystickMode.Floating);
            var leftSurface = Service.GetArea(leftArea);
            var rightSurface = Service.GetArea(rightArea);
            yield return null;

            Touch.Begin(FirstFingerId, PointInLeftHalf);
            Touch.Begin(SecondFingerId, PointInRightHalf);
            yield return null;

            var radius = leftSurface.Joystick.Radius;

            Touch.Move(FirstFingerId, PointInLeftHalf + new Vector2(radius * 2f, 0f));
            Touch.Move(SecondFingerId, PointInRightHalf - new Vector2(radius * 2f, 0f));
            yield return WaitFrames(2);

            Assert.That(leftSurface.Joystick.IsActive, Is.True);
            Assert.That(rightSurface.Joystick.IsActive, Is.True);
            Assert.That(leftSurface.Joystick.Direction.x, Is.EqualTo(1f).Within(0.01f));
            Assert.That(rightSurface.Joystick.Direction.x, Is.EqualTo(-1f).Within(0.01f));

            Touch.End(FirstFingerId);
            yield return WaitFrames(2);

            Assert.That(leftSurface.Joystick.IsActive, Is.False);
            Assert.That(rightSurface.Joystick.IsActive, Is.True);
        }

        [UnityTest]
        public IEnumerator Joystick_WhenBlockedWhileTouching_ComesBackToRest()
        {
            var area = CreateArea("Floating", Vector2.zero, Vector2.one, JoystickMode.Floating);
            var surface = Service.GetArea(area);
            yield return null;

            Touch.Begin(FirstFingerId, ScreenCenter);
            yield return null;

            var radius = surface.Joystick.Radius;

            Touch.Move(FirstFingerId, ScreenCenter + new Vector2(radius * 2f, 0f));
            yield return WaitFrames(2);

            Service.Block(this);
            yield return WaitFrames(2);

            Assert.That(surface.Joystick.IsActive, Is.False);
            Assert.That(surface.Joystick.Direction, Is.EqualTo(Vector2.zero));
        }
    }
}
