using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Constraints;
using Is = UnityEngine.TestTools.Constraints.Is;

namespace AngryKoala.Input.Tests
{
    public sealed class InputServiceAllocationTests : InputServiceTestBase
    {
        private const int _warmUpFrames = 10;
        private const float _moveStepPixels = 3f;

        private int _dragCount;

        [UnityTest]
        public IEnumerator ProcessFrame_WithAFingerDraggingAndAHandler_AllocatesNothing()
        {
            ScreenSurface.OnDrag += CountDrag;

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

            Assert.That(ScreenSurface.IsTouching, Is.True);
            Assert.That(_dragCount, Is.GreaterThan(0));

            var dragCountBefore = _dragCount;

            position += new Vector2(GetStep(_warmUpFrames), 0f);
            Touch.Move(FirstFingerId, position);
            InputSystem.Update();

            Assert.That(() => Service.ProcessFrame(), Is.Not.AllocatingGCMemory());
            Assert.That(_dragCount, Is.GreaterThan(dragCountBefore));

            ScreenSurface.OnDrag -= CountDrag;
        }

        private static float GetStep(int frame) => frame % 2 == 0 ? _moveStepPixels : -_moveStepPixels;

        private void CountDrag(TouchData touch) => _dragCount++;
    }
}
