using System.Collections;
using System.Text.RegularExpressions;
using AngryKoala.Services;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace AngryKoala.Input.Tests
{
    public sealed class InputServiceLifetimeTests : InputServiceTestBase
    {
        [UnityTest]
        public IEnumerator Awake_RegistersTheServiceAsIInputService()
        {
            var isRegistered = ServiceLocator.TryGet<IInputService>(out var registered);

            Assert.That(isRegistered, Is.True);
            Assert.That(registered, Is.SameAs(Service));

            yield return null;
        }

        [UnityTest]
        public IEnumerator Destroy_DeregistersTheService()
        {
            Object.Destroy(Service.gameObject);
            yield return null;

            Assert.That(ServiceLocator.IsRegistered<IInputService>(), Is.False);
        }

        [UnityTest]
        public IEnumerator Screen_RightAfterAddComponent_IsNotNull()
        {
            Object.DestroyImmediate(Service.gameObject);

            var service = CreateObject("Fresh InputService").AddComponent<InputService>();

            Assert.That(service.Screen, Is.Not.Null);

            yield return null;
        }

        [UnityTest]
        public IEnumerator Awake_WhenAnotherServiceIsRegistered_LogsAnErrorAndDisablesItself()
        {
            LogAssert.Expect(LogType.Error, new Regex(nameof(InputService)));

            var second = CreateObject("Second InputService").AddComponent<InputService>();
            yield return null;

            Assert.That(second.enabled, Is.False);
        }

        [UnityTest]
        public IEnumerator Awake_WhenAnotherServiceIsRegistered_TheFirstKeepsWorking()
        {
            LogAssert.Expect(LogType.Error, new Regex(nameof(InputService)));

            CreateObject("Second InputService").AddComponent<InputService>();
            var recorder = Record(ScreenSurface);

            yield return Touch.Tap(ScreenCenter);

            Assert.That(ServiceLocator.Get<IInputService>(), Is.SameAs(Service));
            Assert.That(recorder.Taps.Count, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator Disable_WhileTouching_CancelsTheTouch()
        {
            var recorder = Record(ScreenSurface);

            Touch.Begin(FirstFingerId, ScreenCenter);
            yield return null;

            Service.enabled = false;
            yield return null;

            Assert.That(recorder.TouchEnded.Count, Is.EqualTo(1));
            Assert.That(recorder.TouchEnded[0].WasCanceled, Is.True);
            Assert.That(ScreenSurface.TouchCount, Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator Enable_WhileAFingerIsAlreadyDown_DoesNotGiveItToASurface()
        {
            var recorder = Record(ScreenSurface);
            Service.enabled = false;

            Touch.Begin(FirstFingerId, ScreenCenter);
            yield return null;

            Service.enabled = true;
            yield return null;

            Touch.Move(FirstFingerId, ScreenPoint(0.6f, 0.5f));
            yield return null;

            Assert.That(recorder.TouchBegan.Count, Is.EqualTo(0));
            Assert.That(ScreenSurface.TouchCount, Is.EqualTo(0));

            Touch.End(FirstFingerId);
            yield return WaitFrames(2);

            Assert.That(recorder.TouchBegan.Count, Is.EqualTo(0));
            Assert.That(recorder.Taps.Count, Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator Enable_TheNextFingerAfterwards_IsGivenToTheScreenSurface()
        {
            var recorder = Record(ScreenSurface);
            Service.enabled = false;
            yield return null;

            Service.enabled = true;
            yield return null;

            yield return Touch.Tap(ScreenCenter);

            Assert.That(recorder.TouchBegan.Count, Is.EqualTo(1));
            Assert.That(recorder.Taps.Count, Is.EqualTo(1));
        }
    }
}
