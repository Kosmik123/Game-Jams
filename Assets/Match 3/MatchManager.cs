using UnityEngine;

namespace Bipolar.Match3
{

    public class MatchManager : MonoBehaviour
    {
        [SerializeField]
        private BoardController boardController;
        [SerializeField]
        private TokensClickDetector tokensClickDetector;
        [SerializeField]
        private SwipeDetector swipeDetector;

        private void OnEnable()
        {
            swipeDetector.OnTokenSwiped += SwipeDetector_OnTokenSwiped;
        }

        private void SwipeDetector_OnTokenSwiped(Token token, UnityEngine.EventSystems.MoveDirection direction)
        {
            Debug.Log("Token " + token.name + " was swiped " +  direction);
        }

        private void OnDisable()
        {
            swipeDetector.OnTokenSwiped -= SwipeDetector_OnTokenSwiped;
        }
    }
}
