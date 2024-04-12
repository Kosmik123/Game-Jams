using UnityEngine;
using UnityEngine.SceneManagement;

public class TestLoadingScenes : MonoBehaviour
{

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            SceneManager.LoadSceneAsync(5);
    }
}
