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
    private int requestNumber;
    [SerializeField, ReadOnly]
    public MatchRequest currentRequest;
    [SerializeField, ReadOnly]
    public int requestsCountDone;

    private int previousTypeIndex = 0;

    private void OnEnable()
    {
        matchManager.OnTokensMatched += MatchManager_OnTokensMatched;
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
        int typeIndex = Random.Range(1, settings.TokenTypes.Count);
        if (typeIndex == previousTypeIndex)
            typeIndex = 0;
        var randomType = settings.TokenTypes[typeIndex];
        currentRequest = new MatchRequest()
        {
            type = randomType,
            horizontalCount = 0,
            verticalCount = 0,
            size = 1,
            requestsCount = requestNumber,
        };
        previousTypeIndex = typeIndex;
        OnNewRequestRequested?.Invoke(currentRequest, typeIndex);
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

        requestsCountDone += chain.Size + chain.HorizontalTriosCount + chain.VerticalTriosCount - 1;
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
