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
    private Board board;

    [SerializeField]
    private Transform target;
    [SerializeField]
    private Effect effectPrototype;

    private void OnEnable()
    {
        matchManager.OnPiecesMatched += MatchManager_OnTokensMatched;
    }

    private void MatchManager_OnTokensMatched(PiecesChain chain)
    {
        var type = chain.PieceType;
        var sprite = settings.GetPieceSprite(type);
        
        foreach (var coord in chain.PiecesCoords)
        {
            var position = board.CoordToWorld(coord);
            var effect = Instantiate(effectPrototype, position, Quaternion.identity);
            effect.Target = target;
            if (effect.TryGetComponent<SpriteRenderer>(out var renderer))
                renderer.sprite = sprite;
        }
    }

    private void OnDisable()
    {
        matchManager.OnPiecesMatched -= MatchManager_OnTokensMatched;
    }
}
