using System;
using System.Collections;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;

namespace AngryKoala.Input.Tests
{
    public sealed class InputServiceBackTests : InputServiceTestBase
    {
        private int _backCount;
        private int _otherBackCount;

        [SetUp]
        public void ResetCounts()
        {
            _backCount = 0;
            _otherBackCount = 0;
        }

        [UnityTest]
        public IEnumerator OnBackPressed_EscapePressed_IsRaisedOnce()
        {
            var keyboard = AddDevice<Keyboard>();
            Service.OnBackPressed += CountBack;

            yield return PressAndRelease(keyboard, Key.Escape);

            Assert.That(_backCount, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator OnBackPressed_OtherKeyPressed_IsNotRaised()
        {
            var keyboard = AddDevice<Keyboard>();
            Service.OnBackPressed += CountBack;

            yield return PressAndRelease(keyboard, Key.A);

            Assert.That(_backCount, Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator OnBackPressed_WhileBlocked_IsStillRaised()
        {
            var keyboard = AddDevice<Keyboard>();
            Service.OnBackPressed += CountBack;
            Service.Block(this);

            yield return PressAndRelease(keyboard, Key.Escape);

            Assert.That(_backCount, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator OnBackPressed_WhenAHandlerThrows_IsLoggedAndTheOtherHandlersStillRun()
        {
            var keyboard = AddDevice<Keyboard>();
            Service.OnBackPressed += ThrowOnBack;
            Service.OnBackPressed += CountBack;

            LogAssert.Expect(LogType.Error, new Regex(nameof(IInputService.OnBackPressed)));
            LogAssert.Expect(LogType.Exception, new Regex(HandlerFailureMessage));

            yield return PressAndRelease(keyboard, Key.Escape);

            Assert.That(_backCount, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator OnBackPressed_WithSeveralHandlers_RaisesEachOnce()
        {
            var keyboard = AddDevice<Keyboard>();
            Service.OnBackPressed += CountBack;
            Service.OnBackPressed += CountOtherBack;

            yield return PressAndRelease(keyboard, Key.Escape);

            Assert.That(_backCount, Is.EqualTo(1));
            Assert.That(_otherBackCount, Is.EqualTo(1));
        }

        private static IEnumerator PressAndRelease(Keyboard keyboard, Key key)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(key));
            yield return null;

            InputSystem.QueueStateEvent(keyboard, new KeyboardState());

            yield return WaitFrames(2);
        }

        private void CountBack() => _backCount++;

        private void CountOtherBack() => _otherBackCount++;

        private void ThrowOnBack() => throw new InvalidOperationException(HandlerFailureMessage);
    }
}
