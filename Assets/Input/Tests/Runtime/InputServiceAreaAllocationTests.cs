using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Constraints;
using Is = UnityEngine.TestTools.Constraints.Is;

namespace AngryKoala.Input.Tests
{
    public sealed class InputServiceAreaAllocationTests : InputServiceAreaTestBase
    {
        private const int _warmUpFrames = 10;
        private const float _moveStepPixels = 3f;

        private int _dragCount;

        [SetUp]
        public void ResetCounters()
        {
            _dragCount = 0;
        }

        [UnityTest]
        public IEnumerator ProcessFrame_WithAFingerDraggingInAnAreaWithAJoystick_AllocatesNothing()
        {
            var area = CreateArea("Following", Vector2.zero, Vector2.one, JoystickMode.Following);
            var surface = Service.GetArea(area);
            surface.OnDrag += CountDrag;
            yield return null;

            var position = ScreenCenter;
            Touch.Begin(FirstFingerId, position);
            yield return null;

            position += new Vector2(0f, DragLength * 0.5f);
            Touch.Move(FirstFingerId, position);
            yield return null;

            for (var frame = 0; frame < _warmUpFrames; frame++)
            {
                position += new Vector2(GetStep(frame), 0f);
                Touch.Move(FirstFingerId, position);
                yield return null;
            }

            Assert.That(surface.IsTouching, Is.True);
            Assert.That(_dragCount, Is.GreaterThan(0));

            var dragCountBefore = _dragCount;

            position += new Vector2(GetStep(_warmUpFrames), 0f);
            Touch.Move(FirstFingerId, position);
            InputSystem.Update();

            Assert.That(() => Service.ProcessFrame(), Is.Not.AllocatingGCMemory());
            Assert.That(_dragCount, Is.GreaterThan(dragCountBefore));

            surface.OnDrag -= CountDrag;
        }

        [UnityTest]
        public IEnumerator Joystick_ReadWhileTheFingerIsDown_AllocatesNothing()
        {
            var area = CreateArea("Floating", Vector2.zero, Vector2.one, JoystickMode.Floating);
            var surface = Service.GetArea(area);
            var joystick = default(JoystickData);
            yield return null;

            Touch.Begin(FirstFingerId, ScreenCenter);
            yield return WaitFrames(2);

            joystick = surface.Joystick;

            Assert.That(() => joystick = surface.Joystick, Is.Not.AllocatingGCMemory());
            Assert.That(joystick.IsActive, Is.True);
        }

        private static float GetStep(int frame) => frame % 2 == 0 ? _moveStepPixels : -_moveStepPixels;

        private void CountDrag(TouchData touch) => _dragCount++;
    }
}
