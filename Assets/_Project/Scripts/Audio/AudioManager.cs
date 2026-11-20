// Pooled 3D/2D audio manager with music/ambience crossfading, floor ambience routing, and low-health heartbeat/tinnitus.
using System.Collections;
using UnityEngine;

namespace LayeredHell
{
    public sealed class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        private const int PoolSize = 24;

        private AudioSource[] _pool;
        private int _poolCursor;

        private AudioSource _musicA;
        private AudioSource _musicB;
        private bool _usingMusicA = true;
        private string _currentMusicId = string.Empty;
        private Coroutine _musicFadeRoutine;

        private AudioSource _ambienceA;
        private AudioSource _ambienceB;
        private bool _usingAmbienceA = true;
        private string _currentAmbienceZone = string.Empty;
        private Coroutine _ambienceFadeRoutine;

        private float _heartbeatTimer;
        private float _lastTinnitusTime = -10f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
            BuildSources();
        }

        private void OnEnable()
        {
            GameEvents.FloorChanged += OnFloorChanged;
            GameEvents.PlayerDamaged += OnPlayerDamaged;
        }

        private void OnDisable()
        {
            GameEvents.FloorChanged -= OnFloorChanged;
            GameEvents.PlayerDamaged -= OnPlayerDamaged;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void BuildSources()
        {
            if (_pool != null) return;

            GameObject poolRoot = new GameObject("AudioPool");
            poolRoot.transform.SetParent(transform, false);

            _pool = new AudioSource[PoolSize];
            for (int i = 0; i < PoolSize; i++)
            {
                GameObject srcGo = new GameObject("SFX_" + i.ToString("00"));
                srcGo.transform.SetParent(poolRoot.transform, false);
                AudioSource src = srcGo.AddComponent<AudioSource>();
                src.playOnAwake = false;
                src.spatialBlend = 1.0f;
                src.rolloffMode = AudioRolloffMode.Logarithmic;
                src.minDistance = 1.5f;
                src.maxDistance = 32f;
                _pool[i] = src;
            }

            _musicA = CreateChannelSource(poolRoot.transform, "Music_A");
            _musicB = CreateChannelSource(poolRoot.transform, "Music_B");
            _ambienceA = CreateChannelSource(poolRoot.transform, "Ambience_A");
            _ambienceB = CreateChannelSource(poolRoot.transform, "Ambience_B");
        }

        private static AudioSource CreateChannelSource(Transform parent, string name)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            AudioSource src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.spatialBlend = 0f;
            src.loop = true;
            src.volume = 0f;
            return src;
        }

        private void Update()
        {
            if (GameManager.Instance != null && !GameManager.Instance.IsPlaying) return;
            if (PlayerController.Instance == null || PlayerController.Instance.Stats == null) return;

            PlayerStats stats = PlayerController.Instance.Stats;
            if (!stats.IsDead && stats.Health > 0f && stats.Health < 30f)
            {
                float ratio = Mathf.Clamp01(stats.Health / 30f);
                float interval = Mathf.Lerp(0.55f, 1.15f, ratio);
                _heartbeatTimer -= Time.deltaTime;
                if (_heartbeatTimer <= 0f)
                {
                    _heartbeatTimer = interval;
                    Play2D("heartbeat", Mathf.Lerp(0.85f, 0.35f, ratio));
                }
            }
        }

        private void OnFloorChanged(int oldFloor, int newFloor)
        {
            FloorInfo info = FloorCatalog.Get(newFloor);
            if (info != null && !string.IsNullOrEmpty(info.ambienceId))
            {
                SetAmbience(info.ambienceId);
            }

            if (info != null && !string.IsNullOrEmpty(info.musicId))
            {
                PlayMusic(info.musicId, true, 2.5f);
            }
        }

        private void OnPlayerDamaged(float amount)
        {
            bool lowHealth = PlayerController.Instance != null &&
                             PlayerController.Instance.Stats != null &&
                             PlayerController.Instance.Stats.Health < 22f;

            if ((amount >= 28f || lowHealth) && Time.unscaledTime - _lastTinnitusTime > 6f)
            {
                _lastTinnitusTime = Time.unscaledTime;
                Play2D("tinnitus", 0.55f);
            }
        }

        public static void Play(string baseId, Vector3 pos, float volume = 1f, float pitchVariation = 0.06f)
        {
            if (Instance == null || string.IsNullOrEmpty(baseId)) return;
            Instance.PlayInternal(baseId, pos, volume, pitchVariation, true);
        }

        public static void Play2D(string baseId, float volume = 1f)
        {
            if (Instance == null || string.IsNullOrEmpty(baseId)) return;
            Instance.PlayInternal(baseId, Vector3.zero, volume, 0.03f, false);
        }

        public static void PlayMusic(string baseId, bool loop = true, float fade = 2f)
        {
            if (Instance == null || string.IsNullOrEmpty(baseId)) return;
            Instance.PlayMusicInternal(baseId, loop, fade);
        }

        public static void SetAmbience(string zoneId)
        {
            if (Instance == null || string.IsNullOrEmpty(zoneId)) return;
            Instance.SetAmbienceInternal(zoneId);
        }

        public static void StopMusic(float fade = 1.5f)
        {
            if (Instance == null) return;
            Instance.StopMusicInternal(fade);
        }

        private void PlayInternal(string baseId, Vector3 pos, float volume, float pitchVariation, bool spatial)
        {
            if (_pool == null) BuildSources();
            if (AudioBank.Instance == null) return;

            AudioClip clip = AudioBank.Instance.GetRandom(baseId);
            if (clip == null) return;

            AudioSource src = GetNextSource();
            if (src == null) return;

            src.transform.position = pos;
            src.spatialBlend = spatial ? 1.0f : 0.0f;
            src.volume = Mathf.Clamp01(volume * GameSettings.SfxVolume * GameSettings.MasterVolume);
            src.pitch = pitchVariation > 0f
                ? 1f + UnityEngine.Random.Range(-pitchVariation, pitchVariation)
                : 1f;
            src.clip = clip;
            src.loop = false;
            src.Play();
        }

        private AudioSource GetNextSource()
        {
            for (int i = 0; i < PoolSize; i++)
            {
                int idx = (_poolCursor + i) % PoolSize;
                if (!_pool[idx].isPlaying)
                {
                    _poolCursor = (idx + 1) % PoolSize;
                    return _pool[idx];
                }
            }

            AudioSource fallback = _pool[_poolCursor];
            _poolCursor = (_poolCursor + 1) % PoolSize;
            return fallback;
        }

        private void PlayMusicInternal(string baseId, bool loop, float fade)
        {
            string normalized = baseId.StartsWith("mus_") ? baseId : ("mus_" + baseId);
            if (_currentMusicId == normalized) return;
            _currentMusicId = normalized;

            if (AudioBank.Instance == null) return;
            AudioClip clip = AudioBank.Instance.GetRandom(normalized);
            if (clip == null && normalized != baseId)
            {
                clip = AudioBank.Instance.GetRandom(baseId);
            }
            if (clip == null) return;

            AudioSource outgoing = _usingMusicA ? _musicA : _musicB;
            AudioSource incoming = _usingMusicA ? _musicB : _musicA;
            _usingMusicA = !_usingMusicA;

            incoming.clip = clip;
            incoming.loop = loop;
            incoming.volume = 0f;
            incoming.Play();

            if (_musicFadeRoutine != null) StopCoroutine(_musicFadeRoutine);
            float targetVol = GameSettings.MusicVolume * GameSettings.MasterVolume;
            _musicFadeRoutine = StartCoroutine(CrossfadeRoutine(outgoing, incoming, targetVol, fade));
        }

        private void StopMusicInternal(float fade)
        {
            _currentMusicId = string.Empty;
            if (_musicFadeRoutine != null) StopCoroutine(_musicFadeRoutine);
            AudioSource active = _usingMusicA ? _musicA : _musicB;
            _musicFadeRoutine = StartCoroutine(CrossfadeRoutine(active, null, 0f, fade));
        }

        private void SetAmbienceInternal(string zoneId)
        {
            string normalized = zoneId.StartsWith("amb_") ? zoneId : ("amb_" + zoneId);
            if (_currentAmbienceZone == normalized) return;
            _currentAmbienceZone = normalized;

            if (AudioBank.Instance == null) return;
            AudioClip clip = AudioBank.Instance.GetRandom(normalized);
            if (clip == null) return;

            AudioSource outgoing = _usingAmbienceA ? _ambienceA : _ambienceB;
            AudioSource incoming = _usingAmbienceA ? _ambienceB : _ambienceA;
            _usingAmbienceA = !_usingAmbienceA;

            incoming.clip = clip;
            incoming.loop = true;
            incoming.volume = 0f;
            incoming.Play();

            if (_ambienceFadeRoutine != null) StopCoroutine(_ambienceFadeRoutine);
            float targetVol = GameSettings.AmbienceVolume * GameSettings.MasterVolume;
            _ambienceFadeRoutine = StartCoroutine(CrossfadeRoutine(outgoing, incoming, targetVol, 2.0f));
        }

        private static IEnumerator CrossfadeRoutine(AudioSource outgoing, AudioSource incoming, float targetVolume, float duration)
        {
            float startOut = outgoing != null ? outgoing.volume : 0f;
            float dur = Mathf.Max(0.05f, duration);
            float t = 0f;

            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / dur);
                if (outgoing != null) outgoing.volume = Mathf.Lerp(startOut, 0f, k);
                if (incoming != null) incoming.volume = Mathf.Lerp(0f, targetVolume, k);
                yield return null;
            }

            if (outgoing != null)
            {
                outgoing.volume = 0f;
                outgoing.Stop();
            }
            if (incoming != null)
            {
                incoming.volume = targetVolume;
            }
        }
    }
}
