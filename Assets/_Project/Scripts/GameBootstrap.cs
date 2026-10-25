using UnityEngine;
using UnityEngine.SceneManagement;
using ZeroHour.Menu;
using ZeroHour.Tower;

namespace ZeroHour
{
    // Scenes contain only a camera; the content of each scene is built from code when it loads.
    public static class GameBootstrap
    {
        public const string VersionSelectScene = "VersionSelect";
        public const string GameScene = "Game";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            switch (scene.name)
            {
                case VersionSelectScene:
                    new GameObject(nameof(VersionSelectMenu)).AddComponent<VersionSelectMenu>();
                    break;
                case GameScene:
                    TowerGame.Create(GameVersion.CreateRuleset());
                    break;
            }
        }
    }
}
