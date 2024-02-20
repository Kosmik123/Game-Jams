using UnityEngine;

namespace Bipolar.PuzzleBoard
{
    public abstract class PieceTypeProvider : MonoBehaviour
    {
        public abstract PieceType GetPieceType(int x, int y);
    }
}
