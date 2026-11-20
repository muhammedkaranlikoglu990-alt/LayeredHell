#if UNITY_EDITOR
// Scans Assets/Audio/** for <base>_NN.wav clips and populates Assets/Resources/AudioBank.asset.
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace LayeredHell.EditorTools
{
    public static class AudioBankBuilder
    {
        [MenuItem("Layered Hell/4. Build AudioBank")]
        public static void BuildAudioBank()
        {
            if (!Directory.Exists("Assets/Resources"))
                Directory.CreateDirectory("Assets/Resources");

            const string bankPath = "Assets/Resources/AudioBank.asset";
            var bank = AssetDatabase.LoadAssetAtPath<AudioBank>(bankPath);
            if (bank == null)
            {
                bank = ScriptableObject.CreateInstance<AudioBank>();
                AssetDatabase.CreateAsset(bank, bankPath);
            }

            var grouped = new Dictionary<string, List<AudioClip>>(StringComparer.Ordinal);
            var categoryByBase = new Dictionary<string, string>(StringComparer.Ordinal);

            if (Directory.Exists("Assets/Audio"))
            {
                string[] guids = AssetDatabase.FindAssets("t:AudioClip", new[] { "Assets/Audio" });
                for (int i = 0; i < guids.Length; i++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                    var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
                    if (clip == null) continue;

                    string stem = Path.GetFileNameWithoutExtension(path);
                    string baseId = StripVariationSuffix(stem);
                    string category = ExtractCategory(path);

                    List<AudioClip> list;
                    if (!grouped.TryGetValue(baseId, out list))
                    {
                        list = new List<AudioClip>();
                        grouped[baseId] = list;
                        categoryByBase[baseId] = category;
                    }
                    list.Add(clip);
                }
            }

            var entries = new List<AudioBank.Entry>(grouped.Count);
            foreach (var kv in grouped)
            {
                kv.Value.Sort((a, b) => string.CompareOrdinal(a.name, b.name));
                entries.Add(new AudioBank.Entry
                {
                    baseId = kv.Key,
                    category = categoryByBase[kv.Key],
                    loop = AudioImportPostprocessor.IsLoopingAudioName(kv.Key),
                    clips = kv.Value.ToArray()
                });
            }

            entries.Sort((a, b) => string.CompareOrdinal(a.baseId, b.baseId));
            bank.SetEntries(entries);
            EditorUtility.SetDirty(bank);
            AssetDatabase.SaveAssets();

            Debug.Log("[LayeredHell] AudioBank built with " + entries.Count + " sound bases.");
        }

        public static string StripVariationSuffix(string stem)
        {
            if (string.IsNullOrEmpty(stem)) return string.Empty;
            int idx = stem.LastIndexOf('_');
            if (idx > 0 && idx + 1 < stem.Length)
            {
                bool allDigits = true;
                for (int i = idx + 1; i < stem.Length; i++)
                {
                    if (!char.IsDigit(stem[i]))
                    {
                        allDigits = false;
                        break;
                    }
                }
                if (allDigits)
                    return stem.Substring(0, idx);
            }
            return stem;
        }

        private static string ExtractCategory(string path)
        {
            string[] parts = path.Replace('\\', '/').Split('/');
            for (int i = 0; i < parts.Length - 1; i++)
            {
                if (string.Equals(parts[i], "Audio", StringComparison.OrdinalIgnoreCase) && i + 1 < parts.Length - 1)
                    return parts[i + 1];
            }
            return "World";
        }
    }
}
#endif
