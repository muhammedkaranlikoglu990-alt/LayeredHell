// 45: Layered Hell — RadioSystem: 88.7 Kule Radyosu transmission triggers, audio playback, and replay history.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LayeredHell
{
    public class RadioSystem : MonoBehaviour, ISaveable
    {
        public static RadioSystem Instance { get; private set; }

        private readonly HashSet<string> _triggeredIds = new HashSet<string>(StringComparer.Ordinal);
        private readonly List<RadioLog> _heardLogs = new List<RadioLog>();

        private bool _startBroadcastFired;
        private float _startDelayTimer;
        private float _ghostTimer;
        private int _ghostIndex;

        public IReadOnlyList<RadioLog> HeardLogs => _heardLogs;
        public string SaveKey => "radio_system";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnEnable()
        {
            GameEvents.FloorChanged += OnFloorChanged;
            GameEvents.FloorLoaded += OnFloorLoaded;
            GameEvents.ObjectiveTriggered += OnObjectiveTriggered;
        }

        private void OnDisable()
        {
            GameEvents.FloorChanged -= OnFloorChanged;
            GameEvents.FloorLoaded -= OnFloorLoaded;
            GameEvents.ObjectiveTriggered -= OnObjectiveTriggered;
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

        private void Update()
        {
            if (GameManager.Instance != null && !GameManager.Instance.IsPlaying)
                return;

            if (!_startBroadcastFired)
            {
                _startDelayTimer += Time.deltaTime;
                if (_startDelayTimer >= 3.0f)
                {
                    _startBroadcastFired = true;
                    TriggerByKey("start");
                }
            }

            _ghostTimer += Time.deltaTime;
            if (_ghostTimer >= 165f)
            {
                _ghostTimer = 0f;
                TriggerNextGhostTransmission();
            }
        }

        public void ResetForNewGame()
        {
            _triggeredIds.Clear();
            _heardLogs.Clear();
            _startBroadcastFired = false;
            _startDelayTimer = 0f;
            _ghostTimer = 0f;
            _ghostIndex = 0;
        }

        public void Replay(string radioId)
        {
            var logs = StoryDatabase.RadioLogs;
            for (int i = 0; i < logs.Count; i++)
            {
                if (logs[i].id == radioId)
                {
                    PlayTransmission(logs[i], false);
                    return;
                }
            }
        }

        private void OnFloorChanged(int oldFloor, int newFloor)
        {
            TriggerByKey("floor_" + newFloor);
        }

        private void OnFloorLoaded(int floor)
        {
            if (FloorStreamer.Instance != null && FloorStreamer.Instance.CurrentFloor == floor)
            {
                TriggerByKey("floor_" + floor);
            }
        }

        private void OnObjectiveTriggered(string key)
        {
            if (!string.IsNullOrEmpty(key))
            {
                TriggerByKey(key);
            }
        }

        private void TriggerByKey(string triggerKey)
        {
            if (string.IsNullOrEmpty(triggerKey)) return;

            var logs = StoryDatabase.RadioLogs;
            for (int i = 0; i < logs.Count; i++)
            {
                var log = logs[i];
                if (log.triggerKey == triggerKey && !_triggeredIds.Contains(log.id))
                {
                    _triggeredIds.Add(log.id);
                    _heardLogs.Add(log);
                    PlayTransmission(log, true);
                }
            }
        }

        private void TriggerNextGhostTransmission()
        {
            var logs = StoryDatabase.RadioLogs;
            var ghosts = new List<RadioLog>();
            for (int i = 0; i < logs.Count; i++)
            {
                if (logs[i].triggerKey == "ghost")
                    ghosts.Add(logs[i]);
            }

            if (ghosts.Count == 0) return;
            var pick = ghosts[_ghostIndex % ghosts.Count];
            _ghostIndex++;

            if (_triggeredIds.Add(pick.id))
            {
                _heardLogs.Add(pick);
            }
            PlayTransmission(pick, true);
        }

        private static void PlayTransmission(RadioLog log, bool withStaticIntro)
        {
            if (log == null) return;
            if (withStaticIntro)
            {
                AudioManager.Play2D("radio_squelch", 0.75f);
                AudioManager.Play2D("radio_static", 0.45f);
            }
            AudioManager.Play2D("radio_voice", 0.9f);
            GameEvents.RaiseRadio(log.speaker, log.text);
        }

        [Serializable]
        private class SaveData
        {
            public List<string> triggeredIds = new List<string>();
            public bool startBroadcastFired;
        }

        public string CaptureState()
        {
            return JsonUtility.ToJson(new SaveData
            {
                triggeredIds = new List<string>(_triggeredIds),
                startBroadcastFired = _startBroadcastFired
            });
        }

        public void RestoreState(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            var data = JsonUtility.FromJson<SaveData>(json);
            if (data == null) return;

            _triggeredIds.Clear();
            _heardLogs.Clear();
            _startBroadcastFired = data.startBroadcastFired;

            if (data.triggeredIds != null)
            {
                var all = StoryDatabase.RadioLogs;
                for (int i = 0; i < data.triggeredIds.Count; i++)
                {
                    string id = data.triggeredIds[i];
                    if (string.IsNullOrEmpty(id) || !_triggeredIds.Add(id)) continue;

                    for (int j = 0; j < all.Count; j++)
                    {
                        if (all[j].id == id)
                        {
                            _heardLogs.Add(all[j]);
                            break;
                        }
                    }
                }
            }
        }
    }
}
