using System.Collections.Generic;
using UnityEngine;

public static class GenomeValidator
{
    /// <summary>Checks that the genome has exactly one brain, no overlapping blocks and is one connected piece.</summary>
    public static bool TryValidate(Genome genome, out string error)
    {
        if (genome.Count == 0)
        {
            error = "Creature is empty";
            return false;
        }

        var positions = new HashSet<Vector2Int>();
        Vector2Int brain = default;
        int brains = 0;
        for (int i = 0; i < genome.Count; i++)
        {
            BlockGene block = genome.Blocks[i];
            if (!positions.Add(block.position))
            {
                error = "Two blocks on the same cell";
                return false;
            }
            if (block.module == ModuleType.Brain)
            {
                brain = block.position;
                brains++;
            }
        }

        if (brains != 1)
        {
            error = "Creature needs exactly one brain";
            return false;
        }

        var visited = new HashSet<Vector2Int> { brain };
        var queue = new Queue<Vector2Int>();
        queue.Enqueue(brain);
        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            for (int d = 0; d < 4; d++)
            {
                Vector2Int next = current + ((Direction)d).ToVector();
                if (positions.Contains(next) && visited.Add(next))
                {
                    queue.Enqueue(next);
                }
            }
        }

        if (visited.Count != positions.Count)
        {
            error = "Blocks must all be connected to the brain";
            return false;
        }

        error = null;
        return true;
    }
}
