using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools.Utils;
using static AngryKoala.Input.Tests.InputSurfaceTestUtility;

namespace AngryKoala.Input.Tests
{
    public sealed class InputSurfaceJoystickDirectionTests : InputSurfaceJoystickTestBase
    {
        private static readonly Vector2EqualityComparer _directionComparer = new(0.001f);

        private static readonly JoystickMode[] _modes =
        {
            JoystickMode.Fixed,
            JoystickMode.Floating,
            JoystickMode.Following
        };

        private static IEnumerable<TestCaseData> DirectionCases()
        {
            var cases = new[]
            {
                (deadZone: 0.2f, offset: new Vector2(50f, 0f), expected: new Vector2(0.375f, 0f)),
                (deadZone: 0.2f, offset: new Vector2(19f, 0f), expected: Vector2.zero),
                (deadZone: 0.2f, offset: new Vector2(21f, 0f), expected: new Vector2(0.0125f, 0f)),
                (deadZone: 0.2f, offset: Vector2.zero, expected: Vector2.zero),
                (deadZone: 0.2f, offset: new Vector2(0f, -10f), expected: Vector2.zero),
                (deadZone: 0.2f, offset: new Vector2(100f, 0f), expected: new Vector2(1f, 0f)),
                (deadZone: 0.2f, offset: new Vector2(300f, 0f), expected: new Vector2(1f, 0f)),
                (deadZone: 0.2f, offset: new Vector2(0f, -100f), expected: new Vector2(0f, -1f)),
                (deadZone: 0.2f, offset: new Vector2(30f, 40f), expected: new Vector2(0.225f, 0.3f)),
                (deadZone: 0.2f, offset: new Vector2(-60f, -80f), expected: new Vector2(-0.6f, -0.8f)),
                (deadZone: 0.2f, offset: new Vector2(-300f, -400f), expected: new Vector2(-0.6f, -0.8f)),
                (deadZone: 0f, offset: new Vector2(50f, 0f), expected: new Vector2(0.5f, 0f)),
                (deadZone: 0f, offset: new Vector2(1f, 0f), expected: new Vector2(0.01f, 0f)),
                (deadZone: 0.5f, offset: new Vector2(75f, 0f), expected: new Vector2(0.5f, 0f)),
                (deadZone: 0.5f, offset: new Vector2(0f, -300f), expected: new Vector2(0f, -1f)),
                (deadZone: 0.5f, offset: new Vector2(0f, 40f), expected: Vector2.zero)
            };

            foreach (var mode in _modes)
            {
                foreach (var directionCase in cases)
                {
                    var testName = $"Direction_{mode}_DeadZone{directionCase.deadZone}_Offset{directionCase.offset}";

                    yield return new TestCaseData(mode, directionCase.deadZone, directionCase.offset,
                        directionCase.expected).SetName(testName);
                }
            }
        }

        [TestCaseSource(nameof(DirectionCases))]
        public void Direction_FromTheCenterToTheFinger_FollowsTheDeadZoneRule(JoystickMode mode, float deadZone,
            Vector2 offset, Vector2 expected)
        {
            var center = GetStartCenter(mode);
            SetJoystick(mode, deadZone);

            Press(FirstFingerId, center);
            Move(FirstFingerId, center + offset);

            Assert.That(Surface.Joystick.Direction, Is.EqualTo(expected).Using(_directionComparer));
        }

        [TestCase(JoystickMode.Fixed)]
        [TestCase(JoystickMode.Floating)]
        [TestCase(JoystickMode.Following)]
        public void Direction_RightAfterThePress_IsZero(JoystickMode mode)
        {
            SetJoystick(mode, 0.1f);

            Press(FirstFingerId, GetStartCenter(mode));

            Assert.That(Surface.Joystick.Direction, Is.EqualTo(Vector2.zero).Using(_directionComparer));
        }

        [TestCase(JoystickMode.Fixed)]
        [TestCase(JoystickMode.Floating)]
        [TestCase(JoystickMode.Following)]
        public void Direction_ReadTwiceInAFrame_GivesTheSameAnswer(JoystickMode mode)
        {
            var center = GetStartCenter(mode);
            SetJoystick(mode, 0.1f);

            Press(FirstFingerId, center);
            Move(FirstFingerId, center + new Vector2(50f, 0f));

            var first = Surface.Joystick;
            var second = Surface.Joystick;

            Assert.That(second.Direction, Is.EqualTo(first.Direction).Using(_directionComparer));
            Assert.That(second.Center, Is.EqualTo(first.Center).Using(VectorComparer));
            Assert.That(second.HandlePosition, Is.EqualTo(first.HandlePosition).Using(VectorComparer));
        }

        [TestCase(JoystickMode.Fixed)]
        [TestCase(JoystickMode.Floating)]
        [TestCase(JoystickMode.Following)]
        public void SecondFinger_DoesNotDriveTheJoystick(JoystickMode mode)
        {
            var center = GetStartCenter(mode);
            SetJoystick(mode, 0.1f);

            Press(FirstFingerId, center);
            Press(SecondFingerId, center + new Vector2(300f, 0f));
            Move(SecondFingerId, center + new Vector2(350f, 20f));

            var joystick = Surface.Joystick;

            Assert.That(joystick.IsActive, Is.True);
            Assert.That(joystick.Center, Is.EqualTo(center).Using(VectorComparer));
            Assert.That(joystick.HandlePosition, Is.EqualTo(center).Using(VectorComparer));
            Assert.That(joystick.Direction, Is.EqualTo(Vector2.zero).Using(_directionComparer));
        }

        [TestCase(JoystickMode.Fixed)]
        [TestCase(JoystickMode.Floating)]
        [TestCase(JoystickMode.Following)]
        public void PrimaryFinger_WithASecondFingerDown_StillDrivesTheJoystick(JoystickMode mode)
        {
            var center = GetStartCenter(mode);
            SetJoystick(mode, 0.1f);

            Press(FirstFingerId, center);
            Press(SecondFingerId, center + new Vector2(0f, 300f));
            Move(FirstFingerId, center + new Vector2(50f, 0f));

            var expected = new Vector2(0.4f / 0.9f, 0f);

            Assert.That(Surface.Joystick.Direction, Is.EqualTo(expected).Using(_directionComparer));
        }

        [TestCase(JoystickMode.Floating)]
        [TestCase(JoystickMode.Following)]
        public void Handover_WhenThePrimaryLifts_FloatingAndFollowingCenterOnTheNewPrimary(JoystickMode mode)
        {
            var newPrimaryPosition = Start + new Vector2(30f, 40f);
            SetJoystick(mode, 0.1f);

            Press(FirstFingerId, Start);
            Press(SecondFingerId, newPrimaryPosition);
            Lift(FirstFingerId, Start);

            var joystick = Surface.Joystick;

            Assert.That(joystick.IsActive, Is.True);
            Assert.That(joystick.Center, Is.EqualTo(newPrimaryPosition).Using(VectorComparer));
            Assert.That(joystick.HandlePosition, Is.EqualTo(newPrimaryPosition).Using(VectorComparer));
            Assert.That(joystick.Direction, Is.EqualTo(Vector2.zero).Using(_directionComparer));
        }

        [TestCase(JoystickMode.Floating)]
        [TestCase(JoystickMode.Following)]
        public void Handover_AfterwardsTheNewPrimaryDrivesTheDirectionFromTheNewCenter(JoystickMode mode)
        {
            var newPrimaryPosition = Start + new Vector2(30f, 40f);
            SetJoystick(mode, 0.1f);

            Press(FirstFingerId, Start);
            Press(SecondFingerId, newPrimaryPosition);
            Lift(FirstFingerId, Start);
            Move(SecondFingerId, newPrimaryPosition + new Vector2(50f, 0f));

            var expected = new Vector2(0.4f / 0.9f, 0f);

            Assert.That(Surface.Joystick.Direction, Is.EqualTo(expected).Using(_directionComparer));
            Assert.That(Surface.Joystick.Center, Is.EqualTo(newPrimaryPosition).Using(VectorComparer));
        }

        [Test]
        public void Handover_WhenThePrimaryLifts_FixedFollowsTheNewPrimary()
        {
            var newPrimaryPosition = Rest + new Vector2(30f, 40f);
            SetJoystick(JoystickMode.Fixed, 0.1f);

            Press(FirstFingerId, Rest);
            Press(SecondFingerId, newPrimaryPosition);
            Lift(FirstFingerId, Rest);

            var joystick = Surface.Joystick;
            var expectedDirection = new Vector2(0.6f, 0.8f) * (0.4f / 0.9f);

            Assert.That(joystick.IsActive, Is.True);
            Assert.That(joystick.Center, Is.EqualTo(Rest).Using(VectorComparer));
            Assert.That(joystick.HandlePosition, Is.EqualTo(newPrimaryPosition).Using(VectorComparer));
            Assert.That(joystick.Direction, Is.EqualTo(expectedDirection).Using(_directionComparer));
        }

        [TestCase(JoystickMode.Fixed)]
        [TestCase(JoystickMode.Floating)]
        [TestCase(JoystickMode.Following)]
        public void Handover_WhenTheLastFingerLifts_IsAtRest(JoystickMode mode)
        {
            var center = GetStartCenter(mode);
            SetJoystick(mode, 0.2f);

            Press(FirstFingerId, center);
            Press(SecondFingerId, center + new Vector2(30f, 40f));
            Lift(FirstFingerId, center);
            Lift(SecondFingerId, center + new Vector2(30f, 40f));

            AssertAtRest(Surface.Joystick);
        }

        [Test]
        public void SecondFingerLifting_WhileThePrimaryStays_DoesNotMoveTheCenter()
        {
            SetJoystick(JoystickMode.Floating, 0.1f);

            Press(FirstFingerId, Start);
            Move(FirstFingerId, Offset(50f, 0f));
            Press(SecondFingerId, Offset(300f, 300f));
            Lift(SecondFingerId, Offset(300f, 300f));

            Assert.That(Surface.Joystick.IsActive, Is.True);
            Assert.That(Surface.Joystick.Center, Is.EqualTo(Start).Using(VectorComparer));
            Assert.That(Surface.Joystick.Direction, Is.EqualTo(new Vector2(0.4f / 0.9f, 0f)).Using(_directionComparer));
        }

        [TestCase(JoystickMode.Fixed)]
        [TestCase(JoystickMode.Floating)]
        [TestCase(JoystickMode.Following)]
        public void CancelTouch_OfThePrimary_PutsTheJoystickAtRest(JoystickMode mode)
        {
            var center = GetStartCenter(mode);
            SetJoystick(mode, 0.2f);

            Press(FirstFingerId, center);
            Move(FirstFingerId, center + new Vector2(60f, 0f));
            Cancel(FirstFingerId);

            AssertAtRest(Surface.Joystick);
        }

        [TestCase(JoystickMode.Fixed)]
        [TestCase(JoystickMode.Floating)]
        [TestCase(JoystickMode.Following)]
        public void CancelAllTouches_PutsTheJoystickAtRest(JoystickMode mode)
        {
            var center = GetStartCenter(mode);
            SetJoystick(mode, 0.2f);

            Press(FirstFingerId, center);
            Press(SecondFingerId, center + new Vector2(10f, 10f));
            Move(FirstFingerId, center + new Vector2(60f, 0f));
            Surface.CancelAllTouches(5.0);

            AssertAtRest(Surface.Joystick);
        }

        [TestCase(JoystickMode.Fixed, 0.1f)]
        [TestCase(JoystickMode.Fixed, 0f)]
        [TestCase(JoystickMode.Floating, 0.1f)]
        [TestCase(JoystickMode.Floating, 0f)]
        [TestCase(JoystickMode.Following, 0.1f)]
        [TestCase(JoystickMode.Following, 0f)]
        public void Radius_Zero_GivesZeroDirectionAndNoNaN(JoystickMode mode, float deadZone)
        {
            var center = GetStartCenter(mode);
            SetJoystick(mode, deadZone, 0f);

            AssertFinite(Surface.Joystick);

            Press(FirstFingerId, center);
            AssertFinite(Surface.Joystick);
            Assert.That(Surface.Joystick.Direction, Is.EqualTo(Vector2.zero));

            Move(FirstFingerId, center + new Vector2(50f, 20f));
            AssertFinite(Surface.Joystick);
            Assert.That(Surface.Joystick.Direction, Is.EqualTo(Vector2.zero));
            Assert.That(Surface.Joystick.Radius, Is.EqualTo(0f));

            Lift(FirstFingerId, center);
            AssertFinite(Surface.Joystick);
        }

        [TestCase(JoystickMode.Fixed, 0.1f)]
        [TestCase(JoystickMode.Fixed, 0f)]
        [TestCase(JoystickMode.Floating, 0.1f)]
        [TestCase(JoystickMode.Floating, 0f)]
        [TestCase(JoystickMode.Following, 0.1f)]
        [TestCase(JoystickMode.Following, 0f)]
        public void PixelsPerCentimeter_Zero_GivesZeroDirectionAndNoNaN(JoystickMode mode, float deadZone)
        {
            var center = GetStartCenter(mode);
            Surface.SetContext(CreateRules(), 0f, PixelRect);
            SetJoystick(mode, deadZone);

            AssertFinite(Surface.Joystick);

            Press(FirstFingerId, center);
            AssertFinite(Surface.Joystick);
            Assert.That(Surface.Joystick.Direction, Is.EqualTo(Vector2.zero));

            Move(FirstFingerId, center + new Vector2(50f, 20f));
            AssertFinite(Surface.Joystick);
            Assert.That(Surface.Joystick.Direction, Is.EqualTo(Vector2.zero));
            Assert.That(Surface.Joystick.Radius, Is.EqualTo(0f));

            Lift(FirstFingerId, center);
            AssertFinite(Surface.Joystick);
        }
    }
}
