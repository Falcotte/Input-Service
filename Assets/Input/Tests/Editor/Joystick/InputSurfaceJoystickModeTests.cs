using NUnit.Framework;
using UnityEngine;
using static AngryKoala.Input.Tests.InputSurfaceTestUtility;

namespace AngryKoala.Input.Tests
{
    public sealed class InputSurfaceJoystickModeTests : InputSurfaceJoystickTestBase
    {
        [Test]
        public void Joystick_BeforeSetJoystickIsCalled_IsDefault()
        {
            Assert.That(Surface.Joystick, Is.EqualTo(default(JoystickData)));
        }

        [Test]
        public void Joystick_WithModeNone_IsDefault()
        {
            SetJoystick(JoystickMode.None);

            Assert.That(Surface.Joystick, Is.EqualTo(default(JoystickData)));
        }

        [Test]
        public void Joystick_WithModeNoneWhileTouching_IsDefault()
        {
            SetJoystick(JoystickMode.None);

            Press(FirstFingerId, Start);
            Move(FirstFingerId, Offset(50f, 0f));

            Assert.That(Surface.Joystick, Is.EqualTo(default(JoystickData)));
        }

        [Test]
        public void Joystick_ChangedBackToNone_IsDefault()
        {
            SetJoystick(JoystickMode.Fixed);
            SetJoystick(JoystickMode.None);

            Assert.That(Surface.Joystick, Is.EqualTo(default(JoystickData)));
        }

        [TestCase(JoystickMode.Fixed)]
        [TestCase(JoystickMode.Floating)]
        [TestCase(JoystickMode.Following)]
        public void Joystick_NotTouching_IsAtRest(JoystickMode mode)
        {
            SetJoystick(mode, 0.2f);

            AssertAtRest(Surface.Joystick);
        }

        [TestCase(JoystickMode.Fixed)]
        [TestCase(JoystickMode.Floating)]
        [TestCase(JoystickMode.Following)]
        public void Joystick_NotTouching_FollowsTheRestPositionGivenEachFrame(JoystickMode mode)
        {
            var newRest = new Vector2(640f, 480f);

            SetJoystick(mode);
            Surface.SetJoystick(mode, RadiusCentimeters, NoDeadZone, newRest);

            Assert.That(Surface.Joystick.Center, Is.EqualTo(newRest).Using(PositionComparer));
            Assert.That(Surface.Joystick.HandlePosition, Is.EqualTo(newRest).Using(PositionComparer));
        }

        [TestCase(JoystickMode.Fixed)]
        [TestCase(JoystickMode.Floating)]
        [TestCase(JoystickMode.Following)]
        public void Joystick_Radius_IsTheCentimetresTimesThePixelsPerCentimeterOfSetContext(JoystickMode mode)
        {
            Surface.SetContext(CreateRules(), 50f, PixelRect);

            Surface.SetJoystick(mode, 3f, NoDeadZone, Rest);

            Assert.That(Surface.Joystick.Radius, Is.EqualTo(150f).Within(0.01f));
        }

        [TestCase(JoystickMode.Fixed)]
        [TestCase(JoystickMode.Floating)]
        [TestCase(JoystickMode.Following)]
        public void Joystick_WhileTouching_IsActive(JoystickMode mode)
        {
            SetJoystick(mode);

            Press(FirstFingerId, Start);

            Assert.That(Surface.Joystick.IsActive, Is.True);
            Assert.That(Surface.Joystick.Radius, Is.EqualTo(RadiusPixels).Within(0.01f));
        }

        [TestCase(JoystickMode.Fixed)]
        [TestCase(JoystickMode.Floating)]
        [TestCase(JoystickMode.Following)]
        public void Joystick_AfterTheTouchLifts_IsAtRestAgain(JoystickMode mode)
        {
            SetJoystick(mode, 0.2f);

            Press(FirstFingerId, Start);
            Move(FirstFingerId, Offset(60f, 0f));
            Lift(FirstFingerId, Offset(60f, 0f));

            AssertAtRest(Surface.Joystick);
        }

        [TestCase(JoystickMode.Fixed)]
        [TestCase(JoystickMode.Floating)]
        [TestCase(JoystickMode.Following)]
        public void Joystick_AfterAnotherPress_IsActiveAgain(JoystickMode mode)
        {
            SetJoystick(mode);

            Press(FirstFingerId, Start);
            Lift(FirstFingerId, Start);
            Press(FirstFingerId, Offset(30f, 0f));

            Assert.That(Surface.Joystick.IsActive, Is.True);
        }

        [Test]
        public void Fixed_WhilePressedAwayFromTheRestPosition_KeepsTheCenterAtTheRestPosition()
        {
            SetJoystick(JoystickMode.Fixed);

            Press(FirstFingerId, Start);
            Move(FirstFingerId, Offset(300f, 100f));

            Assert.That(Surface.Joystick.Center, Is.EqualTo(Rest).Using(PositionComparer));
        }

        [Test]
        public void Fixed_FingerInsideTheRadius_PutsTheHandleOnTheFinger()
        {
            SetJoystick(JoystickMode.Fixed);
            var finger = Rest + new Vector2(30f, 40f);

            Press(FirstFingerId, Rest);
            Move(FirstFingerId, finger);

            Assert.That(Surface.Joystick.HandlePosition, Is.EqualTo(finger).Using(PositionComparer));
        }

        [Test]
        public void Fixed_FingerBeyondTheRadius_PullsTheHandleBackOntoTheCircle()
        {
            SetJoystick(JoystickMode.Fixed);

            Press(FirstFingerId, Rest);
            Move(FirstFingerId, Rest + new Vector2(-300f, -400f));

            var expected = Rest + new Vector2(-60f, -80f);

            Assert.That(Surface.Joystick.HandlePosition, Is.EqualTo(expected).Using(PositionComparer));
            Assert.That(Surface.Joystick.Center, Is.EqualTo(Rest).Using(PositionComparer));
        }

        [Test]
        public void Fixed_PressedFarFromTheRestPosition_PointsFromTheCenterToTheFinger()
        {
            SetJoystick(JoystickMode.Fixed);

            Press(FirstFingerId, Rest + new Vector2(300f, 400f));
            Move(FirstFingerId, Rest + new Vector2(300f, 400f));

            Assert.That(Surface.Joystick.Direction, Is.EqualTo(new Vector2(0.6f, 0.8f)).Using(VectorComparer));
        }

        [Test]
        public void Fixed_RestPositionChangedWhileTouching_MovesTheCenter()
        {
            var newRest = new Vector2(640f, 480f);

            SetJoystick(JoystickMode.Fixed);
            Press(FirstFingerId, Start);

            Surface.SetJoystick(JoystickMode.Fixed, RadiusCentimeters, NoDeadZone, newRest);

            Assert.That(Surface.Joystick.Center, Is.EqualTo(newRest).Using(PositionComparer));
        }

        [Test]
        public void Floating_WhenTheFingerGoesDown_CentersOnTheTouchDownPosition()
        {
            SetJoystick(JoystickMode.Floating);
            var touchDown = Offset(120f, -80f);

            Press(FirstFingerId, touchDown);

            var joystick = Surface.Joystick;

            Assert.That(joystick.Center, Is.EqualTo(touchDown).Using(PositionComparer));
            Assert.That(joystick.HandlePosition, Is.EqualTo(touchDown).Using(PositionComparer));
            Assert.That(joystick.Direction, Is.EqualTo(Vector2.zero).Using(VectorComparer));
        }

        [Test]
        public void Floating_WhileTheFingerMoves_KeepsTheCenterAtTheTouchDownPosition()
        {
            SetJoystick(JoystickMode.Floating);

            Press(FirstFingerId, Start);
            Move(FirstFingerId, Offset(60f, 0f));
            Move(FirstFingerId, Offset(500f, 300f));

            Assert.That(Surface.Joystick.Center, Is.EqualTo(Start).Using(PositionComparer));
        }

        [Test]
        public void Floating_FingerBeyondTheRadius_PullsTheHandleBackOntoTheCircle()
        {
            SetJoystick(JoystickMode.Floating);

            Press(FirstFingerId, Start);
            Move(FirstFingerId, Offset(0f, 400f));

            Assert.That(Surface.Joystick.HandlePosition, Is.EqualTo(Offset(0f, RadiusPixels)).Using(PositionComparer));
        }

        [Test]
        public void Floating_FingerInsideTheRadius_PutsTheHandleOnTheFinger()
        {
            SetJoystick(JoystickMode.Floating);

            Press(FirstFingerId, Start);
            Move(FirstFingerId, Offset(-30f, 40f));

            Assert.That(Surface.Joystick.HandlePosition, Is.EqualTo(Offset(-30f, 40f)).Using(PositionComparer));
        }

        [Test]
        public void Floating_PressedAgainElsewhere_CentersOnTheNewTouchDownPosition()
        {
            SetJoystick(JoystickMode.Floating);

            Press(FirstFingerId, Start);
            Lift(FirstFingerId, Start);
            Press(FirstFingerId, Offset(-200f, 150f));

            Assert.That(Surface.Joystick.Center, Is.EqualTo(Offset(-200f, 150f)).Using(PositionComparer));
        }

        [Test]
        public void Floating_SetJoystickCalledEveryFrame_KeepsTheCenterAndTheDirection()
        {
            SetJoystick(JoystickMode.Floating);
            Press(FirstFingerId, Start);

            for (var frame = 1; frame <= 5; frame++)
            {
                SetJoystick(JoystickMode.Floating);
                Move(FirstFingerId, Offset(frame * 20f, 0f));
            }

            SetJoystick(JoystickMode.Floating);

            Assert.That(Surface.Joystick.Center, Is.EqualTo(Start).Using(PositionComparer));
            Assert.That(Surface.Joystick.Direction, Is.EqualTo(new Vector2(1f, 0f)).Using(VectorComparer));
        }

        [Test]
        public void Floating_ModeSetOnlyAfterTheFingerWentDown_StillWorksOnTheNextPress()
        {
            Press(FirstFingerId, Start);
            Lift(FirstFingerId, Start);

            SetJoystick(JoystickMode.Floating);
            Press(FirstFingerId, Offset(70f, 0f));

            Assert.That(Surface.Joystick.IsActive, Is.True);
            Assert.That(Surface.Joystick.Center, Is.EqualTo(Offset(70f, 0f)).Using(PositionComparer));
        }

        [Test]
        public void Following_WhileTheFingerStaysInsideTheRadius_KeepsTheCenter()
        {
            SetJoystick(JoystickMode.Following);

            Press(FirstFingerId, Start);
            Move(FirstFingerId, Offset(60f, 0f));
            Move(FirstFingerId, Offset(-50f, 50f));

            Assert.That(Surface.Joystick.Center, Is.EqualTo(Start).Using(PositionComparer));
        }

        [Test]
        public void Following_FingerBeyondTheRadius_MovesTheCenterSoTheFingerSitsOnTheCircle()
        {
            SetJoystick(JoystickMode.Following);

            Press(FirstFingerId, Start);
            Move(FirstFingerId, Offset(250f, 0f));

            var joystick = Surface.Joystick;

            Assert.That(joystick.Center, Is.EqualTo(Offset(150f, 0f)).Using(PositionComparer));
            Assert.That(joystick.HandlePosition, Is.EqualTo(Offset(250f, 0f)).Using(PositionComparer));
            Assert.That(joystick.Direction, Is.EqualTo(new Vector2(1f, 0f)).Using(VectorComparer));
        }

        [Test]
        public void Following_FingerBeyondTheRadiusDiagonally_MovesTheCenterAlongTheLineToTheFinger()
        {
            SetJoystick(JoystickMode.Following);

            Press(FirstFingerId, Start);
            Move(FirstFingerId, Offset(300f, 400f));

            Assert.That(Surface.Joystick.Center, Is.EqualTo(Offset(240f, 320f)).Using(PositionComparer));
        }

        [Test]
        public void Following_FingerMovesBackInside_KeepsTheMovedCenter()
        {
            SetJoystick(JoystickMode.Following);

            Press(FirstFingerId, Start);
            Move(FirstFingerId, Offset(250f, 0f));
            Move(FirstFingerId, Offset(200f, 0f));

            var joystick = Surface.Joystick;

            Assert.That(joystick.Center, Is.EqualTo(Offset(150f, 0f)).Using(PositionComparer));
            Assert.That(joystick.HandlePosition, Is.EqualTo(Offset(200f, 0f)).Using(PositionComparer));
        }

        [Test]
        public void Following_FingerKeepsGoing_KeepsTheFingerOnTheCircle()
        {
            SetJoystick(JoystickMode.Following);

            Press(FirstFingerId, Start);
            Move(FirstFingerId, Offset(250f, 0f));
            Move(FirstFingerId, Offset(250f, 300f));

            var joystick = Surface.Joystick;
            var finger = Offset(250f, 300f);

            Assert.That(Vector2.Distance(joystick.Center, finger), Is.EqualTo(RadiusPixels).Within(0.01f));
            Assert.That(joystick.HandlePosition, Is.EqualTo(finger).Using(PositionComparer));
        }

        [Test]
        public void Following_AfterTheLift_ComesBackToRestAndCentersOnTheNextPress()
        {
            SetJoystick(JoystickMode.Following);

            Press(FirstFingerId, Start);
            Move(FirstFingerId, Offset(250f, 0f));
            Lift(FirstFingerId, Offset(250f, 0f));

            Assert.That(Surface.Joystick.Center, Is.EqualTo(Rest).Using(PositionComparer));

            Press(FirstFingerId, Offset(-100f, -100f));

            Assert.That(Surface.Joystick.Center, Is.EqualTo(Offset(-100f, -100f)).Using(PositionComparer));
        }
    }
}
