using System;
using System.Collections.Generic;
using UnityEngine;

namespace AngryKoala.Input.Tests
{
    public sealed class TestPointerFilter : IPointerFilter
    {
        private readonly Rect _area;
        private readonly List<Vector2> _askedPositions = new();

        public int CallCount => _askedPositions.Count;

        public IReadOnlyList<Vector2> AskedPositions => _askedPositions;

        public bool ShouldThrow { get; set; }

        public TestPointerFilter(Rect area)
        {
            _area = area;
        }

        public bool TakesPointer(Vector2 screenPosition)
        {
            _askedPositions.Add(screenPosition);

            if (ShouldThrow)
            {
                throw new InvalidOperationException(InputServiceTestBase.FilterFailureMessage);
            }

            return _area.Contains(screenPosition);
        }
    }
}
