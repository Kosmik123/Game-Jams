using UnityEditor;
using UnityEngine;

// Default size of the WebGL canvas on the page (both game versions).
// CLI: Unity -batchmode -quit -executeMethod WebGLWindowSettings.Apply
public static class WebGLWindowSettings
{
    private const int Width = 540;
    private const int Height = 720;

    [MenuItem("Tools/Apply WebGL Window Size")]
    public static void Apply()
    {
        PlayerSettings.defaultWebScreenWidth = Width;
        PlayerSettings.defaultWebScreenHeight = Height;
        AssetDatabase.SaveAssets();
        Debug.Log($"[WebGLWindowSettings] WebGL window set to {Width}x{Height}");
    }
}
