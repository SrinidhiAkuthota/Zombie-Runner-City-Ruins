using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ZombieRunner.EditorTools
{
    /// <summary>
    /// On first open of an empty project, creates and saves Assets/Scenes/ZombieRunner.unity
    /// and adds it to Build Settings. (The game itself works in any scene - see GameManager.Create.)
    /// </summary>
    [InitializeOnLoad]
    public static class SetupScene
    {
        const string ScenePath = "Assets/Scenes/ZombieRunner.unity";

        static SetupScene()
        {
            EditorApplication.delayCall += EnsureScene;
        }

        static void EnsureScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            if (AssetDatabase.FindAssets("t:Scene", new[] { "Assets" }).Length > 0) return;
            CreateScene();
        }

        [MenuItem("Zombie Runner/Create Game Scene")]
        static void CreateScene()
        {
            System.IO.Directory.CreateDirectory("Assets/Scenes");
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            AssetDatabase.Refresh();
            Debug.Log("[Zombie Runner] Scene ready: " + ScenePath + " - press Play!");
        }

        [MenuItem("Zombie Runner/Reset Best Score")]
        static void ResetBest()
        {
            PlayerPrefs.DeleteKey("ZR_Best");
        }
    }
}
