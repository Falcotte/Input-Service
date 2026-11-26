using System;
using System.Collections.Generic;

namespace AngryKoala.Input.Tests
{
    public sealed class SurfaceRecorder : IDisposable
    {
        private readonly IInputSurface _surface;

        public List<TouchData> TouchBegan { get; } = new();

        public List<TouchData> TouchEnded { get; } = new();

        public List<TouchData> Taps { get; } = new();

        public List<TouchData> Holds { get; } = new();

        public List<TouchData> DragBegan { get; } = new();

        public List<TouchData> Drags { get; } = new();

        public List<TouchData> DragEnded { get; } = new();

        public List<SwipeData> Swipes { get; } = new();

        public List<PinchData> Pinches { get; } = new();

        public SurfaceRecorder(IInputSurface surface)
        {
            _surface = surface ?? throw new ArgumentNullException(nameof(surface));

            _surface.OnTouchBegan += HandleTouchBegan;
            _surface.OnTouchEnded += HandleTouchEnded;
            _surface.OnTap += HandleTap;
            _surface.OnHold += HandleHold;
            _surface.OnDragBegan += HandleDragBegan;
            _surface.OnDrag += HandleDrag;
            _surface.OnDragEnded += HandleDragEnded;
            _surface.OnSwipe += HandleSwipe;
            _surface.OnPinch += HandlePinch;
        }

        public void Dispose()
        {
            _surface.OnTouchBegan -= HandleTouchBegan;
            _surface.OnTouchEnded -= HandleTouchEnded;
            _surface.OnTap -= HandleTap;
            _surface.OnHold -= HandleHold;
            _surface.OnDragBegan -= HandleDragBegan;
            _surface.OnDrag -= HandleDrag;
            _surface.OnDragEnded -= HandleDragEnded;
            _surface.OnSwipe -= HandleSwipe;
            _surface.OnPinch -= HandlePinch;
        }

        private void HandleTouchBegan(TouchData touch) => TouchBegan.Add(touch);

        private void HandleTouchEnded(TouchData touch) => TouchEnded.Add(touch);

        private void HandleTap(TouchData touch) => Taps.Add(touch);

        private void HandleHold(TouchData touch) => Holds.Add(touch);

        private void HandleDragBegan(TouchData touch) => DragBegan.Add(touch);

        private void HandleDrag(TouchData touch) => Drags.Add(touch);

        private void HandleDragEnded(TouchData touch) => DragEnded.Add(touch);

        private void HandleSwipe(SwipeData swipe) => Swipes.Add(swipe);

        private void HandlePinch(PinchData pinch) => Pinches.Add(pinch);
    }
}
