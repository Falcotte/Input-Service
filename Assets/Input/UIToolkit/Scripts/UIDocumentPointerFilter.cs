using UnityEngine;
using UnityEngine.UIElements;

namespace AngryKoala.Input
{
    /// <summary>
    /// Makes the input service leave a finger to the UI Toolkit document when it lands on an element that can be picked.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class UIDocumentPointerFilter : MonoBehaviour, IPointerFilter
    {
        public bool TakesPointer(Vector2 screenPosition) => false;
    }
}
