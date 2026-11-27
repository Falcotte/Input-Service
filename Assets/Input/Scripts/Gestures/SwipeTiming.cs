namespace AngryKoala.Input
{
    public enum SwipeTiming
    {
        /// <summary>
        /// The swipe is raised at the moment the finger has travelled the swipe distance.
        /// </summary>
        WhenCrossed,

        /// <summary>
        /// The swipe is raised when the finger lifts, if it ended at least the swipe distance from where it began.
        /// </summary>
        OnRelease
    }
}
