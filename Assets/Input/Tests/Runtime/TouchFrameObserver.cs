using AngryKoala.Services;
using UnityEngine;

namespace AngryKoala.Input.Tests
{
    /// <summary>
    /// A default-order script that polls the screen surface in its own Update and notes the first frame it sees a touch.
    /// </summary>
    public sealed class TouchFrameObserver : MonoBehaviour
    {
        public int FirstTouchingFrame { get; private set; } = -1;

        private IInputService _inputService;

        private void Start()
        {
            _inputService = ServiceLocator.Get<IInputService>();
        }

        private void Update()
        {
            if (FirstTouchingFrame >= 0 || _inputService == null)
            {
                return;
            }

            if (_inputService.Screen.IsTouching)
            {
                FirstTouchingFrame = Time.frameCount;
            }
        }
    }
}
