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
            PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly;   // Unity Services (tablica) lapie wyjatki; przy None WebGL ginie z "Uncaught undefined"
            // rozmiar: agresywny stripping kodu zarzadzanego i IL2CPP pod rozmiar
            PlayerSettings.SetManagedStrippingLevel(UnityEditor.Build.NamedBuildTarget.WebGL, ManagedStrippingLevel.High);
            PlayerSettings.SetIl2CppCodeGeneration(UnityEditor.Build.NamedBuildTarget.WebGL, UnityEditor.Build.Il2CppCodeGeneration.OptimizeSize);

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

            // wszystkie nazwy plikow w katalogu buildu (rekurencyjnie), z prawdziwa wielkoscia liter
            var actualNames = new System.Collections.Generic.List<string>();
            foreach (var path in Directory.GetFiles(dir, "*", SearchOption.AllDirectories))
                actualNames.Add(Path.GetFileName(path));

            bool ok = true, changed = false;
            // literaly typu "Build/WebGL.loader.js" albo "/WebGL.data" (sciezka skladana w JS z buildUrl + "/" + nazwa)
            var rx = new Regex("([\"'])((?:[\\w./-]*/)?)([\\w.-]+\\.(?:js|data|wasm|json|css|ttf|ico|png|jpg|svg|unityweb))\\1");
            foreach (Match m in rx.Matches(html))
            {
                string prefix = m.Groups[2].Value, name = m.Groups[3].Value;
                if (prefix.StartsWith("http") || prefix.Contains("..")) continue;
                if (actualNames.Contains(name)) continue;   // dokladne dopasowanie (Ordinal)
                string actual = actualNames.Find(n => string.Equals(n, name, StringComparison.OrdinalIgnoreCase));
                if (actual == null) { Debug.LogError("index.html references missing file: " + name); ok = false; continue; }
                Debug.LogWarning("Fixing case in index.html: " + name + " -> " + actual);
                html = html.Replace(m.Value, m.Groups[1].Value + prefix + actual + m.Groups[1].Value);
                changed = true;
            }
            if (changed) File.WriteAllText(indexPath, html);
            Debug.Log("index.html path check: " + (ok ? "OK" : "FAILED") + (changed ? " (fixed case)" : ""));
            return ok;
        }
    }
}
