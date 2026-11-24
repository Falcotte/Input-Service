using UnityEngine;

namespace AngryKoala.Input
{
    [CreateAssetMenu(fileName = "GestureSettings", menuName = "Angry Koala/Input/Gesture Settings")]
    public sealed class GestureSettings : ScriptableObject
    {
        [SerializeField]
        [Tooltip("A touch that lifts within this many seconds can be a tap.")]
        private float _tapMaximumDuration = GestureRules.DefaultTapMaximumDuration;
        public float TapMaximumDuration => _tapMaximumDuration;

        [SerializeField]
        [Tooltip("A touch that strays further than this, in centimetres, is neither a tap nor a hold.")]
        private float _tapMaximumDistance = GestureRules.DefaultTapMaximumDistance;
        public float TapMaximumDistance => _tapMaximumDistance;

        [SerializeField]
        [Tooltip("Seconds a finger has to stay in place to count as a hold.")]
        private float _holdDuration = GestureRules.DefaultHoldDuration;
        public float HoldDuration => _holdDuration;

        [SerializeField]
        [Tooltip("A drag begins when the finger has travelled this far, in centimetres.")]
        private float _dragStartDistance = GestureRules.DefaultDragStartDistance;
        public float DragStartDistance => _dragStartDistance;

        [SerializeField]
        [Tooltip("A swipe needs the finger to travel this far, in centimetres.")]
        private float _swipeDistance = GestureRules.DefaultSwipeDistance;
        public float SwipeDistance => _swipeDistance;

        [SerializeField]
        [Tooltip("A swipe has to be done within this many seconds. 0 means no limit.")]
        private float _swipeMaximumDuration = GestureRules.DefaultSwipeMaximumDuration;
        public float SwipeMaximumDuration => _swipeMaximumDuration;

        [SerializeField]
        [Tooltip("When Crossed raises the swipe while the finger is still down. On Release raises it when the finger lifts.")]
        private SwipeTiming _swipeTiming = SwipeTiming.WhenCrossed;
        public SwipeTiming SwipeTiming => _swipeTiming;

        [SerializeField]
        [Tooltip("The DPI to assume when the screen reports none.")]
        private float _fallbackDpi = DefaultFallbackDpi;
        public float FallbackDpi => _fallbackDpi;

        internal const float DefaultFallbackDpi = 160f;
        internal const float MinimumHoldDuration = 0.05f;
        internal const float MinimumSwipeDistance = 0.01f;
        internal const float MinimumFallbackDpi = 30f;

        private void OnValidate()
        {
            _tapMaximumDuration = ClampTapMaximumDuration(_tapMaximumDuration);
            _tapMaximumDistance = ClampTapMaximumDistance(_tapMaximumDistance);
            _holdDuration = ClampHoldDuration(_holdDuration);
            _dragStartDistance = ClampDragStartDistance(_dragStartDistance);
            _swipeDistance = ClampSwipeDistance(_swipeDistance);
            _swipeMaximumDuration = ClampSwipeMaximumDuration(_swipeMaximumDuration);
            _fallbackDpi = ClampFallbackDpi(_fallbackDpi);
        }

        internal GestureRules ToRules()
        {
            return new GestureRules(ClampTapMaximumDuration(_tapMaximumDuration),
                ClampTapMaximumDistance(_tapMaximumDistance), ClampHoldDuration(_holdDuration),
                ClampDragStartDistance(_dragStartDistance), ClampSwipeDistance(_swipeDistance),
                ClampSwipeMaximumDuration(_swipeMaximumDuration), _swipeTiming);
        }

        internal static float ResolveDpi(float screenDpi, GestureSettings gestureSettings)
        {
            if (screenDpi > 0f)
            {
                return screenDpi;
            }

            if (gestureSettings != null)
            {
                return ClampFallbackDpi(gestureSettings.FallbackDpi);
            }

            return DefaultFallbackDpi;
        }

        private static float ClampTapMaximumDuration(float value) => Mathf.Max(0f, value);

        private static float ClampTapMaximumDistance(float value) => Mathf.Max(0f, value);

        private static float ClampHoldDuration(float value) => Mathf.Max(MinimumHoldDuration, value);

        private static float ClampDragStartDistance(float value) => Mathf.Max(0f, value);

        private static float ClampSwipeDistance(float value) => Mathf.Max(MinimumSwipeDistance, value);

        private static float ClampSwipeMaximumDuration(float value) => Mathf.Max(0f, value);

        private static float ClampFallbackDpi(float value)
        {
            if (float.IsNaN(value) || value <= 0f)
            {
                return DefaultFallbackDpi;
            }

            return Mathf.Max(MinimumFallbackDpi, value);
        }
    }
}
