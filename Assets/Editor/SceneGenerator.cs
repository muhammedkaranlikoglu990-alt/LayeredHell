#if UNITY_EDITOR
// Generates Boot.unity, MainMenu.unity, GameWorld.unity, and all 50 additive floor scenes
// (Assets/Scenes/Floors/Floor_M04.unity .. Floor_P45.unity), registering all 53 scenes in Build Settings.
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LayeredHell.EditorTools
{
    public static class SceneGenerator
    {
        [MenuItem("Layered Hell/6. Generate All 53 Scenes (Boot + MainMenu + GameWorld + 50 Floors)")]
        public static void GenerateAllScenes()
        {
            if (!Directory.Exists("Assets/Scenes"))
                Directory.CreateDirectory("Assets/Scenes");
            if (!Directory.Exists("Assets/Scenes/Floors"))
                Directory.CreateDirectory("Assets/Scenes/Floors");

            var buildScenes = new List<EditorBuildSettingsScene>();

            // 1. Boot scene
            string bootPath = "Assets/Scenes/" + GameConstants.BootScene + ".unity";
            {
                Scene bootScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                var bootGo = new GameObject("GameBootstrap");
                bootGo.AddComponent<GameBootstrap>();
                var renderGo = new GameObject("[RenderingController]");
                renderGo.AddComponent<GameRenderingController>();
                renderGo.AddComponent<ProceduralSkyDarkener>();
                EditorSceneManager.SaveScene(bootScene, bootPath);
                buildScenes.Add(new EditorBuildSettingsScene(bootPath, true));
            }

            // 2. MainMenu scene
            string menuPath = "Assets/Scenes/" + GameConstants.MainMenuScene + ".unity";
            {
                Scene menuScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                var camGo = new GameObject("MainMenuCamera");
                camGo.tag = "MainCamera";
                var cam = camGo.AddComponent<Camera>();
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.03f, 0.04f, 0.05f, 1f);
                camGo.AddComponent<AudioListener>();
                EditorSceneManager.SaveScene(menuScene, menuPath);
                buildScenes.Add(new EditorBuildSettingsScene(menuPath, true));
            }

            // 3. GameWorld scene (single persistent lighting & atmosphere shell)
            string worldPath = "Assets/Scenes/" + GameConstants.GameWorldScene + ".unity";
            {
                Scene worldScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                var envRoot = new GameObject("WorldAtmosphere");
                envRoot.AddComponent<ProceduralSkyDarkener>();
                EditorSceneManager.SaveScene(worldScene, worldPath);
                buildScenes.Add(new EditorBuildSettingsScene(worldPath, true));
            }

            // 4. 50 Additive Floor Scenes (-4 .. 45)
            for (int f = GameConstants.MinFloor; f <= GameConstants.MaxFloor; f++)
            {
                string sceneName = GameConstants.FloorSceneName(f);
                string floorPath = "Assets/Scenes/Floors/" + sceneName + ".unity";
                Scene floorScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

                var rootGo = new GameObject(sceneName + "_Root");
                rootGo.transform.position = new Vector3(0f, GameConstants.FloorY(f), 0f);
                var chunk = rootGo.AddComponent<FloorChunk>();
                chunk.floor = f;

                EditorSceneManager.SaveScene(floorScene, floorPath);
                buildScenes.Add(new EditorBuildSettingsScene(floorPath, true));
            }

            EditorBuildSettings.scenes = buildScenes.ToArray();
            AssetDatabase.SaveAssets();

            // Re-open Boot scene as default editor entry
            EditorSceneManager.OpenScene(bootPath, OpenSceneMode.Single);
            Debug.Log("[LayeredHell] Generated and registered " + buildScenes.Count + " scenes (Boot + MainMenu + GameWorld + 50 Floors).");
        }
    }
}
#endif
