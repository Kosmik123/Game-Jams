using Bipolar.Match3;
using System.Linq;
using UnityEngine;

public class MatchRequesterUI : MonoBehaviour
{
    [System.Serializable]
    public struct TokenTypeToSpriteAssetMapping
    {
        public PieceType tokenType;
        public string spriteAssetName;
    }

    [SerializeField]
    private MatchRequester matchRequester;
    [SerializeField]
    private TMPro.TextMeshProUGUI label;

    [SerializeField]
    private TokenTypeToSpriteAssetMapping[] tokenSpriteNamesMappings;

    private string currentSpriteName;

    private void OnEnable()
    {
        matchRequester.OnNewRequestRequested += MatchRequester_OnNewRequestRequested;
        matchRequester.OnRequestUpdated += RefreshBubbleState;
    }

    private void MatchRequester_OnNewRequestRequested(MatchRequest request)
    {
        currentSpriteName = tokenSpriteNamesMappings.First(mapping => mapping.tokenType == request.type).spriteAssetName;
        SetRequestText(request.requestsCount, currentSpriteName);
    }
    
    private void RefreshBubbleState(int remaining)
    {
        SetRequestText(remaining, currentSpriteName);
    }

    private void SetRequestText(int remainingCount, string spriteAssetName)
    {
        label.text = $"{remainingCount} x <sprite name={spriteAssetName}>";
    }



    private void OnDisable()
    {
        matchRequester.OnRequestUpdated -= RefreshBubbleState;
        matchRequester.OnNewRequestRequested -= MatchRequester_OnNewRequestRequested;
    }
}
