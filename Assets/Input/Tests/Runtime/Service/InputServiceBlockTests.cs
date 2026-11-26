using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace AngryKoala.Input.Tests
{
    public sealed class InputServiceBlockTests : InputServiceTestBase
    {
        [UnityTest]
        public IEnumerator IsBlocked_Initially_IsFalse()
        {
            Assert.That(Service.IsBlocked, Is.False);

            yield return null;
        }

        [UnityTest]
        public IEnumerator Block_WithASource_IsBlockedUntilTheSameSourceUnblocks()
        {
            var source = new object();

            Service.Block(source);
            Assert.That(Service.IsBlocked, Is.True);

            Service.Unblock(source);
            Assert.That(Service.IsBlocked, Is.False);

            yield return null;
        }

        [UnityTest]
        public IEnumerator Block_TwoSources_BothHaveToUnblock()
        {
            var firstSource = new object();
            var secondSource = new object();

            Service.Block(firstSource);
            Service.Block(secondSource);

            Service.Unblock(firstSource);
            Assert.That(Service.IsBlocked, Is.True);

            Service.Unblock(secondSource);
            Assert.That(Service.IsBlocked, Is.False);

            yield return null;
        }

        [UnityTest]
        public IEnumerator Block_SameSourceTwice_CountsOnce()
        {
            var source = new object();

            Service.Block(source);
            Service.Block(source);
            Service.Unblock(source);

            Assert.That(Service.IsBlocked, Is.False);

            yield return null;
        }

        [UnityTest]
        public IEnumerator Unblock_UnknownSource_DoesNothing()
        {
            var source = new object();
            Service.Block(source);

            Assert.DoesNotThrow(() => Service.Unblock(new object()));
            Assert.That(Service.IsBlocked, Is.True);

            yield return null;
        }

        [UnityTest]
        public IEnumerator Block_Null_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Service.Block(null));

            yield return null;
        }

        [UnityTest]
        public IEnumerator Unblock_Null_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Service.Unblock(null));

            yield return null;
        }

        [UnityTest]
        public IEnumerator Block_WhileTouching_CancelsTheTouch()
        {
            var recorder = Record(ScreenSurface);

            Touch.Begin(FirstFingerId, ScreenCenter);
            yield return null;

            Service.Block(this);
            yield return null;

            Assert.That(recorder.TouchEnded.Count, Is.EqualTo(1));
            Assert.That(recorder.TouchEnded[0].WasCanceled, Is.True);
            Assert.That(ScreenSurface.TouchCount, Is.EqualTo(0));
            Assert.That(recorder.Taps.Count, Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator Begin_WhileBlocked_IsNeverGivenToASurface()
        {
            var recorder = Record(ScreenSurface);
            Service.Block(this);

            Touch.Begin(FirstFingerId, ScreenCenter);
            yield return null;

            Assert.That(recorder.TouchBegan.Count, Is.EqualTo(0));
            Assert.That(ScreenSurface.TouchCount, Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator Unblock_WhileAFingerThatWentDownBlockedIsStillDown_DoesNotGiveItToASurface()
        {
            var recorder = Record(ScreenSurface);
            Service.Block(this);

            Touch.Begin(FirstFingerId, ScreenCenter);
            yield return null;

            Service.Unblock(this);
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
        public IEnumerator Tap_AfterUnblock_Works()
        {
            var recorder = Record(ScreenSurface);
            Service.Block(this);
            Service.Unblock(this);

            yield return Touch.Tap(ScreenCenter);

            Assert.That(recorder.TouchBegan.Count, Is.EqualTo(1));
            Assert.That(recorder.Taps.Count, Is.EqualTo(1));
        }

        [UnityTest]
        public IEnumerator Tap_WhileBlocked_IsNotRaised()
        {
            var recorder = Record(ScreenSurface);
            Service.Block(this);

            yield return Touch.Tap(ScreenCenter);

            Assert.That(recorder.Taps.Count, Is.EqualTo(0));
        }

        [UnityTest]
        public IEnumerator Block_SourceThatIsADestroyedObject_NoLongerBlocks()
        {
            var sourceObject = CreateObject("Block Source");
            Service.Block(sourceObject);
            Assert.That(Service.IsBlocked, Is.True);

            Object.Destroy(sourceObject);
            yield return null;

            Assert.That(Service.IsBlocked, Is.False);
        }
    }
}
