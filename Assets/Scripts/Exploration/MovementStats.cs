using System;
using UnityEngine;

namespace MicrojamEvolution.Exploration
{
    /// <summary>
    /// Numbers that drive creature movement. Fixed for now; later they will be computed from the genome
    /// (flagella raise the speed and so on), so everything that moves a creature should read them from here.
    /// </summary>
    [Serializable]
    public struct MovementStats
    {
        [Tooltip("World units per second, the same in every direction.")]
        [SerializeField]
        private float moveSpeed;

        [Tooltip("Degrees per second.")]
        [SerializeField]
        private float rotationSpeed;

        public MovementStats(float moveSpeed, float rotationSpeed)
        {
            this.moveSpeed = moveSpeed;
            this.rotationSpeed = rotationSpeed;
        }

        public static MovementStats Default => new MovementStats(1f, 90f);

        public float MoveSpeed => moveSpeed;
        public float RotationSpeed => rotationSpeed;
    }
}
