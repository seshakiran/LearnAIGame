using System.IO;
using LearnAIGame.Gameplay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace LearnAIGame.EditorTools
{
    /// One-time (re-runnable) scene generator so Spike A needs zero manual scene wiring.
    /// Run via: Unity -batchmode -executeMethod LearnAIGame.EditorTools.BootstrapSceneBuilder.CreateBootstrapScene -quit
    public static class BootstrapSceneBuilder
    {
        [MenuItem("LearnAIGame/Build Bootstrap Scene")]
        public static void CreateBootstrapScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var go = new GameObject("GameLoop");
            go.AddComponent<GameLoopController>();

            Directory.CreateDirectory("Assets/Scenes");
            const string path = "Assets/Scenes/Bootstrap.unity";
            EditorSceneManager.SaveScene(scene, path);

            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(path, true) };

            Debug.Log($"Bootstrap scene created at {path}");
        }
    }
}
