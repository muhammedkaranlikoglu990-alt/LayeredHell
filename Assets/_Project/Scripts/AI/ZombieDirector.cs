// 45: Layered Hell - Floor population budget manager, roof horde defense waves, B4 Goliath spawn, and ISaveable persistence.
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LayeredHell
{
    public class ZombieDirector : MonoBehaviour, ISaveable
    {
        public static ZombieDirector Instance { get; private set; }

        [Serializable]
        private class DirectorSaveData
        {
            public List<string> killedSpawnKeys = new List<string>();
            public bool bossDefeated;
        }

        private class FloorPopulation
        {
            public int floor;
            public FloorChunk chunk;
            public readonly List<Transform> spawnPoints = new List<Transform>();
            public readonly List<ZombieAI> activeZombies = new List<ZombieAI>();
        }

        private readonly Dictionary<int, FloorPopulation> _populations = new Dictionary<int, FloorPopulation>();
        private readonly HashSet<string> _killedSpawnKeys = new HashSet<string>();
        private readonly List<ZombieAI> _hordeZombies = new List<ZombieAI>();

        private ZombieAI _activeBoss;
        private bool _bossDefeated;
        private bool _hordeActive;

        public string SaveKey { get { return "director"; } }

        public int AliveCount
        {
            get
            {
                int total = 0;
                foreach (KeyValuePair<int, FloorPopulation> kv in _populations)
                {
                    List<ZombieAI> list = kv.Value.activeZombies;
                    for (int i = list.Count - 1; i >= 0; i--)
                    {
                        if (list[i] == null || list[i].IsDead)
                        {
                            list.RemoveAt(i);
                        }
                        else
                        {
                            total++;
                        }
                    }
                }

                for (int i = _hordeZombies.Count - 1; i >= 0; i--)
                {
                    if (_hordeZombies[i] == null || _hordeZombies[i].IsDead)
                    {
                        _hordeZombies.RemoveAt(i);
                    }
                    else
                    {
                        total++;
                    }
                }

                if (_activeBoss != null && !_activeBoss.IsDead)
                {
                    total++;
                }

                return total;
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            if (SaveSystem.Instance != null)
            {
                SaveSystem.Instance.Register(this);
            }
        }

        private void OnDestroy()
        {
            if (SaveSystem.Instance != null)
            {
                SaveSystem.Instance.Unregister(this);
            }
        }

        public void RegisterChunk(FloorChunk chunk, List<Transform> spawnPoints)
        {
            if (chunk == null) return;
            int floor = chunk.floor;

            // Safe house floor 38 never spawns regular ambient zombies inside
            if (floor == GameConstants.BaseFloor) return;

            UnregisterChunk(floor);

            FloorInfo info = FloorCatalog.Get(floor);
            if (info == null || info.zombieMix == null || info.zombieMix.Length == 0) return;

            FloorPopulation pop = new FloorPopulation
            {
                floor = floor,
                chunk = chunk
            };
            if (spawnPoints != null)
            {
                pop.spawnPoints.AddRange(spawnPoints);
            }
            _populations[floor] = pop;

            System.Random rng = new System.Random(floor * 4999 + 73);
            int budget = Mathf.Clamp(2 + info.danger, 2, pop.spawnPoints.Count);

            for (int i = 0; i < budget && i < pop.spawnPoints.Count; i++)
            {
                Transform sp = pop.spawnPoints[i];
                if (sp == null) continue;

                string spawnKey = MakeSpawnKey(floor, i);
                if (_killedSpawnKeys.Contains(spawnKey))
                {
                    // Respawn only with a low 20% chance when revisiting cleared floors
                    if (rng.NextDouble() > 0.20) continue;
                }

                string archId = info.zombieMix[rng.Next(0, info.zombieMix.Length)];
                ZombieAI z = ZombieFactory.Spawn(archId, sp.position, floor, rng);
                if (z != null)
                {
                    z.spawnIndex = i;
                    z.transform.SetParent(chunk.transform, true);
                    pop.activeZombies.Add(z);
                }
            }
        }

        public void UnregisterChunk(int floor)
        {
            if (!_populations.TryGetValue(floor, out FloorPopulation pop)) return;

            for (int i = 0; i < pop.activeZombies.Count; i++)
            {
                if (pop.activeZombies[i] != null)
                {
                    Destroy(pop.activeZombies[i].gameObject);
                }
            }

            pop.activeZombies.Clear();
            _populations.Remove(floor);

            if (floor == GameConstants.MinFloor && _activeBoss != null && !_activeBoss.IsDead)
            {
                Destroy(_activeBoss.gameObject);
                _activeBoss = null;
            }
        }

        public void SpawnBoss()
        {
            if (_bossDefeated) return;
            if (QuestManager.Instance != null && QuestManager.Instance.HasFlag("goliath_defeated"))
            {
                _bossDefeated = true;
                return;
            }
            if (_activeBoss != null && !_activeBoss.IsDead) return;

            int bossFloor = GameConstants.MinFloor; // B4 (-4)
            float y = GameConstants.FloorY(bossFloor) + 0.1f;
            Vector3 bossPos = new Vector3(0f, y, 10.5f);

            System.Random rng = new System.Random(90045);
            _activeBoss = ZombieFactory.Spawn("goliath", bossPos, bossFloor, rng);

            if (_activeBoss != null && FloorStreamer.Instance != null)
            {
                FloorChunk b4Chunk = FloorStreamer.Instance.GetChunk(bossFloor);
                if (b4Chunk != null)
                {
                    _activeBoss.transform.SetParent(b4Chunk.transform, true);
                }
            }
        }

        public void StartHordeDefense(int waves, Action onComplete)
        {
            if (_hordeActive) return;
            int totalWaves = Mathf.Clamp(waves, 1, 6);
            StartCoroutine(HordeDefenseRoutine(totalWaves, onComplete));
        }

        public void AlertNearby(Vector3 pos, int floor, float radius, Vector3 targetPos = default)
        {
            Vector3 destination = targetPos == default ? pos : targetPos;
            float rSq = radius * radius;

            if (_populations.TryGetValue(floor, out FloorPopulation pop))
            {
                for (int i = 0; i < pop.activeZombies.Count; i++)
                {
                    ZombieAI z = pop.activeZombies[i];
                    if (z == null || z.IsDead) continue;
                    if ((z.transform.position - pos).sqrMagnitude <= rSq)
                    {
                        z.AlertToPosition(destination, true);
                    }
                }
            }

            for (int i = 0; i < _hordeZombies.Count; i++)
            {
                ZombieAI z = _hordeZombies[i];
                if (z == null || z.IsDead) continue;
                if ((z.transform.position - pos).sqrMagnitude <= rSq)
                {
                    z.AlertToPosition(destination, true);
                }
            }
        }

        public void NotifyZombieKilled(ZombieAI zombie)
        {
            if (zombie == null) return;

            if (zombie.spawnIndex >= 0)
            {
                _killedSpawnKeys.Add(MakeSpawnKey(zombie.floor, zombie.spawnIndex));
            }

            if (string.Equals(zombie.archetypeId, "goliath", StringComparison.OrdinalIgnoreCase))
            {
                _bossDefeated = true;
            }
        }

        private IEnumerator HordeDefenseRoutine(int waves, Action onComplete)
        {
            _hordeActive = true;
            int defenseFloor = FloorStreamer.Instance != null ? FloorStreamer.Instance.CurrentFloor : GameConstants.MaxFloor;
            float baseY = GameConstants.FloorY(defenseFloor) + 0.1f;

            AudioManager.PlayMusic("mus_combat", true, 1.5f);
            GameEvents.RaiseToast("Tahliye Helikopteri Yaklaşıyor! Çatı Pistini Savun!");

            System.Random rng = new System.Random(4500 + waves * 97);
            string[] hordeMix = { "runner", "runner", "walker", "spitter", "armored" };

            for (int w = 1; w <= waves; w++)
            {
                GameEvents.RaiseToast("Sürü Dalgası " + w + " / " + waves + " başladı!");
                int waveCount = 3 + w * 2;

                for (int i = 0; i < waveCount; i++)
                {
                    float angle = (i / (float)waveCount) * Mathf.PI * 2f;
                    float radius = 14f + (i % 3) * 2.5f;
                    Vector3 spawnPos = new Vector3(Mathf.Cos(angle) * radius, baseY, Mathf.Sin(angle) * radius);
                    string arch = hordeMix[(w + i) % hordeMix.Length];

                    ZombieAI z = ZombieFactory.Spawn(arch, spawnPos, defenseFloor, rng);
                    if (z != null)
                    {
                        z.ForceHordeAggro();
                        _hordeZombies.Add(z);
                    }
                    yield return new WaitForSeconds(0.45f);
                }

                // Wait until all zombies in this wave are killed or timeout (35s per wave)
                float waveTimer = 35f;
                while (waveTimer > 0f)
                {
                    int aliveInHorde = 0;
                    for (int i = _hordeZombies.Count - 1; i >= 0; i--)
                    {
                        if (_hordeZombies[i] == null || _hordeZombies[i].IsDead)
                        {
                            _hordeZombies.RemoveAt(i);
                        }
                        else
                        {
                            aliveInHorde++;
                        }
                    }

                    if (aliveInHorde == 0) break;
                    waveTimer -= 0.5f;
                    yield return new WaitForSeconds(0.5f);
                }

                yield return new WaitForSeconds(1.5f);
            }

            _hordeActive = false;
            GameEvents.RaiseToast("Çatı savunması tamamlandı! Helikopter inişe geçiyor!");
            GameEvents.RaiseObjective("helicopter_defense_complete");
            onComplete?.Invoke();
        }

        private static string MakeSpawnKey(int floor, int index)
        {
            return floor + "_" + index;
        }

        public string CaptureState()
        {
            DirectorSaveData data = new DirectorSaveData
            {
                killedSpawnKeys = new List<string>(_killedSpawnKeys),
                bossDefeated = _bossDefeated
            };
            return JsonUtility.ToJson(data);
        }

        public void RestoreState(string json)
        {
            _killedSpawnKeys.Clear();
            _bossDefeated = false;

            if (string.IsNullOrEmpty(json)) return;

            try
            {
                DirectorSaveData data = JsonUtility.FromJson<DirectorSaveData>(json);
                if (data != null)
                {
                    _bossDefeated = data.bossDefeated;
                    if (data.killedSpawnKeys != null)
                    {
                        for (int i = 0; i < data.killedSpawnKeys.Count; i++)
                        {
                            if (!string.IsNullOrEmpty(data.killedSpawnKeys[i]))
                            {
                                _killedSpawnKeys.Add(data.killedSpawnKeys[i]);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Ignore corrupted save fragment
            }
        }
    }
}
