using System.Collections;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;

namespace AngryKoala.Input.Tests
{
    public sealed class InputServiceAreaJoystickModeTests : InputServiceAreaTestBase
    {
        private static readonly Vector2AbsoluteEqualityComparer _positionComparer = new(1f);

        [UnityTest]
        public IEnumerator JoystickMode_OfAnAreaSurface_IsTheModeOfTheAsset()
        {
            var area = CreateArea("Floating", Vector2.zero, Vector2.one, JoystickMode.Floating);
            var surface = Service.GetArea(area);
            yield return WaitFrames(2);

            Assert.That(surface.JoystickMode, Is.EqualTo(JoystickMode.Floating));
        }

        [UnityTest]
        public IEnumerator JoystickMode_SetOnAnAreaSurface_SurvivesTheFramesAndLeavesTheAsset()
        {
            var area = CreateArea("Floating", Vector2.zero, Vector2.one, JoystickMode.Floating);
            var surface = Service.GetArea(area);
            yield return null;

            surface.JoystickMode = JoystickMode.Fixed;
            yield return WaitFrames(5);

            Assert.That(surface.JoystickMode, Is.EqualTo(JoystickMode.Fixed));
            Assert.That(area.JoystickMode, Is.EqualTo(JoystickMode.Floating));
        }

        [UnityTest]
        public IEnumerator JoystickMode_SetToFixed_PutsTheCenterAtTheFixedPositionOfTheArea()
        {
            var area = CreateArea("Floating", LeftHalfMinimum, LeftHalfMaximum, JoystickMode.Floating, 1f, 0f,
                new Vector2(0.25f, 0.5f));
            var surface = Service.GetArea(area);
            yield return null;

            surface.JoystickMode = JoystickMode.Fixed;
            yield return WaitFrames(2);

            var restPosition = new Vector2(Screen.width * 0.125f, Screen.height * 0.5f);

            Touch.Begin(FirstFingerId, PointInLeftHalf + new Vector2(0f, Screen.height * 0.2f));
            yield return WaitFrames(3);

            Assert.That(surface.Joystick.IsActive, Is.True);
            Assert.That(surface.Joystick.Center, Is.EqualTo(restPosition).Using(_positionComparer));
            Assert.That(area.JoystickMode, Is.EqualTo(JoystickMode.Floating));
        }

        [UnityTest]
        public IEnumerator JoystickMode_SetToFloating_OnAnAreaThatHasNone_StartsAJoystickAtTheTouchDown()
        {
            var area = CreateArea("No Joystick", Vector2.zero, Vector2.one);
            var surface = Service.GetArea(area);
            yield return null;

            surface.JoystickMode = JoystickMode.Floating;
            yield return WaitFrames(2);

            Touch.Begin(FirstFingerId, ScreenCenter);
            yield return WaitFrames(3);

            Assert.That(surface.Joystick.IsActive, Is.True);
            Assert.That(surface.Joystick.Center, Is.EqualTo(ScreenCenter).Using(_positionComparer));
            Assert.That(area.JoystickMode, Is.EqualTo(JoystickMode.None));
        }

        [UnityTest]
        public IEnumerator JoystickMode_SetToNone_OnAFloatingArea_GivesTheDefaultJoystick()
        {
            var area = CreateArea("Floating", Vector2.zero, Vector2.one, JoystickMode.Floating);
            var surface = Service.GetArea(area);
            yield return null;

            surface.JoystickMode = JoystickMode.None;
            yield return WaitFrames(2);

            Touch.Begin(FirstFingerId, ScreenCenter);
            yield return WaitFrames(3);

            Assert.That(surface.Joystick, Is.EqualTo(default(JoystickData)));
        }

        [UnityTest]
        public IEnumerator JoystickMode_SetOnOneArea_LeavesTheOtherAreaAlone()
        {
            var leftArea = CreateArea("Left", LeftHalfMinimum, LeftHalfMaximum, JoystickMode.Floating);
            var rightArea = CreateArea("Right", RightHalfMinimum, RightHalfMaximum, JoystickMode.Floating);
            var leftSurface = Service.GetArea(leftArea);
            var rightSurface = Service.GetArea(rightArea);
            yield return null;

            leftSurface.JoystickMode = JoystickMode.Following;
            yield return WaitFrames(3);

            Assert.That(leftSurface.JoystickMode, Is.EqualTo(JoystickMode.Following));
            Assert.That(rightSurface.JoystickMode, Is.EqualTo(JoystickMode.Floating));
        }

        [UnityTest]
        public IEnumerator JoystickMode_SetWhileTheFingerIsDown_TakesEffectFromThatFrame()
        {
            var area = CreateArea("Floating", LeftHalfMinimum, LeftHalfMaximum, JoystickMode.Floating, 1f, 0f,
                new Vector2(0.25f, 0.5f));
            var surface = Service.GetArea(area);
            yield return null;

            var touchDown = ScreenPoint(0.1f, 0.8f);

            Touch.Begin(FirstFingerId, touchDown);
            yield return WaitFrames(3);

            Assert.That(surface.Joystick.Center, Is.EqualTo(touchDown).Using(_positionComparer));

            surface.JoystickMode = JoystickMode.Fixed;
            yield return WaitFrames(3);

            var restPosition = new Vector2(Screen.width * 0.125f, Screen.height * 0.5f);

            Assert.That(surface.Joystick.IsActive, Is.True);
            Assert.That(surface.Joystick.Center, Is.EqualTo(restPosition).Using(_positionComparer));
        }

        [UnityTest]
        public IEnumerator JoystickMode_SetOnTheScreenSurface_LogsAWarningAndStaysNone()
        {
            yield return null;

            LogAssert.Expect(LogType.Warning, new Regex("Joystick"));

            ScreenSurface.JoystickMode = JoystickMode.Floating;
            yield return WaitFrames(3);

            Touch.Begin(FirstFingerId, ScreenCenter);
            yield return WaitFrames(3);

            Assert.That(ScreenSurface.JoystickMode, Is.EqualTo(JoystickMode.None));
            Assert.That(ScreenSurface.Joystick, Is.EqualTo(default(JoystickData)));
        }
    }
}
