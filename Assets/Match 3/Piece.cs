﻿using UnityEngine;

namespace Bipolar.Match3
{
    public class Piece : MonoBehaviour
    {
        public event System.Action<PieceType> OnTypeChanged;
        public event System.Action<Piece> OnCleared;

        [SerializeField]
        private PieceType type;
        public PieceType Type
        {
            get => type;
            set
            {
                type = value;
                if (gameObject.scene.IsValid()) 
                    gameObject.name = $"Piece ({type.name})";
                OnTypeChanged?.Invoke(type);
            }
        }

        private bool isCleared = false;
        public bool IsCleared
        {
            get => isCleared;
            set
            {
                isCleared = value;
                if (isCleared)
                    Invoke(nameof(CallClearedEvent), 0);            
            }
        }

        private void CallClearedEvent()
        {
            OnCleared?.Invoke(this);
        }

        private void OnValidate()
        {
            Type = type;
        }
    }
}
