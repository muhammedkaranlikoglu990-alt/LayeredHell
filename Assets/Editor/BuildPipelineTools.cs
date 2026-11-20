#if UNITY_EDITOR
// Android APK and AAB build pipeline tools (Editor menu + headless CLI entrypoint).
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace LayeredHell.EditorTools
{
    public static class BuildPipelineTools
    {
        [MenuItem("Layered Hell/8. Build Android APK (ARM64 IL2CPP)")]
        public static void BuildAndroidAPK()
        {
            ExecuteAndroidBuild(buildAab: false, outputPath: "Builds/Android/LayeredHell45.apk");
        }

        [MenuItem("Layered Hell/9. Build Android App Bundle AAB (Google Play)")]
        public static void BuildAndroidAAB()
        {
            ExecuteAndroidBuild(buildAab: true, outputPath: "Builds/Android/LayeredHell45.aab");
        }

        /// <summary>
        /// Headless CLI entrypoint:
        /// unity -batchmode -quit -projectPath . -executeMethod LayeredHell.EditorTools.BuildPipelineTools.BuildAndroid
        /// </summary>
        public static void BuildAndroid()
        {
            ProjectSetup.RunFullSetup();
            ExecuteAndroidBuild(buildAab: false, outputPath: "Builds/Android/LayeredHell45.apk");
        }

        private static void ExecuteAndroidBuild(bool buildAab, string outputPath)
        {
            ProjectSetup.ConfigureAndroidPlayerSettings();
            if (!PreBuildValidator.RunValidation())
            {
                Debug.LogError("[BuildPipelineTools] PreBuildValidator failed; aborting Android build.");
                return;
            }

            if (EditorBuildSettings.scenes == null || EditorBuildSettings.scenes.Length == 0)
            {
                SceneGenerator.GenerateAllScenes();
            }

            var enabledScenes = new List<string>();
            foreach (var s in EditorBuildSettings.scenes)
            {
                if (s.enabled) enabledScenes.Add(s.path);
            }

            string dir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            EditorUserBuildSettings.buildAppBundle = buildAab;
            EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.ASTC;

            var options = new BuildPlayerOptions
            {
                scenes = enabledScenes.ToArray(),
                locationPathName = outputPath,
                target = BuildTarget.Android,
                options = BuildOptions.CompressWithLz4HC
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            BuildSummary summary = report.summary;

            if (summary.result == BuildResult.Succeeded)
            {
                double mb = summary.totalSize / (1024.0 * 1024.0);
                Debug.Log("[BuildPipelineTools] Android build SUCCEEDED: " + outputPath + " (" + mb.ToString("F1") + " MB)");
            }
            else
            {
                Debug.LogError("[BuildPipelineTools] Android build FAILED with " + summary.totalErrors + " errors.");
            }
        }
    }
}
#endif
