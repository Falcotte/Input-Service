using System;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Constraints;
using static AngryKoala.Input.Tests.InputSurfaceTestUtility;
using Is = UnityEngine.TestTools.Constraints.Is;

namespace AngryKoala.Input.Tests
{
    public sealed class InputSurfaceEventTests
    {
        private InputSurface _surface;
        private InputSurfaceRecorder _recorder;
        private int _firstCount;
        private int _secondCount;
        private int _dragCount;
        private bool _secondAdded;
        private Vector2 _position;
        private double _time;

        [SetUp]
        public void SetUp()
        {
            _surface = CreateSurface(CreateRules());
            _recorder = new InputSurfaceRecorder(_surface);
            _firstCount = 0;
            _secondCount = 0;
            _dragCount = 0;
            _secondAdded = false;
        }

        [Test]
        public void TouchEnded_IsTheLastEventOfATap()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.EndTouch(1, Start, 0.1);

            Assert.That(_recorder.Events,
                Is.EqualTo(new[]
                {
                    InputSurfaceRecorder.TouchBegan, InputSurfaceRecorder.Tap, InputSurfaceRecorder.TouchEnded
                }));
        }

        [Test]
        public void TouchEnded_IsTheLastEventOfADrag()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.UpdateTouch(1, Offset(30f, 0f), 0.1);
            _surface.UpdateTouch(1, Offset(40f, 0f), 0.2);
            _surface.EndTouch(1, Offset(40f, 0f), 0.3);

            Assert.That(_recorder.Events,
                Is.EqualTo(new[]
                {
                    InputSurfaceRecorder.TouchBegan, InputSurfaceRecorder.DragBegan, InputSurfaceRecorder.Drag,
                    InputSurfaceRecorder.Drag, InputSurfaceRecorder.DragEnded, InputSurfaceRecorder.TouchEnded
                }));
        }

        [Test]
        public void TouchEnded_IsTheLastEventOfACanceledTouch()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.UpdateTouch(1, Offset(30f, 0f), 0.1);
            _surface.CancelTouch(1, 0.2);

            Assert.That(_recorder.Events[_recorder.Events.Count - 1], Is.EqualTo(InputSurfaceRecorder.TouchEnded));
            Assert.That(_recorder.Events, Does.Contain(InputSurfaceRecorder.DragEnded));
        }

        [Test]
        public void TouchEnded_IsTheLastEventForEachFingerWhenAllTouchesAreCanceled()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.BeginTouch(2, Offset(300f, 0f), 0.0);
            _surface.UpdateTouch(1, Offset(30f, 0f), 0.1);
            _surface.UpdateTouch(2, Offset(300f, 0f), 0.1);
            _recorder.Clear();

            _surface.CancelAllTouches(0.2);

            Assert.That(_recorder.Events,
                Is.EqualTo(new[]
                {
                    InputSurfaceRecorder.DragEnded, InputSurfaceRecorder.TouchEnded, InputSurfaceRecorder.TouchEnded
                }));
        }

        [Test]
        public void LastGesture_FollowsTheLastGestureRaised()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.EndTouch(1, Start, 0.1);

            Assert.That(_surface.LastGesture, Is.EqualTo("Tap"));

            _surface.BeginTouch(2, Start, 1.0);
            _surface.UpdateTouch(2, Offset(30f, 0f), 1.1);

            Assert.That(_surface.LastGesture, Is.EqualTo("Drag"));
        }

        [Test]
        public void LastGesture_IsNotChangedByATouchThatRaisesNoGesture()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.EndTouch(1, Start, 0.1);

            _surface.BeginTouch(2, Start, 1.0);
            _surface.UpdateTouch(2, Offset(10f, 0f), 1.1);
            _surface.EndTouch(2, Offset(10f, 0f), 2.0);

            Assert.That(_surface.LastGesture, Is.EqualTo("Tap"));
        }

        [Test]
        public void Handler_ThatThrowsOnTouchBegan_IsLogged_AndOthersStillRun()
        {
            _surface.OnTouchBegan += ThrowFromTouch;
            _surface.OnTouchBegan += CountSecond;

            LogAssert.Expect(LogType.Error, new Regex(nameof(IInputSurface.OnTouchBegan)));
            LogAssert.Expect(LogType.Exception, new Regex("InvalidOperationException"));
            _surface.BeginTouch(1, Start, 0.0);

            Assert.That(_secondCount, Is.EqualTo(1));
            Assert.That(_recorder.TouchesBegan, Has.Count.EqualTo(1));
            Assert.That(_surface.TouchCount, Is.EqualTo(1));
        }

        [Test]
        public void Handler_ThatThrowsOnTap_IsLogged_AndOthersStillRun()
        {
            _surface.OnTap += ThrowFromTouch;
            _surface.OnTap += CountSecond;
            _surface.BeginTouch(1, Start, 0.0);

            LogAssert.Expect(LogType.Error, new Regex(nameof(IInputSurface.OnTap)));
            LogAssert.Expect(LogType.Exception, new Regex("InvalidOperationException"));
            _surface.EndTouch(1, Start, 0.1);

            Assert.That(_secondCount, Is.EqualTo(1));
            Assert.That(_recorder.Taps, Has.Count.EqualTo(1));
            Assert.That(_recorder.TouchesEnded, Has.Count.EqualTo(1));
        }

        [Test]
        public void Handler_ThatThrowsOnDrag_IsLogged_AndOthersStillRun()
        {
            _surface.OnDrag += ThrowFromTouch;
            _surface.OnDrag += CountSecond;
            _surface.BeginTouch(1, Start, 0.0);

            LogAssert.Expect(LogType.Error, new Regex(nameof(IInputSurface.OnDrag)));
            LogAssert.Expect(LogType.Exception, new Regex("InvalidOperationException"));
            _surface.UpdateTouch(1, Offset(30f, 0f), 0.1);

            Assert.That(_secondCount, Is.EqualTo(1));
            Assert.That(_recorder.Drags, Has.Count.EqualTo(1));
        }

        [Test]
        public void Handler_RemovedWhileTheEventIsRaised_IsNotCalledLaterInThatRaise()
        {
            _surface.OnTouchBegan += RemoveSecond;
            _surface.OnTouchBegan += CountSecond;

            _surface.BeginTouch(1, Start, 0.0);

            Assert.That(_secondCount, Is.Zero);
        }

        [Test]
        public void Handler_AddedWhileTheEventIsRaised_IsFirstCalledByTheNextRaise()
        {
            _surface.OnTouchBegan += AddSecondOnce;

            _surface.BeginTouch(1, Start, 0.0);

            Assert.That(_secondCount, Is.Zero);

            _surface.BeginTouch(2, Offset(100f, 0f), 0.1);

            Assert.That(_secondCount, Is.EqualTo(1));
        }

        [Test]
        public void Unsubscribe_OfAHandlerThatWasNeverSubscribed_DoesNothing()
        {
            _surface.OnTouchBegan += CountFirst;

            Assert.DoesNotThrow(() => _surface.OnTouchBegan -= CountSecond);

            _surface.BeginTouch(1, Start, 0.0);

            Assert.That(_firstCount, Is.EqualTo(1));
        }

        [Test]
        public void Unsubscribe_StopsTheCalls()
        {
            _surface.OnTouchBegan += CountFirst;
            _surface.OnTouchBegan -= CountFirst;

            _surface.BeginTouch(1, Start, 0.0);

            Assert.That(_firstCount, Is.Zero);
        }

        [Test]
        public void Drag_WithAFingerDown_DoesNotAllocate()
        {
            _surface = CreateSurface(CreateRules());
            _surface.OnDrag += CountDrag;
            _surface.BeginTouch(1, Start, 0.0);
            _position = Start;
            _time = 0.0;

            for (var frame = 0; frame < 4; frame++)
            {
                DragFrame();
            }

            var dragCountBefore = _dragCount;

            Assert.That(() => DragFrame(), Is.Not.AllocatingGCMemory());
            Assert.That(_dragCount, Is.GreaterThan(dragCountBefore));
        }

        private void DragFrame()
        {
            _position.x += 5f;
            _time += 0.016;

            _surface.UpdateTouch(1, _position, _time);
            _surface.CompleteFrame(_time);
        }

        private void ThrowFromTouch(TouchData touch)
        {
            throw new InvalidOperationException("Test");
        }

        private void CountFirst(TouchData touch)
        {
            _firstCount++;
        }

        private void CountSecond(TouchData touch)
        {
            _secondCount++;
        }

        private void CountDrag(TouchData touch)
        {
            _dragCount++;
        }

        private void RemoveSecond(TouchData touch)
        {
            _surface.OnTouchBegan -= CountSecond;
        }

        private void AddSecondOnce(TouchData touch)
        {
            if (_secondAdded)
            {
                return;
            }

            _secondAdded = true;
            _surface.OnTouchBegan += CountSecond;
        }
    }
}
