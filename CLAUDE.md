# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Development Workflow

This is a Unity project. Building, running, and testing all happen inside the Unity Editor (6000.6.4f1, from `ProjectSettings/ProjectVersion.txt`). Open the project in Unity, or drive the open editor with `unity command` (see the global editor automation rules). Open the solution in your IDE for code editing with IntelliSense.

Work happens on the `development` branch. `main` is the release branch.

To run the editor inspection tool: **Angry Koala > Input > Input Inspector** (Play Mode only).

Tests live in three places:

- `Assets/Input/Tests/Editor`: Edit Mode tests of the gesture logic (`InputSurface`).
- `Assets/Input/Tests/Runtime`: Play Mode tests of the service, through `ScriptedTouch`.
- `Assets/Input/UIToolkit/Tests/Runtime`: Play Mode tests of the UI Toolkit components.

Run them from the Test Runner.

## Repository Layout

- `Assets/Input/` is the package. It holds `package.json` (`com.angrykoala.input`, version 1.0.0), the runtime code in `Scripts/`, the Editor code in `Scripts/Editor/`, the UI Toolkit components in `UIToolkit/Scripts/`, the test tool in `Testing/Scripts/`, and the tests in `Tests/`. Consumers install it from git with `?path=/Assets/Input#<tag>`.
- `Assets/Demo/` is the demo scene, its scripts, materials and UI. It is outside the package and is not distributed with it.
- `Packages/manifest.json` lists `com.angrykoala.services` from git, plus the Unity packages the project uses. The Input System is listed there too.
- `README.md` at the root is the user guide. `Assets/Input/CHANGELOG.md` is the changelog.

## Distribution

The module is a UPM package whose root is `Assets/Input`. It depends on `com.angrykoala.services` 2.0.0 and `com.unity.inputsystem` 1.20.1, as declared in `package.json`. The project must also list the Services module in its own `Packages/manifest.json`, because Unity does not resolve git dependencies between packages. The minimum Unity version is 6000.3 (`package.json`). Only 6000.6.4f1 is tested.

## Architecture

The package lives entirely under `Assets/Input/`. Its namespace is `AngryKoala.Input`, and the UI Toolkit components use the same namespace in their own assembly. The Services module (`AngryKoala.Services`) is external and is not part of this repo.

### Assemblies

- `AngryKoala.Input`: runtime code in `Scripts/`. References `AngryKoala.Services` and the Input System. It names no UI system.
- `AngryKoala.Input.Editor`: `Scripts/Editor/`, Editor only. The Input Inspector.
- `AngryKoala.Input.UIToolkit`: `UIToolkit/Scripts/`. `UIDocumentPointerFilter` and `VirtualJoystickView`.
- `AngryKoala.Input.Testing`: `Testing/Scripts/`. `ScriptedTouch`. Its define constraint is `UNITY_INCLUDE_TESTS`, and it is not auto-referenced, so a test assembly must list it.
- `AngryKoala.Input.Tests.Editor`: `Tests/Editor/`, Edit Mode tests.
- `AngryKoala.Input.Tests.Runtime`: `Tests/Runtime/`, Play Mode tests. Uses `UNITY_INCLUDE_TESTS` as a define constraint and `overrideReferences`.
- `AngryKoala.Input.UIToolkit.Tests.Runtime`: `UIToolkit/Tests/Runtime/`, Play Mode tests for the UI Toolkit components.

`AssemblyInfo.cs` in `Scripts/` makes the internals visible to `AngryKoala.Input.Editor`, `AngryKoala.Input.UIToolkit`, `AngryKoala.Input.Tests.Editor` and `AngryKoala.Input.Tests.Runtime`. The UI Toolkit `AssemblyInfo.cs` does the same for `AngryKoala.Input.UIToolkit.Tests.Runtime`.

### Public API

**`IInputService`** (extends `IService`): `Screen`, `IsBlocked`, `OnBackPressed`, `GetArea(InputArea)`, `Block(object)` and `Unblock(object)`, `AddPointerFilter` and `RemovePointerFilter`, and `GetData(List<InputSurfaceData>)`.

**`IInputSurface`**: `IsTouching`, `TouchCount`, `PrimaryTouch`, `Joystick`, and the nine events `OnTouchBegan`, `OnTouchEnded`, `OnTap`, `OnHold`, `OnDragBegan`, `OnDrag`, `OnDragEnded`, `OnSwipe` and `OnPinch`.

**`IPointerFilter`**: `TakesPointer(Vector2)`. The one-method interface the UI registers through.

**Data**: `TouchData`, `SwipeData`, `PinchData`, `JoystickData` and `InputSurfaceData` (`readonly struct`). Most of their constructors are internal.

**Enums**: `SwipeDirection`, `SwipeDirection8`, `SwipeTiming`, `JoystickMode` and `InputAreaSpace`.

**`InputArea`** (ScriptableObject, menu `Angry Koala/Input/Input Area`) and **`GestureSettings`** (ScriptableObject, menu `Angry Koala/Input/Gesture Settings`). The area is the key for `GetArea`.

**`InputService`** (`sealed`, extends `BaseService<IInputService>`): the implementation. It is a MonoBehaviour with two Inspector fields, the gesture settings and the list of areas.

**`UIDocumentPointerFilter`** and **`VirtualJoystickView`**: MonoBehaviours that sit beside a `UIDocument`.

**`ScriptedTouch`** (`sealed`, `IDisposable`): `Create`, the routines `Tap`, `Hold`, `Drag` and `Pinch`, and the calls `Begin`, `Move`, `End` and `Cancel`.

### Structure

- `InputService` owns the screen surface (`_screen`) and a list of area surfaces (`_areaSurfaces`). A surface is an `InputSurface`, one per screen or area. It owns a list of tracked fingers (`_fingers`), the blockers, the pointer filters, and the back handlers.
- `InputSurface` (internal, one per screen or area) holds the fingers that are down on it, and takes no input itself. The service feeds it with `BeginTouch`, `UpdateTouch`, `EndTouch` and `CancelTouch`, then calls `CompleteFrame`. It raises the gestures from `GestureRules` (internal, the thresholds in plain numbers) and its own `Finger` records. It is the part the Edit Mode tests cover.
- `InputService.ProcessFrame` runs once per update. It prunes destroyed blockers and filters, updates the context of every surface (screen rectangle, safe area, DPI), reads the touches, then the mouse, completes the frame, and reads the Escape key.
- `EventHandlers` and `EventHandlersWithArgument<T>` (internal) hold the subscribers of one event. They keep the list between raises, and a removal during a raise leaves an empty slot that is compacted afterwards.
- `InputArea.GetPixelRect` works out an area's rectangle from the current screen or safe area. The service calls it every frame.

### Owner decision

`InputService.DecideOwner` runs once per finger, when it goes down. It returns null while blocked, then null if a pointer filter takes the position, then the first area surface whose pixel rectangle contains the position, and otherwise the screen. The owner is stored in `_fingers`, and it is not decided again until the finger lifts.

### Touches and the mouse

`ProcessTouches` reads `EnhancedTouch` active touches each frame, and marks which tracked fingers were seen. A tracked finger that is no longer seen is cancelled. `ProcessMouse` treats the left button as a finger with id -1, but only when no touch finger is down. `ProcessBack` raises `OnBackPressed` when `Keyboard.current.escapeKey` was pressed this frame.

### Joystick

`InputSurface.GetJoystick` reads the primary finger's position against the centre. The centre is the rest position for Fixed, and the touch-down position otherwise. `PullJoystickCenter` moves the centre for Following. Radii and distances are in centimetres in the settings and are turned into pixels with the surface's pixels-per-centimetre.

## Conventions and gotchas

- **The namespace hazard.** Inside any namespace under `AngryKoala`, the bare name `Input` resolves to the `AngryKoala.Input` namespace, not `UnityEngine.Input`. Write `UnityEngine.Input` in full where the legacy class is meant. The Input System's `UnityEngine.InputSystem` is not affected.
- **Services 2.0 resolution.** Resolve `IInputService` in `Start` or later, or use `ServiceLocator.WhenRegistered` as the UI components do. The service registers in `Awake`. A second `InputService` in a scene logs an error and disables itself.
- **Pointer filter ordering.** A filter is asked once per finger, when it goes down. A filter that takes a position makes its finger ignored for the whole press.
- **UI Toolkit picking.** An empty `UIDocument` root is not picked. A full-screen layout element is picked, so it takes every touch, unless its picking mode is ignore. Set `picking-mode: ignore` in the USS of such an element.
- **Blocking.** `Block` cancels the touches in progress and ignores their fingers until they lift. Each source is counted once, and `Unblock` removes the source passed in.
- **Mouse in the Editor.** The left button is a finger through the same code as a touch. It does not use Unity's `TouchSimulation`.
- **Units.** Pixels for positions and joystick geometry. Centimetres for gesture distances and joystick radius, converted with `Screen.dpi`, or the `FallbackDpi` of the settings when the screen reports none. Seconds in unscaled time for durations.
- **Handler exceptions.** A handler that throws is logged, with the handler's object as the log context when it is a Unity object, and the other handlers still run. A handler of a destroyed Unity object is skipped and dropped without a log entry. A pointer filter that throws is logged and counts as not taking the position.
- **Movement on release.** `InputSurface.EndTouch` runs the movement steps (drag, then the `WhenCrossed` swipe) for the final position before the release order, because the Input System reports a move and a lift in one update as a single ended touch. It does not run the hold step.
- **Area order.** The surfaces of the service's configured areas are created once, in list order, before any other area's surface (`EnsureConfiguredAreaSurfaces`). `GetArea` and `GetData` call it first, so a `WhenRegistered` callback that runs inside `base.Awake()` cannot get ahead of the list.
- **Tests and the Editor's focus.** The first live `ScriptedTouch` sets two Input System settings and the last one disposed restores them. Do not leave one undisposed in a test. UI Toolkit ignores pointer input while the Game view is not the focused Editor window (`Application.isFocused` is false), so the tests that expect a UI Toolkit click fail when another Editor window has the focus. It is not related to domain reload.
- **Real input in tests.** The service reads the current mouse and keyboard. A real mouse click in the Game view or a real Escape during a test run can disturb the service tests.
- **Test helpers.** `LogAssert.Expect` drops the regex options, so an option passed in a `Regex` does not apply. NUnit reuses the fixture, so counters and collected state must be reset in `[SetUp]`. `Is.Not.AllocatingGCMemory()` needs a statement lambda; an expression lambda that returns a value is not measured. Unity's `Vector2EqualityComparer` takes its tolerance as relative, so the tests compare positions with an absolute comparer (`Vector2AbsoluteEqualityComparer`, one copy per test assembly). Unity's test utilities have no `RectEqualityComparer`, so the tests have their own in each test assembly.
- **Documentation must match the code and the tests.** If behaviour changes, update `README.md`, `Assets/Input/CHANGELOG.md` and the tests together.
