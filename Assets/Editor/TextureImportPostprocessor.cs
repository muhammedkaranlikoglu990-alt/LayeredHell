#if UNITY_EDITOR
// Automatically enforces ASTC 6x6 texture compression on Android and tier-appropriate max sizes (2K / 1K / 512).
using System;
using UnityEditor;
using UnityEngine;

namespace LayeredHell.EditorTools
{
    public sealed class TextureImportPostprocessor : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith("Assets/Art/Textures/", StringComparison.OrdinalIgnoreCase))
                return;

            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Default;
            importer.sRGBTexture = true;
            importer.alphaIsTransparency = false;
            importer.mipmapEnabled = true;
            importer.streamingMipmaps = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.anisoLevel = 2;

            int maxSize = ResolveTierMaxSize(assetPath);
            importer.maxTextureSize = maxSize;
            importer.wrapMode = assetPath.IndexOf("ENV_", StringComparison.OrdinalIgnoreCase) >= 0
                ? TextureWrapMode.Repeat
                : TextureWrapMode.Clamp;

            var androidSettings = importer.GetPlatformTextureSettings("Android");
            androidSettings.overridden = true;
            androidSettings.maxTextureSize = maxSize;
            androidSettings.format = TextureImporterFormat.ASTC_6x6;
            androidSettings.compressionQuality = 50;
            importer.SetPlatformTextureSettings(androidSettings);
        }

        public static int ResolveTierMaxSize(string path)
        {
            if (path.IndexOf("_2K", StringComparison.OrdinalIgnoreCase) >= 0) return 2048;
            if (path.IndexOf("_1K", StringComparison.OrdinalIgnoreCase) >= 0) return 1024;
            if (path.IndexOf("_512", StringComparison.OrdinalIgnoreCase) >= 0) return 512;
            return 1024;
        }
    }
}
#endif
