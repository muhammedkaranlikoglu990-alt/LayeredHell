#if UNITY_EDITOR
// Configures imported FBX/GLB models under Assets/Art/Models/**, remaps materials to M_<ATLAS>.mat,
// optimizes meshes, and configures animation clips and loop flags.
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LayeredHell.EditorTools
{
    public sealed class ModelImportPostprocessor : AssetPostprocessor
    {
        private static readonly string[] LoopingClipNames =
        {
            "Idle", "Walk", "Run", "FPS_Idle", "FPS_Walk", "FPS_Run"
        };

        private void OnPreprocessModel()
        {
            if (!assetPath.StartsWith("Assets/Art/Models/", StringComparison.OrdinalIgnoreCase))
                return;

            var importer = (ModelImporter)assetImporter;
            importer.globalScale = 1.0f;
            importer.useFileUnits = true;
            importer.bakeAxisConversion = false;
            importer.importBlendShapes = false;
            importer.importVisibility = false;
            importer.importCameras = false;
            importer.importLights = false;
            importer.generateSecondaryUV = false;
            importer.meshOptimizationFlags = MeshOptimizationFlags.Everything;
            importer.meshCompression = ModelImporterMeshCompression.Low;
            importer.isReadable = false;

            string fileName = Path.GetFileNameWithoutExtension(assetPath).ToLowerInvariant();
            bool isRigged = fileName.StartsWith("zmb_", StringComparison.Ordinal)
                         || fileName.StartsWith("chr_", StringComparison.Ordinal);

            if (isRigged)
            {
                importer.animationType = ModelImporterAnimationType.Generic;
                importer.importAnimation = true;
                importer.animationCompression = ModelImporterAnimationCompression.KeyframeReduction;
            }
            else
            {
                importer.animationType = ModelImporterAnimationType.None;
                importer.importAnimation = false;
            }

            importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
            importer.materialLocation = ModelImporterMaterialLocation.External;
        }

        private void OnPreprocessAnimation()
        {
            if (!assetPath.StartsWith("Assets/Art/Models/", StringComparison.OrdinalIgnoreCase))
                return;

            var importer = (ModelImporter)assetImporter;
            var defaultClips = importer.defaultClipAnimations;
            if (defaultClips == null || defaultClips.Length == 0)
                return;

            for (int i = 0; i < defaultClips.Length; i++)
            {
                string clipName = CleanActionName(defaultClips[i].takeName);
                defaultClips[i].name = clipName;
                bool loop = IsLoopingClip(clipName);
                defaultClips[i].loopTime = loop;
                defaultClips[i].loopPose = loop;
            }
            importer.clipAnimations = defaultClips;
        }

        private Material OnAssignMaterialModel(Material material, Renderer renderer)
        {
            if (!assetPath.StartsWith("Assets/Art/Models/", StringComparison.OrdinalIgnoreCase))
                return null;

            string matName = material != null ? material.name : string.Empty;
            if (!string.IsNullOrEmpty(matName))
            {
                int dotIdx = matName.IndexOf('.');
                if (dotIdx > 0) matName = matName.Substring(0, dotIdx);
            }

            if (string.IsNullOrEmpty(matName) || !matName.StartsWith("M_", StringComparison.Ordinal))
            {
                matName = GuessMaterialNameFromPath(assetPath);
            }

            string matAssetPath = "Assets/Art/Materials/" + matName + ".mat";
            var existing = AssetDatabase.LoadAssetAtPath<Material>(matAssetPath);
            if (existing != null)
                return existing;

            return null;
        }

        private static string CleanActionName(string rawName)
        {
            if (string.IsNullOrEmpty(rawName)) return "Idle";
            int pipe = rawName.LastIndexOf('|');
            if (pipe >= 0 && pipe + 1 < rawName.Length)
                rawName = rawName.Substring(pipe + 1);
            return rawName.Trim();
        }

        private static bool IsLoopingClip(string clipName)
        {
            for (int i = 0; i < LoopingClipNames.Length; i++)
            {
                if (string.Equals(clipName, LoopingClipNames[i], StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        public static string GuessMaterialNameFromPath(string modelAssetPath)
        {
            string lower = modelAssetPath.ToLowerInvariant();
            if (lower.Contains("/weapons/")) return "M_HERO_WEAPONS_1K";
            if (lower.Contains("/zombies/")) return "M_HERO_ZOMBIE_1K";
            if (lower.Contains("/characters/")) return "M_HERO_SURVIVOR_1K";
            if (lower.Contains("/architecture/")) return "M_ENV_STRUCT_2K";
            if (lower.Contains("/city/") || lower.Contains("/vehicles/")) return "M_ENV_CITY_2K";
            if (lower.Contains("/loot/")) return "M_PROP_A_512";
            return "M_ENV_MAT_2K";
        }
    }
}
#endif
