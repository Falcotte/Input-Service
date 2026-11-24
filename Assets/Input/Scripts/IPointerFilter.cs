using UnityEngine;

namespace AngryKoala.Input
{
    /// <summary>
    /// Something that can own a finger before the game does, such as a UI.
    /// </summary>
    public interface IPointerFilter
    {
        /// <summary>
        /// Whether a finger going down at this screen position, in pixels from the bottom left, belongs to the filter.
        /// </summary>
        bool TakesPointer(Vector2 screenPosition);
    }
}
