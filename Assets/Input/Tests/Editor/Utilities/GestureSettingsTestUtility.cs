using System;
using UnityEngine;

namespace AngryKoala.Input.Tests
{
    internal static class GestureSettingsTestUtility
    {
        [Serializable]
        private sealed class GestureSettingsFields
        {
            [SerializeField]
            private float _tapMaximumDuration;

            [SerializeField]
            private float _tapMaximumDistance;

            [SerializeField]
            private float _holdDuration;

            [SerializeField]
            private float _dragStartDistance;

            [SerializeField]
            private float _swipeDistance;

            [SerializeField]
            private float _swipeMaximumDuration;

            [SerializeField]
            private int _swipeTiming;

            [SerializeField]
            private float _fallbackDpi;

            public GestureSettingsFields(float tapMaximumDuration, float tapMaximumDistance, float holdDuration,
                float dragStartDistance, float swipeDistance, float swipeMaximumDuration, SwipeTiming swipeTiming,
                float fallbackDpi)
            {
                _tapMaximumDuration = tapMaximumDuration;
                _tapMaximumDistance = tapMaximumDistance;
                _holdDuration = holdDuration;
                _dragStartDistance = dragStartDistance;
                _swipeDistance = swipeDistance;
                _swipeMaximumDuration = swipeMaximumDuration;
                _swipeTiming = (int)swipeTiming;
                _fallbackDpi = fallbackDpi;
            }
        }

        public static GestureSettings CreateSettings(float tapMaximumDuration = 0.3f, float tapMaximumDistance = 0.25f,
            float holdDuration = 0.5f, float dragStartDistance = 0.25f, float swipeDistance = 1f,
            float swipeMaximumDuration = 0f, SwipeTiming swipeTiming = SwipeTiming.WhenCrossed,
            float fallbackDpi = 160f)
        {
            var settings = ScriptableObject.CreateInstance<GestureSettings>();

            var fields = new GestureSettingsFields(tapMaximumDuration, tapMaximumDistance, holdDuration,
                dragStartDistance, swipeDistance, swipeMaximumDuration, swipeTiming, fallbackDpi);

            JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(fields), settings);

            return settings;
        }
    }
}
