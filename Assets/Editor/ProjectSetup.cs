#if UNITY_EDITOR
// Master one-click setup for Unity 6 Android (API 29-35+, ARM64, IL2CPP, Vulkan+GLES3, ASTC 6x6, URP, 53 Scenes, Addressables).
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace LayeredHell.EditorTools
{
    [InitializeOnLoad]
    public static class ProjectSetup
    {
        private const string SetupPrefKey = "LayeredHell_45_AutoSetupDone_v1";

        static ProjectSetup()
        {
            EditorApplication.delayCall += TryRunFirstTimeSetup;
        }

        private static void TryRunFirstTimeSetup()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;
            if (SessionState.GetBool(SetupPrefKey, false))
                return;

            SessionState.SetBool(SetupPrefKey, true);
            ConfigureAndroidPlayerSettings();
        }

        [MenuItem("Layered Hell/1. Full Setup (Android API 29-35 + URP + Materials + Libraries + 53 Scenes)")]
        public static void RunFullSetup()
        {
            ConfigureAndroidPlayerSettings();
            EnsureLayersAndTags();
            URPAssetBuilder.BuildURPAssets();
            MaterialBuilder.BuildAllMaterials();
            ModelLibraryBuilder.BuildModelLibrary();
            AudioBankBuilder.BuildAudioBank();
            SceneGenerator.GenerateAllScenes();
            PreBuildValidator.RunValidation();

            Debug.Log("[LayeredHell] === 45: LAYERED HELL FULL UNITY 6 SETUP COMPLETE ===");
        }

        public static void ConfigureAndroidPlayerSettings()
        {
            PlayerSettings.companyName = "AethelVanguardStudios";
            PlayerSettings.productName = "45: Layered Hell";
            PlayerSettings.bundleVersion = "1.0.0";
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.aethelvanguard.layeredhell45");

            // Orientation: Landscape
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.AutoRotation;
            PlayerSettings.allowedAutorotateToPortrait = false;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = true;
            PlayerSettings.allowedAutorotateToLandscapeRight = true;

            // Android API Levels: Min Android 10 (API 29), Target Android 15 (API 35)
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29;
            PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)35;

            // IL2CPP + ARM64 + .NET Standard 2.1 + Strip Engine Code
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Android, ApiCompatibilityLevel.NET_Standard_2_0);
            PlayerSettings.stripEngineCode = true;
            PlayerSettings.SetManagedStrippingLevel(BuildTargetGroup.Android, ManagedStrippingLevel.Medium);

            // Graphics APIs: Vulkan first, OpenGLES3 fallback
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[]
            {
                GraphicsDeviceType.Vulkan,
                GraphicsDeviceType.OpenGLES3
            });

            // ASTC texture compression subtarget
            EditorUserBuildSettings.androidBuildSubtarget = MobileTextureSubtarget.ASTC;

            AssetDatabase.SaveAssets();
            Debug.Log("[LayeredHell] Configured Android PlayerSettings (Min API 29, Target API 35, IL2CPP, ARM64, Vulkan+GLES3, ASTC).");
        }

        public static void EnsureLayersAndTags()
        {
            SerializedObject tagManager = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            SerializedProperty layersProp = tagManager.FindProperty("layers");
            if (layersProp != null)
            {
                SetLayerIfEmpty(layersProp, 8, LayerNames.Player);
                SetLayerIfEmpty(layersProp, 9, LayerNames.Enemy);
                SetLayerIfEmpty(layersProp, 10, LayerNames.Interactable);
                tagManager.ApplyModifiedProperties();
            }
        }

        private static void SetLayerIfEmpty(SerializedProperty layersProp, int index, string layerName)
        {
            if (index >= layersProp.arraySize) return;
            SerializedProperty sp = layersProp.GetArrayElementAtIndex(index);
            if (sp != null && string.IsNullOrEmpty(sp.stringValue))
                sp.stringValue = layerName;
        }
    }
}
#endif
