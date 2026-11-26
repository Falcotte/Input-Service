using NUnit.Framework;
using UnityEngine;
using static AngryKoala.Input.Tests.InputSurfaceTestUtility;

namespace AngryKoala.Input.Tests
{
    public sealed class InputSurfaceDragTests
    {
        private InputSurface _surface;
        private InputSurfaceRecorder _recorder;

        [SetUp]
        public void SetUp()
        {
            _surface = CreateSurface(CreateRules());
            _recorder = new InputSurfaceRecorder(_surface);
        }

        [Test]
        public void Drag_ReachingTheDragStartDistance_RaisesDragBeganThenDragInOneCall()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _recorder.Clear();

            _surface.UpdateTouch(1, Offset(30f, 0f), 0.1);

            Assert.That(_recorder.Events,
                Is.EqualTo(new[] { InputSurfaceRecorder.DragBegan, InputSurfaceRecorder.Drag }));
        }

        [Test]
        public void Drag_BelowTheDragStartDistance_IsNotRaised()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.UpdateTouch(1, Offset(20f, 0f), 0.1);

            Assert.That(_recorder.DragsBegan, Is.Empty);
            Assert.That(_recorder.Drags, Is.Empty);
        }

        [Test]
        public void Drag_IsMeasuredFromTheStartAndNotPerFrame()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.UpdateTouch(1, Offset(10f, 0f), 0.1);
            _surface.UpdateTouch(1, Offset(20f, 0f), 0.2);

            Assert.That(_recorder.DragsBegan, Is.Empty);

            _surface.UpdateTouch(1, Offset(30f, 0f), 0.3);

            Assert.That(_recorder.DragsBegan, Has.Count.EqualTo(1));
        }

        [Test]
        public void Drag_DistanceInAnyDirection_BeginsADrag()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.UpdateTouch(1, Offset(-18f, 18f), 0.1);

            Assert.That(_recorder.DragsBegan, Has.Count.EqualTo(1));
        }

        [Test]
        public void DragBegan_CarriesTheTouchData()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.UpdateTouch(1, Offset(30f, 0f), 0.1);

            var touch = _recorder.DragsBegan[0];

            Assert.That(touch.FingerId, Is.EqualTo(1));
            Assert.That(touch.Position, Is.EqualTo(Offset(30f, 0f)));
            Assert.That(touch.StartPosition, Is.EqualTo(Start));
            Assert.That(touch.Delta, Is.EqualTo(new Vector2(30f, 0f)));
        }

        [Test]
        public void Drag_BeginsOnlyOncePerTouch()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.UpdateTouch(1, Offset(30f, 0f), 0.1);
            _surface.UpdateTouch(1, Offset(60f, 0f), 0.2);
            _surface.UpdateTouch(1, Start, 0.3);

            Assert.That(_recorder.DragsBegan, Has.Count.EqualTo(1));
        }

        [Test]
        public void Drag_LastGestureIsDrag_WhenADragBegins()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.UpdateTouch(1, Offset(30f, 0f), 0.1);

            Assert.That(_surface.LastGesture, Is.EqualTo("Drag"));
        }

        [Test]
        public void Drag_InALaterFrameWithMovement_IsRaised()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.UpdateTouch(1, Offset(30f, 0f), 0.1);

            _surface.UpdateTouch(1, Offset(45f, 5f), 0.2);

            Assert.That(_recorder.Drags, Has.Count.EqualTo(2));
            Assert.That(_recorder.Drags[1].Delta, Is.EqualTo(new Vector2(15f, 5f)));
            Assert.That(_recorder.DragsBegan, Has.Count.EqualTo(1));
        }

        [Test]
        public void Drag_FingerEndsFurtherOnThanTheLastUpdate_RaisesTheLastStepBeforeDragEnded()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.UpdateTouch(1, Offset(30f, 0f), 0.1);
            _surface.UpdateTouch(1, Offset(60f, 0f), 0.2);
            _recorder.Clear();

            _surface.EndTouch(1, Offset(90f, 0f), 0.3);

            Assert.That(_recorder.Events,
                Is.EqualTo(new[]
                {
                    InputSurfaceRecorder.Drag, InputSurfaceRecorder.DragEnded, InputSurfaceRecorder.TouchEnded
                }));
            Assert.That(_recorder.Drags[0].Delta, Is.EqualTo(new Vector2(30f, 0f)));
        }

        [Test]
        public void Drag_AllDeltasTogether_EqualTheTotalDeltaOfTheTouchThatEnds()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.UpdateTouch(1, Offset(30f, 0f), 0.1);
            _surface.UpdateTouch(1, Offset(60f, 10f), 0.2);
            _surface.EndTouch(1, Offset(90f, 25f), 0.3);

            var sum = Vector2.zero;

            foreach (var touch in _recorder.Drags)
            {
                sum += touch.Delta;
            }

            Assert.That(_recorder.DragsEnded, Has.Count.EqualTo(1));
            Assert.That(_recorder.DragsEnded[0].TotalDelta, Is.EqualTo(new Vector2(90f, 25f)));
            Assert.That(sum, Is.EqualTo(_recorder.DragsEnded[0].TotalDelta));
        }

        [Test]
        public void Drag_FingerEndsWhereTheLastUpdateLeftIt_RaisesNoExtraDrag()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.UpdateTouch(1, Offset(30f, 0f), 0.1);
            _surface.UpdateTouch(1, Offset(60f, 0f), 0.2);

            _surface.EndTouch(1, Offset(60f, 0f), 0.3);

            Assert.That(_recorder.Drags, Has.Count.EqualTo(2));
        }

        [Test]
        public void Drag_BeganHandlerCancelsTheTouch_RaisesNoDragAfterTouchEnded()
        {
            _surface.OnDragBegan += CancelTouchOne;
            _surface.BeginTouch(1, Start, 0.0);

            _surface.UpdateTouch(1, Offset(30f, 0f), 0.1);

            Assert.That(_recorder.Events,
                Is.EqualTo(new[]
                {
                    InputSurfaceRecorder.TouchBegan, InputSurfaceRecorder.DragBegan, InputSurfaceRecorder.DragEnded,
                    InputSurfaceRecorder.TouchEnded
                }));
            Assert.That(_recorder.TouchesEnded[0].WasCanceled, Is.True);
        }

        [Test]
        public void Drag_BeganHandlerCancelsAllTouches_RaisesNoDragAfterTouchEnded()
        {
            _surface.OnDragBegan += CancelAllTouches;
            _surface.BeginTouch(1, Start, 0.0);

            _surface.UpdateTouch(1, Offset(30f, 0f), 0.1);

            Assert.That(_recorder.Events,
                Is.EqualTo(new[]
                {
                    InputSurfaceRecorder.TouchBegan, InputSurfaceRecorder.DragBegan, InputSurfaceRecorder.DragEnded,
                    InputSurfaceRecorder.TouchEnded
                }));
            Assert.That(_recorder.TouchesEnded[0].WasCanceled, Is.True);
        }

        [Test]
        public void Drag_InALaterFrameWithoutMovement_IsNotRaised()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.UpdateTouch(1, Offset(30f, 0f), 0.1);

            _surface.UpdateTouch(1, Offset(30f, 0f), 0.2);
            _surface.UpdateTouch(1, Offset(30f, 0f), 0.3);

            Assert.That(_recorder.Drags, Has.Count.EqualTo(1));
        }

        [Test]
        public void Drag_AfterAPause_RaisesAgainWhenTheFingerMovesAgain()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.UpdateTouch(1, Offset(30f, 0f), 0.1);
            _surface.UpdateTouch(1, Offset(30f, 0f), 0.2);

            _surface.UpdateTouch(1, Offset(31f, 0f), 0.3);

            Assert.That(_recorder.Drags, Has.Count.EqualTo(2));
        }

        [Test]
        public void Drag_WhenTheFingerLifts_RaisesDragEndedBeforeTouchEnded()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.UpdateTouch(1, Offset(30f, 0f), 0.1);
            _recorder.Clear();

            _surface.EndTouch(1, Offset(30f, 0f), 0.2);

            Assert.That(_recorder.Events,
                Is.EqualTo(new[] { InputSurfaceRecorder.DragEnded, InputSurfaceRecorder.TouchEnded }));
            Assert.That(_recorder.DragsEnded[0].WasCanceled, Is.False);
        }

        [Test]
        public void Drag_WhenTheFingerIsCanceled_RaisesDragEndedAsCanceledBeforeTouchEnded()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.UpdateTouch(1, Offset(30f, 0f), 0.1);
            _recorder.Clear();

            _surface.CancelTouch(1, 0.2);

            Assert.That(_recorder.Events,
                Is.EqualTo(new[] { InputSurfaceRecorder.DragEnded, InputSurfaceRecorder.TouchEnded }));
            Assert.That(_recorder.DragsEnded[0].WasCanceled, Is.True);
        }

        [Test]
        public void Drag_WhenAllTouchesAreCanceled_RaisesDragEndedAsCanceled()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.UpdateTouch(1, Offset(30f, 0f), 0.1);

            _surface.CancelAllTouches(0.2);

            Assert.That(_recorder.DragsEnded, Has.Count.EqualTo(1));
            Assert.That(_recorder.DragsEnded[0].WasCanceled, Is.True);
        }

        [Test]
        public void DragEnded_ForATouchThatNeverBeganADrag_IsNotRaised()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.UpdateTouch(1, Offset(10f, 0f), 0.1);
            _surface.EndTouch(1, Offset(10f, 0f), 0.2);

            Assert.That(_recorder.DragsEnded, Is.Empty);
        }

        [Test]
        public void DragEnded_ForACanceledTouchThatNeverBeganADrag_IsNotRaised()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.CancelTouch(1, 0.1);

            Assert.That(_recorder.DragsEnded, Is.Empty);
        }

        [Test]
        public void Drag_ForANonPrimaryFinger_IsNotRaised()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.BeginTouch(2, Offset(300f, 0f), 0.0);

            _surface.UpdateTouch(1, Start, 0.1);
            _surface.UpdateTouch(2, Offset(400f, 0f), 0.1);

            Assert.That(_recorder.DragsBegan, Is.Empty);
            Assert.That(_recorder.Drags, Is.Empty);
        }

        [Test]
        public void Drag_WhenANonPrimaryLifts_RaisesNoDragEnded()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.BeginTouch(2, Offset(300f, 0f), 0.0);
            _surface.UpdateTouch(2, Offset(400f, 0f), 0.1);

            _surface.EndTouch(2, Offset(400f, 0f), 0.2);

            Assert.That(_recorder.DragsEnded, Is.Empty);
        }

        [Test]
        public void Drag_WhenTheDraggingPrimaryLiftsWhileAnotherFingerIsDown_EndsTheDragOfThePrimary()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.BeginTouch(2, Offset(300f, 0f), 0.0);
            _surface.UpdateTouch(1, Offset(30f, 0f), 0.1);
            _surface.UpdateTouch(2, Offset(300f, 0f), 0.1);

            _surface.EndTouch(1, Offset(30f, 0f), 0.2);

            Assert.That(_recorder.DragsEnded, Has.Count.EqualTo(1));
            Assert.That(_recorder.DragsEnded[0].FingerId, Is.EqualTo(1));
        }

        [Test]
        public void Drag_ForAPrimaryByHandover_IsMeasuredFromTheNewStart()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.BeginTouch(2, Offset(200f, 0f), 0.0);
            _surface.UpdateTouch(1, Start, 0.1);
            _surface.UpdateTouch(2, Offset(300f, 0f), 0.1);
            _surface.EndTouch(1, Start, 0.2);
            _surface.UpdateTouch(2, Offset(300f, 0f), 0.2);

            Assert.That(_recorder.DragsBegan, Is.Empty);

            _surface.UpdateTouch(2, Offset(320f, 0f), 0.3);

            Assert.That(_recorder.DragsBegan, Is.Empty);

            _surface.UpdateTouch(2, Offset(335f, 0f), 0.4);

            Assert.That(_recorder.DragsBegan, Has.Count.EqualTo(1));
            Assert.That(_recorder.DragsBegan[0].FingerId, Is.EqualTo(2));
            Assert.That(_recorder.DragsBegan[0].StartPosition, Is.EqualTo(Offset(300f, 0f)));
        }

        private void CancelTouchOne(TouchData touch)
        {
            _surface.CancelTouch(1, 0.15);
        }

        private void CancelAllTouches(TouchData touch)
        {
            _surface.CancelAllTouches(0.15);
        }
    }
}
