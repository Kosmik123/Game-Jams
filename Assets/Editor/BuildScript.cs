using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Build entry points for CLI: Unity -batchmode -executeMethod BuildScript.<Target>
public static class BuildScript
{
    private const string BuildsFolder = "Builds";

    public static void Windows() => Build(BuildTarget.StandaloneWindows64, "windows", $"{SafeName()}.exe");

    public static void Linux() => Build(BuildTarget.StandaloneLinux64, "linux", SafeName());

    public static void WebGL()
    {
        // itch.io serves files without compression headers, so keep the build uncompressed.
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
        PlayerSettings.WebGL.decompressionFallback = false;
        Build(BuildTarget.WebGL, "webgl", string.Empty);
    }

    private static void Build(BuildTarget target, string folder, string fileName)
    {
        string[] scenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();

        if (scenes.Length == 0)
        {
            Debug.LogError("[BuildScript] No enabled scenes in Build Settings.");
            EditorApplication.Exit(2);
            return;
        }

        string outputDirectory = Path.Combine(BuildsFolder, folder);
        if (Directory.Exists(outputDirectory))
            Directory.Delete(outputDirectory, true);

        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            target = target,
            locationPathName = string.IsNullOrEmpty(fileName) ? outputDirectory : Path.Combine(outputDirectory, fileName),
            options = BuildOptions.None,
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        Debug.Log($"[BuildScript] {target}: {report.summary.result}, {report.summary.totalSize} bytes, {report.summary.totalErrors} errors");
        if (report.summary.result != BuildResult.Succeeded)
            EditorApplication.Exit(1);
    }

    private static string SafeName()
    {
        var name = Regex.Replace(PlayerSettings.productName ?? "", @"[^\w\-]+", "");
        return name.Length > 0 ? name : "Game";
    }
}
