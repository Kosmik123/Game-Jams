using System;
using UnityEngine;

namespace MicrojamEvolution.Creature
{
    /// <summary>
    /// One block of a creature: grid position, module on it and the side the module works on.
    /// Immutable data only. Runtime state (current HP etc.) lives on the creature's block objects.
    /// </summary>
    [Serializable]
    public struct BlockGene : IEquatable<BlockGene>
    {
        [SerializeField]
        private Vector2Int position;

        [SerializeField]
        private ModuleType module;

        [SerializeField]
        private Direction direction;

        public Vector2Int Position => position;
        public ModuleType Module => module;
        public Direction Direction => direction;

        public BlockGene(Vector2Int position, ModuleType module = ModuleType.None, Direction direction = Direction.Up)
        {
            this.position = position;
            this.module = module;
            this.direction = direction;
        }

        public BlockGene(int x, int y, ModuleType module = ModuleType.None, Direction direction = Direction.Up)
            : this(new Vector2Int(x, y), module, direction)
        { }

        /// <summary>Copy of this block with another module (and the side it works on).</summary>
        public BlockGene WithModule(ModuleType newModule, Direction newDirection = Direction.Up)
        {
            return new BlockGene(position, newModule, newDirection);
        }

        /// <summary>Copy of this block with the module turned to another side.</summary>
        public BlockGene WithDirection(Direction newDirection)
        {
            return new BlockGene(position, module, newDirection);
        }

        public readonly bool Equals(BlockGene other)
        {
            return position == other.position && module == other.module && direction == other.direction;
        }

        public override readonly bool Equals(object obj)
        {
            return obj is BlockGene other && Equals(other);
        }

        public override readonly int GetHashCode()
        {
            return HashCode.Combine(position, module, direction);
        }
    }
}
