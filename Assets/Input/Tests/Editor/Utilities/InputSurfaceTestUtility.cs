using UnityEngine;

namespace AngryKoala.Input.Tests
{
    internal static class InputSurfaceTestUtility
    {
        public const float PixelsPerCentimeter = 100f;

        public static readonly Rect PixelRect = new(0f, 0f, 1000f, 2000f);

        public static readonly Vector2 Start = new(500f, 1000f);

        public static GestureRules CreateRules(float tapMaximumDuration = 0.3f, float tapMaximumDistance = 0.25f,
            float holdDuration = 0.5f, float dragStartDistance = 0.25f, float swipeDistance = 1f,
            float swipeMaximumDuration = 0f, SwipeTiming swipeTiming = SwipeTiming.WhenCrossed)
        {
            return new GestureRules(tapMaximumDuration, tapMaximumDistance, holdDuration, dragStartDistance,
                swipeDistance, swipeMaximumDuration, swipeTiming);
        }

        public static InputSurface CreateSurface(GestureRules rules)
        {
            var surface = new InputSurface("Screen", null);
            surface.SetContext(rules, PixelsPerCentimeter, PixelRect);

            return surface;
        }

        public static Vector2 Offset(float x, float y)
        {
            return Start + new Vector2(x, y);
        }

        public static void Frame(InputSurface surface, int fingerId, Vector2 position, double time)
        {
            surface.UpdateTouch(fingerId, position, time);
            surface.CompleteFrame(time);
        }
    }
}
