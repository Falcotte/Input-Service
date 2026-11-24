using UnityEngine;

namespace AngryKoala.Input.Tests
{
    /// <summary>
    /// A filter that takes every pointer and can be destroyed.
    /// </summary>
    public sealed class TestPointerFilterBehaviour : MonoBehaviour, IPointerFilter
    {
        public bool TakesPointer(Vector2 screenPosition)
        {
            return true;
        }
    }
}
