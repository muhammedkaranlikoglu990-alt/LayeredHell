#if UNITY_EDITOR
// Scans Assets/Art/Models/** (and catalog_manifest.json fallback), builds rigged prefabs with Animator + AC_<id>,
// registers Addressables entries (address == model id), and writes Assets/Resources/ModelLibrary.asset.
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using UnityEngine;

namespace LayeredHell.EditorTools
{
    public static class ModelLibraryBuilder
    {
        [Serializable]
        private class CatalogManifestItem
        {
            public string id;
            public string category;
            public string rig;
        }

        [Serializable]
        private class CatalogManifestRoot
        {
            public int count;
            public List<CatalogManifestItem> models;
        }

        [MenuItem("Layered Hell/3. Build ModelLibrary & Addressables")]
        public static void BuildModelLibrary()
        {
            if (!Directory.Exists("Assets/Resources"))
                Directory.CreateDirectory("Assets/Resources");
            if (!Directory.Exists("Assets/Art/Prefabs"))
                Directory.CreateDirectory("Assets/Art/Prefabs");

            const string assetPath = "Assets/Resources/ModelLibrary.asset";
            var library = AssetDatabase.LoadAssetAtPath<ModelLibrary>(assetPath);
            if (library == null)
            {
                library = ScriptableObject.CreateInstance<ModelLibrary>();
                AssetDatabase.CreateAsset(library, assetPath);
            }

            var entries = new List<ModelLibrary.Entry>();
            var seenIds = new HashSet<string>(StringComparer.Ordinal);

            AddressableAssetSettings addrSettings = AddressableAssetSettingsDefaultObject.GetSettings(true);
            AddressableAssetGroup addrGroup = null;
            if (addrSettings != null)
            {
                addrGroup = addrSettings.FindGroup("LayeredHell_Models");
                if (addrGroup == null)
                {
                    addrGroup = addrSettings.CreateGroup(
                        "LayeredHell_Models",
                        false,
                        false,
                        true,
                        null,
                        typeof(BundledAssetGroupSchema),
                        typeof(ContentUpdateGroupSchema));
                }
            }

            // 1. Scan imported FBX/GLB files under Assets/Art/Models
            if (Directory.Exists("Assets/Art/Models"))
            {
                string[] guids = AssetDatabase.FindAssets("t:GameObject", new[] { "Assets/Art/Models" });
                for (int i = 0; i < guids.Length; i++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                    if (!path.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase) &&
                        !path.EndsWith(".glb", StringComparison.OrdinalIgnoreCase))
                        continue;

                    string id = Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
                    if (!seenIds.Add(id)) continue;

                    string category = ExtractCategoryFromPath(path);
                    bool isRigged = id.StartsWith("zmb_", StringComparison.Ordinal) ||
                                    id.StartsWith("chr_", StringComparison.Ordinal);
                    bool isFps = id.StartsWith("chr_fps_arms_", StringComparison.Ordinal);

                    string targetAddressPath = path;
                    GameObject targetAsset = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                    if (isRigged && targetAsset != null)
                    {
                        var controller = AnimatorControllerBuilder.BuildControllerForModel(id, path, isFps);
                        string prefabPath = "Assets/Art/Prefabs/" + id + ".prefab";
                        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(targetAsset);
                        var animator = instance.GetComponent<Animator>();
                        if (animator == null) animator = instance.AddComponent<Animator>();
                        animator.runtimeAnimatorController = controller;
                        animator.applyRootMotion = false;
                        animator.cullingMode = isFps
                            ? AnimatorCullingMode.AlwaysAnimate
                            : AnimatorCullingMode.CullUpdateTransforms;

                        PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
                        UnityEngine.Object.DestroyImmediate(instance);
                        targetAddressPath = prefabPath;
                    }

                    if (addrSettings != null && addrGroup != null)
                    {
                        string targetGuid = AssetDatabase.AssetPathToGUID(targetAddressPath);
                        if (!string.IsNullOrEmpty(targetGuid))
                        {
                            var addrEntry = addrSettings.CreateOrMoveEntry(targetGuid, addrGroup, false, false);
                            addrEntry.address = id;
                        }
                    }

                    entries.Add(new ModelLibrary.Entry
                    {
                        id = id,
                        category = category,
                        address = id,
                        rigged = isRigged,
                        directPrefab = null
                    });
                }
            }

            // 2. Also populate any remaining entries from Tools/Blender/catalog_manifest.json
            // so prefix queries (ModelLibrary.GetRandomId) return all 840+ model IDs even before Blender export!
            string manifestPath = Path.Combine(Directory.GetCurrentDirectory(), "Tools/Blender/catalog_manifest.json");
            if (File.Exists(manifestPath))
            {
                try
                {
                    string json = File.ReadAllText(manifestPath);
                    var manifest = JsonUtility.FromJson<CatalogManifestRoot>(json);
                    if (manifest != null && manifest.models != null)
                    {
                        for (int i = 0; i < manifest.models.Count; i++)
                        {
                            var m = manifest.models[i];
                            if (string.IsNullOrEmpty(m.id) || !seenIds.Add(m.id))
                                continue;

                            entries.Add(new ModelLibrary.Entry
                            {
                                id = m.id,
                                category = m.category ?? "Props",
                                address = m.id,
                                rigged = m.rig != null && m.rig != "none",
                                directPrefab = null
                            });
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning("[LayeredHell] Could not parse catalog_manifest.json: " + ex.Message);
                }
            }

            entries.Sort((a, b) => string.CompareOrdinal(a.id, b.id));
            library.SetEntries(entries);
            EditorUtility.SetDirty(library);
            if (addrSettings != null)
                EditorUtility.SetDirty(addrSettings);
            AssetDatabase.SaveAssets();

            Debug.Log("[LayeredHell] ModelLibrary built with " + entries.Count + " model entries.");
        }

        private static string ExtractCategoryFromPath(string assetPath)
        {
            string[] parts = assetPath.Replace('\\', '/').Split('/');
            for (int i = 0; i < parts.Length - 1; i++)
            {
                if (string.Equals(parts[i], "Models", StringComparison.OrdinalIgnoreCase) && i + 1 < parts.Length - 1)
                    return parts[i + 1];
            }
            return "Architecture";
        }
    }
}
#endif
