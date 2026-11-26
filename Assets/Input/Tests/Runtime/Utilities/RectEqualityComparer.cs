using System.Collections.Generic;
using UnityEngine;

namespace AngryKoala.Input.Tests
{
    public sealed class RectEqualityComparer : IEqualityComparer<Rect>
    {
        private readonly float _tolerance;

        public RectEqualityComparer(float tolerance)
        {
            _tolerance = tolerance;
        }

        public bool Equals(Rect expected, Rect actual)
        {
            return Mathf.Abs(expected.x - actual.x) <= _tolerance
                && Mathf.Abs(expected.y - actual.y) <= _tolerance
                && Mathf.Abs(expected.width - actual.width) <= _tolerance
                && Mathf.Abs(expected.height - actual.height) <= _tolerance;
        }

        public int GetHashCode(Rect rect)
        {
            return 0;
        }
    }
}
