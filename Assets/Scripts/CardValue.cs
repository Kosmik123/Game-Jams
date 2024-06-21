using System;
using UnityEngine;

namespace UniMakao
{
    [CreateAssetMenu(menuName = CreateAssetPath.Root + "Card Value")]
    public class CardValue : ScriptableObject, IComparable<CardValue>
    {
        [SerializeField]
        private int value;

        public int CompareTo(CardValue other)
        {
            return value.CompareTo(other.value);
        }
    }
}
