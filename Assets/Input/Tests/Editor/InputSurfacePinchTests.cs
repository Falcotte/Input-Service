using NUnit.Framework;
using UnityEngine;
using static AngryKoala.Input.Tests.InputSurfaceTestUtility;

namespace AngryKoala.Input.Tests
{
    public sealed class InputSurfacePinchTests
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
        public void Pinch_DistanceUnchanged_IsNotRaised()
        {
            BeginPair();

            FeedPair(Offset(-100f, 0f), Offset(100f, 0f), 0.1);
            FeedPair(Offset(-100f, 0f), Offset(100f, 0f), 0.2);

            Assert.That(_recorder.Pinches, Is.Empty);
        }

        [Test]
        public void Pinch_FingersSpreadApart_RaisesScaleAndCenter()
        {
            BeginPair();

            FeedPair(Offset(-100f, 0f), Offset(200f, 0f), 0.1);

            Assert.That(_recorder.Pinches, Has.Count.EqualTo(1));
            Assert.That(_recorder.Pinches[0].Center, Is.EqualTo(Offset(50f, 0f)));
            Assert.That(_recorder.Pinches[0].ScaleDelta, Is.EqualTo(1.5f).Within(0.0001f));
            Assert.That(_recorder.Pinches[0].TotalScale, Is.EqualTo(1.5f).Within(0.0001f));
        }

        [Test]
        public void Pinch_FingersSqueezeTogether_RaisesAScaleBelowOne()
        {
            BeginPair();

            FeedPair(Offset(-50f, 0f), Offset(50f, 0f), 0.1);

            Assert.That(_recorder.Pinches, Has.Count.EqualTo(1));
            Assert.That(_recorder.Pinches[0].ScaleDelta, Is.EqualTo(0.5f).Within(0.0001f));
            Assert.That(_recorder.Pinches[0].TotalScale, Is.EqualTo(0.5f).Within(0.0001f));
        }

        [Test]
        public void Pinch_InALaterFrame_ScaleDeltaIsRelativeToTheLastFrameAndTotalScaleToTheStart()
        {
            BeginPair();

            FeedPair(Offset(-100f, 0f), Offset(200f, 0f), 0.1);
            FeedPair(Offset(-100f, 0f), Offset(300f, 0f), 0.2);

            Assert.That(_recorder.Pinches, Has.Count.EqualTo(2));
            Assert.That(_recorder.Pinches[1].ScaleDelta, Is.EqualTo(400f / 300f).Within(0.0001f));
            Assert.That(_recorder.Pinches[1].TotalScale, Is.EqualTo(2f).Within(0.0001f));
            Assert.That(_recorder.Pinches[1].Center, Is.EqualTo(Offset(100f, 0f)));
        }

        [Test]
        public void Pinch_FrameWithoutChangeAfterAChange_IsNotRaised()
        {
            BeginPair();
            FeedPair(Offset(-100f, 0f), Offset(200f, 0f), 0.1);

            FeedPair(Offset(-100f, 0f), Offset(200f, 0f), 0.2);

            Assert.That(_recorder.Pinches, Has.Count.EqualTo(1));
        }

        [Test]
        public void Pinch_BothFingersMovingTheSameWay_IsNotRaised()
        {
            BeginPair();

            FeedPair(Offset(-50f, 30f), Offset(150f, 30f), 0.1);

            Assert.That(_recorder.Pinches, Is.Empty);
        }

        [Test]
        public void Pinch_IsDecidedInCompleteFrame_NotInUpdateTouch()
        {
            BeginPair();

            _surface.UpdateTouch(1, Offset(-100f, 0f), 0.1);
            _surface.UpdateTouch(2, Offset(200f, 0f), 0.1);

            Assert.That(_recorder.Pinches, Is.Empty);

            _surface.CompleteFrame(0.1);

            Assert.That(_recorder.Pinches, Has.Count.EqualTo(1));
        }

        [Test]
        public void Pinch_LastGestureIsPinch()
        {
            BeginPair();

            FeedPair(Offset(-100f, 0f), Offset(200f, 0f), 0.1);

            Assert.That(_surface.LastGesture, Is.EqualTo("Pinch"));
        }

        [Test]
        public void Pinch_WithOneFinger_IsNotRaised()
        {
            _surface.BeginTouch(1, Start, 0.0);

            Frame(_surface, 1, Offset(100f, 0f), 0.1);
            Frame(_surface, 1, Offset(300f, 0f), 0.2);

            Assert.That(_recorder.Pinches, Is.Empty);
        }

        [Test]
        public void Pinch_UsesTheTwoEarliestFingers()
        {
            BeginPair();
            _surface.BeginTouch(3, Offset(0f, 300f), 0.0);

            _surface.UpdateTouch(1, Offset(-100f, 0f), 0.1);
            _surface.UpdateTouch(2, Offset(100f, 0f), 0.1);
            _surface.UpdateTouch(3, Offset(0f, 500f), 0.1);
            _surface.CompleteFrame(0.1);

            Assert.That(_recorder.Pinches, Is.Empty);

            _surface.UpdateTouch(1, Offset(-100f, 0f), 0.2);
            _surface.UpdateTouch(2, Offset(200f, 0f), 0.2);
            _surface.UpdateTouch(3, Offset(0f, 500f), 0.2);
            _surface.CompleteFrame(0.2);

            Assert.That(_recorder.Pinches, Has.Count.EqualTo(1));
            Assert.That(_recorder.Pinches[0].TotalScale, Is.EqualTo(1.5f).Within(0.0001f));
        }

        [Test]
        public void Pinch_WhenAFingerOfThePairLifts_Ends()
        {
            BeginPair();
            FeedPair(Offset(-100f, 0f), Offset(200f, 0f), 0.1);
            _recorder.Clear();

            _surface.EndTouch(2, Offset(200f, 0f), 0.2);
            Frame(_surface, 1, Offset(-300f, 0f), 0.2);
            Frame(_surface, 1, Offset(-400f, 0f), 0.3);

            Assert.That(_recorder.Pinches, Is.Empty);
        }

        [Test]
        public void Pinch_WhenAFingerOfThePairIsCanceled_Ends()
        {
            BeginPair();
            FeedPair(Offset(-100f, 0f), Offset(200f, 0f), 0.1);
            _recorder.Clear();

            _surface.CancelTouch(2, 0.2);
            Frame(_surface, 1, Offset(-300f, 0f), 0.2);

            Assert.That(_recorder.Pinches, Is.Empty);
        }

        [Test]
        public void Pinch_AfterThePairLifted_ANewPairStartsFresh()
        {
            BeginPair();
            FeedPair(Offset(-100f, 0f), Offset(200f, 0f), 0.1);
            _surface.EndTouch(2, Offset(200f, 0f), 0.2);
            _surface.BeginTouch(3, Offset(200f, 0f), 0.3);
            _recorder.Clear();

            FeedPair(Offset(-100f, 0f), Offset(200f, 0f), 0.3, 3);

            Assert.That(_recorder.Pinches, Is.Empty);

            FeedPair(Offset(-100f, 0f), Offset(300f, 0f), 0.4, 3);

            Assert.That(_recorder.Pinches, Has.Count.EqualTo(1));
            Assert.That(_recorder.Pinches[0].ScaleDelta, Is.EqualTo(400f / 300f).Within(0.0001f));
            Assert.That(_recorder.Pinches[0].TotalScale, Is.EqualTo(400f / 300f).Within(0.0001f));
        }

        [Test]
        public void Pinch_WhenThePrimaryLifts_TheRemainingTwoFingersFormANewPair()
        {
            BeginPair();
            _surface.BeginTouch(3, Offset(300f, 0f), 0.0);
            _surface.EndTouch(1, Offset(-100f, 0f), 0.1);
            _recorder.Clear();

            _surface.UpdateTouch(2, Offset(100f, 0f), 0.1);
            _surface.UpdateTouch(3, Offset(300f, 0f), 0.1);
            _surface.CompleteFrame(0.1);

            Assert.That(_recorder.Pinches, Is.Empty);

            _surface.UpdateTouch(2, Offset(100f, 0f), 0.2);
            _surface.UpdateTouch(3, Offset(500f, 0f), 0.2);
            _surface.CompleteFrame(0.2);

            Assert.That(_recorder.Pinches, Has.Count.EqualTo(1));
            Assert.That(_recorder.Pinches[0].TotalScale, Is.EqualTo(2f).Within(0.0001f));
            Assert.That(_recorder.Pinches[0].Center, Is.EqualTo(Offset(300f, 0f)));
        }

        private void BeginPair()
        {
            _surface.BeginTouch(1, Offset(-100f, 0f), 0.0);
            _surface.BeginTouch(2, Offset(100f, 0f), 0.0);
        }

        private void FeedPair(Vector2 first, Vector2 second, double time, int secondFingerId = 2)
        {
            _surface.UpdateTouch(1, first, time);
            _surface.UpdateTouch(secondFingerId, second, time);
            _surface.CompleteFrame(time);
        }
    }
}
