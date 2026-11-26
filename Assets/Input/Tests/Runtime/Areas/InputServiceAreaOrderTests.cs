using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using AngryKoala.Services;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace AngryKoala.Input.Tests
{
    public sealed class InputServiceAreaOrderTests : InputServiceAreaTestBase
    {
        private InputArea _requestedArea;

        private static readonly Vector2 _firstAreaMaximum = new(0.6f, 1f);
        private static readonly Vector2 _secondAreaMinimum = new(0.4f, 0f);

        private static Vector2 PointInTheOverlap => ScreenPoint(0.5f, 0.5f);

        [SetUp]
        public void ResetRequestedArea()
        {
            _requestedArea = null;
        }

        [UnityTest]
        public IEnumerator Awake_WhenARegistrationCallbackAsksForALaterArea_KeepsTheConfiguredOrder()
        {
            Object.DestroyImmediate(Service.gameObject);

            var firstArea = CreateArea("First Area", Vector2.zero, _firstAreaMaximum);
            var secondArea = CreateArea("Second Area", _secondAreaMinimum, Vector2.one);

            _requestedArea = secondArea;
            ServiceLocator.WhenRegistered<IInputService>(RequestArea);

            var service = CreateServiceWithAreas(firstArea, secondArea);
            var firstSurface = service.GetArea(firstArea);
            var secondSurface = service.GetArea(secondArea);
            yield return null;

            Touch.Begin(FirstFingerId, PointInTheOverlap);
            yield return null;

            Assert.That(firstSurface.TouchCount, Is.EqualTo(1));
            Assert.That(secondSurface.TouchCount, Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator Awake_WhenARegistrationCallbackAsksForALaterArea_TheDataListsTheAreasInTheConfiguredOrder()
        {
            Object.DestroyImmediate(Service.gameObject);

            var firstArea = CreateArea("First Area", Vector2.zero, _firstAreaMaximum);
            var secondArea = CreateArea("Second Area", _secondAreaMinimum, Vector2.one);

            _requestedArea = secondArea;
            ServiceLocator.WhenRegistered<IInputService>(RequestArea);

            var service = CreateServiceWithAreas(firstArea, secondArea);
            var results = new List<InputSurfaceData>();
            yield return null;

            service.GetData(results);

            Assert.That(results, Has.Count.EqualTo(3));
            Assert.That(results[1].Name, Is.EqualTo("First Area"));
            Assert.That(results[2].Name, Is.EqualTo("Second Area"));
        }

        [UnityTest]
        public IEnumerator GetArea_OfAnAreaInTheList_NeverChangesTheOrder()
        {
            Object.DestroyImmediate(Service.gameObject);

            var firstArea = CreateArea("First Area", Vector2.zero, _firstAreaMaximum);
            var secondArea = CreateArea("Second Area", _secondAreaMinimum, Vector2.one);

            var service = CreateServiceWithAreas(firstArea, secondArea);
            var secondSurface = service.GetArea(secondArea);
            var firstSurface = service.GetArea(firstArea);
            yield return null;

            Touch.Begin(FirstFingerId, PointInTheOverlap);
            yield return null;

            Assert.That(firstSurface.TouchCount, Is.EqualTo(1));
            Assert.That(secondSurface.TouchCount, Is.EqualTo(0));
        }

        private void RequestArea(IInputService service)
        {
            service.GetArea(_requestedArea);
        }

        private InputService CreateServiceWithAreas(params InputArea[] areas)
        {
            var gameObject = CreateObject("Configured InputService");
            gameObject.SetActive(false);

            var service = gameObject.AddComponent<InputService>();
            var areasField = typeof(InputService).GetField("_areas", BindingFlags.Instance | BindingFlags.NonPublic);

            areasField.SetValue(service, new List<InputArea>(areas));
            gameObject.SetActive(true);

            return service;
        }
    }
}
