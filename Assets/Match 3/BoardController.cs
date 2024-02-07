using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Bipolar.Match3
{
    [RequireComponent(typeof(Board))]
    public class BoardController : MonoBehaviour
    {
        public delegate void TokensSwapEventHandler(Vector2Int token1Coord, Vector2Int token2Coord);

        public event System.Action OnTokensMovementStopped;
        public event System.Action OnTokensColapsed;
        public event TokensSwapEventHandler OnTokensSwapped;

        private Board _board;
        public Board Board
        {
            get
            {
                if (_board == null)
                    _board = GetComponent<Board>();
                return _board;
            }
        }

        [SerializeField]
        private Settings settings;
        [SerializeField]
        private TokensSpawner tokensSpawner;

        [SerializeField]
        private MoveDirection collapseDirection;
        
        private readonly List<Token> currentlyMovingTokens = new List<Token>();
        public bool AreTokensMoving => currentlyMovingTokens.Count > 0;

        private void Start()
        {
            PopulateBoard();
        }

        private void PopulateBoard()
        {
            var tokens = new Token[Board.Dimentions.y, Board.Dimentions.x];

            OnTokensMovementStopped += CallInitialColapseEvent;
            for (int j = 0; j < Board.Dimentions.y; j++)
            {
                for (int i = 0; i < Board.Dimentions.x; i++)
                {
                    var token = CreateToken(i, j, true, true);
                    tokens[j, i] = token;
                }
            }
            Board.SetTokens(tokens);
        }

        private void CallInitialColapseEvent()
        {
            OnTokensMovementStopped -= CallInitialColapseEvent;
            OnTokensColapsed?.Invoke();
        }

        private Token CreateToken(int xCoord, int yCoord, bool withMove = true, bool avoidMatches = false)
        {
            int xSpawnCoord = xCoord;
            int ySpawnCoord = yCoord;
            if (withMove)
            {
                switch (collapseDirection)
                {
                    case MoveDirection.Left:
                        xSpawnCoord += Board.Dimentions.x;
                        break;
                    case MoveDirection.Up:
                        ySpawnCoord -= Board.Dimentions.y;
                        break;
                    case MoveDirection.Right:
                        xSpawnCoord -= Board.Dimentions.x;
                        break;
                    case MoveDirection.Down:
                        ySpawnCoord += Board.Dimentions.y;
                        break;
                }
            }

            Vector3 spawnPosition = Board.CoordToWorld(xSpawnCoord, ySpawnCoord);

            var token = tokensSpawner.SpawnToken();
            token.transform.position = spawnPosition;
            token.gameObject.name = $"Token {xCoord}:{yCoord}";
            token.Type = settings.TokenTypes[Random.Range(0, settings.TokenTypes.Count)];
            if (withMove)
            {
                StartTokenMovement(token, xCoord, yCoord);
            }
            
            return token;
        }


        private System.Action swapEndedCallback;
        public void SwapTokens(Vector2Int token1Coord, Vector2Int token2Coord)
        {
            var token1 = Board.GetToken(token1Coord);
            var token2 = Board.GetToken(token2Coord);

            StartTokenMovement(token2, token1Coord);
            StartTokenMovement(token1, token2Coord);

            Board.SwapTokens(token1Coord, token2Coord);
            OnTokensMovementStopped += BoardController_OnTokensMovementStopped;
            swapEndedCallback = () =>
            {
                swapEndedCallback = null;
                OnTokensSwapped?.Invoke(token1Coord, token2Coord);
            };
        }

        private void BoardController_OnTokensMovementStopped()
        {
            OnTokensMovementStopped -= BoardController_OnTokensMovementStopped;
            swapEndedCallback.Invoke();
        }

        public void StartTokenMovement(Token token, Vector2Int targetCoord) => StartTokenMovement(token, targetCoord.x, targetCoord.y);
        public void StartTokenMovement(Token token, int xTargetCoord, int yTargetCoord)
        {
            token.OnMovementEnded += Token_OnMovementEnded;
            token.BeginMovingToPosition(Board.CoordToWorld(xTargetCoord, yTargetCoord));
            currentlyMovingTokens.Add(token);
        }

        private void Token_OnMovementEnded(Token token)
        {
            token.OnMovementEnded -= Token_OnMovementEnded;
            currentlyMovingTokens.Remove(token);
            if (AreTokensMoving == false)
            {
                OnTokensMovementStopped?.Invoke();
            }
        }
    }
}
