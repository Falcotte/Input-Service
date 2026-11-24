using NUnit.Framework;
using static AngryKoala.Input.Tests.InputSurfaceTestUtility;

namespace AngryKoala.Input.Tests
{
    public sealed class InputSurfaceTapAndHoldTests
    {
        private InputSurface _surface;
        private InputSurfaceRecorder _recorder;

        [SetUp]
        public void SetUp()
        {
            Recreate(CreateRules());
        }

        [Test]
        public void Tap_LiftedWithinTheTapTime_IsRaised()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.EndTouch(1, Start, 0.2);

            Assert.That(_recorder.Taps, Has.Count.EqualTo(1));
            Assert.That(_recorder.Taps[0].FingerId, Is.EqualTo(1));
            Assert.That(_recorder.Taps[0].Position, Is.EqualTo(Start));
        }

        [Test]
        public void Tap_IsRaisedOnTheLift_NotBefore()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.UpdateTouch(1, Start, 0.1);

            Assert.That(_recorder.Taps, Is.Empty);
        }

        [Test]
        public void Tap_LastGestureIsTap()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.EndTouch(1, Start, 0.1);

            Assert.That(_surface.LastGesture, Is.EqualTo("Tap"));
        }

        [Test]
        public void Tap_LiftedAfterTheTapTime_IsNotRaised()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.UpdateTouch(1, Start, 0.2);
            _surface.EndTouch(1, Start, 0.4);

            Assert.That(_recorder.Taps, Is.Empty);
        }

        [Test]
        public void Tap_FingerMovedWithinTheTapDistance_IsRaised()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.UpdateTouch(1, Offset(20f, 0f), 0.1);
            _surface.EndTouch(1, Offset(20f, 0f), 0.2);

            Assert.That(_recorder.Taps, Has.Count.EqualTo(1));
        }

        [Test]
        public void Tap_FingerStrayedBeyondTheTapDistance_IsNotRaised()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.UpdateTouch(1, Offset(40f, 0f), 0.1);
            _surface.EndTouch(1, Offset(40f, 0f), 0.2);

            Assert.That(_recorder.Taps, Is.Empty);
        }

        [Test]
        public void Tap_FingerStrayedAndCameBack_IsNotRaised()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.UpdateTouch(1, Offset(40f, 0f), 0.1);
            _surface.UpdateTouch(1, Start, 0.15);
            _surface.EndTouch(1, Start, 0.2);

            Assert.That(_recorder.Taps, Is.Empty);
        }

        [Test]
        public void Tap_SecondFingerWasDownDuringTheTouch_IsNotRaised()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.BeginTouch(2, Offset(300f, 0f), 0.05);
            _surface.EndTouch(2, Offset(300f, 0f), 0.1);
            _surface.EndTouch(1, Start, 0.2);

            Assert.That(_recorder.Taps, Is.Empty);
        }

        [Test]
        public void Tap_SecondFingerWasDownBeforeTheFirstWentDownAndStaysDown_IsNotRaisedForEither()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.BeginTouch(2, Offset(300f, 0f), 0.0);
            _surface.EndTouch(2, Offset(300f, 0f), 0.1);
            _surface.EndTouch(1, Start, 0.15);

            Assert.That(_recorder.Taps, Is.Empty);
        }

        [Test]
        public void Tap_Canceled_IsNotRaised()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.CancelTouch(1, 0.1);

            Assert.That(_recorder.Taps, Is.Empty);
        }

        [Test]
        public void Tap_CanceledWithCancelAllTouches_IsNotRaised()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.CancelAllTouches(0.1);

            Assert.That(_recorder.Taps, Is.Empty);
        }

        [Test]
        public void Tap_ForANonPrimaryFinger_IsNotRaised()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.BeginTouch(2, Offset(300f, 0f), 0.05);
            _surface.EndTouch(2, Offset(300f, 0f), 0.1);

            Assert.That(_recorder.Taps, Is.Empty);
        }

        [Test]
        public void Tap_ForAPrimaryByHandover_IsNotRaised()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.BeginTouch(2, Offset(300f, 0f), 0.05);
            _surface.EndTouch(1, Start, 0.1);
            _surface.EndTouch(2, Offset(300f, 0f), 0.15);

            Assert.That(_recorder.Taps, Is.Empty);
        }

        [Test]
        public void Tap_AfterAHold_IsNotRaised()
        {
            Recreate(CreateRules(tapMaximumDuration: 1f));

            _surface.BeginTouch(1, Start, 0.0);
            _surface.UpdateTouch(1, Start, 0.6);
            _surface.EndTouch(1, Start, 0.7);

            Assert.That(_recorder.Holds, Has.Count.EqualTo(1));
            Assert.That(_recorder.Taps, Is.Empty);
        }

        [Test]
        public void Tap_ASecondTouchAfterTheFirstEnded_IsRaisedAgain()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.EndTouch(1, Start, 0.1);
            _surface.BeginTouch(2, Start, 1.0);
            _surface.EndTouch(2, Start, 1.1);

            Assert.That(_recorder.Taps, Has.Count.EqualTo(2));
        }

        [Test]
        public void Hold_ReachingTheHoldDuration_IsRaisedInUpdateTouch()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.UpdateTouch(1, Start, 0.4);

            Assert.That(_recorder.Holds, Is.Empty);

            _surface.UpdateTouch(1, Start, 0.55);

            Assert.That(_recorder.Holds, Has.Count.EqualTo(1));
            Assert.That(_recorder.Holds[0].FingerId, Is.EqualTo(1));
            Assert.That(_recorder.Holds[0].Duration, Is.GreaterThanOrEqualTo(0.5f));
        }

        [Test]
        public void Hold_IsRaisedOnlyOnce()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.UpdateTouch(1, Start, 0.55);
            _surface.UpdateTouch(1, Start, 0.7);
            _surface.UpdateTouch(1, Start, 0.9);

            Assert.That(_recorder.Holds, Has.Count.EqualTo(1));
        }

        [Test]
        public void Hold_LastGestureIsHold()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.UpdateTouch(1, Start, 0.55);

            Assert.That(_surface.LastGesture, Is.EqualTo("Hold"));
        }

        [Test]
        public void Hold_FingerMovedWithinTheTapDistance_IsRaised()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.UpdateTouch(1, Offset(20f, 0f), 0.2);
            _surface.UpdateTouch(1, Offset(20f, 0f), 0.55);

            Assert.That(_recorder.Holds, Has.Count.EqualTo(1));
        }

        [Test]
        public void Hold_FingerStrayedBeyondTheTapDistance_IsNotRaised()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.UpdateTouch(1, Offset(40f, 0f), 0.1);
            _surface.UpdateTouch(1, Start, 0.2);
            _surface.UpdateTouch(1, Start, 0.6);

            Assert.That(_recorder.Holds, Is.Empty);
        }

        [Test]
        public void Hold_SecondFingerWasDownDuringTheTouch_IsNotRaised()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.BeginTouch(2, Offset(300f, 0f), 0.1);
            _surface.EndTouch(2, Offset(300f, 0f), 0.2);
            _surface.UpdateTouch(1, Start, 0.6);

            Assert.That(_recorder.Holds, Is.Empty);
        }

        [Test]
        public void Hold_ForANonPrimaryFinger_IsNotRaised()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.BeginTouch(2, Offset(300f, 0f), 0.0);
            _surface.UpdateTouch(1, Start, 0.6);
            _surface.UpdateTouch(2, Offset(300f, 0f), 0.6);

            Assert.That(_recorder.Holds, Is.Empty);
        }

        [Test]
        public void Hold_ForAPrimaryByHandover_IsNotRaised()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.BeginTouch(2, Offset(300f, 0f), 0.0);
            _surface.EndTouch(1, Start, 0.1);
            _surface.UpdateTouch(2, Offset(300f, 0f), 0.7);

            Assert.That(_recorder.Holds, Is.Empty);
        }

        [Test]
        public void Hold_Canceled_IsNotRaised()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.UpdateTouch(1, Start, 0.3);
            _surface.CancelTouch(1, 0.4);
            _surface.UpdateTouch(1, Start, 0.6);

            Assert.That(_recorder.Holds, Is.Empty);
        }

        [Test]
        public void Hold_LiftedBeforeTheHoldDuration_IsNotRaised()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.UpdateTouch(1, Start, 0.3);
            _surface.EndTouch(1, Start, 0.4);

            Assert.That(_recorder.Holds, Is.Empty);
        }

        [Test]
        public void Tap_ADragBeganInsideTheTapDistance_IsNotRaised()
        {
            Recreate(CreateRules(tapMaximumDistance: 0.25f, dragStartDistance: 0.1f));

            _surface.BeginTouch(1, Start, 0.0);
            _surface.UpdateTouch(1, Offset(15f, 0f), 0.05);
            _surface.EndTouch(1, Offset(15f, 0f), 0.1);

            Assert.That(_recorder.DragsBegan, Has.Count.EqualTo(1));
            Assert.That(_recorder.DragsEnded, Has.Count.EqualTo(1));
            Assert.That(_recorder.Taps, Is.Empty);
        }

        [Test]
        public void Tap_TravelOfExactlyTheDragStartDistance_IsNotRaised()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.UpdateTouch(1, Offset(25f, 0f), 0.05);
            _surface.EndTouch(1, Offset(25f, 0f), 0.1);

            Assert.That(_recorder.DragsBegan, Has.Count.EqualTo(1));
            Assert.That(_recorder.Taps, Is.Empty);
        }

        [Test]
        public void Hold_ADragBeganInsideTheTapDistance_IsNotRaised()
        {
            Recreate(CreateRules(tapMaximumDistance: 0.25f, dragStartDistance: 0.1f));

            _surface.BeginTouch(1, Start, 0.0);
            _surface.UpdateTouch(1, Offset(15f, 0f), 0.05);
            _surface.UpdateTouch(1, Offset(15f, 0f), 0.6);

            Assert.That(_recorder.DragsBegan, Has.Count.EqualTo(1));
            Assert.That(_recorder.Holds, Is.Empty);
        }

        [Test]
        public void Tap_ASecondTouchWithTheSameFingerId_IsRaisedAgain()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.EndTouch(1, Start, 0.1);
            _surface.BeginTouch(1, Offset(200f, 0f), 0.12);
            _surface.EndTouch(1, Offset(200f, 0f), 0.2);

            Assert.That(_recorder.Taps, Has.Count.EqualTo(2));
            Assert.That(_recorder.Taps[1].Position, Is.EqualTo(Offset(200f, 0f)));
            Assert.That(_recorder.Taps[1].StartPosition, Is.EqualTo(Offset(200f, 0f)));
            Assert.That(_recorder.Taps[1].Duration, Is.EqualTo(0.08f).Within(0.001f));
            Assert.That(_surface.TouchCount, Is.EqualTo(0));
        }

        [Test]
        public void Tap_AfterATouchWithTheSameFingerIdWasHeld_ForgetsTheHold()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.UpdateTouch(1, Start, 0.6);
            _surface.EndTouch(1, Start, 0.7);
            _surface.BeginTouch(1, Start, 0.72);
            _surface.EndTouch(1, Start, 0.8);

            Assert.That(_recorder.Holds, Has.Count.EqualTo(1));
            Assert.That(_recorder.Taps, Has.Count.EqualTo(1));
            Assert.That(_recorder.TouchesBegan, Has.Count.EqualTo(2));
            Assert.That(_recorder.TouchesEnded, Has.Count.EqualTo(2));
        }

        [Test]
        public void Tap_AfterATouchWithTheSameFingerIdWasDragged_ForgetsTheDrag()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.UpdateTouch(1, Offset(100f, 0f), 0.1);
            _surface.EndTouch(1, Offset(100f, 0f), 0.2);
            _surface.BeginTouch(1, Start, 0.22);
            _surface.EndTouch(1, Start, 0.3);

            Assert.That(_recorder.DragsBegan, Has.Count.EqualTo(1));
            Assert.That(_recorder.DragsEnded, Has.Count.EqualTo(1));
            Assert.That(_recorder.Taps, Has.Count.EqualTo(1));
        }

        [Test]
        public void Hold_ASecondTouchWithTheSameFingerId_IsRaisedAgain()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.UpdateTouch(1, Start, 0.6);
            _surface.EndTouch(1, Start, 0.7);
            _surface.BeginTouch(1, Start, 0.72);
            _surface.UpdateTouch(1, Start, 1.3);

            Assert.That(_recorder.Holds, Has.Count.EqualTo(2));
        }

        private void Recreate(GestureRules rules)
        {
            _surface = CreateSurface(rules);
            _recorder = new InputSurfaceRecorder(_surface);
        }
    }
}
