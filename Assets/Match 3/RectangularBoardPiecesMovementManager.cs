﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ComponentExtensions
{
    public static T GetCachedComponent<T>(this Component owner, ref T component) where T : Component
    {
        if (component == null)
            component = owner.GetComponent<T>();
        return component;
    }
}

namespace Bipolar.Match3
{
    [RequireComponent(typeof(RectangularBoard))]
    public class RectangularBoardPiecesMovementManager : PiecesMovementManager
    {
        public override event System.Action OnPiecesMovementStopped;

        [SerializeField]
        private RectangularBoard board;
        [SerializeField]
        private float defaultMovementDuration;

        private readonly Dictionary<Piece, Coroutine> pieceMovementCoroutines = new Dictionary<Piece, Coroutine>();
        public override bool ArePiecesMoving => pieceMovementCoroutines.Count > 0;

        public void StartPieceMovement(Piece piece, Vector2Int targetCoord, float duration = -1) 
        {
            if (duration < 0)
                duration = defaultMovementDuration;
            var movementCoroutine = StartCoroutine(MovementCo(piece, board.CoordToWorld(targetCoord), duration));
            pieceMovementCoroutines.Add(piece, movementCoroutine);
        }

        private IEnumerator MovementCo(Piece piece, Vector3 target, float duration)
        {
            Vector3 startPosition = piece.transform.position;
            Vector3 targetPosition = target;
            float moveProgress = 0;
            float progressSpeed = 1f / duration;
            while (moveProgress < 1)
            {
                moveProgress += progressSpeed * Time.deltaTime;
                piece.transform.position = Vector3.Lerp(startPosition, targetPosition, moveProgress);
                yield return null;
            }
            piece.transform.position = targetPosition;

            pieceMovementCoroutines.Remove(piece);
            if (ArePiecesMoving == false)
                OnPiecesMovementStopped?.Invoke();
        }
    }
}