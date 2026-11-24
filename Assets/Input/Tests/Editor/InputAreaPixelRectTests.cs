using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools.Utils;
using Object = UnityEngine.Object;

namespace AngryKoala.Input.Tests
{
    public sealed class InputAreaPixelRectTests
    {
        private static readonly Rect _screenRect = new(0f, 0f, 1000f, 2000f);
        private static readonly Rect _safeArea = new(50f, 100f, 800f, 1600f);

        private static readonly RectEqualityComparer _rectComparer = new(0.001f);

        private InputArea _area;

        [TearDown]
        public void TearDown()
        {
            if (_area != null)
            {
                Object.DestroyImmediate(_area);
            }
        }

        [Test]
        public void GetPixelRect_FullAreaInSafeAreaSpace_IsTheSafeArea()
        {
            _area = CreateArea(InputAreaSpace.SafeArea, Vector2.zero, Vector2.one);

            Assert.That(_area.GetPixelRect(_screenRect, _safeArea), Is.EqualTo(_safeArea).Using(_rectComparer));
        }

        [Test]
        public void GetPixelRect_FullAreaInScreenSpace_IsTheScreenRect()
        {
            _area = CreateArea(InputAreaSpace.Screen, Vector2.zero, Vector2.one);

            Assert.That(_area.GetPixelRect(_screenRect, _safeArea), Is.EqualTo(_screenRect).Using(_rectComparer));
        }

        [Test]
        public void GetPixelRect_InSafeAreaSpace_TakesFractionsOfTheSafeArea()
        {
            _area = CreateArea(InputAreaSpace.SafeArea, new Vector2(0.25f, 0.5f), new Vector2(0.75f, 1f));

            var expected = new Rect(250f, 900f, 400f, 800f);

            Assert.That(_area.GetPixelRect(_screenRect, _safeArea), Is.EqualTo(expected).Using(_rectComparer));
        }

        [Test]
        public void GetPixelRect_InScreenSpace_TakesFractionsOfTheScreenRect()
        {
            _area = CreateArea(InputAreaSpace.Screen, new Vector2(0.25f, 0.5f), new Vector2(0.75f, 1f));

            var expected = new Rect(250f, 1000f, 500f, 1000f);

            Assert.That(_area.GetPixelRect(_screenRect, _safeArea), Is.EqualTo(expected).Using(_rectComparer));
        }

        [Test]
        public void GetPixelRect_ScreenRectNotAtTheOrigin_OffsetsTheResult()
        {
            _area = CreateArea(InputAreaSpace.Screen, new Vector2(0.5f, 0.5f), Vector2.one);

            var offsetScreenRect = new Rect(20f, 40f, 400f, 600f);
            var expected = new Rect(220f, 340f, 200f, 300f);

            Assert.That(_area.GetPixelRect(offsetScreenRect, _safeArea), Is.EqualTo(expected).Using(_rectComparer));
        }

        [Test]
        public void GetPixelRect_FractionsOutsideZeroToOne_AreClamped()
        {
            _area = CreateArea(InputAreaSpace.SafeArea, new Vector2(-0.5f, -1f), new Vector2(2f, 3f));

            Assert.That(_area.GetPixelRect(_screenRect, _safeArea), Is.EqualTo(_safeArea).Using(_rectComparer));
        }

        [Test]
        public void GetPixelRect_OnlyTheMaximumOutsideTheRange_IsClampedAlone()
        {
            _area = CreateArea(InputAreaSpace.Screen, new Vector2(0.5f, 0.5f), new Vector2(1.5f, 1.5f));

            var expected = new Rect(500f, 1000f, 500f, 1000f);

            Assert.That(_area.GetPixelRect(_screenRect, _safeArea), Is.EqualTo(expected).Using(_rectComparer));
        }

        [Test]
        public void GetPixelRect_MaximumBelowTheMinimumOnX_HasZeroWidthAndKeepsTheHeight()
        {
            _area = CreateArea(InputAreaSpace.Screen, new Vector2(0.8f, 0.25f), new Vector2(0.3f, 0.75f));

            var pixelRect = _area.GetPixelRect(_screenRect, _safeArea);

            Assert.That(pixelRect.width, Is.EqualTo(0f));
            Assert.That(pixelRect.x, Is.EqualTo(800f).Within(0.001f));
            Assert.That(pixelRect.y, Is.EqualTo(500f).Within(0.001f));
            Assert.That(pixelRect.height, Is.EqualTo(1000f).Within(0.001f));
        }

        [Test]
        public void GetPixelRect_MaximumBelowTheMinimumOnY_HasZeroHeightAndKeepsTheWidth()
        {
            _area = CreateArea(InputAreaSpace.Screen, new Vector2(0.25f, 0.9f), new Vector2(0.75f, 0.1f));

            var pixelRect = _area.GetPixelRect(_screenRect, _safeArea);

            Assert.That(pixelRect.height, Is.EqualTo(0f));
            Assert.That(pixelRect.y, Is.EqualTo(1800f).Within(0.001f));
            Assert.That(pixelRect.x, Is.EqualTo(250f).Within(0.001f));
            Assert.That(pixelRect.width, Is.EqualTo(500f).Within(0.001f));
        }

        [Test]
        public void GetPixelRect_MaximumEqualToTheMinimum_HasNoSize()
        {
            _area = CreateArea(InputAreaSpace.Screen, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));

            var pixelRect = _area.GetPixelRect(_screenRect, _safeArea);

            Assert.That(pixelRect.width, Is.EqualTo(0f));
            Assert.That(pixelRect.height, Is.EqualTo(0f));
        }

        [Test]
        public void GetPixelRect_SafeAreaEqualToTheScreen_GivesTheSameResultInBothSpaces()
        {
            _area = CreateArea(InputAreaSpace.SafeArea, new Vector2(0.1f, 0.2f), new Vector2(0.6f, 0.9f));
            var safeAreaResult = _area.GetPixelRect(_screenRect, _screenRect);
            Object.DestroyImmediate(_area);

            _area = CreateArea(InputAreaSpace.Screen, new Vector2(0.1f, 0.2f), new Vector2(0.6f, 0.9f));
            var screenResult = _area.GetPixelRect(_screenRect, _screenRect);

            Assert.That(safeAreaResult, Is.EqualTo(screenResult).Using(_rectComparer));
            Assert.That(screenResult.width, Is.EqualTo(500f).Within(0.001f));
        }

        [Test]
        public void GetPixelRect_SwappedMinimumAndMaximum_IsNotNegativeAndInsideTheScreen()
        {
            _area = CreateArea(InputAreaSpace.Screen, new Vector2(0.9f, 0.8f), new Vector2(0.2f, 0.1f));

            AssertNotNegativeAndInside(_area.GetPixelRect(_screenRect, _safeArea), _screenRect);
        }

        [Test]
        public void GetPixelRect_FractionsFarOutsideTheRange_IsNotNegativeAndInsideTheScreen()
        {
            _area = CreateArea(InputAreaSpace.Screen, new Vector2(-4f, 3f), new Vector2(7f, -2f));

            AssertNotNegativeAndInside(_area.GetPixelRect(_screenRect, _safeArea), _screenRect);
        }

        [Test]
        public void GetPixelRect_MinimumBeyondTheTopRight_IsNotNegativeAndInsideTheSafeArea()
        {
            _area = CreateArea(InputAreaSpace.SafeArea, new Vector2(1.5f, 2f), new Vector2(3f, 4f));

            AssertNotNegativeAndInside(_area.GetPixelRect(_screenRect, _safeArea), _safeArea);
        }

        [Test]
        public void GetPixelRect_SwappedInSafeAreaSpace_IsNotNegativeAndInsideTheSafeArea()
        {
            _area = CreateArea(InputAreaSpace.SafeArea, new Vector2(0.75f, 1f), new Vector2(0.25f, 0.5f));

            AssertNotNegativeAndInside(_area.GetPixelRect(_screenRect, _safeArea), _safeArea);
        }

        private static void AssertNotNegativeAndInside(Rect pixelRect, Rect container)
        {
            Assert.That(pixelRect.width, Is.GreaterThanOrEqualTo(0f));
            Assert.That(pixelRect.height, Is.GreaterThanOrEqualTo(0f));
            Assert.That(pixelRect.xMin, Is.GreaterThanOrEqualTo(container.xMin - 0.001f));
            Assert.That(pixelRect.yMin, Is.GreaterThanOrEqualTo(container.yMin - 0.001f));
            Assert.That(pixelRect.xMax, Is.LessThanOrEqualTo(container.xMax + 0.001f));
            Assert.That(pixelRect.yMax, Is.LessThanOrEqualTo(container.yMax + 0.001f));
        }

        private static InputArea CreateArea(InputAreaSpace space, Vector2 minimum, Vector2 maximum)
        {
            return InputAreaTestUtility.CreateArea("Test Area", space, minimum, maximum);
        }
    }
}
