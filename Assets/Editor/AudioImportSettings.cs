using UnityEditor;
using UnityEngine;

// Import settings for Resources/Audio: short effects are decompressed on load (PCM, mono),
// the music loop stays compressed in memory (Vorbis) and loads in the background.
// CLI reimport: Unity -batchmode -quit -executeMethod AudioImportSettings.ReimportAudio
public class AudioImportSettings : AssetPostprocessor
{
    private const string AudioFolder = "Assets/_Project/Resources/Audio";
    private const string MusicPrefix = "music_";

    private void OnPreprocessAudio()
    {
        string path = assetPath.Replace('\\', '/');
        if (!path.Contains("/Resources/Audio/"))
            return;

        var importer = (AudioImporter)assetImporter;
        importer.forceToMono = true;
        bool isMusic = System.IO.Path.GetFileName(path).StartsWith(MusicPrefix);
        importer.loadInBackground = isMusic;

        AudioImporterSampleSettings settings = importer.defaultSampleSettings;
        if (isMusic)
        {
            settings.loadType = AudioClipLoadType.CompressedInMemory;
            settings.compressionFormat = AudioCompressionFormat.Vorbis;
            settings.quality = 0.5f;
        }
        else
        {
            settings.loadType = AudioClipLoadType.DecompressOnLoad;
            settings.compressionFormat = AudioCompressionFormat.PCM;
        }
        importer.defaultSampleSettings = settings;
    }

    [MenuItem("Tools/Reimport Audio")]
    public static void ReimportAudio()
    {
        AssetDatabase.ImportAsset(AudioFolder, ImportAssetOptions.ForceUpdate | ImportAssetOptions.ImportRecursive);
        AssetDatabase.SaveAssets();
        Debug.Log($"[AudioImportSettings] Reimported {AudioFolder}");
    }
}
