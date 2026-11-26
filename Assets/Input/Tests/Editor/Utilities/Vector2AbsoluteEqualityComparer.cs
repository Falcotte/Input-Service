using System.Collections.Generic;
using UnityEngine;

namespace AngryKoala.Input.Tests
{
    /// <summary>
    /// Compares with a fixed tolerance per axis. Unity's Vector2EqualityComparer takes its tolerance as relative.
    /// </summary>
    public sealed class Vector2AbsoluteEqualityComparer : IEqualityComparer<Vector2>
    {
        private readonly float _tolerance;

        public Vector2AbsoluteEqualityComparer(float tolerance)
        {
            _tolerance = tolerance;
        }

        public bool Equals(Vector2 expected, Vector2 actual)
        {
            return Mathf.Abs(expected.x - actual.x) <= _tolerance
                && Mathf.Abs(expected.y - actual.y) <= _tolerance;
        }

        public int GetHashCode(Vector2 vector)
        {
            return 0;
        }
    }
}
