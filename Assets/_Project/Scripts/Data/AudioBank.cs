// Runtime audio variation bank mapping base sound IDs (e.g. "zmb_walker_breath") to their clip arrays.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LayeredHell
{
    [CreateAssetMenu(menuName = "LayeredHell/AudioBank", fileName = "AudioBank")]
    public sealed class AudioBank : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            public string baseId;
            public string category;
            public bool loop;
            public AudioClip[] clips;
        }

        [SerializeField] private List<Entry> entries = new List<Entry>();

        private static AudioBank _instance;
        public static AudioBank Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<AudioBank>("AudioBank");
                    if (_instance == null)
                    {
                        _instance = CreateInstance<AudioBank>();
                        _instance.name = "AudioBank_RuntimeFallback";
                    }
                    _instance.BuildLookup();
                }
                return _instance;
            }
        }

        private Dictionary<string, AudioClip[]> _lookup;
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
            _lookup = new Dictionary<string, AudioClip[]>(StringComparer.Ordinal);
            if (entries == null) entries = new List<Entry>();
            for (int i = 0; i < entries.Count; i++)
            {
                var e = entries[i];
                if (!string.IsNullOrEmpty(e.baseId) && e.clips != null && e.clips.Length > 0)
                {
                    _lookup[e.baseId] = e.clips;
                }
            }
        }

        public bool Has(string baseId)
        {
            if (string.IsNullOrEmpty(baseId)) return false;
            if (_lookup == null) BuildLookup();
            return _lookup.ContainsKey(baseId);
        }

        public AudioClip[] GetClips(string baseId)
        {
            if (string.IsNullOrEmpty(baseId)) return Array.Empty<AudioClip>();
            if (_lookup == null) BuildLookup();
            AudioClip[] arr;
            if (_lookup.TryGetValue(baseId, out arr) && arr != null)
            {
                return arr;
            }
            return Array.Empty<AudioClip>();
        }

        public AudioClip GetRandom(string baseId)
        {
            var clips = GetClips(baseId);
            if (clips == null || clips.Length == 0) return null;
            if (clips.Length == 1) return clips[0];
            return clips[UnityEngine.Random.Range(0, clips.Length)];
        }
    }
}
