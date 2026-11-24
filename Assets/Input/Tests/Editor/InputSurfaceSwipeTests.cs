using NUnit.Framework;
using UnityEngine;
using static AngryKoala.Input.Tests.InputSurfaceTestUtility;

namespace AngryKoala.Input.Tests
{
    public sealed class InputSurfaceSwipeTests
    {
        private InputSurface _surface;
        private InputSurfaceRecorder _recorder;

        [SetUp]
        public void SetUp()
        {
            Recreate(CreateRules());
        }

        [Test]
        public void Swipe_WhenCrossed_IsRaisedInTheUpdateThatReachesTheDistance()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.UpdateTouch(1, Offset(60f, 0f), 0.1);

            Assert.That(_recorder.Swipes, Is.Empty);

            _surface.UpdateTouch(1, Offset(110f, 0f), 0.2);

            Assert.That(_recorder.Swipes, Has.Count.EqualTo(1));
            Assert.That(_recorder.Swipes[0].Touch.FingerId, Is.EqualTo(1));
            Assert.That(_recorder.Swipes[0].Vector, Is.EqualTo(new Vector2(110f, 0f)));
        }

        [Test]
        public void Swipe_WhenCrossed_BelowTheDistance_IsNotRaised()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.UpdateTouch(1, Offset(90f, 0f), 0.1);
            _surface.EndTouch(1, Offset(90f, 0f), 0.2);

            Assert.That(_recorder.Swipes, Is.Empty);
        }

        [Test]
        public void Swipe_WhenCrossed_IsRaisedAtMostOncePerTouch()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.UpdateTouch(1, Offset(110f, 0f), 0.1);
            _surface.UpdateTouch(1, Offset(220f, 0f), 0.2);
            _surface.UpdateTouch(1, Offset(330f, 0f), 0.3);
            _surface.EndTouch(1, Offset(330f, 0f), 0.4);

            Assert.That(_recorder.Swipes, Has.Count.EqualTo(1));
        }

        [Test]
        public void Swipe_WhenCrossed_IsRaisedBeforeTheFingerLifts()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.UpdateTouch(1, Offset(110f, 0f), 0.1);

            Assert.That(_recorder.Swipes, Has.Count.EqualTo(1));
            Assert.That(_recorder.TouchesEnded, Is.Empty);
        }

        [Test]
        public void Swipe_WhenCrossed_CrossingTooLate_IsNotRaised()
        {
            Recreate(CreateRules(swipeMaximumDuration: 0.5f));

            _surface.BeginTouch(1, Start, 0.0);
            _surface.UpdateTouch(1, Offset(50f, 0f), 0.3);
            _surface.UpdateTouch(1, Offset(150f, 0f), 0.8);
            _surface.UpdateTouch(1, Offset(250f, 0f), 0.9);
            _surface.EndTouch(1, Offset(250f, 0f), 1.0);

            Assert.That(_recorder.Swipes, Is.Empty);
        }

        [Test]
        public void Swipe_WhenCrossed_CrossingWithinTheMaximumDuration_IsRaised()
        {
            Recreate(CreateRules(swipeMaximumDuration: 0.5f));

            _surface.BeginTouch(1, Start, 0.0);
            _surface.UpdateTouch(1, Offset(150f, 0f), 0.4);

            Assert.That(_recorder.Swipes, Has.Count.EqualTo(1));
        }

        [Test]
        public void Swipe_WithMaximumDurationZero_HasNoTimeLimit()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.UpdateTouch(1, Offset(50f, 0f), 2.0);
            _surface.UpdateTouch(1, Offset(150f, 0f), 5.0);

            Assert.That(_recorder.Swipes, Has.Count.EqualTo(1));
        }

        [Test]
        public void Swipe_Canceled_IsNotRaised()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.UpdateTouch(1, Offset(50f, 0f), 0.1);
            _surface.CancelTouch(1, 0.2);

            Assert.That(_recorder.Swipes, Is.Empty);
        }

        [Test]
        public void Swipe_SecondFingerDownBeforeItIsRecognised_IsNotRaised()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.BeginTouch(2, Offset(300f, 300f), 0.05);
            _surface.UpdateTouch(1, Start, 0.1);
            _surface.UpdateTouch(2, Offset(300f, 300f), 0.1);

            _surface.UpdateTouch(1, Offset(110f, 0f), 0.2);
            _surface.UpdateTouch(2, Offset(300f, 300f), 0.2);

            Assert.That(_recorder.Swipes, Is.Empty);
        }

        [Test]
        public void Swipe_ForANonPrimaryFinger_IsNotRaised()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.BeginTouch(2, Offset(300f, 300f), 0.0);

            _surface.UpdateTouch(1, Start, 0.1);
            _surface.UpdateTouch(2, Offset(450f, 300f), 0.1);

            Assert.That(_recorder.Swipes, Is.Empty);
        }

        [Test]
        public void Swipe_ForAPrimaryByHandover_IsNotRaised()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.BeginTouch(2, Offset(300f, 300f), 0.0);
            _surface.EndTouch(1, Start, 0.1);

            _surface.UpdateTouch(2, Offset(450f, 300f), 0.2);

            Assert.That(_recorder.Swipes, Is.Empty);
        }

        [Test]
        public void Swipe_StillRaisesTheDragEvents()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.UpdateTouch(1, Offset(110f, 0f), 0.1);
            _surface.UpdateTouch(1, Offset(120f, 0f), 0.2);
            _surface.EndTouch(1, Offset(120f, 0f), 0.3);

            Assert.That(_recorder.Swipes, Has.Count.EqualTo(1));
            Assert.That(_recorder.DragsBegan, Has.Count.EqualTo(1));
            Assert.That(_recorder.Drags, Has.Count.EqualTo(2));
            Assert.That(_recorder.DragsEnded, Has.Count.EqualTo(1));
        }

        [Test]
        public void Swipe_Speed_IsCentimetersPerSecondOverTheWholeSwipe()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.UpdateTouch(1, Offset(110f, 0f), 0.2);

            Assert.That(_recorder.Swipes[0].Speed, Is.EqualTo(5.5f).Within(0.01f));
            Assert.That(_recorder.Swipes[0].Touch.Duration, Is.EqualTo(0.2f).Within(0.001f));
        }

        [TestCase(110f, 0f, SwipeDirection.Right, SwipeDirection8.Right)]
        [TestCase(110f, 20f, SwipeDirection.Right, SwipeDirection8.Right)]
        [TestCase(110f, 63f, SwipeDirection.Right, SwipeDirection8.UpRight)]
        [TestCase(110f, 110f, SwipeDirection.Up, SwipeDirection8.UpRight)]
        [TestCase(63f, 110f, SwipeDirection.Up, SwipeDirection8.UpRight)]
        [TestCase(40f, 110f, SwipeDirection.Up, SwipeDirection8.Up)]
        [TestCase(0f, 110f, SwipeDirection.Up, SwipeDirection8.Up)]
        [TestCase(-110f, 110f, SwipeDirection.Up, SwipeDirection8.UpLeft)]
        [TestCase(-110f, 63f, SwipeDirection.Left, SwipeDirection8.UpLeft)]
        [TestCase(-110f, 20f, SwipeDirection.Left, SwipeDirection8.Left)]
        [TestCase(-110f, 0f, SwipeDirection.Left, SwipeDirection8.Left)]
        [TestCase(-110f, -63f, SwipeDirection.Left, SwipeDirection8.DownLeft)]
        [TestCase(-110f, -110f, SwipeDirection.Down, SwipeDirection8.DownLeft)]
        [TestCase(-40f, -110f, SwipeDirection.Down, SwipeDirection8.Down)]
        [TestCase(0f, -110f, SwipeDirection.Down, SwipeDirection8.Down)]
        [TestCase(110f, -110f, SwipeDirection.Down, SwipeDirection8.DownRight)]
        [TestCase(110f, -63f, SwipeDirection.Right, SwipeDirection8.DownRight)]
        [TestCase(110f, -20f, SwipeDirection.Right, SwipeDirection8.Right)]
        public void Swipe_Vector_GivesTheDirectionAndTheNearestOfEight(float x, float y, SwipeDirection direction,
            SwipeDirection8 direction8)
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.UpdateTouch(1, Offset(x, y), 0.1);

            Assert.That(_recorder.Swipes, Has.Count.EqualTo(1));
            Assert.That(_recorder.Swipes[0].Vector, Is.EqualTo(new Vector2(x, y)));
            Assert.That(_recorder.Swipes[0].Direction, Is.EqualTo(direction));
            Assert.That(_recorder.Swipes[0].Direction8, Is.EqualTo(direction8));
        }

        [TestCase(110f, 0f, "Swipe Right")]
        [TestCase(-110f, 0f, "Swipe Left")]
        [TestCase(0f, 110f, "Swipe Up")]
        [TestCase(0f, -110f, "Swipe Down")]
        public void Swipe_LastGestureNamesTheFourWayDirection(float x, float y, string expected)
        {
            Recreate(CreateRules(swipeTiming: SwipeTiming.OnRelease));

            _surface.BeginTouch(1, Start, 0.0);
            _surface.UpdateTouch(1, Offset(x, y), 0.1);
            _surface.EndTouch(1, Offset(x, y), 0.2);

            Assert.That(_surface.LastGesture, Is.EqualTo(expected));
        }

        [Test]
        public void Swipe_OnRelease_NothingIsRaisedBeforeTheRelease()
        {
            Recreate(CreateRules(swipeTiming: SwipeTiming.OnRelease));

            _surface.BeginTouch(1, Start, 0.0);
            _surface.UpdateTouch(1, Offset(150f, 0f), 0.1);
            _surface.UpdateTouch(1, Offset(300f, 0f), 0.2);

            Assert.That(_recorder.Swipes, Is.Empty);
        }

        [Test]
        public void Swipe_OnRelease_IsRaisedInEndTouch_WithTheEndPosition()
        {
            Recreate(CreateRules(swipeTiming: SwipeTiming.OnRelease));

            _surface.BeginTouch(1, Start, 0.0);
            _surface.UpdateTouch(1, Offset(50f, 0f), 0.1);
            _surface.EndTouch(1, Offset(110f, 0f), 0.2);

            Assert.That(_recorder.Swipes, Has.Count.EqualTo(1));
            Assert.That(_recorder.Swipes[0].Vector, Is.EqualTo(new Vector2(110f, 0f)));
            Assert.That(_recorder.Swipes[0].Direction, Is.EqualTo(SwipeDirection.Right));
            Assert.That(_recorder.Swipes[0].Speed, Is.EqualTo(5.5f).Within(0.01f));
        }

        [Test]
        public void Swipe_OnRelease_EndedBelowTheDistance_IsNotRaised()
        {
            Recreate(CreateRules(swipeTiming: SwipeTiming.OnRelease));

            _surface.BeginTouch(1, Start, 0.0);
            _surface.UpdateTouch(1, Offset(90f, 0f), 0.1);
            _surface.EndTouch(1, Offset(90f, 0f), 0.2);

            Assert.That(_recorder.Swipes, Is.Empty);
        }

        [Test]
        public void Swipe_OnRelease_OutAndBackToTheStart_IsNotRaised()
        {
            Recreate(CreateRules(swipeTiming: SwipeTiming.OnRelease));

            _surface.BeginTouch(1, Start, 0.0);
            _surface.UpdateTouch(1, Offset(200f, 0f), 0.1);
            _surface.UpdateTouch(1, Start, 0.2);
            _surface.EndTouch(1, Start, 0.3);

            Assert.That(_recorder.Swipes, Is.Empty);
        }

        [Test]
        public void Swipe_OnRelease_IsRaisedAtMostOncePerTouch()
        {
            Recreate(CreateRules(swipeTiming: SwipeTiming.OnRelease));

            _surface.BeginTouch(1, Start, 0.0);
            _surface.UpdateTouch(1, Offset(150f, 0f), 0.1);
            _surface.EndTouch(1, Offset(150f, 0f), 0.2);

            Assert.That(_recorder.Swipes, Has.Count.EqualTo(1));
        }

        [Test]
        public void Swipe_OnRelease_ReleasedTooLate_IsNotRaised()
        {
            Recreate(CreateRules(swipeMaximumDuration: 0.5f, swipeTiming: SwipeTiming.OnRelease));

            _surface.BeginTouch(1, Start, 0.0);
            _surface.UpdateTouch(1, Offset(150f, 0f), 0.2);
            _surface.EndTouch(1, Offset(150f, 0f), 0.8);

            Assert.That(_recorder.Swipes, Is.Empty);
        }

        [Test]
        public void Swipe_OnRelease_ReleasedWithinTheMaximumDuration_IsRaised()
        {
            Recreate(CreateRules(swipeMaximumDuration: 0.5f, swipeTiming: SwipeTiming.OnRelease));

            _surface.BeginTouch(1, Start, 0.0);
            _surface.UpdateTouch(1, Offset(150f, 0f), 0.2);
            _surface.EndTouch(1, Offset(150f, 0f), 0.4);

            Assert.That(_recorder.Swipes, Has.Count.EqualTo(1));
        }

        [Test]
        public void Swipe_OnRelease_Canceled_IsNotRaised()
        {
            Recreate(CreateRules(swipeTiming: SwipeTiming.OnRelease));

            _surface.BeginTouch(1, Start, 0.0);
            _surface.UpdateTouch(1, Offset(150f, 0f), 0.1);
            _surface.CancelTouch(1, 0.2);

            Assert.That(_recorder.Swipes, Is.Empty);
        }

        [Test]
        public void Swipe_OnRelease_SecondFingerWasDown_IsNotRaised()
        {
            Recreate(CreateRules(swipeTiming: SwipeTiming.OnRelease));

            _surface.BeginTouch(1, Start, 0.0);
            _surface.BeginTouch(2, Offset(300f, 300f), 0.05);
            _surface.UpdateTouch(1, Offset(150f, 0f), 0.1);
            _surface.UpdateTouch(2, Offset(300f, 300f), 0.1);

            _surface.EndTouch(1, Offset(150f, 0f), 0.2);

            Assert.That(_recorder.Swipes, Is.Empty);
        }

        [Test]
        public void Swipe_OnRelease_RaisesTheSwipeBeforeTouchEnded()
        {
            Recreate(CreateRules(swipeTiming: SwipeTiming.OnRelease));

            _surface.BeginTouch(1, Start, 0.0);
            _surface.UpdateTouch(1, Offset(150f, 0f), 0.1);
            _recorder.Clear();

            _surface.EndTouch(1, Offset(150f, 0f), 0.2);

            Assert.That(_recorder.Events.IndexOf(InputSurfaceRecorder.Swipe),
                Is.LessThan(_recorder.Events.IndexOf(InputSurfaceRecorder.TouchEnded)));
            Assert.That(_recorder.Events[_recorder.Events.Count - 1], Is.EqualTo(InputSurfaceRecorder.TouchEnded));
        }

        private void Recreate(GestureRules rules)
        {
            _surface = CreateSurface(rules);
            _recorder = new InputSurfaceRecorder(_surface);
        }
    }
}
