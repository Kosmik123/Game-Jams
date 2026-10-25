using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using ZeroHour.Tower;

// Creates the VersionSelect scene (camera only, the menu is built from code)
// and puts it first in Build Settings, followed by Game.
// CLI: Unity -batchmode -quit -executeMethod VersionSelectSceneCreator.Create
public static class VersionSelectSceneCreator
{
    private const string ScenePath = "Assets/_Project/Scenes/VersionSelect/VersionSelect.unity";
    private const string GameScenePath = "Assets/_Project/Scenes/Game/Game.unity";

    [MenuItem("Tools/Create Version Select Scene")]
    public static void Create()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);
        var camera = cameraObject.GetComponent<Camera>();
        camera.orthographic = true;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = TowerGame.SkyColor;

        Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
        if (!EditorSceneManager.SaveScene(scene, ScenePath))
        {
            Debug.LogError($"[VersionSelectSceneCreator] Could not save {ScenePath}");
            if (Application.isBatchMode)
                EditorApplication.Exit(1);
            return;
        }

        var scenes = new List<EditorBuildSettingsScene>
        {
            new EditorBuildSettingsScene(ScenePath, true),
            new EditorBuildSettingsScene(GameScenePath, true),
        };
        scenes.AddRange(EditorBuildSettings.scenes.Where(s => s.path != ScenePath && s.path != GameScenePath));
        EditorBuildSettings.scenes = scenes.ToArray();
        AssetDatabase.SaveAssets();

        Debug.Log($"[VersionSelectSceneCreator] Saved {ScenePath}; build scenes: {string.Join(", ", scenes.Select(s => s.path))}");
    }
}
