using UnityEngine;

namespace Bipolar.Match3
{
    public abstract class PieceTypeProvider : MonoBehaviour
    {
        public abstract PieceType GetPieceType(int x, int y);
    }
}
