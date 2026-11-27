# Changelog

All notable changes to this package are documented here. The format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [1.0.0] - 2026-11-27

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
- `IInputSurface.JoystickMode` with a setter, to change the joystick mode of a surface at runtime. The mode set this way overrides the area asset's mode for that surface only, and the asset is never changed. When the asset's mode changes afterwards, the surface follows the asset again. Setting a mode on the screen surface logs a warning and is ignored.
- `UIDocumentPointerFilter` and `VirtualJoystickView` in `AngryKoala.Input.UIToolkit`.
- `VirtualJoystickView` draws the joystick mode the surface has at the time. A fixed joystick is always shown, and Hide When Idle applies to floating and following joysticks. After the finger lifts, a joystick that hides stays where it was while it fades, and its handle moves back to the centre over `ReturnDuration` (default 0.1 s, unscaled time). A joystick that does not hide moves back to its rest position over the same time.
- `VirtualJoystickViewEditor` in `AngryKoala.Input.UIToolkit.Editor`, an "Area Joystick" box under the view's fields. It shows and edits the Joystick Mode, Joystick Radius, Joystick Dead Zone and Fixed Joystick Position of the view's area asset.
- `ScriptedTouch` in `AngryKoala.Input.Testing`, a pretend touchscreen for Play Mode tests.
- Input Inspector window (menu `Angry Koala/Input/Input Inspector`). Its Fingers table always shows its header and at least one row, with "-" when no finger is down. The Status and Fingers sections only grow during a Play session, so the Screen Map keeps its size when a finger goes down.
- Edit Mode tests in `Tests/Editor` and Play Mode tests in `Tests/Runtime` and `UIToolkit/Tests/Runtime`.
- Assembly definitions `AngryKoala.Input`, `AngryKoala.Input.Editor`, `AngryKoala.Input.UIToolkit`, `AngryKoala.Input.Testing`, `AngryKoala.Input.UIToolkit.Editor`, `AngryKoala.Input.Tests.Editor`, `AngryKoala.Input.Tests.Runtime` and `AngryKoala.Input.UIToolkit.Tests.Runtime`.
- `package.json` for distribution as a UPM package from `Assets/Input`, depending on `com.angrykoala.services` 2.0.0 and `com.unity.inputsystem` 1.20.1.
