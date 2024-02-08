using Bipolar.Match3;
using UnityEngine;


public class MatchRequester : MonoBehaviour
{
    public event System.Action<MatchRequest> OnRequestFulfilled;

    [SerializeField]
    private MatchManager matchManager;

    [SerializeField]
    private MatchRequest currentRequest;

    private void OnEnable()
    {
        matchManager.OnTokensMatched += MatchManager_OnTokensMatched;
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

        Debug.Log("<color=yellow>BRAWO</color>");
        OnRequestFulfilled?.Invoke(request);
    }

    private void OnDisable()
    {
        matchManager.OnTokensMatched += MatchManager_OnTokensMatched;
    }
}
