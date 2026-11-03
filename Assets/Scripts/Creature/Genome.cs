using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Creature blueprint: list of blocks on a grid. Same format for the player, NPCs, offspring and saved forms.
/// </summary>
[System.Serializable]
public class Genome
{
	public static readonly IEnumerable<BlockGene>[] StartingTetrisGenomes =
	{
		new[] { new BlockGene(0, -1), new BlockGene(0, 0), new BlockGene(0, 1), new BlockGene(0, 2) },
		new[] { new BlockGene(0, -2), new BlockGene(0, -1), new BlockGene(0, 0), new BlockGene(0, 1) },

		new[] { new BlockGene(1, -1), new BlockGene(1, 0), new BlockGene(0, 0), new BlockGene(0, 1) },
		new[] { new BlockGene(-1, -1), new BlockGene(-1, 0), new BlockGene(0, 0), new BlockGene(0, 1) },

		new[] { new BlockGene(0, -1), new BlockGene(0, 0), new BlockGene(1, 0), new BlockGene(0, 1) },
		new[] { new BlockGene(0, -1), new BlockGene(0, 0), new BlockGene(-1, 0), new BlockGene(0, 1) },
		new[] { new BlockGene(-1, 0), new BlockGene(0, 0), new BlockGene(1, 0), new BlockGene(0, 1) },

		new[] { new BlockGene(0, -1), new BlockGene(0, 0), new BlockGene(0, 1), new BlockGene(1, 1) },
		new[] { new BlockGene(0, -1), new BlockGene(0, 0), new BlockGene(0, 1), new BlockGene(-1, 1) },
		new[] { new BlockGene(1, -1), new BlockGene(0, -1), new BlockGene(0, 0), new BlockGene(0, 1) },
		new[] { new BlockGene(-1, -1), new BlockGene(0, -1), new BlockGene(0, 0), new BlockGene(0, 1) },
	};

	[SerializeField]
	private List<BlockGene> blocks = new List<BlockGene>();

	public IReadOnlyList<BlockGene> Blocks => blocks;
	public int Count => blocks.Count;

	public Genome() { }

	public Genome(IEnumerable<BlockGene> blocks)
	{
		this.blocks = new List<BlockGene>(blocks);
	}

	/// <summary>Starting creature: a single block with the brain.</summary>
	public static Genome CreateStarting()
	{
		var genome = new Genome(StartingTetrisGenomes[Random.Range(0, StartingTetrisGenomes.Length)]);
		genome.Replace(new BlockGene(0, 0, ModuleType.Brain));
		return genome;
	}

	public Genome Clone()
	{
		return new Genome(blocks);
	}

	public bool TryGetBlock(Vector2Int position, out BlockGene block)
	{
		block = default;
		if (TryGetIndex(position, out int index) == false)
			return false;

		block = blocks[index];
		return true;
	}

	public bool Contains(Vector2Int position) => TryGetIndex(position, out _);

	/// <summary>Adds a block. Returns false if the position is already taken.</summary>
	public bool Add(BlockGene block)
	{
		if (Contains(block.position))
			return false;

		blocks.Add(block);
		return true;
	}

	/// <summary>Replaces the block at the same position. Returns false if there is none.</summary>
	public bool Replace(BlockGene block)
	{
		if (TryGetIndex(block.position, out int index) == false)
			return false;

		blocks[index] = block;
		return true;
	}

	public bool Remove(Vector2Int position)
	{
		if (TryGetIndex(position, out int index) == false)
			return false;

		blocks.RemoveAt(index);
		return true;
	}

	public int CountModules(ModuleType module)
	{
		int count = 0;
		for (int i = 0; i < blocks.Count; i++)
			if (blocks[i].module == module)
				count++;

		return count;
	}

	private bool TryGetIndex(Vector2Int position, out int index)
	{
		for (int i = 0; i < blocks.Count; i++)
		{
			if (blocks[i].position == position)
			{
				index = i;
				return true;
			}
		}

		index = -1;
		return false;
	}
}
