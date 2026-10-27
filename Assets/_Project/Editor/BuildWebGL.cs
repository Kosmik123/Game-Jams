using System;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace PayWithPixels.Build
{
    /// <summary>
    /// Build WebGL z linii polecen:
    /// Unity.exe -batchmode -nographics -quit -projectPath ... -executeMethod PayWithPixels.Build.WebGLBuilder.BuildWebGL
    /// </summary>
    public static class WebGLBuilder
    {
        private const string ScenePath = "Assets/_Project/Scenes/Game/Game.unity";
        private const string OutputPath = "Builds/WebGL";

        [MenuItem("Jam/Build WebGL")]
        public static void BuildWebGL()
        {
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
            PlayerSettings.WebGL.decompressionFallback = false;
            PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.None;

            var options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = OutputPath,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;
            Debug.Log("WebGL build: " + summary.result + ", " + summary.totalSize / 1024 / 1024 + " MB, " + summary.totalErrors + " errors");

            if (Application.isBatchMode)
                EditorApplication.Exit(summary.result == BuildResult.Succeeded ? 0 : 1);
        }
    }
}
