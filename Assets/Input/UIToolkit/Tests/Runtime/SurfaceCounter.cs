using System;

namespace AngryKoala.Input.Tests
{
    public sealed class SurfaceCounter : IDisposable
    {
        private readonly IInputSurface _surface;

        public int TouchBeganCount { get; private set; }

        public int TouchEndedCount { get; private set; }

        public int TapCount { get; private set; }

        public SurfaceCounter(IInputSurface surface)
        {
            _surface = surface ?? throw new ArgumentNullException(nameof(surface));

            _surface.OnTouchBegan += HandleTouchBegan;
            _surface.OnTouchEnded += HandleTouchEnded;
            _surface.OnTap += HandleTap;
        }

        public void Dispose()
        {
            _surface.OnTouchBegan -= HandleTouchBegan;
            _surface.OnTouchEnded -= HandleTouchEnded;
            _surface.OnTap -= HandleTap;
        }

        private void HandleTouchBegan(TouchData touch) => TouchBeganCount++;

        private void HandleTouchEnded(TouchData touch) => TouchEndedCount++;

        private void HandleTap(TouchData touch) => TapCount++;
    }
}
