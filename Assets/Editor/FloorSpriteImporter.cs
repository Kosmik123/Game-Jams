using UnityEditor;
using UnityEngine;

// Pixel-art floor sprites (Resources/Floors and Resources/FloorsWide): single sprite, point filter,
// no compression, no mipmaps, 100 pixels per unit.
// CLI reimport: Unity -batchmode -quit -executeMethod FloorSpriteImporter.ReimportWideFloors
public class FloorSpriteImporter : AssetPostprocessor
{
    private const string WideFloorsFolder = "Assets/_Project/Resources/FloorsWide";
    private static readonly string[] Platforms = { "Standalone", "WebGL" };

    private void OnPreprocessTexture()
    {
        string path = assetPath.Replace('\\', '/');
        if (!path.Contains("/Resources/Floors/") && !path.Contains("/Resources/FloorsWide/"))
            return;

        var importer = (TextureImporter)assetImporter;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        importer.spritePixelsPerUnit = 100f;

        foreach (string platform in Platforms)
        {
            TextureImporterPlatformSettings settings = importer.GetPlatformTextureSettings(platform);
            settings.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SetPlatformTextureSettings(settings);
        }
    }

    [MenuItem("Tools/Reimport Wide Floor Sprites")]
    public static void ReimportWideFloors()
    {
        AssetDatabase.ImportAsset(WideFloorsFolder, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ImportRecursive);
        AssetDatabase.SaveAssets();
        Debug.Log($"[FloorSpriteImporter] Reimported {WideFloorsFolder}");
    }
}
