using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools.Constraints;
using static AngryKoala.Input.Tests.InputSurfaceTestUtility;
using Is = UnityEngine.TestTools.Constraints.Is;

namespace AngryKoala.Input.Tests
{
    public sealed class InputSurfaceJoystickAllocationTests : InputSurfaceJoystickTestBase
    {
        private JoystickData _joystick;

        [SetUp]
        public void ResetJoystick()
        {
            _joystick = default;
        }

        [TestCase(JoystickMode.Fixed)]
        [TestCase(JoystickMode.Floating)]
        [TestCase(JoystickMode.Following)]
        public void Joystick_ReadWhileTouching_AllocatesNothing(JoystickMode mode)
        {
            var center = GetStartCenter(mode);
            SetJoystick(mode, 0.1f);

            Press(FirstFingerId, center);
            Move(FirstFingerId, center + new Vector2(150f, 0f));

            _joystick = Surface.Joystick;

            Assert.That(() => { _joystick = Surface.Joystick; }, Is.Not.AllocatingGCMemory());
            Assert.That(_joystick.IsActive, Is.True);
        }

        [TestCase(JoystickMode.Fixed)]
        [TestCase(JoystickMode.Floating)]
        [TestCase(JoystickMode.Following)]
        public void Joystick_ReadAtRest_AllocatesNothing(JoystickMode mode)
        {
            SetJoystick(mode, 0.1f);

            _joystick = Surface.Joystick;

            Assert.That(() => { _joystick = Surface.Joystick; }, Is.Not.AllocatingGCMemory());
        }

        [TestCase(JoystickMode.Fixed)]
        [TestCase(JoystickMode.Floating)]
        [TestCase(JoystickMode.Following)]
        public void SetJoystick_CalledEveryFrame_AllocatesNothing(JoystickMode mode)
        {
            SetJoystick(mode, 0.1f);

            Press(FirstFingerId, GetStartCenter(mode));
            SetJoystick(mode, 0.1f);

            Assert.That(() => SetJoystick(mode, 0.1f), Is.Not.AllocatingGCMemory());
        }
    }
}
