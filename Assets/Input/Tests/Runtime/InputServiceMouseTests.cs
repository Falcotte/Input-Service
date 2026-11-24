using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;

namespace AngryKoala.Input.Tests
{
    public sealed class InputServiceMouseTests : InputServiceTestBase
    {
        private static readonly Vector2EqualityComparer _positionComparer = Vector2EqualityComparer.Instance;

        [UnityTest]
        public IEnumerator Press_OnTheScreen_BeginsATouchAtThePosition()
        {
            var recorder = Record(ScreenSurface);
            var mouse = AddDevice<Mouse>();
            yield return PlaceMouse(mouse, ScreenCenter);

            QueueMouse(mouse, ScreenCenter, true);
            yield return null;

            Assert.That(recorder.TouchBegan.Count, Is.EqualTo(1));
            Assert.That(ScreenSurface.IsTouching, Is.True);
            Assert.That(ScreenSurface.PrimaryTouch.Position, Is.EqualTo(ScreenCenter).Using(_positionComparer));
        }

        [UnityTest]
        public IEnumerator Move_WhilePressed_UpdatesThePrimaryTouchPosition()
        {
            var target = ScreenPoint(0.7f, 0.3f);
            var mouse = AddDevice<Mouse>();
            yield return PlaceMouse(mouse, ScreenCenter);

            QueueMouse(mouse, ScreenCenter, true);
            yield return null;

            QueueMouse(mouse, target, true);
            yield return null;

            Assert.That(ScreenSurface.PrimaryTouch.Position, Is.EqualTo(target).Using(_positionComparer));
        }

        [UnityTest]
        public IEnumerator Release_AfterPress_EndsTheTouch()
        {
            var recorder = Record(ScreenSurface);
            var mouse = AddDevice<Mouse>();
            yield return PlaceMouse(mouse, ScreenCenter);

            QueueMouse(mouse, ScreenCenter, true);
            yield return null;

            QueueMouse(mouse, ScreenCenter, false);
            yield return null;

            Assert.That(recorder.TouchEnded.Count, Is.EqualTo(1));
            Assert.That(recorder.TouchEnded[0].WasCanceled, Is.False);
            Assert.That(ScreenSurface.TouchCount, Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator PressAndRelease_InPlace_RaisesTap()
        {
            var recorder = Record(ScreenSurface);
            var mouse = AddDevice<Mouse>();
            yield return PlaceMouse(mouse, ScreenCenter);

            QueueMouse(mouse, ScreenCenter, true);
            yield return null;

            QueueMouse(mouse, ScreenCenter, false);
            yield return WaitFrames(3);

            Assert.That(recorder.Taps.Count, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator Press_WhereAFilterTakesThePosition_IsNotTheScreens()
        {
            var recorder = Record(ScreenSurface);
            var filter = new TestPointerFilter(FilterArea);
            Service.AddPointerFilter(filter);
            var mouse = AddDevice<Mouse>();
            yield return PlaceMouse(mouse, PointInsideFilterArea);

            QueueMouse(mouse, PointInsideFilterArea, true);
            yield return null;

            QueueMouse(mouse, PointInsideFilterArea, false);
            yield return WaitFrames(3);

            Assert.That(filter.CallCount, Is.GreaterThan(0));
            Assert.That(recorder.TouchBegan.Count, Is.EqualTo(0));
            Assert.That(recorder.Taps.Count, Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator Press_WhileARealTouchIsDown_IsIgnored()
        {
            var recorder = Record(ScreenSurface);
            var mouse = AddDevice<Mouse>();
            var touchPosition = ScreenPoint(0.3f, 0.5f);
            var mousePosition = ScreenPoint(0.8f, 0.5f);
            yield return PlaceMouse(mouse, mousePosition);

            Touch.Begin(FirstFingerId, touchPosition);
            yield return null;

            QueueMouse(mouse, mousePosition, true);
            yield return null;

            QueueMouse(mouse, mousePosition, false);
            yield return null;

            Assert.That(recorder.TouchBegan.Count, Is.EqualTo(1));
            Assert.That(ScreenSurface.TouchCount, Is.EqualTo(1));
            Assert.That(ScreenSurface.PrimaryTouch.Position, Is.EqualTo(touchPosition).Using(_positionComparer));
            Assert.That(recorder.TouchEnded.Count, Is.EqualTo(0));
        }

        private static IEnumerator PlaceMouse(Mouse mouse, Vector2 position)
        {
            QueueMouse(mouse, position, false);

            yield return null;
        }

        private static void QueueMouse(Mouse mouse, Vector2 position, bool isLeftButtonDown)
        {
            var state = new MouseState { position = position };
            state = state.WithButton(MouseButton.Left, isLeftButtonDown);

            InputSystem.QueueStateEvent(mouse, state);
        }
    }
}
