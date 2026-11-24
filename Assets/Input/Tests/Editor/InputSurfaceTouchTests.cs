using NUnit.Framework;
using UnityEngine;
using static AngryKoala.Input.Tests.InputSurfaceTestUtility;

namespace AngryKoala.Input.Tests
{
    public sealed class InputSurfaceTouchTests
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
        public void New_HasNoTouchesAndNoLastGesture()
        {
            Assert.That(_surface.TouchCount, Is.Zero);
            Assert.That(_surface.IsTouching, Is.False);
            Assert.That(_surface.LastGesture, Is.Null);
        }

        [Test]
        public void PixelRect_ReturnsTheRectangleGivenToSetContext()
        {
            var pixelRect = new Rect(100f, 200f, 640f, 480f);

            _surface.SetContext(CreateRules(), PixelsPerCentimeter, pixelRect);

            Assert.That(_surface.PixelRect, Is.EqualTo(pixelRect));
        }

        [Test]
        public void BeginTouch_RaisesTouchBeganOnce_WithFingerAndPosition()
        {
            _surface.BeginTouch(3, Start, 1.0);

            Assert.That(_recorder.TouchesBegan, Has.Count.EqualTo(1));

            var touch = _recorder.TouchesBegan[0];

            Assert.That(touch.FingerId, Is.EqualTo(3));
            Assert.That(touch.Position, Is.EqualTo(Start));
            Assert.That(touch.StartPosition, Is.EqualTo(Start));
            Assert.That(touch.Delta, Is.EqualTo(Vector2.zero));
            Assert.That(touch.Duration, Is.Zero);
            Assert.That(touch.WasCanceled, Is.False);
        }

        [Test]
        public void BeginTouch_CountsTheFinger()
        {
            _surface.BeginTouch(1, Start, 0.0);

            Assert.That(_surface.TouchCount, Is.EqualTo(1));
            Assert.That(_surface.IsTouching, Is.True);
        }

        [Test]
        public void BeginTouch_CountsEveryFinger()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.BeginTouch(2, Offset(100f, 0f), 0.0);

            Assert.That(_surface.TouchCount, Is.EqualTo(2));
        }

        [Test]
        public void BeginTouch_ForFingerAlreadyDown_IsIgnored()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.BeginTouch(1, Offset(100f, 0f), 0.1);

            Assert.That(_surface.TouchCount, Is.EqualTo(1));
            Assert.That(_recorder.TouchesBegan, Has.Count.EqualTo(1));
            Assert.That(_surface.PrimaryTouch.StartPosition, Is.EqualTo(Start));
        }

        [Test]
        public void PrimaryTouch_WithNoFingerDown_IsDefault()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.EndTouch(1, Start, 0.1);

            Assert.That(_surface.PrimaryTouch.Position, Is.EqualTo(Vector2.zero));
            Assert.That(_surface.PrimaryTouch.Duration, Is.Zero);
            Assert.That(_surface.TouchCount, Is.Zero);
        }

        [Test]
        public void PrimaryTouch_IsTheFirstFingerDown()
        {
            _surface.BeginTouch(5, Start, 0.0);
            _surface.BeginTouch(2, Offset(100f, 0f), 0.1);

            Assert.That(_surface.PrimaryTouch.FingerId, Is.EqualTo(5));
            Assert.That(_surface.PrimaryTouch.Position, Is.EqualTo(Start));
        }

        [Test]
        public void PrimaryTouch_FollowsTheFingerAsItMoves()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.UpdateTouch(1, Offset(10f, 20f), 0.1);

            Assert.That(_surface.PrimaryTouch.Position, Is.EqualTo(Offset(10f, 20f)));
            Assert.That(_surface.PrimaryTouch.StartPosition, Is.EqualTo(Start));
        }

        [Test]
        public void NormalizedPosition_IsRelativeToTheRectangle_FromBottomLeft()
        {
            _surface.BeginTouch(1, new Vector2(250f, 500f), 0.0);

            Assert.That(_recorder.TouchesBegan[0].NormalizedPosition, Is.EqualTo(new Vector2(0.25f, 0.25f)));
        }

        [Test]
        public void NormalizedPosition_IsRelativeToTheRectanglePosition()
        {
            _surface.SetContext(CreateRules(), PixelsPerCentimeter, new Rect(100f, 200f, 1000f, 2000f));

            _surface.BeginTouch(1, new Vector2(600f, 1200f), 0.0);

            Assert.That(_recorder.TouchesBegan[0].NormalizedPosition, Is.EqualTo(new Vector2(0.5f, 0.5f)));
        }

        [Test]
        public void NormalizedPosition_IsClampedToZeroAndOne()
        {
            _surface.BeginTouch(1, new Vector2(-50f, 3000f), 0.0);

            Assert.That(_recorder.TouchesBegan[0].NormalizedPosition, Is.EqualTo(new Vector2(0f, 1f)));
        }

        [Test]
        public void Delta_IsTheMovementSinceThePreviousFeed()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.UpdateTouch(1, Offset(10f, 5f), 0.1);

            Assert.That(_surface.PrimaryTouch.Delta, Is.EqualTo(new Vector2(10f, 5f)));

            _surface.UpdateTouch(1, Offset(25f, 5f), 0.2);

            Assert.That(_surface.PrimaryTouch.Delta, Is.EqualTo(new Vector2(15f, 0f)));
        }

        [Test]
        public void Delta_IsZero_InAFrameWithoutMovement()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.UpdateTouch(1, Offset(10f, 0f), 0.1);
            _surface.UpdateTouch(1, Offset(10f, 0f), 0.2);

            Assert.That(_surface.PrimaryTouch.Delta, Is.EqualTo(Vector2.zero));
        }

        [Test]
        public void EndTouch_UsesThePreviousFeedForTheDelta()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.UpdateTouch(1, Offset(10f, 0f), 0.1);
            _surface.EndTouch(1, Offset(16f, 0f), 0.2);

            Assert.That(_recorder.TouchesEnded[0].Delta, Is.EqualTo(new Vector2(6f, 0f)));
        }

        [Test]
        public void TotalDelta_IsPositionMinusStartPosition()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.UpdateTouch(1, Offset(10f, 0f), 0.1);
            _surface.UpdateTouch(1, Offset(15f, -5f), 0.2);

            Assert.That(_surface.PrimaryTouch.TotalDelta, Is.EqualTo(new Vector2(15f, -5f)));
        }

        [Test]
        public void DeltaCentimeters_AreThePixelsDividedByPixelsPerCentimeter()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.UpdateTouch(1, Offset(150f, 0f), 0.1);
            _surface.UpdateTouch(1, Offset(250f, 0f), 0.2);

            Assert.That(_surface.PrimaryTouch.DeltaCentimeters, Is.EqualTo(new Vector2(1f, 0f)));
            Assert.That(_surface.PrimaryTouch.TotalDeltaCentimeters, Is.EqualTo(new Vector2(2.5f, 0f)));
        }

        [Test]
        public void Duration_IsTheTimeSinceTheFingerWentDown()
        {
            _surface.BeginTouch(1, Start, 10.0);
            _surface.UpdateTouch(1, Start, 10.25);

            Assert.That(_surface.PrimaryTouch.Duration, Is.EqualTo(0.25f).Within(0.001f));
        }

        [Test]
        public void UpdateTouch_ForAnUnknownFinger_RaisesNothing()
        {
            _surface.UpdateTouch(9, Offset(100f, 0f), 0.1);
            _surface.CompleteFrame(0.1);

            Assert.That(_recorder.Events, Is.Empty);
            Assert.That(_surface.TouchCount, Is.Zero);
        }

        [Test]
        public void EndTouch_ForAnUnknownFinger_RaisesNothing()
        {
            _surface.EndTouch(9, Start, 0.1);

            Assert.That(_recorder.Events, Is.Empty);
        }

        [Test]
        public void CancelTouch_ForAnUnknownFinger_RaisesNothing()
        {
            _surface.CancelTouch(9, 0.1);

            Assert.That(_recorder.Events, Is.Empty);
        }

        [Test]
        public void EndTouch_RaisesTouchEnded_AndReleasesTheFinger()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.EndTouch(1, Offset(10f, 0f), 0.1);

            Assert.That(_recorder.TouchesEnded, Has.Count.EqualTo(1));
            Assert.That(_recorder.TouchesEnded[0].FingerId, Is.EqualTo(1));
            Assert.That(_recorder.TouchesEnded[0].Position, Is.EqualTo(Offset(10f, 0f)));
            Assert.That(_recorder.TouchesEnded[0].WasCanceled, Is.False);
            Assert.That(_surface.TouchCount, Is.Zero);
            Assert.That(_surface.IsTouching, Is.False);
        }

        [Test]
        public void EndTouch_ForAFingerAlreadyLifted_RaisesNothingMore()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.EndTouch(1, Start, 0.1);
            _surface.EndTouch(1, Start, 0.2);

            Assert.That(_recorder.TouchesEnded, Has.Count.EqualTo(1));
        }

        [Test]
        public void CancelTouch_RaisesTouchEndedAsCanceled_AtTheLastKnownPosition()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.UpdateTouch(1, Offset(12f, 0f), 0.1);
            _surface.CancelTouch(1, 0.2);

            Assert.That(_recorder.TouchesEnded, Has.Count.EqualTo(1));
            Assert.That(_recorder.TouchesEnded[0].WasCanceled, Is.True);
            Assert.That(_recorder.TouchesEnded[0].Position, Is.EqualTo(Offset(12f, 0f)));
            Assert.That(_surface.TouchCount, Is.Zero);
        }

        [Test]
        public void CancelAllTouches_CancelsEveryFinger()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.BeginTouch(2, Offset(100f, 0f), 0.0);
            _surface.BeginTouch(3, Offset(200f, 0f), 0.0);

            _surface.CancelAllTouches(0.1);

            Assert.That(_recorder.TouchesEnded, Has.Count.EqualTo(3));
            Assert.That(_recorder.TouchesEnded, Has.All.Matches<TouchData>(touch => touch.WasCanceled));
            Assert.That(_surface.TouchCount, Is.Zero);
            Assert.That(_surface.IsTouching, Is.False);
        }

        [Test]
        public void CancelAllTouches_WithNoFingerDown_RaisesNothing()
        {
            _surface.CancelAllTouches(0.1);

            Assert.That(_recorder.Events, Is.Empty);
        }

        [Test]
        public void EachFinger_KeepsItsOwnDelta()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.BeginTouch(2, Offset(300f, 0f), 0.0);

            _surface.UpdateTouch(1, Offset(10f, 0f), 0.1);
            _surface.UpdateTouch(2, Offset(300f, 40f), 0.1);

            Assert.That(_surface.PrimaryTouch.Delta, Is.EqualTo(new Vector2(10f, 0f)));

            _surface.EndTouch(2, Offset(300f, 50f), 0.2);

            Assert.That(_recorder.TouchesEnded[0].Delta, Is.EqualTo(new Vector2(0f, 10f)));
        }

        [Test]
        public void PrimaryTouch_WhenThePrimaryLifts_PassesToTheEarliestRemainingFinger()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.BeginTouch(2, Offset(100f, 0f), 0.1);
            _surface.BeginTouch(3, Offset(200f, 0f), 0.2);

            _surface.EndTouch(1, Start, 0.3);

            Assert.That(_surface.PrimaryTouch.FingerId, Is.EqualTo(2));
            Assert.That(_surface.TouchCount, Is.EqualTo(2));
        }

        [Test]
        public void PrimaryTouch_WhenThePrimaryLifts_RestartsFromTheCurrentPosition()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.BeginTouch(2, Offset(100f, 0f), 0.0);
            _surface.UpdateTouch(1, Start, 0.1);
            _surface.UpdateTouch(2, Offset(150f, 0f), 0.1);

            _surface.EndTouch(1, Start, 0.2);

            var touch = _surface.PrimaryTouch;

            Assert.That(touch.FingerId, Is.EqualTo(2));
            Assert.That(touch.StartPosition, Is.EqualTo(Offset(150f, 0f)));
            Assert.That(touch.Position, Is.EqualTo(Offset(150f, 0f)));
            Assert.That(touch.Delta, Is.EqualTo(Vector2.zero));
            Assert.That(touch.TotalDelta, Is.EqualTo(Vector2.zero));
        }

        [Test]
        public void PrimaryTouch_WhenThePrimaryLifts_RestartsTheDuration()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.BeginTouch(2, Offset(100f, 0f), 0.0);
            _surface.EndTouch(1, Start, 0.2);

            _surface.UpdateTouch(2, Offset(100f, 0f), 0.3);

            Assert.That(_surface.PrimaryTouch.Duration, Is.EqualTo(0.1f).Within(0.001f));
        }

        [Test]
        public void PrimaryTouch_WhenThePrimaryIsCanceled_RestartsFromTheCurrentPosition()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.BeginTouch(2, Offset(100f, 0f), 0.0);
            _surface.UpdateTouch(1, Start, 0.1);
            _surface.UpdateTouch(2, Offset(150f, 0f), 0.1);

            _surface.CancelTouch(1, 0.2);

            var touch = _surface.PrimaryTouch;

            Assert.That(touch.FingerId, Is.EqualTo(2));
            Assert.That(touch.StartPosition, Is.EqualTo(Offset(150f, 0f)));
            Assert.That(touch.Delta, Is.EqualTo(Vector2.zero));
            Assert.That(touch.TotalDelta, Is.EqualTo(Vector2.zero));
        }

        [Test]
        public void PrimaryTouch_WhenANonPrimaryLifts_DoesNotChange()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.BeginTouch(2, Offset(100f, 0f), 0.0);
            _surface.UpdateTouch(1, Offset(20f, 0f), 0.1);

            _surface.EndTouch(2, Offset(100f, 0f), 0.2);

            Assert.That(_surface.PrimaryTouch.FingerId, Is.EqualTo(1));
            Assert.That(_surface.PrimaryTouch.StartPosition, Is.EqualTo(Start));
        }

        [Test]
        public void PrimaryTouch_WhenTheNextFingerGoesDownAfterAllLifted_IsTheNewFinger()
        {
            _surface.BeginTouch(1, Start, 0.0);
            _surface.EndTouch(1, Start, 0.1);
            _surface.BeginTouch(2, Offset(100f, 0f), 0.2);

            Assert.That(_surface.PrimaryTouch.FingerId, Is.EqualTo(2));
        }

        [Test]
        public void CompleteFrame_WithNoFingerDown_RaisesNothing()
        {
            _surface.CompleteFrame(0.1);

            Assert.That(_recorder.Events, Is.Empty);
        }
    }
}
