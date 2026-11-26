namespace AngryKoala.Input
{
    /// <summary>
    /// The thresholds a surface recognises gestures with. Distances are in centimetres, durations in seconds.
    /// </summary>
    internal readonly struct GestureRules
    {
        public float TapMaximumDuration { get; }

        public float TapMaximumDistance { get; }

        public float HoldDuration { get; }

        public float DragStartDistance { get; }

        public float SwipeDistance { get; }

        /// <summary>
        /// Zero means no limit.
        /// </summary>
        public float SwipeMaximumDuration { get; }

        public SwipeTiming SwipeTiming { get; }

        public static GestureRules Default => new(DefaultTapMaximumDuration, DefaultTapMaximumDistance,
            DefaultHoldDuration, DefaultDragStartDistance, DefaultSwipeDistance, DefaultSwipeMaximumDuration,
            SwipeTiming.WhenCrossed);

        public const float DefaultTapMaximumDuration = 0.3f;
        public const float DefaultTapMaximumDistance = 0.25f;
        public const float DefaultHoldDuration = 0.5f;
        public const float DefaultDragStartDistance = 0.25f;
        public const float DefaultSwipeDistance = 1f;
        public const float DefaultSwipeMaximumDuration = 0f;

        public GestureRules(float tapMaximumDuration, float tapMaximumDistance, float holdDuration,
            float dragStartDistance, float swipeDistance, float swipeMaximumDuration, SwipeTiming swipeTiming)
        {
            TapMaximumDuration = tapMaximumDuration;
            TapMaximumDistance = tapMaximumDistance;
            HoldDuration = holdDuration;
            DragStartDistance = dragStartDistance;
            SwipeDistance = swipeDistance;
            SwipeMaximumDuration = swipeMaximumDuration;
            SwipeTiming = swipeTiming;
        }
    }
}
