using UnityEngine;

namespace Bipolar.Match3
{
    public class RandomPieceTypeProvider : PieceTypeProvider
    {
        [SerializeField]
        private Settings settings;

        public override PieceType GetPieceType(int x, int y)
        {
            return settings.GetPieceType();
        }
    }
}
