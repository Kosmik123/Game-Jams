using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.Serialization;

namespace Bipolar.PuzzleBoard
{
    public interface IPieceTypeProvider
    {
        public PieceType GetPieceType();
    }

    [CreateAssetMenu(menuName = CreateAssetsPath.Root + "Settings")]
    public class Settings : ScriptableObject, IPieceTypeProvider
    {
        [SerializeField]
        private PieceType[] possiblePieceTypes;
        public IReadOnlyList<PieceType> PieceTypes => possiblePieceTypes;

        public PieceType GetPieceType()
        {
            return PieceTypes[Random.Range(0, PieceTypes.Count)];
        }

        public PieceType GetPieceTypeExcept(PieceType exception)
        {
            int index = Random.Range(1, PieceTypes.Count);
            if (PieceTypes[index] == exception)
                return PieceTypes[0];
            
            return PieceTypes[index];
        }

        private readonly List<PieceType> tempAvailableTypes = new List<PieceType>();
        public PieceType GetPieceTypeExcept(IEnumerable<PieceType> exceptions)
        {
            tempAvailableTypes.Clear();
            foreach (var type in PieceTypes)
                if (exceptions.Contains(type) == false)
                    tempAvailableTypes.Add(type);

            if (tempAvailableTypes.Count <= 0)
                return GetPieceType();

            return tempAvailableTypes[Random.Range(0, tempAvailableTypes.Count)];
        }
    }
}
