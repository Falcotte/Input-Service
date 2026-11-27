using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace AngryKoala.Input.Tests
{
    public sealed class InputServiceExecutionOrderTests : InputServiceTestBase
    {
        private int _touchBeganFrame;

        [SetUp]
        public void ResetFrames()
        {
            _touchBeganFrame = -1;
        }

        [Test]
        public void ExecutionOrder_OfTheService_IsBelowTheDefault()
        {
            var attributes = typeof(InputService).GetCustomAttributes(typeof(DefaultExecutionOrder), true);

            Assert.That(attributes.Length, Is.EqualTo(1));
            Assert.That(((DefaultExecutionOrder)attributes[0]).order, Is.LessThan(0));
        }

        [UnityTest]
        public IEnumerator Update_OfAScriptCreatedBeforeTheService_SeesTheTouchInTheFrameItBegan()
        {
            Object.DestroyImmediate(Service.gameObject);

            var observer = CreateObject("Touch Frame Observer").AddComponent<TouchFrameObserver>();
            var service = CreateObject("Late InputService").AddComponent<InputService>();
            service.Screen.OnTouchBegan += HandleTouchBegan;
            yield return null;

            Touch.Begin(FirstFingerId, ScreenCenter);
            yield return WaitFrames(3);

            Assert.That(_touchBeganFrame, Is.GreaterThan(0));
            Assert.That(observer.FirstTouchingFrame, Is.EqualTo(_touchBeganFrame));
        }

        private void HandleTouchBegan(TouchData touch)
        {
            _touchBeganFrame = Time.frameCount;
        }
    }
}
