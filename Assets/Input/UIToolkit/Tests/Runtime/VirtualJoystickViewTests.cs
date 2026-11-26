using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.TestTools.Utils;
using UnityEngine.UIElements;

namespace AngryKoala.Input.Tests
{
    public sealed class VirtualJoystickViewTests : UIToolkitInputTestBase
    {
        [Serializable]
        private sealed class ViewFields
        {
            [SerializeField]
            private Color _backgroundColor = new(1f, 1f, 1f, 0.25f);

            [SerializeField]
            private Color _handleColor = new(1f, 1f, 1f, 0.6f);

            [SerializeField]
            private float _handleSize = 0.5f;

            [SerializeField]
            private bool _hideWhenIdle = true;

            [SerializeField]
            private float _fadeDuration = 0.15f;

            [SerializeField]
            private float _returnDuration = 0.1f;

            public ViewFields(float handleSize = 0.5f, bool hideWhenIdle = true, float returnDuration = 0.1f)
            {
                _handleSize = handleSize;
                _hideWhenIdle = hideWhenIdle;
                _returnDuration = returnDuration;
            }
        }

        private static readonly Vector2AbsoluteEqualityComparer _panelPositionComparer = new(1f);

        private const float _panelTolerance = 1f;
        private const float _opacityTolerance = 0.01f;
        private const float _fadeDuration = 0.15f;
        private const int _fingerId = 1;
        private const float _panelScale = 2f;
        private const float _longReturnDuration = 0.5f;

        #region Elements

        [UnityTest]
        public IEnumerator Enable_AddsTheBackgroundAndTheHandleToTheRoot()
        {
            yield return CreateSceneWithService();

            var view = AddView(CreateArea(JoystickMode.Floating));
            yield return WaitFrames(2);

            Assert.That(view.Background, Is.Not.Null);
            Assert.That(view.Handle, Is.Not.Null);
            Assert.That(Root.Contains(view.Background), Is.True);
            Assert.That(Root.Contains(view.Handle), Is.True);
        }

        [UnityTest]
        public IEnumerator Enable_BothElementsIgnorePicking()
        {
            yield return CreateSceneWithService();

            var view = AddView(CreateArea(JoystickMode.Floating));
            yield return WaitFrames(2);

            Assert.That(view.Background.pickingMode, Is.EqualTo(PickingMode.Ignore));
            Assert.That(view.Handle.pickingMode, Is.EqualTo(PickingMode.Ignore));
        }

        [UnityTest]
        public IEnumerator Enable_BothElementsAreAbsolutelyPositionedCircles()
        {
            yield return CreateSceneWithService();

            var view = AddView(CreateArea(JoystickMode.Floating));
            yield return WaitFrames(2);

            AssertIsCircle(view.Background);
            AssertIsCircle(view.Handle);
        }

        [UnityTest]
        public IEnumerator Enable_ElementsAreColouredFromTheFields()
        {
            yield return CreateSceneWithService();

            var view = AddView(CreateArea(JoystickMode.Floating));
            yield return WaitFrames(2);

            Assert.That(view.Background.style.backgroundColor.value, Is.EqualTo(view.BackgroundColor));
            Assert.That(view.Handle.style.backgroundColor.value, Is.EqualTo(view.HandleColor));
            Assert.That(view.BackgroundColor, Is.EqualTo(new Color(1f, 1f, 1f, 0.25f)));
            Assert.That(view.HandleColor, Is.EqualTo(new Color(1f, 1f, 1f, 0.6f)));
        }

        [UnityTest]
        public IEnumerator Enable_BothElementsHaveABorderOfTheOutlineWidthOnAllSides()
        {
            yield return CreateSceneWithService();

            var view = AddView(CreateArea(JoystickMode.Floating));
            yield return WaitFrames(3);

            Assert.That(view.OutlineWidth, Is.GreaterThan(0f));
            AssertBorderWidth(view.Background, view.OutlineWidth);
            AssertBorderWidth(view.Handle, view.OutlineWidth);
        }

        [UnityTest]
        public IEnumerator Enable_BothElementsHaveABorderOfTheOutlineColourOnAllSides()
        {
            yield return CreateSceneWithService();

            var view = AddView(CreateArea(JoystickMode.Floating));
            yield return WaitFrames(3);

            Assert.That(view.OutlineColor, Is.EqualTo(new Color(0f, 0f, 0f, 0.35f)));
            AssertBorderColor(view.Background, view.OutlineColor);
            AssertBorderColor(view.Handle, view.OutlineColor);
        }

        [UnityTest]
        public IEnumerator Outline_DoesNotChangeTheSizeOfTheBackground()
        {
            yield return CreateSceneWithService();

            var area = CreateArea(JoystickMode.Floating);
            var view = AddView(area);
            yield return WaitFrames(2);

            Touch.Begin(_fingerId, ScreenPoint(0.25f, 0.5f));
            yield return WaitFrames(4);

            var radiusInPixels = Service.GetArea(area).Joystick.Radius;

            Assert.That(view.OutlineWidth, Is.GreaterThan(0f));
            Assert.That(view.Background.worldBound.width,
                Is.EqualTo(2f * radiusInPixels / PanelScale).Within(_panelTolerance));
            Assert.That(view.Background.worldBound.height,
                Is.EqualTo(2f * radiusInPixels / PanelScale).Within(_panelTolerance));
        }

        [UnityTest]
        public IEnumerator Disable_RemovesBothElementsFromTheRoot()
        {
            yield return CreateSceneWithService();

            var view = AddView(CreateArea(JoystickMode.Floating));
            yield return WaitFrames(2);

            var background = view.Background;
            var handle = view.Handle;

            view.enabled = false;
            yield return WaitFrames(2);

            Assert.That(background, Is.Not.Null);
            Assert.That(handle, Is.Not.Null);
            Assert.That(Root.Contains(background), Is.False);
            Assert.That(Root.Contains(handle), Is.False);
        }

        [UnityTest]
        public IEnumerator EnableAgain_AddsTheElementsAgain()
        {
            yield return CreateSceneWithService();

            var view = AddView(CreateArea(JoystickMode.Floating));
            yield return WaitFrames(2);

            view.enabled = false;
            yield return WaitFrames(2);

            view.enabled = true;
            yield return WaitFrames(2);

            Assert.That(view.Background, Is.Not.Null);
            Assert.That(view.Handle, Is.Not.Null);
            Assert.That(Root.Contains(view.Background), Is.True);
            Assert.That(Root.Contains(view.Handle), Is.True);
        }

        #endregion

        #region Visibility

        [UnityTest]
        public IEnumerator NoArea_BothElementsAreHidden()
        {
            yield return CreateSceneWithService();

            var view = AddView(null);
            yield return WaitFrames(3);

            Assert.That(view.Background.style.display.value, Is.EqualTo(DisplayStyle.None));
            Assert.That(view.Handle.style.display.value, Is.EqualTo(DisplayStyle.None));
        }

        [UnityTest]
        public IEnumerator AreaWithoutJoystick_BothElementsAreHidden()
        {
            yield return CreateSceneWithService();

            var view = AddView(CreateArea(JoystickMode.None));
            yield return WaitFrames(3);

            Assert.That(view.Background.style.display.value, Is.EqualTo(DisplayStyle.None));
            Assert.That(view.Handle.style.display.value, Is.EqualTo(DisplayStyle.None));
        }

        [UnityTest]
        public IEnumerator AreaWithJoystick_BothElementsAreShown()
        {
            yield return CreateSceneWithService();

            var view = AddView(CreateArea(JoystickMode.Floating));
            yield return WaitFrames(3);

            Assert.That(view.Background.style.display.value, Is.Not.EqualTo(DisplayStyle.None));
            Assert.That(view.Handle.style.display.value, Is.Not.EqualTo(DisplayStyle.None));
        }

        [UnityTest]
        public IEnumerator SetArea_AfterEnable_AppliesFromThenOn()
        {
            yield return CreateSceneWithService();

            var view = AddView(null);
            yield return WaitFrames(3);

            Assert.That(view.Background.style.display.value, Is.EqualTo(DisplayStyle.None));

            view.SetArea(CreateArea(JoystickMode.Floating));
            yield return WaitFrames(3);

            Assert.That(view.Background.style.display.value, Is.Not.EqualTo(DisplayStyle.None));
            Assert.That(view.Handle.style.display.value, Is.Not.EqualTo(DisplayStyle.None));
        }

        [UnityTest]
        public IEnumerator SetArea_ToAnAreaWithoutJoystick_HidesTheElementsAgain()
        {
            yield return CreateSceneWithService();

            var view = AddView(CreateArea(JoystickMode.Floating));
            yield return WaitFrames(3);

            view.SetArea(CreateArea(JoystickMode.None));
            yield return WaitFrames(3);

            Assert.That(view.Background.style.display.value, Is.EqualTo(DisplayStyle.None));
            Assert.That(view.Handle.style.display.value, Is.EqualTo(DisplayStyle.None));
        }

        [UnityTest]
        public IEnumerator HideWhenIdle_IdleJoystick_OpacityIsZero()
        {
            yield return CreateSceneWithService();

            var view = AddView(CreateArea(JoystickMode.Floating));
            yield return WaitFrames(5);

            Assert.That(view.Background.resolvedStyle.opacity, Is.EqualTo(0f).Within(_opacityTolerance));
            Assert.That(view.Handle.resolvedStyle.opacity, Is.EqualTo(0f).Within(_opacityTolerance));
        }

        [UnityTest]
        public IEnumerator HideWhenIdle_AfterTheTouchBegan_OpacityIsAboveZero()
        {
            yield return CreateSceneWithService();

            var view = AddView(CreateArea(JoystickMode.Floating));
            yield return WaitFrames(2);

            Touch.Begin(_fingerId, ScreenPoint(0.25f, 0.5f));
            yield return WaitFrames(3);

            Assert.That(view.Background.resolvedStyle.opacity, Is.GreaterThan(0f));
        }

        [UnityTest]
        public IEnumerator HideWhenIdle_AfterTheFadeDuration_OpacityIsOne()
        {
            yield return CreateSceneWithService();

            var view = AddView(CreateArea(JoystickMode.Floating));
            yield return WaitFrames(2);

            Touch.Begin(_fingerId, ScreenPoint(0.25f, 0.5f));
            yield return new WaitForSecondsRealtime(_fadeDuration + SettleSeconds);
            yield return WaitFrames(2);

            Assert.That(view.Background.resolvedStyle.opacity, Is.EqualTo(1f).Within(_opacityTolerance));
            Assert.That(view.Handle.resolvedStyle.opacity, Is.EqualTo(1f).Within(_opacityTolerance));
        }

        [UnityTest]
        public IEnumerator HideWhenIdle_AfterTheLiftAndTheFadeDuration_OpacityIsZero()
        {
            yield return CreateSceneWithService();

            var view = AddView(CreateArea(JoystickMode.Floating));
            yield return WaitFrames(2);

            Touch.Begin(_fingerId, ScreenPoint(0.25f, 0.5f));
            yield return new WaitForSecondsRealtime(_fadeDuration + SettleSeconds);

            Touch.End(_fingerId);
            yield return new WaitForSecondsRealtime(_fadeDuration + SettleSeconds);
            yield return WaitFrames(2);

            Assert.That(view.Background.resolvedStyle.opacity, Is.EqualTo(0f).Within(_opacityTolerance));
            Assert.That(view.Handle.resolvedStyle.opacity, Is.EqualTo(0f).Within(_opacityTolerance));
        }

        [UnityTest]
        public IEnumerator ShowWhenIdle_IdleFixedJoystick_OpacityIsOneAndTheBackgroundSitsAtTheRestPosition()
        {
            yield return CreateSceneWithService();

            var area = CreateArea(JoystickMode.Fixed, new Vector2(0.5f, 0.5f));
            var view = AddView(area, new ViewFields(hideWhenIdle: false));
            yield return WaitFrames(5);

            var joystick = Service.GetArea(area).Joystick;
            var restPosition = ScreenPoint(0.25f, 0.5f);

            Assert.That(joystick.IsActive, Is.False);
            Assert.That(view.Background.resolvedStyle.opacity, Is.EqualTo(1f).Within(_opacityTolerance));
            Assert.That(view.Handle.resolvedStyle.opacity, Is.EqualTo(1f).Within(_opacityTolerance));
            Assert.That(view.Background.worldBound.center, Is.EqualTo(ScreenToPanel(restPosition)).Using(_panelPositionComparer));
            AssertPlacedLikeJoystick(view, joystick);
        }

        #endregion

        #region Placement

        [UnityTest]
        public IEnumerator FloatingJoystick_AfterTheTouchBegan_BackgroundIsCenteredOnTheTouch()
        {
            yield return CreateSceneWithService();

            var area = CreateArea(JoystickMode.Floating);
            var view = AddView(area);
            yield return WaitFrames(2);

            var touchPosition = ScreenPoint(0.25f, 0.4f);

            Touch.Begin(_fingerId, touchPosition);
            yield return WaitFrames(4);

            var joystick = Service.GetArea(area).Joystick;

            Assert.That(joystick.IsActive, Is.True);
            Assert.That(view.Background.worldBound.center, Is.EqualTo(ScreenToPanel(touchPosition)).Using(_panelPositionComparer));
            Assert.That(view.Handle.worldBound.center, Is.EqualTo(ScreenToPanel(touchPosition)).Using(_panelPositionComparer));
            Assert.That(view.Background.resolvedStyle.opacity, Is.GreaterThan(0f));
            AssertPlacedLikeJoystick(view, joystick);
        }

        [UnityTest]
        public IEnumerator FloatingJoystick_AfterTheTouchMoved_MovesTheHandleAndNotTheBackground()
        {
            yield return CreateSceneWithService();

            var area = CreateArea(JoystickMode.Floating);
            var view = AddView(area);
            yield return WaitFrames(2);

            var touchPosition = ScreenPoint(0.25f, 0.4f);

            Touch.Begin(_fingerId, touchPosition);
            yield return WaitFrames(4);

            var joystick = Service.GetArea(area).Joystick;
            var backgroundCenter = view.Background.worldBound.center;
            var handleCenter = view.Handle.worldBound.center;
            var movedPosition = touchPosition + new Vector2(joystick.Radius * 0.5f, 0f);

            Touch.Move(_fingerId, movedPosition);
            yield return WaitFrames(4);

            joystick = Service.GetArea(area).Joystick;

            Assert.That(view.Background.worldBound.center, Is.EqualTo(backgroundCenter).Using(_panelPositionComparer));
            Assert.That(view.Handle.worldBound.center, Is.Not.EqualTo(handleCenter).Using(_panelPositionComparer));
            Assert.That(view.Handle.worldBound.center, Is.EqualTo(ScreenToPanel(movedPosition)).Using(_panelPositionComparer));
            AssertPlacedLikeJoystick(view, joystick);
        }

        [UnityTest]
        public IEnumerator HandleSize_OfAThird_ScalesTheHandleDiameter()
        {
            yield return CreateSceneWithService();

            var area = CreateArea(JoystickMode.Floating);
            var view = AddView(area, new ViewFields(handleSize: 0.3f));
            yield return WaitFrames(2);

            Touch.Begin(_fingerId, ScreenPoint(0.25f, 0.5f));
            yield return WaitFrames(4);

            Assert.That(view.HandleSize, Is.EqualTo(0.3f).Within(0.0001f));
            Assert.That(view.Handle.worldBound.width / view.Background.worldBound.width,
                Is.EqualTo(0.3f).Within(0.02f));
        }

        [UnityTest]
        public IEnumerator ScaledPanel_PlacesTheElementsInPanelUnits()
        {
            yield return CreateSceneWithService(_panelScale);

            var area = CreateArea(JoystickMode.Floating);
            var view = AddView(area);
            yield return WaitFrames(2);

            var touchPosition = ScreenPoint(0.25f, 0.4f);

            Touch.Begin(_fingerId, touchPosition);
            yield return WaitFrames(2);

            var radiusInPixels = Service.GetArea(area).Joystick.Radius;

            Touch.Move(_fingerId, touchPosition + new Vector2(radiusInPixels * 0.5f, 0f));
            yield return WaitFrames(4);

            Assert.That(PanelScale, Is.EqualTo(_panelScale).Within(0.01f));
            Assert.That(view.Background.worldBound.width,
                Is.EqualTo(2f * radiusInPixels / _panelScale).Within(_panelTolerance));
            Assert.That(view.Background.worldBound.center, Is.EqualTo(ScreenToPanel(touchPosition)).Using(_panelPositionComparer));
            AssertPlacedLikeJoystick(view, Service.GetArea(area).Joystick);
        }

        [UnityTest]
        public IEnumerator ServiceAppearsAfterTheView_PlacesTheElementsOnceItIsRegistered()
        {
            CreateDocument();
            yield return null;

            var area = CreateArea(JoystickMode.Fixed, new Vector2(0.5f, 0.5f));
            var view = AddView(area, new ViewFields(hideWhenIdle: false));
            yield return WaitFrames(3);

            CreateService();
            yield return WaitFrames(4);

            Assert.That(view.Background.style.display.value, Is.Not.EqualTo(DisplayStyle.None));
            AssertPlacedLikeJoystick(view, Service.GetArea(area).Joystick);
        }

        [UnityTest]
        public IEnumerator ServiceReplacedByANewOne_ShowsTheJoystickForAFingerOnTheNewService()
        {
            yield return CreateSceneWithService();

            var area = CreateArea(JoystickMode.Floating);
            var view = AddView(area);
            yield return WaitFrames(2);

            UnityEngine.Object.Destroy(Service.gameObject);
            yield return null;

            CreateService();
            yield return WaitFrames(2);

            var touchPosition = ScreenPoint(0.25f, 0.4f);

            Touch.Begin(_fingerId, touchPosition);
            yield return WaitFrames(4);

            var joystick = Service.GetArea(area).Joystick;

            Assert.That(joystick.IsActive, Is.True);
            Assert.That(view.Background.style.display.value, Is.Not.EqualTo(DisplayStyle.None));
            Assert.That(view.Handle.style.display.value, Is.Not.EqualTo(DisplayStyle.None));
            Assert.That(view.Background.resolvedStyle.opacity, Is.GreaterThan(0f));
            Assert.That(view.Background.worldBound.center, Is.EqualTo(ScreenToPanel(touchPosition)).Using(_panelPositionComparer));
            AssertPlacedLikeJoystick(view, joystick);
        }

        #endregion

        #region Mode of the surface

        [UnityTest]
        public IEnumerator HideWhenIdle_IdleFixedJoystick_OpacityIsOne()
        {
            yield return CreateSceneWithService();

            var view = AddView(CreateArea(JoystickMode.Fixed, new Vector2(0.5f, 0.5f)));
            Assert.That(view.HideWhenIdle, Is.True);
            yield return WaitFrames(5);

            Assert.That(view.Background.resolvedStyle.opacity, Is.EqualTo(1f).Within(_opacityTolerance));
            Assert.That(view.Handle.resolvedStyle.opacity, Is.EqualTo(1f).Within(_opacityTolerance));
        }

        [UnityTest]
        public IEnumerator HideWhenIdle_SurfaceModeSetToFixedOnAFloatingArea_IdleJoystickIsVisible()
        {
            yield return CreateSceneWithService();

            var area = CreateArea(JoystickMode.Floating, new Vector2(0.5f, 0.5f));
            var view = AddView(area);
            yield return WaitFrames(2);

            Service.GetArea(area).JoystickMode = JoystickMode.Fixed;
            yield return WaitFrames(5);

            Assert.That(view.HideWhenIdle, Is.True);
            Assert.That(view.Background.resolvedStyle.opacity, Is.EqualTo(1f).Within(_opacityTolerance));
            Assert.That(view.Handle.resolvedStyle.opacity, Is.EqualTo(1f).Within(_opacityTolerance));
        }

        [UnityTest]
        public IEnumerator SurfaceModeSetToFloatingOnAnAreaWithNone_BothElementsAreShown()
        {
            yield return CreateSceneWithService();

            var area = CreateArea(JoystickMode.None);
            var view = AddView(area);
            yield return WaitFrames(2);

            Service.GetArea(area).JoystickMode = JoystickMode.Floating;
            yield return WaitFrames(3);

            Assert.That(view.Background.style.display.value, Is.Not.EqualTo(DisplayStyle.None));
            Assert.That(view.Handle.style.display.value, Is.Not.EqualTo(DisplayStyle.None));
        }

        [UnityTest]
        public IEnumerator SurfaceModeSetToNoneOnAFloatingArea_BothElementsAreHidden()
        {
            yield return CreateSceneWithService();

            var area = CreateArea(JoystickMode.Floating);
            var view = AddView(area);
            yield return WaitFrames(3);

            Service.GetArea(area).JoystickMode = JoystickMode.None;
            yield return WaitFrames(3);

            Assert.That(view.Background.style.display.value, Is.EqualTo(DisplayStyle.None));
            Assert.That(view.Handle.style.display.value, Is.EqualTo(DisplayStyle.None));
        }

        #endregion

        #region Return after the lift

        [UnityTest]
        public IEnumerator ReturnDuration_DefaultsToATenthOfASecond()
        {
            yield return CreateSceneWithService();

            var view = AddView(CreateArea(JoystickMode.Floating));
            yield return WaitFrames(2);

            Assert.That(view.ReturnDuration, Is.EqualTo(0.1f).Within(0.0001f));
        }

        [UnityTest]
        public IEnumerator HidingJoystick_OneFrameAfterTheLift_BackgroundStaysWhereItWasLastActive()
        {
            yield return CreateSceneWithService();

            var area = CreateArea(JoystickMode.Floating);
            var view = AddView(area, new ViewFields(returnDuration: _longReturnDuration));
            yield return WaitFrames(2);

            yield return DragHandleAway(area, ScreenPoint(0.25f, 0.4f));

            var activeBackgroundCenter = view.Background.worldBound.center;

            Touch.End(_fingerId);
            yield return WaitFrames(2);

            Assert.That(Service.GetArea(area).Joystick.IsActive, Is.False);
            Assert.That(view.Background.worldBound.center, Is.EqualTo(activeBackgroundCenter).Using(_panelPositionComparer));
        }

        [UnityTest]
        public IEnumerator HidingJoystick_AfterTheLiftAndTheReturnDuration_HandleIsAtTheBackgroundCenter()
        {
            yield return CreateSceneWithService();

            var area = CreateArea(JoystickMode.Floating);
            var view = AddView(area, new ViewFields(returnDuration: _longReturnDuration));
            yield return WaitFrames(2);

            yield return DragHandleAway(area, ScreenPoint(0.25f, 0.4f));

            var activeBackgroundCenter = view.Background.worldBound.center;

            Assert.That(view.Handle.worldBound.center, Is.Not.EqualTo(activeBackgroundCenter).Using(_panelPositionComparer));

            Touch.End(_fingerId);
            yield return new WaitForSecondsRealtime(_longReturnDuration + SettleSeconds);
            yield return WaitFrames(2);

            Assert.That(view.Background.worldBound.center, Is.EqualTo(activeBackgroundCenter).Using(_panelPositionComparer));
            Assert.That(view.Handle.worldBound.center, Is.EqualTo(view.Background.worldBound.center).Using(_panelPositionComparer));
        }

        [UnityTest]
        public IEnumerator HidingJoystick_OneFrameAfterTheLift_HandleIsStillOnItsWayBack()
        {
            yield return CreateSceneWithService();

            var area = CreateArea(JoystickMode.Floating);
            var view = AddView(area, new ViewFields(returnDuration: _longReturnDuration));
            yield return WaitFrames(2);

            yield return DragHandleAway(area, ScreenPoint(0.25f, 0.4f));

            Touch.End(_fingerId);
            yield return WaitFrames(2);

            Assert.That(view.Handle.worldBound.center, Is.Not.EqualTo(view.Background.worldBound.center).Using(_panelPositionComparer));
        }

        [UnityTest]
        public IEnumerator HidingJoystick_WithReturnDurationZero_OneFrameAfterTheLiftHandleIsAtTheBackgroundCenter()
        {
            yield return CreateSceneWithService();

            var area = CreateArea(JoystickMode.Floating);
            var view = AddView(area, new ViewFields(returnDuration: 0f));
            yield return WaitFrames(2);

            yield return DragHandleAway(area, ScreenPoint(0.25f, 0.4f));

            var activeBackgroundCenter = view.Background.worldBound.center;

            Touch.End(_fingerId);
            yield return WaitFrames(2);

            Assert.That(view.Background.worldBound.center, Is.EqualTo(activeBackgroundCenter).Using(_panelPositionComparer));
            Assert.That(view.Handle.worldBound.center, Is.EqualTo(activeBackgroundCenter).Using(_panelPositionComparer));
        }

        [UnityTest]
        public IEnumerator ShownJoystick_OneFrameAfterTheLift_BackgroundIsNotYetAtTheRestPosition()
        {
            yield return CreateSceneWithService();

            var area = CreateArea(JoystickMode.Floating);
            var view = AddView(area, new ViewFields(hideWhenIdle: false, returnDuration: _longReturnDuration));
            yield return WaitFrames(2);

            yield return DragHandleAway(area, ScreenPoint(0.25f, 0.4f));

            Touch.End(_fingerId);
            yield return WaitFrames(2);

            var joystick = Service.GetArea(area).Joystick;
            var restCenter = ScreenToPanel(joystick.Center);

            Assert.That(joystick.IsActive, Is.False);
            Assert.That(Vector2.Distance(view.Background.worldBound.center, restCenter), Is.GreaterThan(_panelTolerance));
        }

        [UnityTest]
        public IEnumerator ShownJoystick_AfterTheLiftAndTheReturnDuration_BackgroundIsAtTheRestPositionAndHandleInIt()
        {
            yield return CreateSceneWithService();

            var area = CreateArea(JoystickMode.Floating);
            var view = AddView(area, new ViewFields(hideWhenIdle: false, returnDuration: _longReturnDuration));
            yield return WaitFrames(2);

            yield return DragHandleAway(area, ScreenPoint(0.25f, 0.4f));

            Touch.End(_fingerId);
            yield return new WaitForSecondsRealtime(_longReturnDuration + SettleSeconds);
            yield return WaitFrames(2);

            var joystick = Service.GetArea(area).Joystick;
            var restCenter = ScreenToPanel(joystick.Center);

            Assert.That(view.Background.worldBound.center, Is.EqualTo(restCenter).Using(_panelPositionComparer));
            Assert.That(view.Handle.worldBound.center, Is.EqualTo(restCenter).Using(_panelPositionComparer));
        }

        [UnityTest]
        public IEnumerator FixedJoystick_OneFrameAfterTheLift_OnlyTheHandleIsStillReturning()
        {
            yield return CreateSceneWithService();

            var area = CreateArea(JoystickMode.Fixed, new Vector2(0.5f, 0.5f));
            var view = AddView(area, new ViewFields(returnDuration: _longReturnDuration));
            yield return WaitFrames(2);

            var restPosition = ScreenPoint(0.25f, 0.5f);

            yield return DragHandleAway(area, restPosition);

            Touch.End(_fingerId);
            yield return WaitFrames(2);

            Assert.That(view.Background.worldBound.center, Is.EqualTo(ScreenToPanel(restPosition)).Using(_panelPositionComparer));
            Assert.That(view.Handle.worldBound.center, Is.Not.EqualTo(view.Background.worldBound.center).Using(_panelPositionComparer));
            Assert.That(view.Background.resolvedStyle.opacity, Is.EqualTo(1f).Within(_opacityTolerance));
        }

        [UnityTest]
        public IEnumerator FixedJoystick_AfterTheLiftAndTheReturnDuration_HandleIsAtTheBackgroundCenter()
        {
            yield return CreateSceneWithService();

            var area = CreateArea(JoystickMode.Fixed, new Vector2(0.5f, 0.5f));
            var view = AddView(area, new ViewFields(returnDuration: _longReturnDuration));
            yield return WaitFrames(2);

            var restPosition = ScreenPoint(0.25f, 0.5f);

            yield return DragHandleAway(area, restPosition);

            Touch.End(_fingerId);
            yield return new WaitForSecondsRealtime(_longReturnDuration + SettleSeconds);
            yield return WaitFrames(2);

            Assert.That(view.Background.worldBound.center, Is.EqualTo(ScreenToPanel(restPosition)).Using(_panelPositionComparer));
            Assert.That(view.Handle.worldBound.center, Is.EqualTo(view.Background.worldBound.center).Using(_panelPositionComparer));
        }

        [UnityTest]
        public IEnumerator FixedJoystick_WithReturnDurationZero_OneFrameAfterTheLiftHandleIsAtTheBackgroundCenter()
        {
            yield return CreateSceneWithService();

            var area = CreateArea(JoystickMode.Fixed, new Vector2(0.5f, 0.5f));
            var view = AddView(area, new ViewFields(returnDuration: 0f));
            yield return WaitFrames(2);

            var restPosition = ScreenPoint(0.25f, 0.5f);

            yield return DragHandleAway(area, restPosition);

            Touch.End(_fingerId);
            yield return WaitFrames(2);

            Assert.That(view.Handle.worldBound.center, Is.EqualTo(view.Background.worldBound.center).Using(_panelPositionComparer));
            Assert.That(view.Handle.worldBound.center, Is.EqualTo(ScreenToPanel(restPosition)).Using(_panelPositionComparer));
        }

        [UnityTest]
        public IEnumerator FloatingJoystick_PressedAgainDuringTheReturn_PlacementFollowsTheSurfaceAtOnce()
        {
            yield return CreateSceneWithService();

            var area = CreateArea(JoystickMode.Floating);
            var view = AddView(area, new ViewFields(hideWhenIdle: false, returnDuration: _longReturnDuration));
            yield return WaitFrames(2);

            yield return DragHandleAway(area, ScreenPoint(0.25f, 0.4f));

            Touch.End(_fingerId);
            yield return WaitFrames(2);

            var secondTouch = ScreenPoint(0.15f, 0.7f);

            Touch.Begin(_fingerId, secondTouch);
            yield return WaitFrames(3);

            Assert.That(Service.GetArea(area).Joystick.IsActive, Is.True);
            Assert.That(view.Background.worldBound.center, Is.EqualTo(ScreenToPanel(secondTouch)).Using(_panelPositionComparer));
            Assert.That(view.Handle.worldBound.center, Is.EqualTo(ScreenToPanel(secondTouch)).Using(_panelPositionComparer));
        }

        #endregion

        #region Pointer filter

        [UnityTest]
        public IEnumerator FingerOnTheVisibleJoystick_WithAFilterOnTheDocument_StillBelongsToTheAreaSurface()
        {
            yield return CreateSceneWithService();

            var area = CreateArea(JoystickMode.Fixed, new Vector2(0.5f, 0.5f));
            var view = AddView(area, new ViewFields(hideWhenIdle: false));
            Document.gameObject.AddComponent<UIDocumentPointerFilter>();
            yield return WaitFrames(4);

            var areaCounter = Count(Service.GetArea(area));
            var screenCounter = Count(Service.Screen);

            Touch.Begin(_fingerId, ScreenPositionOf(view.Background));
            yield return WaitFrames(3);

            Assert.That(areaCounter.TouchBeganCount, Is.EqualTo(1));
            Assert.That(screenCounter.TouchBeganCount, Is.EqualTo(0));
        }

        #endregion

        #region Utility

        private IEnumerator CreateSceneWithService(float panelScale = 1f)
        {
            CreateService();
            CreateDocument(panelScale);
            yield return WaitFrames(2);
        }

        private InputArea CreateArea(JoystickMode joystickMode, Vector2 fixedJoystickPosition = default)
        {
            return Track(JoystickAreaFactory.CreateLeftHalfArea(joystickMode, fixedJoystickPosition));
        }

        /// <summary>
        /// The fields are applied while the component is disabled, so that the elements are built from them.
        /// </summary>
        private VirtualJoystickView AddView(InputArea area, ViewFields fields = null)
        {
            var view = Document.gameObject.AddComponent<VirtualJoystickView>();

            if (fields != null)
            {
                view.enabled = false;
                JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(fields), view);
                view.enabled = true;
            }

            view.SetArea(area);

            return view;
        }

        /// <summary>
        /// Puts a finger down at the position, then moves it half a radius to the right.
        /// </summary>
        private IEnumerator DragHandleAway(InputArea area, Vector2 touchPosition)
        {
            Touch.Begin(_fingerId, touchPosition);
            yield return WaitFrames(4);

            var radius = Service.GetArea(area).Joystick.Radius;

            Touch.Move(_fingerId, touchPosition + new Vector2(radius * 0.5f, 0f));
            yield return WaitFrames(4);
        }

        private void AssertPlacedLikeJoystick(VirtualJoystickView view, JoystickData joystick)
        {
            var background = view.Background;
            var handle = view.Handle;

            var diameter = 2f * joystick.Radius / PanelScale;

            Assert.That(background.worldBound.center, Is.EqualTo(ScreenToPanel(joystick.Center)).Using(_panelPositionComparer));
            Assert.That(background.worldBound.width, Is.EqualTo(diameter).Within(_panelTolerance));
            Assert.That(background.worldBound.height, Is.EqualTo(diameter).Within(_panelTolerance));
            Assert.That(handle.worldBound.center, Is.EqualTo(ScreenToPanel(joystick.HandlePosition)).Using(_panelPositionComparer));
            Assert.That(handle.worldBound.width, Is.EqualTo(diameter * view.HandleSize).Within(_panelTolerance));
            Assert.That(handle.worldBound.width / background.worldBound.width,
                Is.EqualTo(view.HandleSize).Within(0.02f));
        }

        private static void AssertBorderWidth(VisualElement element, float width)
        {
            Assert.That(element.resolvedStyle.borderTopWidth, Is.EqualTo(width).Within(0.01f));
            Assert.That(element.resolvedStyle.borderRightWidth, Is.EqualTo(width).Within(0.01f));
            Assert.That(element.resolvedStyle.borderBottomWidth, Is.EqualTo(width).Within(0.01f));
            Assert.That(element.resolvedStyle.borderLeftWidth, Is.EqualTo(width).Within(0.01f));
        }

        private static void AssertBorderColor(VisualElement element, Color color)
        {
            Assert.That(element.resolvedStyle.borderTopColor, Is.EqualTo(color));
            Assert.That(element.resolvedStyle.borderRightColor, Is.EqualTo(color));
            Assert.That(element.resolvedStyle.borderBottomColor, Is.EqualTo(color));
            Assert.That(element.resolvedStyle.borderLeftColor, Is.EqualTo(color));
        }

        private static void AssertIsCircle(VisualElement element)
        {
            var halfLength = new Length(50f, LengthUnit.Percent);

            Assert.That(element.style.position.value, Is.EqualTo(Position.Absolute));
            Assert.That(element.style.borderTopLeftRadius.value, Is.EqualTo(halfLength));
            Assert.That(element.style.borderTopRightRadius.value, Is.EqualTo(halfLength));
            Assert.That(element.style.borderBottomLeftRadius.value, Is.EqualTo(halfLength));
            Assert.That(element.style.borderBottomRightRadius.value, Is.EqualTo(halfLength));
        }

        #endregion
    }
}
