using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace AngryKoala.Input.Tests
{
    public abstract class UIToolkitInputTestBase
    {
        private readonly List<Object> _objects = new();
        private readonly List<SurfaceCounter> _counters = new();

        protected InputService Service { get; private set; }

        protected ScriptedTouch Touch { get; private set; }

        protected PanelSettings PanelSettings { get; private set; }

        protected UIDocument Document { get; private set; }

        protected VisualElement Root => Document.rootVisualElement;

        /// <summary>
        /// Screen pixels per panel unit. Read it once the document has had a frame to build its panel.
        /// </summary>
        protected float PanelScale
        {
            get
            {
                var panelSize = RuntimePanelUtils.ScreenToPanel(Root.panel, new Vector2(Screen.width, Screen.height));

                return Screen.width / panelSize.x;
            }
        }

        protected const float SettleSeconds = 0.2f;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            Touch = ScriptedTouch.Create();

            yield return null;
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            foreach (var counter in _counters)
            {
                counter.Dispose();
            }

            _counters.Clear();

            Touch.Dispose();

            foreach (var unityObject in _objects)
            {
                if (unityObject != null)
                {
                    Object.Destroy(unityObject);
                }
            }

            _objects.Clear();

            Service = null;
            PanelSettings = null;
            Document = null;

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

        protected TObject Track<TObject>(TObject unityObject) where TObject : Object
        {
            _objects.Add(unityObject);

            return unityObject;
        }

        protected void CreateService()
        {
            Service = CreateObject("InputService").AddComponent<InputService>();
        }

        /// <summary>
        /// The panel is built in the next frame, so wait a frame before reading the root.
        /// </summary>
        protected void CreateDocument(float panelScale = 1f)
        {
            PanelSettings = Track(ScriptableObject.CreateInstance<PanelSettings>());

            if (!Mathf.Approximately(panelScale, 1f))
            {
                PanelSettings.scaleMode = PanelScaleMode.ConstantPixelSize;
                PanelSettings.scale = panelScale;
            }

            var documentObject = CreateObject("Document");
            Document = documentObject.AddComponent<UIDocument>();
            Document.panelSettings = PanelSettings;
        }

        protected SurfaceCounter Count(IInputSurface surface)
        {
            var counter = new SurfaceCounter(surface);
            _counters.Add(counter);

            return counter;
        }

        /// <summary>
        /// A position in pixels from the bottom left, to a position in panel units from the top left.
        /// </summary>
        protected Vector2 ScreenToPanel(Vector2 screenPosition)
        {
            return RuntimePanelUtils.ScreenToPanel(Root.panel, new Vector2(screenPosition.x, Screen.height - screenPosition.y));
        }

        protected Vector2 PanelToScreen(Vector2 panelPosition)
        {
            var panelScale = PanelScale;

            return new Vector2(panelPosition.x * panelScale, Screen.height - panelPosition.y * panelScale);
        }

        protected Vector2 ScreenPositionOf(VisualElement element)
        {
            return PanelToScreen(element.worldBound.center);
        }
    }
}
