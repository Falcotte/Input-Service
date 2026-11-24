using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;

namespace AngryKoala.Input.Tests
{
    public sealed class InputServiceAreaDataTests : InputServiceAreaTestBase
    {
        private static readonly RectEqualityComparer _rectComparer = new(0.5f);
        private static readonly Vector2EqualityComparer _positionComparer = new(1f);

        [UnityTest]
        public IEnumerator GetData_WithAreasInUse_HasTheScreenRowFirstThenOneRowPerAreaInOrder()
        {
            var leftArea = CreateLeftHalfArea();
            var rightArea = CreateRightHalfArea();
            var results = new List<InputSurfaceData>();

            Service.GetArea(leftArea);
            Service.GetArea(rightArea);
            yield return null;

            Service.GetData(results);

            Assert.That(results.Count, Is.EqualTo(3));
            Assert.That(results[0].Area, Is.Null);
            Assert.That(results[0].Name, Is.EqualTo("Screen"));
            Assert.That(results[1].Area, Is.SameAs(leftArea));
            Assert.That(results[2].Area, Is.SameAs(rightArea));
        }

        [UnityTest]
        public IEnumerator GetData_AreasUsedInTheOtherOrder_FollowTheOrderOfFirstUse()
        {
            var leftArea = CreateLeftHalfArea();
            var rightArea = CreateRightHalfArea();
            var results = new List<InputSurfaceData>();

            Service.GetArea(rightArea);
            Service.GetArea(leftArea);
            yield return null;

            Service.GetData(results);

            Assert.That(results.Count, Is.EqualTo(3));
            Assert.That(results[1].Area, Is.SameAs(rightArea));
            Assert.That(results[2].Area, Is.SameAs(leftArea));
        }

        [UnityTest]
        public IEnumerator GetData_AreaAskedForTwice_HasOneRowForIt()
        {
            var area = CreateLeftHalfArea();
            var results = new List<InputSurfaceData>();

            Service.GetArea(area);
            Service.GetArea(area);
            yield return null;

            Service.GetData(results);

            Assert.That(results.Count, Is.EqualTo(2));
        }

        [UnityTest]
        public IEnumerator GetData_AreaRow_HasTheNameOfTheAssetAndItsRectangle()
        {
            var area = CreateArea("Middle Strip", new Vector2(0.25f, 0.5f), new Vector2(0.75f, 1f));
            var results = new List<InputSurfaceData>();

            Service.GetArea(area);
            yield return null;

            Service.GetData(results);

            var expectedRect = new Rect(Screen.width * 0.25f, Screen.height * 0.5f, Screen.width * 0.5f,
                Screen.height * 0.5f);

            Assert.That(results.Count, Is.EqualTo(2));
            Assert.That(results[1].Name, Is.EqualTo("Middle Strip"));
            Assert.That(results[1].PixelRect, Is.EqualTo(expectedRect).Using(_rectComparer));
        }

        [UnityTest]
        public IEnumerator GetData_ScreenRow_StillCoversTheWholeScreen()
        {
            var results = new List<InputSurfaceData>();

            Service.GetArea(CreateLeftHalfArea());
            yield return null;

            Service.GetData(results);

            Assert.That(results[0].PixelRect, Is.EqualTo(new Rect(0f, 0f, Screen.width, Screen.height)));
        }

        [UnityTest]
        public IEnumerator GetData_ClearsTheListBeforeFillingIt()
        {
            var results = new List<InputSurfaceData>();
            results.Add(default);
            results.Add(default);
            results.Add(default);
            results.Add(default);

            Service.GetArea(CreateLeftHalfArea());
            yield return null;

            Service.GetData(results);

            Assert.That(results.Count, Is.EqualTo(2));
        }

        [UnityTest]
        public IEnumerator GetData_CalledTwice_HasTheSameNumberOfRows()
        {
            var results = new List<InputSurfaceData>();

            Service.GetArea(CreateLeftHalfArea());
            Service.GetArea(CreateRightHalfArea());
            yield return null;

            Service.GetData(results);
            Service.GetData(results);

            Assert.That(results.Count, Is.EqualTo(3));
        }

        [UnityTest]
        public IEnumerator GetData_WhileAnAreaIsTouched_ReportsTheTouchOnThatRowOnly()
        {
            var results = new List<InputSurfaceData>();

            Service.GetArea(CreateLeftHalfArea());
            Service.GetArea(CreateRightHalfArea());
            yield return null;

            Touch.Begin(FirstFingerId, PointInRightHalf);
            yield return null;

            Service.GetData(results);

            Assert.That(results[0].TouchCount, Is.EqualTo(0));
            Assert.That(results[1].TouchCount, Is.EqualTo(0));
            Assert.That(results[2].TouchCount, Is.EqualTo(1));
            Assert.That(results[2].PrimaryTouch.FingerId, Is.EqualTo(FirstFingerId));
        }

        [UnityTest]
        public IEnumerator GetData_AfterATapOnAnArea_ReportsTheLastGestureOnThatRow()
        {
            var results = new List<InputSurfaceData>();

            Service.GetArea(CreateLeftHalfArea());
            yield return null;

            yield return Touch.Tap(PointInLeftHalf);

            Service.GetData(results);

            Assert.That(results[1].LastGesture, Is.EqualTo("Tap"));
            Assert.That(results[0].LastGesture, Is.Null.Or.Empty);
        }

        [UnityTest]
        public IEnumerator GetData_AreaWithAJoystick_FillsTheJoystickOfItsRow()
        {
            var area = CreateArea("Stick", Vector2.zero, Vector2.one, JoystickMode.Floating);
            var results = new List<InputSurfaceData>();
            var surface = Service.GetArea(area);
            yield return null;

            Touch.Begin(FirstFingerId, ScreenCenter);
            yield return null;

            var radius = surface.Joystick.Radius;

            Touch.Move(FirstFingerId, ScreenCenter + new Vector2(radius * 2f, 0f));
            yield return WaitFrames(2);

            Service.GetData(results);

            var joystick = results[1].Joystick;

            Assert.That(joystick.IsActive, Is.True);
            Assert.That(joystick.Radius, Is.EqualTo(radius).Within(0.01f));
            Assert.That(joystick.Center, Is.EqualTo(ScreenCenter).Using(_positionComparer));
            Assert.That(joystick.Direction.x, Is.EqualTo(1f).Within(0.01f));
            Assert.That(results[0].Joystick, Is.EqualTo(default(JoystickData)));
        }

        [UnityTest]
        public IEnumerator GetData_AreaWithoutAJoystick_HasADefaultJoystickInItsRow()
        {
            var results = new List<InputSurfaceData>();

            Service.GetArea(CreateLeftHalfArea());
            yield return null;

            Service.GetData(results);

            Assert.That(results[1].Joystick, Is.EqualTo(default(JoystickData)));
        }
    }
}
