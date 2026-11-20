// Runtime model registry + Addressables / Resources loader with procedural primitive fallback.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LayeredHell
{
    [CreateAssetMenu(menuName = "LayeredHell/ModelLibrary", fileName = "ModelLibrary")]
    public sealed class ModelLibrary : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            public string id;
            public string category;
            public string address;
            public bool rigged;
            public GameObject directPrefab;
        }

        [SerializeField] private List<Entry> entries = new List<Entry>();

        private static ModelLibrary _instance;
        public static ModelLibrary Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<ModelLibrary>("ModelLibrary");
                    if (_instance == null)
                    {
                        _instance = CreateInstance<ModelLibrary>();
                        _instance.name = "ModelLibrary_RuntimeFallback";
                    }
                    _instance.BuildLookup();
                }
                return _instance;
            }
        }

        private Dictionary<string, Entry> _byId;
        private Dictionary<string, List<string>> _prefixCache;
        private readonly Dictionary<string, GameObject> _loadedCache = new Dictionary<string, GameObject>(StringComparer.Ordinal);
        private static Material _sharedPlaceholderMaterial;

        public List<Entry> Entries { get { return entries; } }

        private void OnEnable()
        {
            BuildLookup();
        }

        public void SetEntries(List<Entry> list)
        {
            entries = list ?? new List<Entry>();
            BuildLookup();
        }

        private void BuildLookup()
        {
            _byId = new Dictionary<string, Entry>(StringComparer.Ordinal);
            _prefixCache = new Dictionary<string, List<string>>(StringComparer.Ordinal);
            if (entries == null) entries = new List<Entry>();
            for (int i = 0; i < entries.Count; i++)
            {
                var e = entries[i];
                if (!string.IsNullOrEmpty(e.id))
                {
                    _byId[e.id] = e;
                }
            }
        }

        public bool Has(string id)
        {
            if (string.IsNullOrEmpty(id)) return false;
            if (_byId == null) BuildLookup();
            return _byId.ContainsKey(id);
        }

        public GameObject Get(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            if (_byId == null) BuildLookup();

            GameObject cached;
            if (_loadedCache.TryGetValue(id, out cached) && cached != null)
            {
                return cached;
            }

            Entry entry;
            if (_byId.TryGetValue(id, out entry))
            {
                if (entry.directPrefab != null)
                {
                    _loadedCache[id] = entry.directPrefab;
                    return entry.directPrefab;
                }
            }

            // Try Resources fallback
            var res = Resources.Load<GameObject>("Models/" + id);
            if (res != null)
            {
                _loadedCache[id] = res;
                return res;
            }

            return null;
        }

        public IReadOnlyList<string> IdsWithPrefix(string prefix)
        {
            if (string.IsNullOrEmpty(prefix)) return Array.Empty<string>();
            if (_byId == null) BuildLookup();

            List<string> list;
            if (_prefixCache.TryGetValue(prefix, out list))
            {
                return list;
            }

            list = new List<string>();
            for (int i = 0; i < entries.Count; i++)
            {
                var id = entries[i].id;
                if (!string.IsNullOrEmpty(id) && id.StartsWith(prefix, StringComparison.Ordinal))
                {
                    list.Add(id);
                }
            }
            _prefixCache[prefix] = list;
            return list;
        }

        public string GetRandomId(string prefix, System.Random rng)
        {
            var list = IdsWithPrefix(prefix);
            if (list == null || list.Count == 0)
            {
                return prefix.EndsWith("_", StringComparison.Ordinal) ? prefix + "01" : prefix + "_01";
            }
            int idx = rng != null ? rng.Next(list.Count) : UnityEngine.Random.Range(0, list.Count);
            return list[idx];
        }

        public GameObject GetRandom(string prefix, System.Random rng)
        {
            string id = GetRandomId(prefix, rng);
            return Get(id);
        }

        public void ReleaseAll()
        {
            _loadedCache.Clear();
        }

        /// <summary>
        /// Instantiates the model prefab if imported, otherwise builds a clean primitive placeholder with collider
        /// so the entire 50-floor building is playable even before Blender FBX import.
        /// </summary>
        public static GameObject InstantiateOrPlaceholder(string id, Transform parent, Vector3 fallbackSize, Color fallbackColor)
        {
            var prefab = Instance != null ? Instance.Get(id) : null;
            if (prefab != null)
            {
                var inst = Instantiate(prefab, parent);
                inst.name = id;
                return inst;
            }

            var root = new GameObject(string.IsNullOrEmpty(id) ? "Placeholder" : id);
            if (parent != null)
            {
                root.transform.SetParent(parent, false);
            }

            var meshGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            meshGo.name = "Mesh";
            meshGo.transform.SetParent(root.transform, false);
            var safeSize = new Vector3(
                Mathf.Max(0.05f, fallbackSize.x),
                Mathf.Max(0.05f, fallbackSize.y),
                Mathf.Max(0.05f, fallbackSize.z));
            meshGo.transform.localScale = safeSize;
            meshGo.transform.localPosition = new Vector3(0f, safeSize.y * 0.5f, 0f);

            var rend = meshGo.GetComponent<MeshRenderer>();
            if (rend != null)
            {
                if (_sharedPlaceholderMaterial == null)
                {
                    var sh = Shader.Find("LayeredHell/AtlasTiling");
                    if (sh == null) sh = Shader.Find("Universal Render Pipeline/Simple Lit");
                    if (sh == null) sh = Shader.Find("Universal Render Pipeline/Lit");
                    if (sh == null) sh = Shader.Find("Standard");
                    if (sh != null)
                    {
                        _sharedPlaceholderMaterial = new Material(sh);
                        _sharedPlaceholderMaterial.enableInstancing = true;
                    }
                }
                if (_sharedPlaceholderMaterial != null)
                {
                    rend.sharedMaterial = _sharedPlaceholderMaterial;
                    var mpb = new MaterialPropertyBlock();
                    mpb.SetColor("_BaseColor", fallbackColor);
                    mpb.SetColor("_Color", fallbackColor);
                    mpb.SetColor("_Tint", fallbackColor);
                    rend.SetPropertyBlock(mpb);
                }
            }
            return root;
        }
    }
}
