using UnityEngine;
using TMPro;

namespace Bipolar.Match3
{
    public class MatchManagerUI : MonoBehaviour
    {
        [SerializeField]
        private MatchManager matchManager;
        [SerializeField]
        private TextMeshProUGUI scoreLabel;
        [SerializeField]
        private float scoreChangeDelay = 0.1f;
        private float timer;

        private int targetScore;
        private int displayedScore;

        private void OnEnable()
        {
            matchManager.OnScoreChanged += MatchManager_OnScoreChanged;
        }

        private void MatchManager_OnScoreChanged(int score)
        {
            targetScore = score;
        }

        private void Update()
        {
            timer += Time.deltaTime;
            if (targetScore > displayedScore && timer > scoreChangeDelay)
            {
                timer = 0;
                displayedScore++;
                scoreLabel.text = displayedScore.ToString();
            }
        }

        private void OnDisable()
        {
            matchManager.OnScoreChanged -= MatchManager_OnScoreChanged;
        }
    }
}
