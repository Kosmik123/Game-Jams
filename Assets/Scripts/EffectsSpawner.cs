using Bipolar.Match3;
using Bipolar.PuzzleBoard;
using UnityEngine;

public class EffectsSpawner : MonoBehaviour
{
    [SerializeField]
    private PieceVisualSettings settings;
    [SerializeField]
    private MatchManager matchManager;
    [SerializeField]
    private BoardController boardController;
    [SerializeField]
    private PiecesSpawner spawner;

    [SerializeField]
    private Transform target;
    [SerializeField]
    private Effect effectPrototype;

    private void OnEnable()
    {
        spawner.OnPieceSpawned += Spawner_OnPieceSpawned;
        matchManager.OnPiecesMatched += MatchManager_OnPiecesMatched;
    }

    private void Spawner_OnPieceSpawned(Piece piece)
    {
        piece.OnCleared += Piece_OnCleared;
    }

    private void Piece_OnCleared(Piece piece)
    {
        piece.OnCleared -= Piece_OnCleared;
        var sprite = settings.GetPieceSprite(piece.Type);
        SpawnEffect(sprite, piece.transform.position);
    }

    private void MatchManager_OnPiecesMatched(PiecesChain chain)
    {
        var type = chain.PieceType;
        var sprite = settings.GetPieceSprite(type);
        
        foreach (var coord in chain.PiecesCoords)
        {
            var position = boardController.Board.CoordToWorld(coord);
            //SpawnEffect(sprite, position);
        }
    }

    private void SpawnEffect(Sprite sprite, Vector3 position)
    {
        var effect = Instantiate(effectPrototype, position, Quaternion.identity);
        effect.Target = target;
        if (effect.TryGetComponent<SpriteRenderer>(out var renderer))
            renderer.sprite = sprite;
    }

    private void OnDisable()
    {
        matchManager.OnPiecesMatched -= MatchManager_OnPiecesMatched;
    }
}
