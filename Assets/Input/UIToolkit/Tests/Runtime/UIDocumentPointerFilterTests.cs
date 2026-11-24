using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace AngryKoala.Input.Tests
{
    public sealed class UIDocumentPointerFilterTests : UIToolkitInputTestBase
    {
        private Button _button;
        private int _clickCount;

        private const float _panelScale = 2f;
        private const float _panelDistanceBesideButton = 40f;

        #region TakesPointer

        [UnityTest]
        public IEnumerator TakesPointer_DocumentWithoutPanel_ReturnsFalse()
        {
            var gameObject = CreateObject("Document Without Panel");
            gameObject.AddComponent<UIDocument>();
            var filter = gameObject.AddComponent<UIDocumentPointerFilter>();
            yield return null;

            Assert.That(filter.TakesPointer(ScreenPoint(0.5f, 0.5f)), Is.False);
        }

        [UnityTest]
        public IEnumerator TakesPointer_OverTheButton_ReturnsTrue()
        {
            yield return CreateSceneWithService();
            var filter = AddFilter();

            Assert.That(filter.TakesPointer(ScreenPositionOf(_button)), Is.True);
        }

        [UnityTest]
        public IEnumerator TakesPointer_BesideTheButton_ReturnsFalse()
        {
            yield return CreateSceneWithService();
            var filter = AddFilter();

            Assert.That(filter.TakesPointer(GetPositionBesideButton()), Is.False);
        }

        [UnityTest]
        public IEnumerator TakesPointer_FullScreenElementWithDefaultPicking_ReturnsTrueBesideTheButton()
        {
            yield return CreateSceneWithService();
            var filter = AddFilter();

            AddFullScreenElement();
            yield return WaitFrames(2);

            Assert.That(filter.TakesPointer(GetPositionBesideButton()), Is.True);
        }

        [UnityTest]
        public IEnumerator TakesPointer_FullScreenElementIgnoringPicking_ReturnsFalseBesideTheButton()
        {
            yield return CreateSceneWithService();
            var filter = AddFilter();

            var element = AddFullScreenElement();
            yield return WaitFrames(2);

            element.pickingMode = PickingMode.Ignore;

            Assert.That(filter.TakesPointer(GetPositionBesideButton()), Is.False);
            Assert.That(filter.TakesPointer(ScreenPositionOf(_button)), Is.True);
        }

        [UnityTest]
        public IEnumerator TakesPointer_ScaledPanelOverTheButton_ReturnsTrue()
        {
            yield return CreateSceneWithService(_panelScale);
            var filter = AddFilter();

            Assert.That(PanelScale, Is.EqualTo(_panelScale).Within(0.01f));
            Assert.That(filter.TakesPointer(ScreenPositionOf(_button)), Is.True);
        }

        [UnityTest]
        public IEnumerator TakesPointer_ScaledPanelBesideTheButton_ReturnsFalse()
        {
            yield return CreateSceneWithService(_panelScale);
            var filter = AddFilter();

            Assert.That(filter.TakesPointer(GetPositionBesideButton()), Is.False);
        }

        #endregion

        #region Taps through the service

        [UnityTest]
        public IEnumerator Tap_OnTheButton_ClicksItAndTheScreenSurfaceGetsNoTouch()
        {
            yield return CreateSceneWithService();
            AddFilter();
            var screenCounter = Count(Service.Screen);

            yield return Touch.Tap(ScreenPositionOf(_button));
            yield return WaitFrames(2);

            Assert.That(_clickCount, Is.EqualTo(1));
            Assert.That(screenCounter.TouchBeganCount, Is.EqualTo(0));
            Assert.That(screenCounter.TouchEndedCount, Is.EqualTo(0));
            Assert.That(screenCounter.TapCount, Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator Tap_BesideTheButton_ReachesTheScreenSurfaceAndDoesNotClick()
        {
            yield return CreateSceneWithService();
            AddFilter();
            var screenCounter = Count(Service.Screen);

            yield return Touch.Tap(GetPositionBesideButton());
            yield return WaitFrames(2);

            Assert.That(_clickCount, Is.EqualTo(0));
            Assert.That(screenCounter.TouchBeganCount, Is.EqualTo(1));
            Assert.That(screenCounter.TapCount, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator Tap_BesideTheButtonWithAFullScreenElement_IsTakenByTheFilter()
        {
            yield return CreateSceneWithService();
            AddFilter();
            var screenCounter = Count(Service.Screen);

            AddFullScreenElement();
            yield return WaitFrames(2);

            yield return Touch.Tap(GetPositionBesideButton());
            yield return WaitFrames(2);

            Assert.That(screenCounter.TouchBeganCount, Is.EqualTo(0));
            Assert.That(screenCounter.TapCount, Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator Tap_BesideTheButtonAfterTheFullScreenElementIgnoresPicking_ReachesTheScreenSurface()
        {
            yield return CreateSceneWithService();
            AddFilter();
            var screenCounter = Count(Service.Screen);

            var element = AddFullScreenElement();
            yield return WaitFrames(2);

            element.pickingMode = PickingMode.Ignore;

            yield return Touch.Tap(GetPositionBesideButton());
            yield return WaitFrames(2);

            Assert.That(screenCounter.TouchBeganCount, Is.EqualTo(1));
            Assert.That(screenCounter.TapCount, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator Tap_OnTheButtonWithTheFilterDisabled_ClicksItAndReachesTheScreenSurface()
        {
            yield return CreateSceneWithService();
            var filter = AddFilter();
            filter.enabled = false;
            var screenCounter = Count(Service.Screen);

            yield return Touch.Tap(ScreenPositionOf(_button));
            yield return WaitFrames(2);

            Assert.That(_clickCount, Is.EqualTo(1));
            Assert.That(screenCounter.TouchBeganCount, Is.EqualTo(1));
            Assert.That(screenCounter.TapCount, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator Tap_OnTheButtonOfAScaledPanel_IsTakenByTheFilter()
        {
            yield return CreateSceneWithService(_panelScale);
            AddFilter();
            var screenCounter = Count(Service.Screen);

            yield return Touch.Tap(ScreenPositionOf(_button));
            yield return WaitFrames(2);

            Assert.That(_clickCount, Is.EqualTo(1));
            Assert.That(screenCounter.TouchBeganCount, Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator Tap_BesideTheButtonOfAScaledPanel_ReachesTheScreenSurface()
        {
            yield return CreateSceneWithService(_panelScale);
            AddFilter();
            var screenCounter = Count(Service.Screen);

            yield return Touch.Tap(GetPositionBesideButton());
            yield return WaitFrames(2);

            Assert.That(_clickCount, Is.EqualTo(0));
            Assert.That(screenCounter.TouchBeganCount, Is.EqualTo(1));
            Assert.That(screenCounter.TapCount, Is.EqualTo(1));
        }

        #endregion

        #region Registration

        [UnityTest]
        public IEnumerator Registration_ServiceAppearsAfterTheComponent_TakesThePointer()
        {
            CreateDocument();
            yield return null;

            AddButton();
            AddFilter();
            yield return WaitFrames(2);

            CreateService();
            yield return null;

            var screenCounter = Count(Service.Screen);

            yield return Touch.Tap(ScreenPositionOf(_button));
            yield return WaitFrames(2);

            Assert.That(_clickCount, Is.EqualTo(1));
            Assert.That(screenCounter.TouchBeganCount, Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator Registration_ComponentDisabled_LeavesThePointerToTheScreenSurface()
        {
            yield return CreateSceneWithService();
            var filter = AddFilter();
            var screenCounter = Count(Service.Screen);

            filter.enabled = false;
            yield return null;

            yield return Touch.Tap(ScreenPositionOf(_button));
            yield return WaitFrames(2);

            Assert.That(screenCounter.TouchBeganCount, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator Registration_ComponentEnabledAgain_TakesThePointerAgain()
        {
            yield return CreateSceneWithService();
            var filter = AddFilter();
            var screenCounter = Count(Service.Screen);

            filter.enabled = false;
            yield return null;

            filter.enabled = true;
            yield return null;

            yield return Touch.Tap(ScreenPositionOf(_button));
            yield return WaitFrames(2);

            Assert.That(screenCounter.TouchBeganCount, Is.EqualTo(0));
            Assert.That(_clickCount, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator Registration_ComponentDestroyed_LeavesThePointerToTheScreenSurface()
        {
            yield return CreateSceneWithService();
            var filter = AddFilter();
            var screenCounter = Count(Service.Screen);

            Object.Destroy(filter);
            yield return null;

            yield return Touch.Tap(ScreenPositionOf(_button));
            yield return WaitFrames(2);

            Assert.That(screenCounter.TouchBeganCount, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator Registration_ServiceAppearsWhileTheComponentIsDisabled_DoesNotRegister()
        {
            CreateDocument();
            yield return null;

            AddButton();
            var filter = AddFilter();
            filter.enabled = false;
            yield return WaitFrames(2);

            CreateService();
            yield return null;

            var screenCounter = Count(Service.Screen);

            yield return Touch.Tap(ScreenPositionOf(_button));
            yield return WaitFrames(2);

            Assert.That(screenCounter.TouchBeganCount, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator Registration_ServiceAppearedWhileDisabledThenEnabled_TakesThePointer()
        {
            CreateDocument();
            yield return null;

            AddButton();
            var filter = AddFilter();
            filter.enabled = false;
            yield return WaitFrames(2);

            CreateService();
            yield return null;

            var screenCounter = Count(Service.Screen);

            filter.enabled = true;
            yield return null;

            yield return Touch.Tap(ScreenPositionOf(_button));
            yield return WaitFrames(2);

            Assert.That(screenCounter.TouchBeganCount, Is.EqualTo(0));
            Assert.That(_clickCount, Is.EqualTo(1));
        }

        #endregion

        #region Utility

        private IEnumerator CreateSceneWithService(float panelScale = 1f)
        {
            CreateService();
            CreateDocument(panelScale);
            yield return null;

            AddButton();
            yield return WaitFrames(2);
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

        private UIDocumentPointerFilter AddFilter()
        {
            return Document.gameObject.AddComponent<UIDocumentPointerFilter>();
        }

        private VisualElement AddFullScreenElement()
        {
            var element = new VisualElement();
            element.name = "Full Screen Element";
            element.style.position = Position.Absolute;
            element.style.left = 0f;
            element.style.top = 0f;
            element.style.right = 0f;
            element.style.bottom = 0f;

            Root.Insert(0, element);
            Root.style.flexGrow = 1f;

            return element;
        }

        private Vector2 GetPositionBesideButton()
        {
            var bound = _button.worldBound;

            return PanelToScreen(new Vector2(bound.xMax + _panelDistanceBesideButton,
                bound.yMax + _panelDistanceBesideButton));
        }

        #endregion
    }
}
