using Bipolar.PuzzleBoard;
using System.Collections.Generic;
using UnityEngine;

namespace Bipolar.Match3
{
    public delegate void PiecesSwapEventHandler(Vector2Int pieceCoord1, Vector2Int pieceCoord2);

    public class MatchManager : MonoBehaviour
    {
        public event System.Action OnMatchingFailed;
        public event System.Action<PiecesChain> OnPiecesMatched;

        [SerializeField]
        private BoardController boardController;
        [SerializeField]
        private SwapManager swapManager;

        [SerializeField]
        private Matcher matcher;

        [SerializeField]
        private int combo;
        public int Combo => combo;

        private void OnEnable()
        {
            boardController.OnPiecesColapsed += BoardController_OnPiecesColapsed;
            swapManager.OnSwapRequested += SwapManager_OnSwapRequested;
        }

        private void Start()
        {
            boardController.Collapse();
        }

        private void SwapManager_OnSwapRequested(Vector2Int pieceCoord1, Vector2Int pieceCoord2)
        {
            if (boardController.ArePiecesMoving || boardController.IsCollapsing)
                return;
            
            if (currentlyClearedPieces.Count > 0)
                return;
            
            SwapTokens(pieceCoord1, pieceCoord2);
        }

        private System.Action swapEndedCallback;
        private void SwapTokens(Vector2Int pieceCoord1, Vector2Int pieceCoord2)
        {
            combo = 0;
            boardController.PiecesMovementManager.OnAllPiecesMovementStopped += PiecesMovementManager_OnAllPiecesMovementStopped;
            swapEndedCallback = () =>
            {
                swapEndedCallback = null;
                FindMatches();
                bool wasCorrectMove = matcher.PieceChains.Count > 0;
                if (wasCorrectMove == false)
                {
                    (boardController.Pieces[pieceCoord1], boardController.Pieces[pieceCoord2]) = (boardController.Pieces[pieceCoord2], boardController.Pieces[pieceCoord1]);
                    OnMatchingFailed?.Invoke();
                }
            };

            (boardController.Pieces[pieceCoord2], boardController.Pieces[pieceCoord1]) = (boardController.Pieces[pieceCoord1], boardController.Pieces[pieceCoord2]);
        }

        private void PiecesMovementManager_OnAllPiecesMovementStopped()
        {
            boardController.PiecesMovementManager.OnAllPiecesMovementStopped -= PiecesMovementManager_OnAllPiecesMovementStopped;
            swapEndedCallback?.Invoke();
        }

        private void BoardController_OnPiecesColapsed()
        {
            FindMatches();
        }

        private void FindMatches()
        {
            matcher.FindAndCreatePieceChains(boardController.Board);

            combo += matcher.PieceChains.Count;
            foreach (var chain in matcher.PieceChains)
            {
                OnPiecesMatched?.Invoke(chain);
                ClearChainPieces(chain);
            }
        }

        private readonly List<Piece> currentlyClearedPieces = new List<Piece>();
        private void ClearChainPieces(PiecesChain chain)
        {
            foreach (var coord in chain.PiecesCoords)
            {
                var piece = boardController.Board.GetPiece(coord);

                currentlyClearedPieces.Add(piece);
                boardController.Pieces[coord] = null;
            }

            foreach (var piece in currentlyClearedPieces)
            {
                piece.OnCleared += Piece_OnCleared;
                if (piece.TryGetComponent<PieceClearingBehavior>(out var clearing))
                {
                    clearing.ClearPiece();
                }
                else
                {
                    piece.IsCleared = true;
                }
            }
        }

        private void Piece_OnCleared(Piece piece)
        {
            piece.OnCleared -= Piece_OnCleared;
            currentlyClearedPieces.Remove(piece);
            if (currentlyClearedPieces.Count <= 0)
                boardController.Collapse();
        }

        private void OnDisable()
        {
            boardController.OnPiecesColapsed -= BoardController_OnPiecesColapsed;
            swapManager.OnSwapRequested -= SwapManager_OnSwapRequested;
        }
    }
}
