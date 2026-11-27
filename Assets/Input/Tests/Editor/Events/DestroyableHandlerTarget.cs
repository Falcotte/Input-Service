using UnityEngine;

namespace AngryKoala.Input.Tests
{
    internal sealed class DestroyableHandlerTarget : ScriptableObject
    {
        public int CallCount { get; private set; }

        public void HandleTouch(TouchData touch)
        {
            CallCount++;
        }

        public void HandleEvent()
        {
            CallCount++;
        }
    }
}
