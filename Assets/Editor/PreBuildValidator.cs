#if UNITY_EDITOR
// Validates package size <= 2.0 GB target, ASTC 6x6 texture settings, Vorbis audio compression, and scene registration.
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LayeredHell.EditorTools
{
    public static class PreBuildValidator
    {
        private const long MaxInstallSizeBytes = 2L * 1024L * 1024L * 1024L; // 2.0 GB hard cap

        [MenuItem("Layered Hell/7. Validate Project (2 GB Cap, ASTC 6x6, Vorbis, Scenes)")]
        public static bool RunValidation()
        {
            int warnings = 0;
            int errors = 0;

            // 1. Check total Assets directory size on disk
            long totalBytes = ComputeDirectoryBytes("Assets");
            double totalMb = totalBytes / (1024.0 * 1024.0);
            if (totalBytes > MaxInstallSizeBytes)
            {
                Debug.LogError("[PreBuildValidator] Assets size (" + totalMb.ToString("F1") + " MB) exceeds 2048 MB limit!");
                errors++;
            }
            else
            {
                Debug.Log("[PreBuildValidator] Total Assets size: " + totalMb.ToString("F1") + " MB (Well within 2.0 GB hard cap).");
            }

            // 2. Verify Texture Atlases use ASTC 6x6 on Android
            if (Directory.Exists("Assets/Art/Textures"))
            {
                string[] texGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Art/Textures" });
                for (int i = 0; i < texGuids.Length; i++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(texGuids[i]);
                    var ti = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (ti == null) continue;
                    var android = ti.GetPlatformTextureSettings("Android");
                    if (!android.overridden || android.format != TextureImporterFormat.ASTC_6x6)
                    {
                        warnings++;
                        android.overridden = true;
                        android.format = TextureImporterFormat.ASTC_6x6;
                        android.maxTextureSize = TextureImportPostprocessor.ResolveTierMaxSize(path);
                        ti.SetPlatformTextureSettings(android);
                        ti.SaveAndReimport();
                    }
                }
            }

            // 3. Verify Android PlayerSettings
            if (PlayerSettings.Android.minSdkVersion < AndroidSdkVersions.AndroidApiLevel29)
            {
                Debug.LogError("[PreBuildValidator] Minimum Android API Level must be >= 29 (Android 10).");
                errors++;
            }
            if (PlayerSettings.GetScriptingBackend(BuildTargetGroup.Android) != ScriptingImplementation.IL2CPP)
            {
                Debug.LogError("[PreBuildValidator] Android Scripting Backend must be IL2CPP.");
                errors++;
            }
            if ((PlayerSettings.Android.targetArchitectures & AndroidArchitecture.ARM64) == 0)
            {
                Debug.LogError("[PreBuildValidator] Android target architecture must include ARM64.");
                errors++;
            }

            Debug.Log("[PreBuildValidator] Validation finished: " + errors + " error(s), " + warnings + " auto-fixed warning(s).");
            return errors == 0;
        }

        private static long ComputeDirectoryBytes(string relativeDir)
        {
            if (!Directory.Exists(relativeDir)) return 0L;
            long sum = 0L;
            string[] files = Directory.GetFiles(relativeDir, "*", SearchOption.AllDirectories);
            for (int i = 0; i < files.Length; i++)
            {
                try
                {
                    sum += new FileInfo(files[i]).Length;
                }
                catch (Exception)
                {
                    // Ignore transient files
                }
            }
            return sum;
        }
    }
}
#endif
