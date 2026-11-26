using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AngryKoala.Input.Tests
{
    public sealed class GestureSettingsTests
    {
        private const float _minimumHoldDuration = 0.05f;
        private const float _minimumSwipeDistance = 0.01f;
        private const float _minimumFallbackDpi = 30f;
        private const float _defaultFallbackDpi = 160f;

        private GestureSettings _settings;

        [TearDown]
        public void TearDown()
        {
            if (_settings != null)
            {
                Object.DestroyImmediate(_settings);
            }
        }

        #region Mapping

        [Test]
        public void ToRules_ValidValues_AreMappedUnchanged()
        {
            _settings = GestureSettingsTestUtility.CreateSettings(0.4f, 0.35f, 0.6f, 0.2f, 1.5f, 2f,
                SwipeTiming.OnRelease);

            var rules = _settings.ToRules();

            Assert.That(rules.TapMaximumDuration, Is.EqualTo(0.4f));
            Assert.That(rules.TapMaximumDistance, Is.EqualTo(0.35f));
            Assert.That(rules.HoldDuration, Is.EqualTo(0.6f));
            Assert.That(rules.DragStartDistance, Is.EqualTo(0.2f));
            Assert.That(rules.SwipeDistance, Is.EqualTo(1.5f));
            Assert.That(rules.SwipeMaximumDuration, Is.EqualTo(2f));
            Assert.That(rules.SwipeTiming, Is.EqualTo(SwipeTiming.OnRelease));
        }

        [Test]
        public void ToRules_NewAsset_IsTheDefaultRules()
        {
            _settings = ScriptableObject.CreateInstance<GestureSettings>();

            var rules = _settings.ToRules();
            var defaultRules = GestureRules.Default;

            Assert.That(rules.TapMaximumDuration, Is.EqualTo(defaultRules.TapMaximumDuration));
            Assert.That(rules.TapMaximumDistance, Is.EqualTo(defaultRules.TapMaximumDistance));
            Assert.That(rules.HoldDuration, Is.EqualTo(defaultRules.HoldDuration));
            Assert.That(rules.DragStartDistance, Is.EqualTo(defaultRules.DragStartDistance));
            Assert.That(rules.SwipeDistance, Is.EqualTo(defaultRules.SwipeDistance));
            Assert.That(rules.SwipeMaximumDuration, Is.EqualTo(defaultRules.SwipeMaximumDuration));
            Assert.That(rules.SwipeTiming, Is.EqualTo(defaultRules.SwipeTiming));
        }

        [Test]
        public void ToRules_ValuesOnTheirLowestAllowedBound_AreKept()
        {
            _settings = GestureSettingsTestUtility.CreateSettings(0f, 0f, _minimumHoldDuration, 0f,
                _minimumSwipeDistance, 0f);

            var rules = _settings.ToRules();

            Assert.That(rules.TapMaximumDuration, Is.EqualTo(0f));
            Assert.That(rules.TapMaximumDistance, Is.EqualTo(0f));
            Assert.That(rules.HoldDuration, Is.EqualTo(_minimumHoldDuration));
            Assert.That(rules.DragStartDistance, Is.EqualTo(0f));
            Assert.That(rules.SwipeDistance, Is.EqualTo(_minimumSwipeDistance));
            Assert.That(rules.SwipeMaximumDuration, Is.EqualTo(0f));
        }

        #endregion

        #region ToRules clamps

        [TestCase(-1f)]
        [TestCase(-0.001f)]
        public void ToRules_NegativeTapMaximumDuration_IsZero(float value)
        {
            _settings = GestureSettingsTestUtility.CreateSettings(tapMaximumDuration: value);

            Assert.That(_settings.ToRules().TapMaximumDuration, Is.EqualTo(0f));
        }

        [TestCase(-1f)]
        [TestCase(-0.001f)]
        public void ToRules_NegativeTapMaximumDistance_IsZero(float value)
        {
            _settings = GestureSettingsTestUtility.CreateSettings(tapMaximumDistance: value);

            Assert.That(_settings.ToRules().TapMaximumDistance, Is.EqualTo(0f));
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        [TestCase(0.01f)]
        public void ToRules_HoldDurationBelowTheMinimum_IsTheMinimum(float value)
        {
            _settings = GestureSettingsTestUtility.CreateSettings(holdDuration: value);

            Assert.That(_settings.ToRules().HoldDuration, Is.EqualTo(_minimumHoldDuration));
        }

        [TestCase(-1f)]
        [TestCase(-0.001f)]
        public void ToRules_NegativeDragStartDistance_IsZero(float value)
        {
            _settings = GestureSettingsTestUtility.CreateSettings(dragStartDistance: value);

            Assert.That(_settings.ToRules().DragStartDistance, Is.EqualTo(0f));
        }

        [TestCase(0f)]
        [TestCase(-2f)]
        [TestCase(0.001f)]
        public void ToRules_SwipeDistanceBelowTheMinimum_IsTheMinimum(float value)
        {
            _settings = GestureSettingsTestUtility.CreateSettings(swipeDistance: value);

            Assert.That(_settings.ToRules().SwipeDistance, Is.EqualTo(_minimumSwipeDistance));
        }

        [TestCase(-1f)]
        [TestCase(-0.001f)]
        public void ToRules_NegativeSwipeMaximumDuration_IsZero(float value)
        {
            _settings = GestureSettingsTestUtility.CreateSettings(swipeMaximumDuration: value);

            Assert.That(_settings.ToRules().SwipeMaximumDuration, Is.EqualTo(0f));
        }

        #endregion

        #region ResolveDpi

        [Test]
        public void ResolveDpi_ScreenReportsADpi_IsTheScreenDpi()
        {
            _settings = GestureSettingsTestUtility.CreateSettings(fallbackDpi: 96f);

            Assert.That(GestureSettings.ResolveDpi(326f, _settings), Is.EqualTo(326f));
        }

        [Test]
        public void ResolveDpi_ScreenReportsNoDpi_IsTheFallbackOfTheSettings()
        {
            _settings = GestureSettingsTestUtility.CreateSettings(fallbackDpi: 96f);

            Assert.That(GestureSettings.ResolveDpi(0f, _settings), Is.EqualTo(96f));
        }

        [Test]
        public void ResolveDpi_ScreenReportsNoDpiAndThereAreNoSettings_IsTheDefaultFallback()
        {
            Assert.That(GestureSettings.ResolveDpi(0f, null), Is.EqualTo(GestureSettings.DefaultFallbackDpi));
            Assert.That(GestureSettings.DefaultFallbackDpi, Is.EqualTo(160f));
        }

        [TestCase(0f)]
        [TestCase(-160f)]
        [TestCase(float.NaN)]
        public void ResolveDpi_ZeroNegativeOrNaNFallback_IsTheDefault(float value)
        {
            _settings = GestureSettingsTestUtility.CreateSettings(fallbackDpi: value);

            Assert.That(GestureSettings.ResolveDpi(0f, _settings), Is.EqualTo(_defaultFallbackDpi));
        }

        [TestCase(0.5f)]
        [TestCase(29f)]
        public void ResolveDpi_PositiveFallbackBelowTheMinimum_IsTheMinimum(float value)
        {
            _settings = GestureSettingsTestUtility.CreateSettings(fallbackDpi: value);

            Assert.That(GestureSettings.ResolveDpi(0f, _settings), Is.EqualTo(_minimumFallbackDpi));
        }

        [TestCase(-1f)]
        [TestCase(float.NaN)]
        public void ResolveDpi_NegativeOrNaNScreenDpi_IsTheFallbackOfTheSettings(float screenDpi)
        {
            _settings = GestureSettingsTestUtility.CreateSettings(fallbackDpi: 96f);

            Assert.That(GestureSettings.ResolveDpi(screenDpi, _settings), Is.EqualTo(96f));
        }

        #endregion

        #region OnValidate

        [Test]
        public void OnValidate_ValidValues_AreKept()
        {
            _settings = GestureSettingsTestUtility.CreateSettings(0.4f, 0.35f, 0.6f, 0.2f, 1.5f, 2f,
                SwipeTiming.OnRelease, 96f);

            ValidationTestUtility.Validate(_settings);

            Assert.That(_settings.TapMaximumDuration, Is.EqualTo(0.4f));
            Assert.That(_settings.TapMaximumDistance, Is.EqualTo(0.35f));
            Assert.That(_settings.HoldDuration, Is.EqualTo(0.6f));
            Assert.That(_settings.DragStartDistance, Is.EqualTo(0.2f));
            Assert.That(_settings.SwipeDistance, Is.EqualTo(1.5f));
            Assert.That(_settings.SwipeMaximumDuration, Is.EqualTo(2f));
            Assert.That(_settings.SwipeTiming, Is.EqualTo(SwipeTiming.OnRelease));
            Assert.That(_settings.FallbackDpi, Is.EqualTo(96f));
        }

        [Test]
        public void OnValidate_NegativeTapMaximumDuration_ClampsTheField()
        {
            _settings = GestureSettingsTestUtility.CreateSettings(tapMaximumDuration: -1f);

            ValidationTestUtility.Validate(_settings);

            Assert.That(_settings.TapMaximumDuration, Is.EqualTo(0f));
        }

        [Test]
        public void OnValidate_NegativeTapMaximumDistance_ClampsTheField()
        {
            _settings = GestureSettingsTestUtility.CreateSettings(tapMaximumDistance: -1f);

            ValidationTestUtility.Validate(_settings);

            Assert.That(_settings.TapMaximumDistance, Is.EqualTo(0f));
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        [TestCase(0.01f)]
        public void OnValidate_HoldDurationBelowTheMinimum_ClampsTheField(float value)
        {
            _settings = GestureSettingsTestUtility.CreateSettings(holdDuration: value);

            ValidationTestUtility.Validate(_settings);

            Assert.That(_settings.HoldDuration, Is.EqualTo(_minimumHoldDuration));
        }

        [Test]
        public void OnValidate_NegativeDragStartDistance_ClampsTheField()
        {
            _settings = GestureSettingsTestUtility.CreateSettings(dragStartDistance: -1f);

            ValidationTestUtility.Validate(_settings);

            Assert.That(_settings.DragStartDistance, Is.EqualTo(0f));
        }

        [TestCase(0f)]
        [TestCase(-2f)]
        [TestCase(0.001f)]
        public void OnValidate_SwipeDistanceBelowTheMinimum_ClampsTheField(float value)
        {
            _settings = GestureSettingsTestUtility.CreateSettings(swipeDistance: value);

            ValidationTestUtility.Validate(_settings);

            Assert.That(_settings.SwipeDistance, Is.EqualTo(_minimumSwipeDistance));
        }

        [Test]
        public void OnValidate_NegativeSwipeMaximumDuration_ClampsTheFieldToZero()
        {
            _settings = GestureSettingsTestUtility.CreateSettings(swipeMaximumDuration: -1f);

            ValidationTestUtility.Validate(_settings);

            Assert.That(_settings.SwipeMaximumDuration, Is.EqualTo(0f));
        }

        [TestCase(0f)]
        [TestCase(-160f)]
        [TestCase(float.NaN)]
        public void OnValidate_ZeroNegativeOrNaNFallbackDpi_IsTheDefault(float value)
        {
            _settings = GestureSettingsTestUtility.CreateSettings(fallbackDpi: value);

            ValidationTestUtility.Validate(_settings);

            Assert.That(_settings.FallbackDpi, Is.EqualTo(_defaultFallbackDpi));
        }

        [TestCase(0.5f)]
        [TestCase(29f)]
        public void OnValidate_PositiveFallbackDpiBelowTheMinimum_IsTheMinimum(float value)
        {
            _settings = GestureSettingsTestUtility.CreateSettings(fallbackDpi: value);

            ValidationTestUtility.Validate(_settings);

            Assert.That(_settings.FallbackDpi, Is.EqualTo(_minimumFallbackDpi));
        }

        #endregion
    }
}
