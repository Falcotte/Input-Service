using NUnit.Framework;
using UnityEngine.InputSystem;

namespace AngryKoala.Input.Tests
{
    public sealed class ScriptedTouchTests
    {
        private ScriptedTouch _first;
        private ScriptedTouch _second;
        private InputSettings.BackgroundBehavior _originalBackgroundBehavior;
#if UNITY_EDITOR
        private InputSettings.EditorInputBehaviorInPlayMode _originalEditorBehavior;
#endif

        private const InputSettings.BackgroundBehavior _scriptedBackgroundBehavior =
            InputSettings.BackgroundBehavior.IgnoreFocus;

#if UNITY_EDITOR
        private const InputSettings.EditorInputBehaviorInPlayMode _scriptedEditorBehavior =
            InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
#endif

        [SetUp]
        public void SetUp()
        {
            _originalBackgroundBehavior = InputSystem.settings.backgroundBehavior;
#if UNITY_EDITOR
            _originalEditorBehavior = InputSystem.settings.editorInputBehaviorInPlayMode;
#endif

            Assume.That(_originalBackgroundBehavior, Is.Not.EqualTo(_scriptedBackgroundBehavior));
#if UNITY_EDITOR
            Assume.That(_originalEditorBehavior, Is.Not.EqualTo(_scriptedEditorBehavior));
#endif

            _first = null;
            _second = null;
        }

        [TearDown]
        public void TearDown()
        {
            _first?.Dispose();
            _second?.Dispose();

            InputSystem.settings.backgroundBehavior = _originalBackgroundBehavior;
#if UNITY_EDITOR
            InputSystem.settings.editorInputBehaviorInPlayMode = _originalEditorBehavior;
#endif
        }

        [Test]
        public void Create_ChangesTheSettings()
        {
            _first = ScriptedTouch.Create();

            AssertScripted();
        }

        [Test]
        public void Dispose_TwoInstancesInTheOrderOfCreation_KeepsTheScriptedSettingsUntilTheLastIsDisposed()
        {
            _first = ScriptedTouch.Create();
            _second = ScriptedTouch.Create();

            _first.Dispose();

            AssertScripted();

            _second.Dispose();

            AssertOriginal();
        }

        [Test]
        public void Dispose_TwoInstancesInReverseOrder_KeepsTheScriptedSettingsUntilTheLastIsDisposed()
        {
            _first = ScriptedTouch.Create();
            _second = ScriptedTouch.Create();

            _second.Dispose();

            AssertScripted();

            _first.Dispose();

            AssertOriginal();
        }

        [Test]
        public void Dispose_CalledTwiceOnOneOfTwoInstances_DoesNotRestoreEarly()
        {
            _first = ScriptedTouch.Create();
            _second = ScriptedTouch.Create();

            _first.Dispose();
            _first.Dispose();

            AssertScripted();

            _second.Dispose();

            AssertOriginal();
        }

        private void AssertScripted()
        {
            Assert.That(InputSystem.settings.backgroundBehavior, Is.EqualTo(_scriptedBackgroundBehavior));
#if UNITY_EDITOR
            Assert.That(InputSystem.settings.editorInputBehaviorInPlayMode, Is.EqualTo(_scriptedEditorBehavior));
#endif
        }

        private void AssertOriginal()
        {
            Assert.That(InputSystem.settings.backgroundBehavior, Is.EqualTo(_originalBackgroundBehavior));
#if UNITY_EDITOR
            Assert.That(InputSystem.settings.editorInputBehaviorInPlayMode, Is.EqualTo(_originalEditorBehavior));
#endif
        }
    }
}
