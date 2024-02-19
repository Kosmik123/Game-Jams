using Bipolar.Match3;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        var type = chain.TokenType;
        var sprite = settings.GetPieceSprite(type);
        
        foreach (var tokenCoord in chain.TokenCoords)
        {
            var position = board.CoordToWorld(tokenCoord);
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
