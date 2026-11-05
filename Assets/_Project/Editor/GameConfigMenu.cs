using System.IO;
using UnityEditor;
using UnityEngine;

namespace PayWithPixels.Build
{
    /// <summary>
    /// Tworzy asset GameConfig z wartosciami domyslnymi z kodu (Resources/GameConfig).
    /// Z linii polecen: Unity.exe -batchmode -nographics -quit -projectPath ... -executeMethod PayWithPixels.Build.GameConfigMenu.Create
    /// </summary>
    public static class GameConfigMenu
    {
        private const string AssetPath = "Assets/_Project/Resources/GameConfig.asset";

        [MenuItem("Jam/Create Game Config")]
        public static void Create()
        {
            if (File.Exists(AssetPath))
            {
                Debug.Log("[game config] " + AssetPath + " already exists");
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameConfig>(AssetPath);
                return;
            }
            var cfg = ScriptableObject.CreateInstance<GameConfig>();   // OnEnable uzupelnia listy broni i wrogow
            AssetDatabase.CreateAsset(cfg, AssetPath);
            AssetDatabase.SaveAssets();
            Selection.activeObject = cfg;
            Debug.Log("[game config] created " + AssetPath);
        }
    }
}
