using System;
using System.Collections.Generic;
using AngryKoala.Services;

namespace AngryKoala.Input
{
    public interface IInputService : IService
    {
        /// <summary>
        /// The surface for every finger that went down on no UI and in no area.
        /// </summary>
        IInputSurface Screen { get; }

        bool IsBlocked { get; }

        event Action OnBackPressed;

        /// <summary>
        /// Returns the surface of an area. The same surface is returned for the same asset every time.
        /// </summary>
        IInputSurface GetArea(InputArea area);

        /// <summary>
        /// Blocks input until the same source calls <see cref="Unblock"/>. Touches in progress are cancelled.
        /// </summary>
        void Block(object source);

        void Unblock(object source);

        /// <summary>
        /// A filter is asked once for every finger that goes down. A finger a filter takes is never given to a surface.
        /// </summary>
        void AddPointerFilter(IPointerFilter pointerFilter);

        void RemovePointerFilter(IPointerFilter pointerFilter);

        void GetData(List<InputSurfaceData> results);
    }
}
