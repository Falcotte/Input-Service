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

        internal GestureRules ToRules()
        {
            return new GestureRules(_tapMaximumDuration, _tapMaximumDistance, _holdDuration, _dragStartDistance,
                _swipeDistance, _swipeMaximumDuration, _swipeTiming);
        }
    }
}
