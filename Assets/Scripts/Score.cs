using System.Collections;
using TMPro;
using UnityEngine;

public class Score : MonoBehaviour
{
    [SerializeField]
    private TMP_Text label;

    [SerializeField]
    private int score;

    private void Start()
    {
        StartCoroutine(IncrementScore());
    }

    private IEnumerator IncrementScore()
    {
        var wait = new WaitForSeconds(1); 
        while (true)
        {
            score++;
            label.text = $"Score: {score}";
            yield return wait;
        }
    }
}
