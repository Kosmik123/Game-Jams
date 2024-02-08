using UnityEngine;

public class MatchRequesterUI : MonoBehaviour
{
    [SerializeField]
    private MatchRequester matchRequester;
    [SerializeField]
    private TMPro.TextMeshProUGUI label;

    private int spriteIndex;

    private void OnEnable()
    {
        matchRequester.OnNewRequestRequested += MatchRequester_OnNewRequestRequested;
        matchRequester.OnRequestUpdated += RefreshBubbleState;
    }

    private void MatchRequester_OnNewRequestRequested(MatchRequest request, int spriteIndex)
    {
        this.spriteIndex = spriteIndex;
        label.text = $"{request.requestsCount} x <sprite index={spriteIndex}>";
    }
    
    private void RefreshBubbleState(int remaining)
    {
        label.text = $"{remaining} x <sprite index={spriteIndex}>";
    }

    private void OnDisable()
    {
        matchRequester.OnRequestUpdated -= RefreshBubbleState;
        matchRequester.OnNewRequestRequested -= MatchRequester_OnNewRequestRequested;
    }
}
