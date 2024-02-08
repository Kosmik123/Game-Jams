using Bipolar.Match3;
using NaughtyAttributes;
using UnityEngine;

public class MatchRequester : MonoBehaviour
{
    public event System.Action<MatchRequest, int> OnNewRequestRequested;
    public event System.Action<int> OnRequestUpdated;

    [SerializeField]
    private MatchManager matchManager;
    //[SerializeField]
    //private MatchRequest[] requests;
    [SerializeField]
    private Settings settings;

    [Header("States")]
    [SerializeField, ReadOnly]
    private int requestIndex;
    [SerializeField, ReadOnly]
    public MatchRequest currentRequest;
    [SerializeField, ReadOnly]
    public int requestsCountDone;

    private void OnEnable()
    {
        matchManager.OnTokensMatched += MatchManager_OnTokensMatched;
    }

    private void Start()
    {
        requestIndex = 1;
        NextRequest();
    }

    private void NextRequest()
    {
        requestsCountDone = 0;
        requestIndex++;
        int randomIndex = Random.Range(0, settings.TokenTypes.Count);
        var randomType = settings.TokenTypes[randomIndex];
        currentRequest = new MatchRequest()
        {
            type = randomType,
            horizontalCount = 0,
            verticalCount = 0,
            size = 1,
            requestsCount = requestIndex,
        };

        OnNewRequestRequested?.Invoke(currentRequest, randomIndex);
    }

    private void MatchManager_OnTokensMatched(TokensChain chain)
    {
        var request = currentRequest;
        if (request.type != null && chain.TokenType != request.type)
            return;

        if (chain.Size < request.size)
            return;

        if (chain.HorizontalTriosCount < request.horizontalCount)
            return;

        if (chain.VerticalTriosCount < request.verticalCount)
            return;

        requestsCountDone += chain.Size;
        int remaining = currentRequest.requestsCount - requestsCountDone;
        if (remaining > 0)
            OnRequestUpdated?.Invoke(remaining);
        if (requestsCountDone >= currentRequest.requestsCount)
            Invoke(nameof(NextRequest), 0);
    }

    private void OnDisable()
    {
        matchManager.OnTokensMatched += MatchManager_OnTokensMatched;
    }
}
