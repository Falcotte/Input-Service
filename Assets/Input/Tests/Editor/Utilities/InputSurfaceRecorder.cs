using System.Collections.Generic;

namespace AngryKoala.Input.Tests
{
    internal sealed class InputSurfaceRecorder
    {
        public const string TouchBegan = "TouchBegan";
        public const string TouchEnded = "TouchEnded";
        public const string Tap = "Tap";
        public const string Hold = "Hold";
        public const string DragBegan = "DragBegan";
        public const string Drag = "Drag";
        public const string DragEnded = "DragEnded";
        public const string Swipe = "Swipe";
        public const string Pinch = "Pinch";

        public List<string> Events { get; } = new();

        public List<TouchData> TouchesBegan { get; } = new();

        public List<TouchData> TouchesEnded { get; } = new();

        public List<TouchData> Taps { get; } = new();

        public List<TouchData> Holds { get; } = new();

        public List<TouchData> DragsBegan { get; } = new();

        public List<TouchData> Drags { get; } = new();

        public List<TouchData> DragsEnded { get; } = new();

        public List<SwipeData> Swipes { get; } = new();

        public List<PinchData> Pinches { get; } = new();

        public InputSurfaceRecorder(InputSurface surface)
        {
            surface.OnTouchBegan += HandleTouchBegan;
            surface.OnTouchEnded += HandleTouchEnded;
            surface.OnTap += HandleTap;
            surface.OnHold += HandleHold;
            surface.OnDragBegan += HandleDragBegan;
            surface.OnDrag += HandleDrag;
            surface.OnDragEnded += HandleDragEnded;
            surface.OnSwipe += HandleSwipe;
            surface.OnPinch += HandlePinch;
        }

        public void Clear()
        {
            Events.Clear();
            TouchesBegan.Clear();
            TouchesEnded.Clear();
            Taps.Clear();
            Holds.Clear();
            DragsBegan.Clear();
            Drags.Clear();
            DragsEnded.Clear();
            Swipes.Clear();
            Pinches.Clear();
        }

        private void HandleTouchBegan(TouchData touch)
        {
            Events.Add(TouchBegan);
            TouchesBegan.Add(touch);
        }

        private void HandleTouchEnded(TouchData touch)
        {
            Events.Add(TouchEnded);
            TouchesEnded.Add(touch);
        }

        private void HandleTap(TouchData touch)
        {
            Events.Add(Tap);
            Taps.Add(touch);
        }

        private void HandleHold(TouchData touch)
        {
            Events.Add(Hold);
            Holds.Add(touch);
        }

        private void HandleDragBegan(TouchData touch)
        {
            Events.Add(DragBegan);
            DragsBegan.Add(touch);
        }

        private void HandleDrag(TouchData touch)
        {
            Events.Add(Drag);
            Drags.Add(touch);
        }

        private void HandleDragEnded(TouchData touch)
        {
            Events.Add(DragEnded);
            DragsEnded.Add(touch);
        }

        private void HandleSwipe(SwipeData swipe)
        {
            Events.Add(Swipe);
            Swipes.Add(swipe);
        }

        private void HandlePinch(PinchData pinch)
        {
            Events.Add(Pinch);
            Pinches.Add(pinch);
        }
    }
}
