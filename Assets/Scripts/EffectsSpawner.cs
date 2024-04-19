using Bipolar.Match3;
using Bipolar.PuzzleBoard;
using Bipolar.PuzzleBoard.Components;
using UnityEngine;

public class EffectsSpawner : MonoBehaviour
{
    [SerializeField]
    private PieceVisualSettings settings;
    [SerializeField]
    private MatchController matchController;
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
        matchController.OnPiecesMatched += MatchManager_OnPiecesMatched;
    }

    private void Spawner_OnPieceSpawned(PieceComponent piece)
    {
        piece.OnCleared += Piece_OnCleared;
    }

    private void Piece_OnCleared(PieceComponent piece)
    {
        piece.OnCleared -= Piece_OnCleared;
        var sprite = settings.GetPieceSprite(piece.Color);
        SpawnEffect(sprite, piece.transform.position);
    }

    private void MatchManager_OnPiecesMatched(PiecesChain chain)
    {
        var type = chain.PieceType;
        var sprite = settings.GetPieceSprite(type);
        
        foreach (var coord in chain.PiecesCoords)
        {
            var position = boardController.BoardComponent.CoordToWorld(coord);
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
        matchController.OnPiecesMatched -= MatchManager_OnPiecesMatched;
    }
}
