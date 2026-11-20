// JSON persistence manager with cached state delivery for streamed floor objects and automatic periodic/floor-change saves.
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace LayeredHell
{
    public sealed class SaveSystem : MonoBehaviour
    {
        public static SaveSystem Instance { get; private set; }

        [Serializable]
        public struct SaveEntry
        {
            public string key;
            public string json;
        }

        [Serializable]
        public class SaveFile
        {
            public int version = GameConstants.SaveVersion;
            public float playSeconds;
            public int currentFloor = GameConstants.BaseFloor;
            public List<SaveEntry> entries = new List<SaveEntry>();
        }

        private readonly Dictionary<string, ISaveable> _registered = new Dictionary<string, ISaveable>();
        private readonly Dictionary<string, string> _cachedEntries = new Dictionary<string, string>();

        private const float AutosaveInterval = 120f;
        private float _autosaveTimer = AutosaveInterval;
        private float _lastSaveTime = -10f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
        }

        private void OnEnable()
        {
            GameEvents.FloorChanged += OnFloorChanged;
        }

        private void OnDisable()
        {
            GameEvents.FloorChanged -= OnFloorChanged;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Update()
        {
            if (GameManager.Instance == null || !GameManager.Instance.IsPlaying) return;

            _autosaveTimer -= Time.unscaledDeltaTime;
            if (_autosaveTimer <= 0f)
            {
                _autosaveTimer = AutosaveInterval;
                Save(0);
            }
        }

        private void OnFloorChanged(int oldFloor, int newFloor)
        {
            if (oldFloor == newFloor) return;
            if (GameManager.Instance != null && GameManager.Instance.IsPlaying)
            {
                if (Time.unscaledTime - _lastSaveTime > 3f)
                {
                    Save(0);
                }
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus && GameManager.Instance != null && GameManager.Instance.IsPlaying)
            {
                Save(0);
            }
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus && GameManager.Instance != null && GameManager.Instance.IsPlaying)
            {
                Save(0);
            }
        }

        public void Register(ISaveable s)
        {
            if (s == null) return;
            string key = s.SaveKey;
            if (string.IsNullOrEmpty(key)) return;

            _registered[key] = s;

            if (_cachedEntries.TryGetValue(key, out string cachedJson))
            {
                try
                {
                    s.RestoreState(cachedJson);
                }
                catch (Exception e)
                {
                    Debug.LogWarning("SaveSystem.Register restore error for key '" + key + "': " + e.Message);
                }
            }
        }

        public void Unregister(ISaveable s)
        {
            if (s == null) return;
            string key = s.SaveKey;
            if (string.IsNullOrEmpty(key)) return;

            if (_registered.TryGetValue(key, out ISaveable current) && ReferenceEquals(current, s))
            {
                try
                {
                    _cachedEntries[key] = s.CaptureState();
                }
                catch (Exception)
                {
                    // Ignore capture failures during teardown
                }
                _registered.Remove(key);
            }
        }

        public bool Save(int slot = 0)
        {
            try
            {
                _lastSaveTime = Time.unscaledTime;
                _autosaveTimer = AutosaveInterval;

                foreach (KeyValuePair<string, ISaveable> kv in _registered)
                {
                    if (kv.Value == null) continue;
                    MonoBehaviour mb = kv.Value as MonoBehaviour;
                    if (mb == null && !(kv.Value is object)) continue;

                    try
                    {
                        string state = kv.Value.CaptureState();
                        if (state != null)
                        {
                            _cachedEntries[kv.Key] = state;
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning("SaveSystem capture failed for '" + kv.Key + "': " + ex.Message);
                    }
                }

                SaveFile file = new SaveFile
                {
                    version = GameConstants.SaveVersion,
                    playSeconds = GameManager.Instance != null ? GameManager.Instance.PlayTimeSeconds : 0f,
                    currentFloor = FloorStreamer.Instance != null ? FloorStreamer.Instance.CurrentFloor : GameConstants.BaseFloor,
                    entries = new List<SaveEntry>(_cachedEntries.Count)
                };

                foreach (KeyValuePair<string, string> kv in _cachedEntries)
                {
                    file.entries.Add(new SaveEntry { key = kv.Key, json = kv.Value });
                }

                string json = JsonUtility.ToJson(file, true);
                string path = GetSavePath(slot);
                string dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                File.WriteAllText(path, json);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning("SaveSystem.Save failed: " + e.Message);
                return false;
            }
        }

        public bool Load(int slot = 0)
        {
            try
            {
                string path = GetSavePath(slot);
                if (!File.Exists(path)) return false;

                string raw = File.ReadAllText(path);
                if (string.IsNullOrEmpty(raw)) return false;

                SaveFile file = JsonUtility.FromJson<SaveFile>(raw);
                if (file == null) return false;

                if (GameManager.Instance != null)
                {
                    GameManager.Instance.PlayTimeSeconds = file.playSeconds;
                }

                _cachedEntries.Clear();
                if (file.entries != null)
                {
                    for (int i = 0; i < file.entries.Count; i++)
                    {
                        SaveEntry entry = file.entries[i];
                        if (!string.IsNullOrEmpty(entry.key))
                        {
                            _cachedEntries[entry.key] = entry.json;
                        }
                    }
                }

                int targetFloor = Mathf.Clamp(file.currentFloor, GameConstants.MinFloor, GameConstants.MaxFloor);
                if (FloorStreamer.Instance != null)
                {
                    FloorStreamer.Instance.BeginGame(targetFloor);
                }

                // Apply immediately to all currently registered saveables
                List<KeyValuePair<string, ISaveable>> snapshot = new List<KeyValuePair<string, ISaveable>>(_registered);
                for (int i = 0; i < snapshot.Count; i++)
                {
                    KeyValuePair<string, ISaveable> kv = snapshot[i];
                    if (kv.Value == null) continue;
                    if (_cachedEntries.TryGetValue(kv.Key, out string json))
                    {
                        try
                        {
                            kv.Value.RestoreState(json);
                        }
                        catch (Exception ex)
                        {
                            Debug.LogWarning("SaveSystem.Load restore failed for '" + kv.Key + "': " + ex.Message);
                        }
                    }
                }

                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning("SaveSystem.Load failed: " + e.Message);
                return false;
            }
        }

        public bool HasSave(int slot = 0)
        {
            string path = GetSavePath(slot);
            return File.Exists(path);
        }

        public void DeleteSave(int slot = 0)
        {
            _cachedEntries.Clear();
            string path = GetSavePath(slot);
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch (Exception e)
            {
                Debug.LogWarning("SaveSystem.DeleteSave failed: " + e.Message);
            }
        }

        private static string GetSavePath(int slot)
        {
            return Path.Combine(Application.persistentDataPath, "lh45_save_" + slot + ".json");
        }
    }
}
