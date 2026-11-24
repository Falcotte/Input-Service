using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace AngryKoala.Input.Tests
{
    public sealed class InputServiceGestureSettingsTests : InputServiceAreaTestBase
    {
        private readonly List<GestureSettings> _settings = new();

        private const float _shortHoldDuration = 0.1f;
        private const float _longHoldDuration = 1.5f;
        private const float _holdSeconds = 0.9f;

        [UnityTearDown]
        public IEnumerator DestroySettings()
        {
            foreach (var settings in _settings)
            {
                if (settings != null)
                {
                    Object.Destroy(settings);
                }
            }

            _settings.Clear();

            yield return null;
        }

        [UnityTest]
        public IEnumerator Hold_ServiceSettingsWithALongerHoldDuration_RaiseNoHoldWithinIt()
        {
            var recorder = Record(ScreenSurface);
            GestureSettingsTestUtility.AssignToService(Service, CreateLongHoldSettings());
            yield return null;

            yield return Touch.Hold(ScreenCenter, _holdSeconds);

            Assert.That(recorder.Holds.Count, Is.EqualTo(0));
            Assert.That(recorder.Taps.Count, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator Hold_ServiceWithoutSettings_RaisesTheHoldOfTheDefaults()
        {
            var recorder = Record(ScreenSurface);

            yield return Touch.Hold(ScreenCenter, _holdSeconds);

            Assert.That(recorder.Holds.Count, Is.EqualTo(1));
            Assert.That(recorder.Taps.Count, Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator Hold_AreaWithItsOwnSettings_OverridesTheDefaults()
        {
            var area = CreateLeftHalfArea();
            GestureSettingsTestUtility.AssignToArea(area, CreateLongHoldSettings());
            var recorder = Record(Service.GetArea(area));
            yield return null;

            yield return Touch.Hold(PointInLeftHalf, _holdSeconds);

            Assert.That(recorder.Holds.Count, Is.EqualTo(0));
            Assert.That(recorder.Taps.Count, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator Hold_AreaWithItsOwnSettings_OverridesTheServiceSettings()
        {
            var area = CreateLeftHalfArea();
            GestureSettingsTestUtility.AssignToService(Service, CreateLongHoldSettings());
            GestureSettingsTestUtility.AssignToArea(area, CreateShortHoldSettings());
            var recorder = Record(Service.GetArea(area));
            yield return null;

            yield return Touch.Hold(PointInLeftHalf, 0.3f);

            Assert.That(recorder.Holds.Count, Is.EqualTo(1));
            Assert.That(recorder.Taps.Count, Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator Hold_AreaWithoutSettings_UsesTheServiceSettings()
        {
            var area = CreateLeftHalfArea();
            GestureSettingsTestUtility.AssignToService(Service, CreateLongHoldSettings());
            var recorder = Record(Service.GetArea(area));
            yield return null;

            yield return Touch.Hold(PointInLeftHalf, _holdSeconds);

            Assert.That(recorder.Holds.Count, Is.EqualTo(0));
            Assert.That(recorder.Taps.Count, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator Hold_OutsideAnAreaWithItsOwnSettings_UsesTheServiceSettings()
        {
            var area = CreateLeftHalfArea();
            GestureSettingsTestUtility.AssignToArea(area, CreateLongHoldSettings());
            Service.GetArea(area);
            var recorder = Record(ScreenSurface);
            yield return null;

            yield return Touch.Hold(PointInRightHalf, _holdSeconds);

            Assert.That(recorder.Holds.Count, Is.EqualTo(1));
            Assert.That(recorder.Taps.Count, Is.EqualTo(0));
        }

        private GestureSettings CreateShortHoldSettings()
        {
            var settings = GestureSettingsTestUtility.CreateSettings(holdDuration: _shortHoldDuration);
            _settings.Add(settings);

            return settings;
        }

        private GestureSettings CreateLongHoldSettings()
        {
            var settings = GestureSettingsTestUtility.CreateSettings(tapMaximumDuration: _longHoldDuration,
                holdDuration: _longHoldDuration);
            _settings.Add(settings);

            return settings;
        }
    }
}
