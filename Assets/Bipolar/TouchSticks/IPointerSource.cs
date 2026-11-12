using System.Collections.Generic;
using UnityEngine;

namespace Bipolar.TouchSticks
{
    /// <summary>One finger (or the mouse emulating a finger) in a single frame.</summary>
    public readonly struct Pointer
    {
        public int Id { get; }
        public Vector2 Position { get; }
        public bool Began { get; }
        public bool Ended { get; }

        public Pointer(int id, Vector2 position, bool began, bool ended)
        {
            Id = id;
            Position = position;
            Began = began;
            Ended = ended;
        }
    }

    /// <summary>Backend that reads raw touch input (legacy Input Manager, Input System, ...).</summary>
    public interface IPointerSource
    {
        /// <summary>Appends all pointers active in the current frame to <paramref name="results"/>.</summary>
        void Collect(List<Pointer> results);
    }
}
