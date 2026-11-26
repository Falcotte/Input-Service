using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AngryKoala.Input.Tests
{
    public sealed class InputAreaValidationTests
    {
        private const float _maximumDeadZone = 0.9f;

        private InputArea _area;

        [TearDown]
        public void TearDown()
        {
            if (_area != null)
            {
                Object.DestroyImmediate(_area);
            }
        }

        [Test]
        public void OnValidate_ValidValues_AreKept()
        {
            _area = CreateArea(new Vector2(0.1f, 0.2f), new Vector2(0.6f, 0.9f), 2.5f, 0.3f);

            ValidationTestUtility.Validate(_area);

            Assert.That(_area.Minimum, Is.EqualTo(new Vector2(0.1f, 0.2f)));
            Assert.That(_area.Maximum, Is.EqualTo(new Vector2(0.6f, 0.9f)));
            Assert.That(_area.JoystickRadius, Is.EqualTo(2.5f));
            Assert.That(_area.JoystickDeadZone, Is.EqualTo(0.3f));
        }

        [Test]
        public void OnValidate_MinimumOutsideZeroToOne_IsClampedPerAxis()
        {
            _area = CreateArea(new Vector2(-0.5f, 2f), Vector2.one);

            ValidationTestUtility.Validate(_area);

            Assert.That(_area.Minimum.x, Is.EqualTo(0f));
            Assert.That(_area.Minimum.y, Is.EqualTo(1f));
        }

        [Test]
        public void OnValidate_MaximumOutsideZeroToOne_IsClampedPerAxis()
        {
            _area = CreateArea(Vector2.zero, new Vector2(1.5f, -1f));

            ValidationTestUtility.Validate(_area);

            Assert.That(_area.Maximum.x, Is.EqualTo(1f));
            Assert.That(_area.Maximum.y, Is.GreaterThanOrEqualTo(0f));
        }

        [Test]
        public void OnValidate_MaximumBelowTheMinimum_IsNotBelowItOnEitherAxis()
        {
            _area = CreateArea(new Vector2(0.8f, 0.9f), new Vector2(0.3f, 0.1f));

            ValidationTestUtility.Validate(_area);

            Assert.That(_area.Maximum.x, Is.GreaterThanOrEqualTo(_area.Minimum.x));
            Assert.That(_area.Maximum.y, Is.GreaterThanOrEqualTo(_area.Minimum.y));
            AssertWithinZeroToOne(_area.Minimum);
            AssertWithinZeroToOne(_area.Maximum);
        }

        [Test]
        public void OnValidate_MaximumBelowTheMinimumOnOneAxis_LeavesTheOtherAxisAlone()
        {
            _area = CreateArea(new Vector2(0.8f, 0.2f), new Vector2(0.3f, 0.7f));

            ValidationTestUtility.Validate(_area);

            Assert.That(_area.Maximum.x, Is.GreaterThanOrEqualTo(_area.Minimum.x));
            Assert.That(_area.Minimum.y, Is.EqualTo(0.2f));
            Assert.That(_area.Maximum.y, Is.EqualTo(0.7f));
        }

        [Test]
        public void OnValidate_NegativeJoystickRadius_IsZero()
        {
            _area = CreateArea(Vector2.zero, Vector2.one, -1f);

            ValidationTestUtility.Validate(_area);

            Assert.That(_area.JoystickRadius, Is.EqualTo(0f));
        }

        [TestCase(0.95f)]
        [TestCase(1f)]
        [TestCase(5f)]
        public void OnValidate_DeadZoneAboveTheMaximum_IsTheMaximum(float value)
        {
            _area = CreateArea(Vector2.zero, Vector2.one, 1f, value);

            ValidationTestUtility.Validate(_area);

            Assert.That(_area.JoystickDeadZone, Is.EqualTo(_maximumDeadZone));
        }

        [TestCase(-0.1f)]
        [TestCase(-3f)]
        public void OnValidate_NegativeDeadZone_IsZero(float value)
        {
            _area = CreateArea(Vector2.zero, Vector2.one, 1f, value);

            ValidationTestUtility.Validate(_area);

            Assert.That(_area.JoystickDeadZone, Is.EqualTo(0f));
        }

        private static InputArea CreateArea(Vector2 minimum, Vector2 maximum, float joystickRadius = 1f,
            float joystickDeadZone = 0.1f)
        {
            return InputAreaTestUtility.CreateArea("Test Area", InputAreaSpace.Screen, minimum, maximum,
                JoystickMode.Fixed, joystickRadius, joystickDeadZone);
        }

        private static void AssertWithinZeroToOne(Vector2 value)
        {
            Assert.That(value.x, Is.InRange(0f, 1f));
            Assert.That(value.y, Is.InRange(0f, 1f));
        }
    }
}
