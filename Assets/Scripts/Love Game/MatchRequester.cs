using Bipolar.Match3;
using Bipolar.PuzzleBoard;
using NaughtyAttributes;
using UnityEngine;

public class MatchRequester : MonoBehaviour
{
    public event System.Action<MatchRequest> OnNewRequestRequested;
    public event System.Action<int> OnRequestUpdated;

    [SerializeField]
    private MatchController matchManager;
    [SerializeField]
    private MatchingManager matchingManager;
    [SerializeField]
    private MatchRequest[] requests;
    [SerializeField]
    private PieceColorProvider settings;

    [Header("States")]
    [SerializeField, ReadOnly]
    private int requestNumber;
    [SerializeField, ReadOnly]
    public MatchRequest currentRequest;
    [SerializeField, ReadOnly]
    public int requestsCountDone;

    private int previousTypeIndex = 0;

    private System.Random rng = new System.Random();

    private void OnEnable()
    {
        //  matchManager.OnPiecesMatched += MatchManager_OnTokensMatched;
    }

    private void Start()
    {
        requestNumber = 1;
        NextRequest();
    }

    private void NextRequest()
    {
        requestsCountDone = 0;
        requestNumber++;

        var randomType = settings.GetPieceColor(rng.Next(), rng.Next());
        currentRequest = new MatchRequest()
        {
            type = randomType,
            horizontalCount = 0,
            verticalCount = 0,
            size = 1,
            requestsCount = requestNumber,
        };
        OnNewRequestRequested?.Invoke(currentRequest);
    }

    private void MatchManager_OnTokensMatched(PiecesChain chain)
    {
        var request = currentRequest;
        if (request.type != null && chain.PieceType != request.type)
            return;

        if (chain.Size < request.size)
            return;

        int obtainedTokens = chain.Size;
        if (chain is TriosPiecesChain triosTokensChain)
        {
            if (triosTokensChain.HorizontalTriosCount < request.horizontalCount)
                return;

            if (triosTokensChain.VerticalTriosCount < request.verticalCount)
                return;

            obtainedTokens += triosTokensChain.HorizontalTriosCount + triosTokensChain.VerticalTriosCount - 1;
        }

        obtainedTokens += matchingManager.Combo - 1;
        requestsCountDone += obtainedTokens;
        int remaining = currentRequest.requestsCount - requestsCountDone;
        if (remaining > 0)
            OnRequestUpdated?.Invoke(remaining);
        if (requestsCountDone >= currentRequest.requestsCount)
            Invoke(nameof(NextRequest), 0);
    }

    private void OnDisable()
    {
        matchManager.OnPiecesMatched += MatchManager_OnTokensMatched;
    }
}
