#if UNITY_EDITOR
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace AngryKoala.Input.Tests
{
    public sealed class ScriptedTouchFocusTests : UIToolkitInputTestBase
    {
        private Button _button;
        private int _clickCount;

        private const string _gameViewTypeName = "GameView";

        [UnityTest]
        public IEnumerator Create_WhileAnotherEditorWindowHasTheFocus_FocusesTheGameView()
        {
            Touch.Dispose();
            FocusAnotherEditorWindow();
            yield return null;

            IgnoreWhenTheGameViewStillHasTheFocus();

            using var scriptedTouch = ScriptedTouch.Create();
            yield return null;

            Assert.That(UnityEditor.EditorWindow.focusedWindow, Is.Not.Null);
            Assert.That(UnityEditor.EditorWindow.focusedWindow.GetType().Name, Is.EqualTo(_gameViewTypeName));
        }

        [UnityTest]
        public IEnumerator Tap_OnTheButtonAfterAnotherEditorWindowHadTheFocus_ClicksItOnce()
        {
            CreateService();
            CreateDocument();
            yield return null;

            AddButton();
            yield return WaitFrames(2);

            Touch.Dispose();
            FocusAnotherEditorWindow();
            yield return null;

            IgnoreWhenTheGameViewStillHasTheFocus();

            if (Application.isFocused)
            {
                Assert.Ignore("The application still counts as focused while the Scene view has the focus, so UI Toolkit takes the click anyway.");
            }

            using var scriptedTouch = ScriptedTouch.Create();
            yield return null;

            yield return scriptedTouch.Tap(ScreenPositionOf(_button));
            yield return WaitFrames(2);

            Assert.That(_clickCount, Is.EqualTo(1));
        }

        private static void FocusAnotherEditorWindow()
        {
            if (!UnityEditor.EditorWindow.HasOpenInstances<UnityEditor.SceneView>())
            {
                Assert.Ignore("No Scene view is open to take the focus from the Game view.");
            }

            UnityEditor.EditorWindow.FocusWindowIfItsOpen<UnityEditor.SceneView>();
        }

        private static void IgnoreWhenTheGameViewStillHasTheFocus()
        {
            var focusedWindow = UnityEditor.EditorWindow.focusedWindow;

            if (focusedWindow == null || focusedWindow.GetType().Name == _gameViewTypeName)
            {
                Assert.Ignore("The Scene view did not take the focus, so the Game view cannot be shown to get it back.");
            }
        }

        private void AddButton()
        {
            _clickCount = 0;

            _button = new Button(HandleButtonClicked);
            _button.name = "Test Button";
            _button.style.position = Position.Absolute;
            _button.style.left = 20f;
            _button.style.top = 20f;
            _button.style.width = 120f;
            _button.style.height = 80f;

            Root.Add(_button);
        }

        private void HandleButtonClicked()
        {
            _clickCount++;
        }
    }
}
#endif
