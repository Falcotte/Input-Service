# Angry Koala Input

A touch input service for Unity on the Input System. The screen and areas of it are surfaces, and each surface reports taps, holds, drags, swipes and pinches. An area can also drive a virtual joystick. A finger belongs to one surface for the whole press, so a button drawn on top of an area takes its touches and the game never sees them. The module has no dependency on any UI system. Scripted touches let a Play Mode test press real buttons. The module depends on the Services module to find the service.

**Use it for**

- Mobile games that read touches on the screen, for example a joystick on the left half, a tap to fire on the right, and swipes for lane changes.
- Games that need the same gestures in several places of the screen, each with its own rectangle.

**Do not use it for**

- Keyboard, gamepad and rebinding. These are not in 1.0.0. Read the Input System directly for them.
- Work that needs a UI system to tell the service which touches are its own. The module asks registered pointer filters, and the UI Toolkit filter is included (see below).

## Install

Add the package to the project's `Packages/manifest.json`. The Services module must be listed too:

```json
"com.angrykoala.input": "https://github.com/Falcotte/Input-Service.git?path=/Assets/Input#v1.0.0",
"com.angrykoala.services": "https://github.com/Falcotte/Service-Locator.git?path=/Assets/Services#v2.0.0"
```

Unity does not resolve git dependencies between packages. Each AngryKoala module must be listed in the project's own `manifest.json`.

Set **Active Input Handling** to **Input System Package (New)** in the Player settings. The service reads the Input System only.

Minimum Unity version: 6000.3 (from `package.json`). Developed and tested on Unity 6000.6.4f1.

`package.json` declares the Services module at 2.0.0 and the Input System at 1.20.1. It does not declare UI Toolkit. The UI Toolkit components need the built-in UI Toolkit module, which the Input System package declares as its own dependency.

**The namespace.** Inside any namespace under `AngryKoala`, the bare name `Input` means the `AngryKoala.Input` namespace, not Unity's old `Input` class. Code there that means the old class must write `UnityEngine.Input` in full.

## Public interface

```csharp
public interface IInputService : IService
{
    // The surface for every finger that went down on no UI and in no area.
    IInputSurface Screen { get; }

    bool IsBlocked { get; }

    // Raised when Escape is pressed. It is raised also while input is blocked.
    event Action OnBackPressed;

    // Returns the surface of an area. The same surface is returned for the same asset every time.
    IInputSurface GetArea(InputArea area);

    // Blocks input until the same source calls Unblock. Touches in progress are cancelled.
    void Block(object source);

    void Unblock(object source);

    // A filter is asked once for every finger that goes down. A finger a filter takes is never given to a surface.
    void AddPointerFilter(IPointerFilter pointerFilter);

    void RemovePointerFilter(IPointerFilter pointerFilter);

    // Clears the list and fills it with one InputSurfaceData for the screen and one per area. Meant for tools.
    void GetData(List<InputSurfaceData> results);
}

public interface IPointerFilter
{
    // Whether a finger going down at this screen position, in pixels from the bottom left, belongs to the filter.
    bool TakesPointer(Vector2 screenPosition);
}

// The whole screen or one area of it. State is polled; events are raised when something happens.
public interface IInputSurface
{
    bool IsTouching { get; }

    int TouchCount { get; }

    // The first finger that went down on this surface. Default while nothing touches it.
    TouchData PrimaryTouch { get; }

    JoystickData Joystick { get; }

    // The mode set at runtime if there is one, otherwise the area asset's. The screen surface stays None.
    JoystickMode JoystickMode { get; set; }

    event Action<TouchData> OnTouchBegan;
    event Action<TouchData> OnTouchEnded;
    event Action<TouchData> OnTap;
    event Action<TouchData> OnHold;
    event Action<TouchData> OnDragBegan;
    event Action<TouchData> OnDrag;
    event Action<TouchData> OnDragEnded;
    event Action<SwipeData> OnSwipe;
    event Action<PinchData> OnPinch;
}

public readonly struct TouchData
{
    public int FingerId { get; }
    public Vector2 Position { get; }          // Pixels.
    public Vector2 StartPosition { get; }     // Pixels.
    public Vector2 NormalizedPosition { get; } // 0 to 1 inside the surface.
    public Vector2 Delta { get; }             // Pixels moved since the last frame.
    public Vector2 TotalDelta { get; }        // Pixels moved since the touch began.
    public Vector2 DeltaCentimeters { get; }
    public Vector2 TotalDeltaCentimeters { get; }
    public float Duration { get; }            // Seconds.
    public bool WasCanceled { get; }
}

public readonly struct SwipeData
{
    public TouchData Touch { get; }
    public Vector2 Vector { get; }            // From where the touch began to where the swipe was recognised, in pixels.
    public SwipeDirection Direction { get; }
    public SwipeCompassDirection CompassDirection { get; }
    public float Speed { get; }               // Centimetres per second, over the whole swipe.
}

public readonly struct PinchData
{
    public Vector2 Center { get; }            // Halfway between the two fingers, in pixels.
    public float ScaleDelta { get; }          // Distance now divided by the distance in the last frame.
    public float TotalScale { get; }          // Distance now divided by the distance when the pinch began.
}

public readonly struct JoystickData
{
    public bool IsActive { get; }
    public Vector2 Center { get; }            // Pixels.
    public Vector2 HandlePosition { get; }    // Pixels. Never further from the centre than the radius.
    public float Radius { get; }              // Pixels.
    public Vector2 Direction { get; }         // Length 0 to 1. Zero inside the dead zone.
}

public enum JoystickMode { None, Fixed, Floating, Following }

public enum InputAreaSpace { SafeArea, Screen }

public enum SwipeTiming { WhenCrossed, OnRelease }

public enum SwipeDirection { Up, Down, Left, Right }

public enum SwipeCompassDirection { Up, UpRight, Right, DownRight, Down, DownLeft, Left, UpLeft }
```

`InputSurfaceData` is the struct that `GetData` fills. It has the name, the area (null for the screen), the pixel rectangle, the touch count, the primary touch, the joystick and the last gesture name.

### Areas and settings

```csharp
// An area of the screen. The asset is also the key to ask the service with.
[CreateAssetMenu(menuName = "Angry Koala/Input/Input Area")]
public sealed class InputArea : ScriptableObject
{
    public InputAreaSpace Space { get; }           // The rectangle is a fraction of the safe area by default.
    public Vector2 Minimum { get; }                // Bottom left corner, from (0, 0) to (1, 1).
    public Vector2 Maximum { get; }                // Top right corner, from (0, 0) to (1, 1).
    public JoystickMode JoystickMode { get; }      // The default. A surface can change it at runtime.
    public float JoystickRadius { get; }           // Centimetres.
    public float JoystickDeadZone { get; }         // The part of the radius around the centre that gives no direction.
    public Vector2 FixedJoystickPosition { get; }  // Where a fixed joystick sits inside the area.
    public GestureSettings GestureSettings { get; } // Optional. Empty means the service's settings.
}

// Thresholds for the gestures. Distances are in centimetres, durations in seconds.
[CreateAssetMenu(menuName = "Angry Koala/Input/Gesture Settings")]
public sealed class GestureSettings : ScriptableObject
{
    public float TapMaximumDuration { get; }     // 0.3
    public float TapMaximumDistance { get; }     // 0.25 cm
    public float HoldDuration { get; }           // 0.5
    public float DragStartDistance { get; }      // 0.25 cm
    public float SwipeDistance { get; }          // 1 cm
    public float SwipeMaximumDuration { get; }   // 0, no limit
    public SwipeTiming SwipeTiming { get; }      // WhenCrossed
    public float FallbackDpi { get; }            // 160, used when the screen reports no DPI. Zero or less means 160.
}

// Adds a filter for Unity UI Toolkit. Put it beside a UIDocument.
public sealed class UIDocumentPointerFilter : MonoBehaviour, IPointerFilter

// Draws the joystick of an area on a UIDocument. Put it beside a UIDocument.
public sealed class VirtualJoystickView : MonoBehaviour
{
    public InputArea Area { get; }
    public Color BackgroundColor { get; }   // Defaults to white at 25 % alpha.
    public Color HandleColor { get; }       // Defaults to white at 60 % alpha.
    public Color OutlineColor { get; }      // Defaults to black at 35 % alpha.
    public float OutlineWidth { get; }      // Panel pixels. Defaults to 2.
    public float HandleSize { get; }        // Fraction of the background diameter. Defaults to 0.5.
    public bool HideWhenIdle { get; }       // Fades a floating or following joystick out while no finger drives it. A fixed one is always shown.
    public float FadeDuration { get; }      // Seconds, not affected by the time scale.
    public float ReturnDuration { get; }    // Seconds, not affected by the time scale. Defaults to 0.1.
}
```

`InputService` is the component that implements `IInputService`. Put one in the scene. Its Inspector fields are **Gesture Settings** (optional) and **Areas**, the list of areas in order. Where two areas overlap, the earlier one gets the finger.

## Example

A player that moves with a joystick on the left half of the screen, and spawns something where the screen is tapped. The area is an `InputArea` asset with **Space** set to Safe Area, **Minimum** (0, 0), **Maximum** (0.5, 1) and **Joystick Mode** set to Floating. The service is in the scene with that area in its list.

```csharp
using AngryKoala.Input;
using AngryKoala.Services;
using UnityEngine;

public sealed class Player : MonoBehaviour
{
    [SerializeField] private InputArea _moveArea;
    [SerializeField] private float _speedPerSecond = 5f;

    private IInputService _inputService;
    private IInputSurface _moveSurface;

    private void Start()
    {
        _inputService = ServiceLocator.Get<IInputService>();
        _moveSurface = _inputService.GetArea(_moveArea);

        _inputService.Screen.OnTap += OnScreenTap;
    }

    private void Update()
    {
        var direction = _moveSurface.Joystick.Direction;

        transform.Translate(new Vector3(direction.x, 0f, direction.y) * (_speedPerSecond * Time.deltaTime));
    }

    private void OnScreenTap(TouchData touch)
    {
        Debug.Log($"Tap at {touch.NormalizedPosition}");
    }
}
```

## Rules

**Owners of a finger**

- When a finger goes down, its owner is decided once and kept until it lifts. The owner is the first of these that applies: nothing, if input is blocked; nothing, if a pointer filter takes the position; the first area whose rectangle contains the position; otherwise the screen.
- Leaving the area's rectangle does not change the owner. Sliding onto a button does not press anything for the game.
- A pointer filter that throws is logged and counts as not taking the position. A filter that is a destroyed Unity object is dropped.
- Adding the same filter twice does nothing. Removing a filter that was never added does nothing.
- `GetArea` returns the same surface for the same asset. An area that is not in the service's list is added at the end of the list on its first use, without a warning. Nothing has to be in the scene.
- Areas are checked in the list order. The list in the Inspector is read once, when the service wakes. Areas added later through `GetArea` go to the end.
- An area's rectangle is worked out from the current screen and safe area each frame. A rotation or a new resolution is followed.

**Surfaces and gestures**

- The first finger on a surface is its primary touch. `PrimaryTouch`, tap, hold, drag, swipe and the joystick follow it.
- A second finger on the same surface starts a pinch. If the primary finger lifts while another is down, that one becomes primary. Its `Delta` is zero in that frame.
- A second finger at any time during a touch rules out tap, hold and swipe for every finger of that surface.
- **Tap:** the finger lifts within `TapMaximumDuration`, and it stayed within `TapMaximumDistance` of where it began. It is not a tap after a hold, after a drag has begun, or when the touch was cancelled.
- **Hold:** the finger has stayed within `TapMaximumDistance` for `HoldDuration`. It is raised once, while the finger is still down. It is not raised after a drag has begun.
- **Drag:** begins when the finger has travelled `DragStartDistance`. `OnDrag` is raised in frames where the finger moved.
- **Swipe:** raised once per touch. With `SwipeTiming.WhenCrossed` it is raised at the moment the finger has travelled `SwipeDistance`. With `SwipeTiming.OnRelease` it is raised when the finger lifts, if the finger ended at least `SwipeDistance` from where it began. With `SwipeMaximumDuration` above zero, a swipe counts only when done within that time. The drag events are still raised.
- **Pinch:** raised in the frames where the distance between two fingers on the surface changes, after the fingers are read.

**Order of events**

- On touch down, `OnTouchBegan` is raised first.
- In one frame, a finger that is still down raises the events in this order: hold, drag began and drag, then swipe (for `WhenCrossed`). The pinch is raised after all fingers have been read.
- On release, movement that arrived together with the release is handled first: drag began and drag, then swipe (for `WhenCrossed`). A fast flick can lift in the same frame it crosses a threshold, and it still raises its drag and its swipe. No hold is raised on release.
- After that, the order on release is: tap, swipe (for `OnRelease`), drag ended, touch ended.
- On cancel, the order is: drag ended (if the finger was dragging), then touch ended with `WasCanceled` true. A cancelled touch raises no tap and no swipe.

**Units**

- Positions are in pixels, from the bottom left of the screen. `NormalizedPosition` is from 0 to 1 inside the surface.
- Distances in the gesture settings are centimetres. They are converted with the screen's DPI. A screen that reports no DPI uses `FallbackDpi`.
- Durations are in seconds of real time. They come from the Input System's own timestamp of each touch, not from the frame that reads it, so a frame hitch does not turn a quick tap into a long press.
- The settings are kept sane when they are read and when an asset is edited. No duration or distance is negative. The hold duration is at least 0.05 s and the swipe distance at least 0.01 cm. A fallback DPI of zero or less means 160, and a positive one below 30 is raised to 30. An area's minimum and maximum stay within 0 to 1, and the maximum is never below the minimum.

**Joystick**

- A joystick belongs to an area. The screen has none. A joystick over the whole screen is an area that covers the screen.
- **None** gives no joystick. **Fixed** keeps the joystick at `FixedJoystickPosition` inside the area. **Floating** places it where the first finger went down, until the finger lifts. **Following** does the same, and moves the joystick along when the finger goes further than the radius.
- The mode is chosen on the area asset (**Joystick Mode**), or in the **Area Joystick** box of the `VirtualJoystickView` that draws it (see Joystick visual). That box edits the same asset.
- `IInputSurface.JoystickMode` can be read and set at runtime. Reading it gives the mode set at runtime, or the asset's mode if none was set. Setting it changes the mode of that surface for the rest of its life, and the asset is never changed. The radius, the dead zone and the fixed position still come from the area. There is no call that clears the runtime mode. Setting another mode on the screen surface logs a warning and is ignored, because the screen has no joystick.

A settings menu that switches the move area between Fixed and Floating:

```csharp
using AngryKoala.Input;
using AngryKoala.Services;
using UnityEngine;

public sealed class JoystickSettings : MonoBehaviour
{
    [SerializeField] private InputArea _moveArea;

    private IInputSurface _moveSurface;

    private void Start()
    {
        var inputService = ServiceLocator.Get<IInputService>();
        _moveSurface = inputService.GetArea(_moveArea);
    }

    // Called by the menu buttons.
    public void UseFixedJoystick() => _moveSurface.JoystickMode = JoystickMode.Fixed;

    public void UseFloatingJoystick() => _moveSurface.JoystickMode = JoystickMode.Floating;
}
```
- The joystick's `Direction` is zero inside the dead zone. Outside it, the length rises from 0 to 1 at the edge of the radius. The dead zone is at most 0.9 of the radius.
- `Joystick.HandlePosition` never goes further from the centre than the radius.

**Blocking and cancellation**

- `Block(source)` counts the sources that are blocking. The service is blocked while any source is in the list. A popup and a tutorial can block and unblock without undoing each other.
- Blocking cancels the touches in progress, and their fingers are ignored until they lift.
- A touch is also cancelled when the service is disabled, and when the system reports a cancelled touch.
- Blocking and unblocking with a source the service does not know does nothing.

**Back**

- `OnBackPressed` is raised when Escape is pressed. It is raised also while input is blocked. Android's back button is expected to arrive as Escape, but that has not been tested on a device.

**Mouse**

- In the Editor and on desktop, the left button is a finger. It goes through the same code as a touch. The mouse finger is ignored while a touch finger is down.

**Performance**

- Events are raised when something happens. A finger that stays still raises nothing once its gestures have been raised. Drag and pinch events are raised only in the frames where the finger or the distance moved.
- Handler lists reuse their storage and do not allocate when an event is raised.
- Continuous values, such as `PrimaryTouch` and `Joystick`, are read from the surface. They are not events.
- The tests assert that a frame with fingers down allocates nothing.

**Logging**

- Problems are logged: a second `InputService` in a scene, a filter or a handler that throws. Routine messages are not logged.
- A handler that throws does not stop the other handlers. When the handler is a method of a Unity object, the log entry points at that object.
- A handler that is a method of a destroyed Unity object is not called. It is dropped from the event without a log entry. Unsubscribing in `OnDisable` or `OnDestroy` is still the rule.

**Update order and the Input System's update mode**

- The service's `Update` runs before scripts with the default execution order (its order is -100). A script that reads `Joystick` or `PrimaryTouch` in its own `Update` sees this frame's values.
- The service reads the touches once per `Update`. Keep the Input System's update mode at its default, Process Events In Dynamic Update. With events processed in Fixed Update, a release can be missed in a frame with two fixed steps, and the touch is then reported as cancelled.

## What it needs

- Unity 6000.3 or later.
- The Services module, `com.angrykoala.services` 2.0.0, listed in the project's `manifest.json`.
- The Input System, 1.20.1, named in `package.json`.
- The UI Toolkit components need the UI Toolkit module, which comes with the Input System (see Install).
- Tests need `com.unity.test-framework`.

## Pointer filters and UI Toolkit

A UI Toolkit screen takes the touches on its buttons through `UIDocumentPointerFilter`. Put it on the same GameObject as the `UIDocument`. It registers with the service when the service is there, and answers from what the panel has under the finger. If the service is destroyed and another one registers, for example after a scene reload, the filter and the joystick view move to the new one.

An empty `UIDocument` root is not picked, so it takes no touches. A full-screen layout element is picked, so it takes every touch that lands on it, including the ones meant for the game. Give such an element `picking-mode: ignore` in USS, or set `pickingMode` to `PickingMode.Ignore` in code. Otherwise every touch counts as UI.

## Joystick visual

`VirtualJoystickView` draws the circle and the handle of one area's joystick. It sits on the same GameObject as the `UIDocument`, and it reads the joystick from the area's surface each frame. It draws the mode the surface has at that time, so a mode set at runtime is what appears.

A fixed joystick is always shown. Hide When Idle applies to floating and following joysticks only. When the finger lifts, the joystick does not jump. A joystick that hides stays where it was while it fades out, and its handle moves back to the centre over **Return Duration**. A joystick that does not hide moves back to its rest position over the same time. Return Duration defaults to 0.1 s and is not affected by the time scale.

The view's Inspector has an **Area Joystick** box below the view's own fields. It shows the area's Joystick Mode, Joystick Radius, Joystick Dead Zone and Fixed Joystick Position, and edits the area asset. The box is in the `AngryKoala.Input.UIToolkit.Editor` assembly, which is Editor only.

The view draws with styles only. The package ships no sprites. It adds two USS classes, `angry-koala-joystick__background` and `angry-koala-joystick__handle`, so a project can restyle them in its own stylesheet. It sets the colours, the outline colour and the outline width in code. The circles ignore picking.

## Blocking

```csharp
using AngryKoala.Input;
using AngryKoala.Services;
using UnityEngine;

public sealed class Popup : MonoBehaviour
{
    private IInputService _inputService;

    private void Start()
    {
        _inputService = ServiceLocator.Get<IInputService>();
    }

    public void Open()
    {
        gameObject.SetActive(true);
        _inputService.Block(this);
    }

    public void Close()
    {
        gameObject.SetActive(false);
        _inputService.Unblock(this);
    }
}
```

The source is any object. Use the object that opened the block, so that two places can block without one unblocking the other.

## Tests

The module has Edit Mode tests in `Assets/Input/Tests/Editor` for the gesture logic, which work on samples and times and need no scene. They are grouped by subject: `Areas`, `Events`, `Gestures`, `Joystick`, `Surfaces` and `Utilities`. It has Play Mode tests in `Assets/Input/Tests/Runtime` for the service, grouped as `Areas`, `Gestures`, `Service`, `Touches` and `Utilities`, with `ScriptedTouchTests` at the root, and in `Assets/Input/UIToolkit/Tests/Runtime` for the UI Toolkit components. Run them from the Test Runner.

### Scripted touches

`ScriptedTouch` is a pretend touchscreen in the Input System. Everything that reads input sees it, the UI Toolkit buttons included. Positions are pixels from the bottom left. Finger ids start at 1.

```csharp
using System.Collections;
using AngryKoala.Input;
using AngryKoala.Services;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public sealed class ScreenTapTests
{
    private int _tapCount;

    [UnityTest]
    public IEnumerator Tap_RaisesOnTapOnTheScreen()
    {
        _tapCount = 0;

        var serviceObject = new GameObject("Input Service", typeof(InputService));
        var inputService = ServiceLocator.Get<IInputService>();

        inputService.Screen.OnTap += OnTap;

        using (var touch = ScriptedTouch.Create())
        {
            yield return touch.Tap(new Vector2(200f, 200f));
        }

        inputService.Screen.OnTap -= OnTap;
        Object.Destroy(serviceObject);

        Assert.That(_tapCount, Is.EqualTo(1));
    }

    private void OnTap(TouchData touch)
    {
        _tapCount++;
    }
}
```

The routines are `Tap`, `Hold`, `Drag` and `Pinch`, and they yield frames while they run. The lower-level calls are `Begin`, `Move`, `End` and `Cancel`, which queue one touch each. Each routine waits a couple of frames after the release, so that the service has read the release before the test looks.

While a `ScriptedTouch` exists, two Input System settings are changed: `backgroundBehavior` to ignore focus, and in the Editor `editorInputBehaviorInPlayMode` to send all device input to the Game view. The first live `ScriptedTouch` changes them, and disposing the last one restores them. Dispose also removes the device. Use it in a `using` block, or call `Dispose` in the test's teardown.

Those settings make the Input System and the service see a scripted touch while the Editor is in the background. UI Toolkit has its own check: it ignores pointer input while the application does not count as focused, which in the Editor depends on the Game view having the focus. So the first live `ScriptedTouch` also gives the focus to the Game view in the Editor. It takes the focus from the window you were in, and it does not give it back. With no Game view open it logs one warning, and scripted clicks on UI Toolkit buttons may then be ignored.

A test assembly that uses `ScriptedTouch` must reference `AngryKoala.Input.Testing`. That assembly is compiled in the Editor, and anywhere else the `UNITY_INCLUDE_TESTS` define is set. A project that installs the package can therefore use `ScriptedTouch` in tests that run in the Editor with no further setup, and the assembly stays out of player builds. Running such tests on a device needs `com.angrykoala.input` listed under `testables` in `Packages/manifest.json`, which also adds the package's own tests to the Test Runner.

A real mouse click in the Game view, or a real Escape key, during a test run can disturb the service tests, because the service reads the current mouse and keyboard.

## Inspector

Open it from **Angry Koala > Input > Input Inspector**. It shows each surface with its touch count, primary touch, joystick, the last gesture and its pixel rectangle. It lists the fingers and the surface that owns each one, the blockers, and the pointer filters. The Fingers table always shows its header and at least one row, with "-" when no finger is down. The Status and Fingers sections grow only during a Play session, so the Screen Map keeps its size when a finger goes down. It draws a map of the screen with the areas, the touches and the joysticks, in its own window. It reads the service in Play Mode.

## Demo

The demo is in `Assets/Demo`, outside the package. Open `Assets/Demo/Scenes/DemoScene.unity`. `DemoPlayer` moves with the joystick of the move area. `DemoSpawner` spawns on a tap on the screen. `DemoGestures` handles swipes and pinches on the screen. `DemoHud` is a UI Toolkit screen that shows the last gesture of each surface, has a toggle that blocks the input, and has a **Clear cubes** button, whose taps are meant to stay off the spawner. It also has **Fixed**, **Floating** and **Following** buttons that set the joystick mode of the move area at runtime, with the selected mode highlighted, and a "Joystick: <mode>" line in its status text.
