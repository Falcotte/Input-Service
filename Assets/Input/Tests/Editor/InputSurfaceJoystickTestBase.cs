using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools.Utils;
using static AngryKoala.Input.Tests.InputSurfaceTestUtility;

namespace AngryKoala.Input.Tests
{
    public abstract class InputSurfaceJoystickTestBase
    {
        protected const float RadiusCentimeters = 1f;
        protected const float RadiusPixels = RadiusCentimeters * PixelsPerCentimeter;
        protected const float NoDeadZone = 0f;
        protected const int FirstFingerId = 1;
        protected const int SecondFingerId = 2;

        protected static readonly Vector2 Rest = new(200f, 300f);

        protected static readonly Vector2EqualityComparer VectorComparer = new(0.01f);

        internal InputSurface Surface { get; private set; }

        private double _time;

        [SetUp]
        public void SetUpSurface()
        {
            Surface = CreateSurface(CreateRules());
            _time = 0.0;
        }

        protected void SetJoystick(JoystickMode mode, float deadZone = NoDeadZone,
            float radiusCentimeters = RadiusCentimeters)
        {
            Surface.SetJoystick(mode, radiusCentimeters, deadZone, Rest);
        }

        protected void Press(int fingerId, Vector2 position)
        {
            _time += 0.016;
            Surface.BeginTouch(fingerId, position, _time);
        }

        protected void Move(int fingerId, Vector2 position)
        {
            _time += 0.016;
            Frame(Surface, fingerId, position, _time);
        }

        protected void Lift(int fingerId, Vector2 position)
        {
            _time += 0.016;
            Surface.EndTouch(fingerId, position, _time);
        }

        protected void Cancel(int fingerId)
        {
            _time += 0.016;
            Surface.CancelTouch(fingerId, _time);
        }

        protected static Vector2 GetStartCenter(JoystickMode mode)
        {
            return mode == JoystickMode.Fixed ? Rest : Start;
        }

        protected static void AssertAtRest(JoystickData joystick)
        {
            Assert.That(joystick.IsActive, Is.False);
            Assert.That(joystick.Center, Is.EqualTo(Rest).Using(VectorComparer));
            Assert.That(joystick.HandlePosition, Is.EqualTo(Rest).Using(VectorComparer));
            Assert.That(joystick.Radius, Is.EqualTo(RadiusPixels).Within(0.01f));
            Assert.That(joystick.Direction, Is.EqualTo(Vector2.zero).Using(VectorComparer));
        }

        protected static void AssertFinite(JoystickData joystick)
        {
            Assert.That(float.IsFinite(joystick.Center.x) && float.IsFinite(joystick.Center.y), Is.True, "Center");
            Assert.That(float.IsFinite(joystick.HandlePosition.x) && float.IsFinite(joystick.HandlePosition.y), Is.True,
                "HandlePosition");
            Assert.That(float.IsFinite(joystick.Radius), Is.True, "Radius");
            Assert.That(float.IsFinite(joystick.Direction.x) && float.IsFinite(joystick.Direction.y), Is.True,
                "Direction");
        }
    }
}
