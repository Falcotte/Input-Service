using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace AngryKoala.Input.Tests
{
    public abstract class InputServiceTestBase
    {
        private readonly List<Object> _objects = new();
        private readonly List<InputDevice> _devices = new();
        private readonly List<SurfaceRecorder> _recorders = new();

        protected InputService Service { get; private set; }

        protected ScriptedTouch Touch { get; private set; }

        protected IInputSurface ScreenSurface => Service.Screen;

        protected static Vector2 ScreenCenter => ScreenPoint(0.5f, 0.5f);

        /// <summary>
        /// The left half of the screen.
        /// </summary>
        protected static Rect FilterArea => new(0f, 0f, Screen.width * 0.5f, Screen.height);

        protected static Vector2 PointInsideFilterArea => ScreenPoint(0.25f, 0.5f);

        protected static Vector2 PointOutsideFilterArea => ScreenPoint(0.75f, 0.5f);

        protected static float DragLength => Mathf.Min(300f, Mathf.Min(Screen.width, Screen.height) * 0.6f);

        public const string FilterFailureMessage = "Test filter failure";
        public const string HandlerFailureMessage = "Test handler failure";

        protected const float DragSeconds = 0.3f;
        protected const float HoldSeconds = 0.7f;
        protected const float PinchSeconds = 0.4f;
        protected const int FirstFingerId = 1;
        protected const int SecondFingerId = 2;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            Service = CreateObject("InputService").AddComponent<InputService>();
            Touch = ScriptedTouch.Create();

            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            foreach (var recorder in _recorders)
            {
                recorder.Dispose();
            }

            _recorders.Clear();

            Touch.Dispose();

            foreach (var device in _devices)
            {
                if (device.added)
                {
                    InputSystem.RemoveDevice(device);
                }
            }

            _devices.Clear();

            foreach (var unityObject in _objects)
            {
                if (unityObject != null)
                {
                    Object.Destroy(unityObject);
                }
            }

            _objects.Clear();

            yield return null;
        }

        protected static Vector2 ScreenPoint(float x, float y)
        {
            return new Vector2(Screen.width * x, Screen.height * y);
        }

        protected static IEnumerator WaitFrames(int frameCount)
        {
            for (var frame = 0; frame < frameCount; frame++)
            {
                yield return null;
            }
        }

        protected GameObject CreateObject(string objectName)
        {
            var gameObject = new GameObject(objectName);
            _objects.Add(gameObject);

            return gameObject;
        }

        protected TDevice AddDevice<TDevice>() where TDevice : InputDevice
        {
            var device = InputSystem.AddDevice<TDevice>();
            _devices.Add(device);

            return device;
        }

        protected SurfaceRecorder Record(IInputSurface surface)
        {
            var recorder = new SurfaceRecorder(surface);
            _recorders.Add(recorder);

            return recorder;
        }

        protected IEnumerator DragAcrossCenter(Vector2 direction, float seconds)
        {
            var halfVector = direction.normalized * (DragLength * 0.5f);

            return Touch.Drag(ScreenCenter - halfVector, ScreenCenter + halfVector, seconds);
        }
    }
}
