using System;
using System.IO;
using System.Text.RegularExpressions;
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
            PlayerSettings.defaultWebScreenWidth = 1280;   // canvas 16:9 (RT 320x180, skalowany calkowicie)
            PlayerSettings.defaultWebScreenHeight = 720;
            PlayerSettings.WebGL.template = "PROJECT:PayWithPixels";
            PlayerSettings.SplashScreen.show = false;
            PlayerSettings.SplashScreen.backgroundColor = Color.black;
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

            bool ok = summary.result == BuildResult.Succeeded && CheckIndexPaths(OutputPath);
            if (Application.isBatchMode)
                EditorApplication.Exit(ok ? 0 : 1);
        }

        /// <summary>
        /// itch.io rozroznia wielkosc liter, Windows nie: kazda sciezka z index.html musi istniec z dokladna wielkoscia liter.
        /// Roznica tylko w wielkosci liter jest poprawiana w index.html; brak pliku = blad.
        /// </summary>
        public static bool CheckIndexPaths(string dir)
        {
            string indexPath = Path.Combine(dir, "index.html");
            if (!File.Exists(indexPath)) { Debug.LogError("index.html missing in " + dir); return false; }
            string html = File.ReadAllText(indexPath);
            bool ok = true, changed = false;
            var rx = new Regex("([\"'])((?:[\\w./-]+/)?[\\w.-]+\\.(?:js|data|wasm|json|css|ttf|ico|png|jpg|svg|unityweb))\\1");
            foreach (Match m in rx.Matches(html))
            {
                string rel = m.Groups[2].Value;
                if (rel.StartsWith("http") || rel.Contains("..")) continue;
                string full = Path.Combine(dir, rel);
                string folder = Path.GetDirectoryName(full);
                string name = Path.GetFileName(full);
                string actual = null;
                if (Directory.Exists(folder))
                    foreach (var f in Directory.GetFiles(folder))
                        if (string.Equals(Path.GetFileName(f), name, StringComparison.OrdinalIgnoreCase)) { actual = Path.GetFileName(f); break; }
                if (actual == null) { Debug.LogError("index.html references missing file: " + rel); ok = false; }
                else if (actual != name)
                {
                    string fixedRel = rel.Substring(0, rel.Length - name.Length) + actual;
                    Debug.LogWarning("Fixing case in index.html: " + rel + " -> " + fixedRel);
                    html = html.Replace(m.Value, m.Groups[1].Value + fixedRel + m.Groups[1].Value);
                    changed = true;
                }
            }
            if (changed) File.WriteAllText(indexPath, html);
            Debug.Log("index.html path check: " + (ok ? "OK" : "FAILED"));
            return ok;
        }
    }
}
