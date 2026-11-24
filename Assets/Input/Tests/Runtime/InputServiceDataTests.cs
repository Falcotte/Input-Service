using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;

namespace AngryKoala.Input.Tests
{
    public sealed class InputServiceDataTests : InputServiceTestBase
    {
        private const string _screenSurfaceName = "Screen";

        private static readonly Vector2EqualityComparer _positionComparer = Vector2EqualityComparer.Instance;

        [UnityTest]
        public IEnumerator GetData_Null_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Service.GetData(null));

            yield return null;
        }

        [UnityTest]
        public IEnumerator GetData_ClearsTheListAndAddsOneRowForTheScreenSurface()
        {
            var results = new List<InputSurfaceData>();
            results.Add(default);
            results.Add(default);

            Service.GetData(results);

            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0].Name, Is.EqualTo(_screenSurfaceName));
            Assert.That(results[0].Area, Is.Null);
            Assert.That(results[0].PixelRect, Is.EqualTo(new Rect(0f, 0f, Screen.width, Screen.height)));
            Assert.That(results[0].TouchCount, Is.EqualTo(0));

            yield return null;
        }

        [UnityTest]
        public IEnumerator GetData_CalledTwice_StillHasOneRow()
        {
            var results = new List<InputSurfaceData>();

            Service.GetData(results);
            Service.GetData(results);

            Assert.That(results.Count, Is.EqualTo(1));

            yield return null;
        }

        [UnityTest]
        public IEnumerator GetData_WhileTouching_ReportsTheTouchCountAndPrimaryTouch()
        {
            var results = new List<InputSurfaceData>();

            Touch.Begin(FirstFingerId, ScreenCenter);
            yield return null;

            Service.GetData(results);

            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0].TouchCount, Is.EqualTo(1));
            Assert.That(results[0].PrimaryTouch.FingerId, Is.EqualTo(FirstFingerId));
            Assert.That(results[0].PrimaryTouch.Position, Is.EqualTo(ScreenCenter).Using(_positionComparer));
        }

        [UnityTest]
        public IEnumerator GetData_AfterATap_ReportsTheLastGesture()
        {
            var results = new List<InputSurfaceData>();

            yield return Touch.Tap(ScreenCenter);

            Service.GetData(results);

            Assert.That(results.Count, Is.EqualTo(1));
            Assert.That(results[0].LastGesture, Is.Not.Null.And.Not.Empty);
        }
    }
}
