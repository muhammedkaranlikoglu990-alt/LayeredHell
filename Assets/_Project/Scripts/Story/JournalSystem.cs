// 45: Layered Hell — JournalSystem: tracks collected Turkish story journals & persists via SaveSystem.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LayeredHell
{
    public class JournalSystem : MonoBehaviour, ISaveable
    {
        public static JournalSystem Instance { get; private set; }

        private readonly List<string> _collected = new List<string>();
        private readonly HashSet<string> _collectedSet = new HashSet<string>(StringComparer.Ordinal);

        public IReadOnlyList<string> Collected => _collected;
        public string SaveKey => "journals";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            ResetForNewGame();
        }

        private void Start()
        {
            if (SaveSystem.Instance != null)
                SaveSystem.Instance.Register(this);
        }

        private void OnDestroy()
        {
            if (SaveSystem.Instance != null)
                SaveSystem.Instance.Unregister(this);
            if (Instance == this)
                Instance = null;
        }

        public bool IsCollected(string id)
        {
            return !string.IsNullOrEmpty(id) && _collectedSet.Contains(id);
        }

        public void Collect(string id)
        {
            if (string.IsNullOrEmpty(id)) return;
            if (!_collectedSet.Add(id))
            {
                var existing = StoryDatabase.GetJournal(id);
                if (existing != null)
                    GameEvents.RaiseToast("Günlük: " + existing.title);
                return;
            }

            _collected.Add(id);
            GameEvents.RaiseJournalCollected(id);

            var entry = StoryDatabase.GetJournal(id);
            if (entry != null)
            {
                GameEvents.RaiseToast("Günlük Eklendi: " + entry.title + " (" + GameConstants.FloorLabel(entry.floor) + ")");
            }
        }

        public void ResetForNewGame()
        {
            _collected.Clear();
            _collectedSet.Clear();

            // Aras starts with his own 38-B Safe House notebook manifesto in his journal
            const string starterId = "jrn_38_aras_manifesto";
            if (StoryDatabase.GetJournal(starterId) != null)
            {
                _collectedSet.Add(starterId);
                _collected.Add(starterId);
            }
        }

        [Serializable]
        private class SaveData
        {
            public List<string> collected = new List<string>();
        }

        public string CaptureState()
        {
            return JsonUtility.ToJson(new SaveData { collected = new List<string>(_collected) });
        }

        public void RestoreState(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            var data = JsonUtility.FromJson<SaveData>(json);
            if (data == null) return;

            _collected.Clear();
            _collectedSet.Clear();

            if (data.collected != null)
            {
                for (int i = 0; i < data.collected.Count; i++)
                {
                    string id = data.collected[i];
                    if (!string.IsNullOrEmpty(id) && _collectedSet.Add(id))
                    {
                        _collected.Add(id);
                    }
                }
            }
        }
    }
}
