#if UNITY_EDITOR
// Enforces Vorbis (OGG) compression at quality 0.45 on Android and appropriate loadType by duration / loop prefix.
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LayeredHell.EditorTools
{
    public sealed class AudioImportPostprocessor : AssetPostprocessor
    {
        private void OnPreprocessAudio()
        {
            if (!assetPath.StartsWith("Assets/Audio/", StringComparison.OrdinalIgnoreCase))
                return;

            var importer = (AudioImporter)assetImporter;
            string fileName = Path.GetFileNameWithoutExtension(assetPath);
            bool isLoop = IsLoopingAudioName(fileName);

            importer.forceToMono = true;
            importer.loadInBackground = isLoop;

            var defaultSample = importer.defaultSampleSettings;
            defaultSample.compressionFormat = AudioCompressionFormat.Vorbis;
            defaultSample.quality = 0.45f;
            defaultSample.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;
            defaultSample.loadType = isLoop
                ? AudioClipLoadType.Streaming
                : AudioClipLoadType.CompressedInMemory;
            importer.defaultSampleSettings = defaultSample;

            var androidSample = importer.GetOverrideSampleSettings("Android");
            androidSample.compressionFormat = AudioCompressionFormat.Vorbis;
            androidSample.quality = 0.45f;
            androidSample.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;
            androidSample.loadType = isLoop
                ? AudioClipLoadType.Streaming
                : AudioClipLoadType.CompressedInMemory;
            importer.SetOverrideSampleSettings("Android", androidSample);
        }

        private void OnPostprocessAudio(AudioClip clip)
        {
            if (clip == null || !assetPath.StartsWith("Assets/Audio/", StringComparison.OrdinalIgnoreCase))
                return;

            string fileName = Path.GetFileNameWithoutExtension(assetPath);
            bool isLoop = IsLoopingAudioName(fileName);
            var importer = (AudioImporter)assetImporter;

            // Very short one-shot SFX (< 0.8s) get DecompressOnLoad for zero-latency combat/footsteps
            if (!isLoop && clip.length > 0f && clip.length < 0.8f)
            {
                var androidSample = importer.GetOverrideSampleSettings("Android");
                if (androidSample.loadType != AudioClipLoadType.DecompressOnLoad)
                {
                    androidSample.loadType = AudioClipLoadType.DecompressOnLoad;
                    importer.SetOverrideSampleSettings("Android", androidSample);
                }
            }
        }

        public static bool IsLoopingAudioName(string stem)
        {
            if (string.IsNullOrEmpty(stem)) return false;
            return stem.StartsWith("amb_", StringComparison.OrdinalIgnoreCase)
                || (stem.StartsWith("mus_", StringComparison.OrdinalIgnoreCase) && !stem.StartsWith("mus_stinger", StringComparison.OrdinalIgnoreCase))
                || stem.StartsWith("generator_loop", StringComparison.OrdinalIgnoreCase);
        }
    }
}
#endif
