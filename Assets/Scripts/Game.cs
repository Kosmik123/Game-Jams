using Bipolar.Prototyping;
using NaughtyAttributes;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Game : SceneSingleton<Game> 
{
    public static event System.Action OnGameWon;
    public static event System.Action OnGameLost;

    private readonly HashSet<VaseType> connectedVaseTypes = new HashSet<VaseType>();

    [SerializeField,Min(1)]
    private int vasesNeededToWin = 3;

    [SerializeField, ReadOnly]
    private float connectedVaseTypesCount;

    [SerializeField]
    private Behaviour[] behaviorsToDisableOnEnd;

    private bool isEnd = false;

    private void Update()
    {
        connectedVaseTypesCount = connectedVaseTypes.Count;
        if (connectedVaseTypesCount >= vasesNeededToWin)
        {
            Victory();
        }

        if (isEnd)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                SceneManager.LoadSceneAsync(1);
            }
        }

    }

    private void Victory()
    {
        DisableBehaviors();
        OnGameWon?.Invoke();
    }

    private void DisableBehaviors()
    {
        foreach (var behavior in behaviorsToDisableOnEnd)
        {
            behavior.enabled = false;
        }
        isEnd = true;
    }

    public static void AddConnectedVaseType(VaseType vaseType)
    {
        Instance.connectedVaseTypes.Add(vaseType);
    }

    public static void Loose()
    {
        Instance.DisableBehaviors();
        Instance.Invoke(nameof(LooseSequence), 1f);
    }

    public void LooseSequence()
    {
        OnGameLost?.Invoke();
    }
}
