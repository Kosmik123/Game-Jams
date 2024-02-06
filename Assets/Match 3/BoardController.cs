using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Bipolar.Match3
{

    [RequireComponent(typeof(Board))]
    public class BoardController : MonoBehaviour
    {
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

        private void Start()
        {
            PopulateBoard();
        }

        private void PopulateBoard()
        {
            var tokens = new Token[Board.Dimentions.y, Board.Dimentions.x];

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
            token.Initialize(settings.TokenTypes[Random.Range(0, settings.TokenTypes.Count)]);
            if (withMove)
            {
                token.OnMovementEnded += Token_OnMovementEnded;
                token.BeginMovingToPosition(Board.CoordToWorld(xCoord, yCoord));
                currentlyMovingTokens.Add(token);
            }
            
            return token;
        }

        private void Token_OnMovementEnded(Token token)
        {
            currentlyMovingTokens.Remove(token);
            if (currentlyMovingTokens.Count == 0 )
            {
                Debug.Log("All takens stopped moving");
            }
        }
    }
}
