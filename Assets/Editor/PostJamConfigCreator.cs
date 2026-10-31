using System.IO;
using UnityEditor;
using UnityEngine;
using ZeroHour.Tower;

// Creates Assets/_Project/Resources/PostJamConfig.asset with the shipped values (an existing asset is left alone).
// CLI: Unity -batchmode -quit -executeMethod PostJamConfigCreator.Create
public static class PostJamConfigCreator
{
    private const string AssetPath = "Assets/_Project/Resources/" + PostJamConfig.ResourceName + ".asset";

    [MenuItem("Tools/Create Post-Jam Config")]
    public static void Create()
    {
        var existing = AssetDatabase.LoadAssetAtPath<PostJamConfig>(AssetPath);
        if (existing != null)
        {
            Debug.Log($"[PostJamConfigCreator] {AssetPath} already exists");
            Selection.activeObject = existing;
            return;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(AssetPath));
        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<PostJamConfig>(), AssetPath);
        AssetDatabase.SaveAssets();
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<PostJamConfig>(AssetPath);
        Debug.Log($"[PostJamConfigCreator] Created {AssetPath}");
    }
}
