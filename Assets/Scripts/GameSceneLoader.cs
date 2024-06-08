using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneLoader : MonoBehaviour
{
    [SerializeField]
    private int gameSceneIndex = 1;

    [SerializeField]
    private NPCMovement cameraTransition;

    private AsyncOperation asyncOperation;

    public float progress;

    public void StartLoadingGameScene()
    {
        asyncOperation = SceneManager.LoadSceneAsync(gameSceneIndex);
        asyncOperation.allowSceneActivation = false;
        StartCoroutine(WaitForAllConditions(asyncOperation));
    }

    private IEnumerator WaitForAllConditions(AsyncOperation operation)
    {
        yield return new WaitWhile(() => operation.progress < 0.9f);
        yield return new WaitWhile(() => cameraTransition.IsMoving);
        operation.allowSceneActivation = true;
    }
}
