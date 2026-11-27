using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using static AngryKoala.Input.Tests.InputSurfaceTestUtility;
using Object = UnityEngine.Object;

namespace AngryKoala.Input.Tests
{
    public sealed class InputSurfaceJoystickModeOverrideTests : InputSurfaceJoystickTestBase
    {
        private InputArea _area;

        [SetUp]
        public void SetUpAreaSurface()
        {
            _area = InputAreaTestUtility.CreateArea("Test Area", InputAreaSpace.Screen, Vector2.zero, Vector2.one);

            Surface = new InputSurface(_area.name, _area);
            Surface.SetContext(CreateRules(), PixelsPerCentimeter, PixelRect);
        }

        [TearDown]
        public void TearDownArea()
        {
            Object.DestroyImmediate(_area);
        }

        [Test]
        public void JoystickMode_BeforeItIsSet_IsTheModeGivenToSetJoystick()
        {
            SetJoystick(JoystickMode.Floating);

            Assert.That(Surface.JoystickMode, Is.EqualTo(JoystickMode.Floating));

            SetJoystick(JoystickMode.Fixed);

            Assert.That(Surface.JoystickMode, Is.EqualTo(JoystickMode.Fixed));
        }

        [Test]
        public void JoystickMode_WhenSet_IsReadBack()
        {
            SetJoystick(JoystickMode.Floating);

            Surface.JoystickMode = JoystickMode.Following;

            Assert.That(Surface.JoystickMode, Is.EqualTo(JoystickMode.Following));
        }

        [Test]
        public void JoystickMode_WhenSet_WinsOverLaterSetJoystickCalls()
        {
            SetJoystick(JoystickMode.Floating);

            Surface.JoystickMode = JoystickMode.Fixed;

            for (var frame = 0; frame < 3; frame++)
            {
                SetJoystick(JoystickMode.Floating);
            }

            Assert.That(Surface.JoystickMode, Is.EqualTo(JoystickMode.Fixed));
        }

        [Test]
        public void JoystickMode_WhenSet_IsDroppedWhenSetJoystickGivesADifferentMode()
        {
            SetJoystick(JoystickMode.Floating);

            Surface.JoystickMode = JoystickMode.Fixed;
            SetJoystick(JoystickMode.Floating);

            SetJoystick(JoystickMode.Following);

            Assert.That(Surface.JoystickMode, Is.EqualTo(JoystickMode.Following));
        }

        [Test]
        public void JoystickMode_DroppedByADifferentMode_StaysDroppedWhenTheEarlierModeComesBack()
        {
            SetJoystick(JoystickMode.Floating);

            Surface.JoystickMode = JoystickMode.Fixed;
            SetJoystick(JoystickMode.Following);
            SetJoystick(JoystickMode.Floating);

            Assert.That(Surface.JoystickMode, Is.EqualTo(JoystickMode.Floating));
        }

        [Test]
        public void JoystickMode_SetAgainAfterItWasDropped_WinsAgain()
        {
            SetJoystick(JoystickMode.Floating);

            Surface.JoystickMode = JoystickMode.Fixed;
            SetJoystick(JoystickMode.Following);

            Surface.JoystickMode = JoystickMode.Fixed;
            SetJoystick(JoystickMode.Following);

            Assert.That(Surface.JoystickMode, Is.EqualTo(JoystickMode.Fixed));
        }

        [Test]
        public void JoystickMode_SetBeforeTheFirstSetJoystick_IsKept()
        {
            Surface.JoystickMode = JoystickMode.Fixed;

            SetJoystick(JoystickMode.Floating);

            Assert.That(Surface.JoystickMode, Is.EqualTo(JoystickMode.Fixed));
        }

        [Test]
        public void JoystickMode_SetToFixed_KeepsTheCenterAtTheRestPosition()
        {
            SetJoystick(JoystickMode.Floating);
            Surface.JoystickMode = JoystickMode.Fixed;

            SetJoystick(JoystickMode.Floating);
            Press(FirstFingerId, Offset(120f, -80f));
            SetJoystick(JoystickMode.Floating);

            Assert.That(Surface.Joystick.Center, Is.EqualTo(Rest).Using(PositionComparer));
        }

        [Test]
        public void JoystickMode_SetToFloating_CentersOnTheTouchDownPosition()
        {
            SetJoystick(JoystickMode.Fixed);
            Surface.JoystickMode = JoystickMode.Floating;

            SetJoystick(JoystickMode.Fixed);
            Press(FirstFingerId, Offset(120f, -80f));
            SetJoystick(JoystickMode.Fixed);

            Assert.That(Surface.Joystick.Center, Is.EqualTo(Offset(120f, -80f)).Using(PositionComparer));
        }

        [Test]
        public void JoystickMode_SetToNone_GivesTheDefaultJoystick()
        {
            SetJoystick(JoystickMode.Floating);
            Surface.JoystickMode = JoystickMode.None;

            SetJoystick(JoystickMode.Floating);
            Press(FirstFingerId, Start);

            Assert.That(Surface.Joystick, Is.EqualTo(default(JoystickData)));
        }

        [Test]
        public void JoystickMode_SetOnAnAreaThatHasNone_StartsAJoystick()
        {
            SetJoystick(JoystickMode.None);
            Surface.JoystickMode = JoystickMode.Floating;

            SetJoystick(JoystickMode.None);
            Press(FirstFingerId, Start);

            Assert.That(Surface.Joystick.IsActive, Is.True);
            Assert.That(Surface.Joystick.Center, Is.EqualTo(Start).Using(PositionComparer));
        }

        [Test]
        public void JoystickMode_SetKeepsTheRadiusAndTheDeadZoneOfTheArea()
        {
            SetJoystick(JoystickMode.Floating, 0.5f, 2f);
            Surface.JoystickMode = JoystickMode.Fixed;

            SetJoystick(JoystickMode.Floating, 0.5f, 2f);
            Press(FirstFingerId, Start);
            Move(FirstFingerId, Rest + new Vector2(150f, 0f));

            Assert.That(Surface.Joystick.Center, Is.EqualTo(Rest).Using(PositionComparer));
            Assert.That(Surface.Joystick.Radius, Is.EqualTo(200f).Within(0.01f));
            Assert.That(Surface.Joystick.Direction.x, Is.EqualTo(0.5f).Within(0.01f));
            Assert.That(Surface.Joystick.Direction.y, Is.EqualTo(0f).Within(0.01f));
        }

        [Test]
        public void JoystickMode_ChangedFromFloatingToFixedWhileTouching_MovesTheCenterToTheRestPosition()
        {
            SetJoystick(JoystickMode.Floating);

            Press(FirstFingerId, Start);
            Move(FirstFingerId, Offset(30f, 0f));

            Assert.That(Surface.Joystick.Center, Is.EqualTo(Start).Using(PositionComparer));

            Assert.DoesNotThrow(() => Surface.JoystickMode = JoystickMode.Fixed);

            SetJoystick(JoystickMode.Floating);
            Move(FirstFingerId, Offset(40f, 0f));

            Assert.That(Surface.Joystick.IsActive, Is.True);
            Assert.That(Surface.Joystick.Center, Is.EqualTo(Rest).Using(PositionComparer));
        }

        [Test]
        public void JoystickMode_ChangedFromFixedToFloatingWhileTouching_CentersOnWhereTheFingerWentDown()
        {
            SetJoystick(JoystickMode.Fixed);

            Press(FirstFingerId, Start);
            Move(FirstFingerId, Offset(30f, 0f));

            Assert.That(Surface.Joystick.Center, Is.EqualTo(Rest).Using(PositionComparer));

            Assert.DoesNotThrow(() => Surface.JoystickMode = JoystickMode.Floating);

            SetJoystick(JoystickMode.Fixed);
            Move(FirstFingerId, Offset(40f, 0f));

            Assert.That(Surface.Joystick.IsActive, Is.True);
            Assert.That(Surface.Joystick.Center, Is.EqualTo(Start).Using(PositionComparer));
        }

        [Test]
        public void JoystickMode_ChangedToNoneWhileTouching_DoesNotThrowAndGivesTheDefaultJoystick()
        {
            SetJoystick(JoystickMode.Following);

            Press(FirstFingerId, Start);
            Move(FirstFingerId, Offset(250f, 0f));

            Assert.DoesNotThrow(() => Surface.JoystickMode = JoystickMode.None);

            SetJoystick(JoystickMode.Following);
            Move(FirstFingerId, Offset(260f, 0f));

            Assert.That(Surface.Joystick, Is.EqualTo(default(JoystickData)));
        }

        [Test]
        public void JoystickMode_OnTheScreenSurface_IsNone()
        {
            var screenSurface = CreateSurface(CreateRules());

            Assert.That(screenSurface.JoystickMode, Is.EqualTo(JoystickMode.None));
        }

        [TestCase(JoystickMode.Fixed)]
        [TestCase(JoystickMode.Floating)]
        [TestCase(JoystickMode.Following)]
        public void JoystickMode_SetOnTheScreenSurface_LogsAWarningAndStaysNone(JoystickMode mode)
        {
            var screenSurface = CreateSurface(CreateRules());

            LogAssert.Expect(LogType.Warning, new Regex("screen surface has no joystick"));

            screenSurface.JoystickMode = mode;

            Assert.That(screenSurface.JoystickMode, Is.EqualTo(JoystickMode.None));
            Assert.That(screenSurface.Joystick, Is.EqualTo(default(JoystickData)));
        }

        [Test]
        public void JoystickMode_SetToNoneOnTheScreenSurface_LogsNothing()
        {
            var screenSurface = CreateSurface(CreateRules());

            screenSurface.JoystickMode = JoystickMode.None;

            LogAssert.NoUnexpectedReceived();

            Assert.That(screenSurface.JoystickMode, Is.EqualTo(JoystickMode.None));
        }
    }
}
