# Changelog

All notable changes to this package are documented here. The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [1.0.0] - Unreleased

### Added

- `IInputService` and `InputService`, a component that reads the touches and the mouse through the Input System, and decides which surface each finger belongs to.
- `IInputService.Screen`, `GetArea(InputArea)` and `GetData(List<InputSurfaceData>)`.
- `IInputService.Block(object)` and `Unblock(object)`, with `IsBlocked`.
- `IInputService.AddPointerFilter(IPointerFilter)` and `RemovePointerFilter(IPointerFilter)`, and the `IPointerFilter` interface.
- `IInputService.OnBackPressed`, raised by Escape, also while input is blocked.
- `IInputSurface` with the state `IsTouching`, `TouchCount`, `PrimaryTouch` and `Joystick`, and the events `OnTouchBegan`, `OnTouchEnded`, `OnTap`, `OnHold`, `OnDragBegan`, `OnDrag`, `OnDragEnded`, `OnSwipe` and `OnPinch`.
- The `TouchData`, `SwipeData`, `PinchData`, `JoystickData` and `InputSurfaceData` readonly structs.
- The `SwipeDirection`, `SwipeCompassDirection`, `SwipeTiming`, `JoystickMode` and `InputAreaSpace` enums.
- `InputArea` ScriptableObject (menu `Angry Koala/Input/Input Area`), with its rectangle, space, joystick settings and optional gesture settings.
- `GestureSettings` ScriptableObject (menu `Angry Koala/Input/Gesture Settings`), with the tap, hold, drag and swipe thresholds in centimetres and seconds.
- Joystick modes None, Fixed, Floating and Following.
- `UIDocumentPointerFilter` and `VirtualJoystickView` in `AngryKoala.Input.UIToolkit`.
- `ScriptedTouch` in `AngryKoala.Input.Testing`, a pretend touchscreen for Play Mode tests.
- Input Inspector window (menu `Angry Koala/Input/Input Inspector`).
- Edit Mode tests in `Tests/Editor` and Play Mode tests in `Tests/Runtime` and `UIToolkit/Tests/Runtime`.
- Assembly definitions `AngryKoala.Input`, `AngryKoala.Input.Editor`, `AngryKoala.Input.UIToolkit`, `AngryKoala.Input.Testing`, `AngryKoala.Input.Tests.Editor`, `AngryKoala.Input.Tests.Runtime` and `AngryKoala.Input.UIToolkit.Tests.Runtime`.
- `package.json` for distribution as a UPM package from `Assets/Input`, depending on `com.angrykoala.services` 2.0.0 and `com.unity.inputsystem` 1.20.1.
