using System;
using System.Collections.Generic;
using AngryKoala.Services;
using UnityEngine;

namespace AngryKoala.Input
{
    public sealed class InputService : BaseService<IInputService>, IInputService
    {
        [SerializeField]
        [Tooltip("Optional. Empty means the default thresholds.")]
        private GestureSettings _gestureSettings;

        [SerializeField]
        [Tooltip("The areas of the screen, in order. Where two overlap, the earlier one gets the finger.")]
        private List<InputArea> _areas = new();

        public IInputSurface Screen => null;

        public bool IsBlocked => false;

        public event Action OnBackPressed;

        public IInputSurface GetArea(InputArea area)
        {
            return null;
        }

        public void Block(object source)
        {
        }

        public void Unblock(object source)
        {
        }

        public void AddPointerFilter(IPointerFilter pointerFilter)
        {
        }

        public void RemovePointerFilter(IPointerFilter pointerFilter)
        {
        }

        public void GetData(List<InputSurfaceData> results)
        {
        }
    }
}
