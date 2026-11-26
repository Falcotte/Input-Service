using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace AngryKoala.Input.Tests
{
    public sealed class InputServiceAreaTests : InputServiceAreaTestBase
    {
        [UnityTest]
        public IEnumerator GetArea_ReturnsASurface()
        {
            var area = CreateLeftHalfArea();

            var surface = Service.GetArea(area);

            Assert.That(surface, Is.Not.Null);
            Assert.That(surface.TouchCount, Is.EqualTo(0));
            Assert.That(surface.IsTouching, Is.False);

            yield return null;
        }

        [UnityTest]
        public IEnumerator GetArea_ReturnsASurfaceThatIsNotTheScreenSurface()
        {
            var area = CreateLeftHalfArea();

            var surface = Service.GetArea(area);

            Assert.That(surface, Is.Not.SameAs(ScreenSurface));

            yield return null;
        }

        [UnityTest]
        public IEnumerator GetArea_SameAssetTwice_ReturnsTheSameSurface()
        {
            var area = CreateLeftHalfArea();

            var firstSurface = Service.GetArea(area);
            var secondSurface = Service.GetArea(area);

            Assert.That(secondSurface, Is.SameAs(firstSurface));

            yield return null;
        }

        [UnityTest]
        public IEnumerator GetArea_SameAssetForTwoConsumers_ReturnsTheSameSurface()
        {
            var area = CreateLeftHalfArea();
            var firstConsumerSurface = Service.GetArea(area);

            yield return WaitFrames(3);

            var secondConsumerSurface = Service.GetArea(area);

            Assert.That(secondConsumerSurface, Is.SameAs(firstConsumerSurface));
        }

        [UnityTest]
        public IEnumerator GetArea_TwoDifferentAssets_ReturnTwoDifferentSurfaces()
        {
            var leftArea = CreateLeftHalfArea();
            var rightArea = CreateRightHalfArea();

            var leftSurface = Service.GetArea(leftArea);
            var rightSurface = Service.GetArea(rightArea);

            Assert.That(rightSurface, Is.Not.Null);
            Assert.That(rightSurface, Is.Not.SameAs(leftSurface));

            yield return null;
        }

        [UnityTest]
        public IEnumerator GetArea_TwoAssetsWithTheSameRectangle_ReturnTwoDifferentSurfaces()
        {
            var firstArea = CreateLeftHalfArea("First");
            var secondArea = CreateLeftHalfArea("Second");

            var firstSurface = Service.GetArea(firstArea);
            var secondSurface = Service.GetArea(secondArea);

            Assert.That(secondSurface, Is.Not.SameAs(firstSurface));

            yield return null;
        }

        [UnityTest]
        public IEnumerator GetArea_Null_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Service.GetArea(null));

            yield return null;
        }

        [UnityTest]
        public IEnumerator GetArea_AreaThatIsNotInTheList_IsAddedWithoutAnyLog()
        {
            var area = CreateLeftHalfArea();

            Service.GetArea(area);
            yield return WaitFrames(3);

            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator GetArea_AfterTheFirstUse_StillReturnsTheSameSurfaceAfterManyFrames()
        {
            var area = CreateLeftHalfArea();
            var surface = Service.GetArea(area);

            yield return Touch.Tap(PointInLeftHalf);
            yield return WaitFrames(5);

            Assert.That(Service.GetArea(area), Is.SameAs(surface));
        }

        [UnityTest]
        public IEnumerator Screen_WithAnAreaInUse_IsStillTheSameSurface()
        {
            var screenSurface = Service.Screen;

            Service.GetArea(CreateLeftHalfArea());
            yield return null;

            Assert.That(Service.Screen, Is.SameAs(screenSurface));
        }
    }
}
