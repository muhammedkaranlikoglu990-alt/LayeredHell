// 45: Layered Hell - 50-Floor dynamic additive scene/chunk streamer keeping CurrentFloor ±1 loaded.
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LayeredHell
{
    public class FloorStreamer : MonoBehaviour
    {
        public static FloorStreamer Instance { get; private set; }

        private readonly Dictionary<int, FloorChunk> _loadedChunks = new Dictionary<int, FloorChunk>();
        private readonly HashSet<int> _loadingFloors = new HashSet<int>();
        private readonly HashSet<int> _sceneBackedFloors = new HashSet<int>();

        private CityBackdrop _cityBackdrop;
        private float _pollTimer;

        public int CurrentFloor { get; private set; } = GameConstants.BaseFloor;
        public bool IsBusy { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Update()
        {
            _pollTimer += Time.unscaledDeltaTime;
            if (_pollTimer < 0.25f) return;
            _pollTimer = 0f;

            if (PlayerController.Instance == null) return;

            float py = PlayerController.Instance.Position.y;
            int detectedFloor = Mathf.Clamp(
                Mathf.RoundToInt(py / GameConstants.FloorHeight),
                GameConstants.MinFloor,
                GameConstants.MaxFloor);

            if (detectedFloor != CurrentFloor && !IsBusy)
            {
                int oldFloor = CurrentFloor;
                CurrentFloor = detectedFloor;
                GameEvents.RaiseFloorChanged(oldFloor, CurrentFloor);
                StartCoroutine(SyncSurroundingFloorsRoutine(CurrentFloor));
            }
        }

        public void BeginGame(int floor)
        {
            int target = Mathf.Clamp(floor, GameConstants.MinFloor, GameConstants.MaxFloor);
            EnsureCityBackdrop();
            StopAllCoroutines();

            // Guarantee the target starting floor chunk exists immediately so PlayerFactory.Spawn can query GetPlayerSpawn right away
            int oldFloor = CurrentFloor;
            CurrentFloor = target;
            FloorChunk immediateChunk = EnsureChunkImmediate(target);
            if (immediateChunk != null && !immediateChunk.IsGenerated)
            {
                immediateChunk.GenerateImmediate();
            }

            GameEvents.RaiseFloorChanged(oldFloor, CurrentFloor);
            StartCoroutine(BeginGameRoutine(target));
        }

        public bool IsFloorLoaded(int floor)
        {
            return _loadedChunks.TryGetValue(floor, out FloorChunk chunk) && chunk != null && chunk.IsGenerated;
        }

        public FloorChunk GetChunk(int floor)
        {
            _loadedChunks.TryGetValue(floor, out FloorChunk chunk);
            return chunk;
        }

        public Vector3 GetPlayerSpawn(int floor)
        {
            int clamped = Mathf.Clamp(floor, GameConstants.MinFloor, GameConstants.MaxFloor);
            if (_loadedChunks.TryGetValue(clamped, out FloorChunk chunk) && chunk != null)
            {
                if (!chunk.IsGenerated)
                {
                    chunk.GenerateImmediate();
                }
                if (chunk.PlayerSpawn != null)
                {
                    return chunk.PlayerSpawn.position;
                }
            }

            float y = GameConstants.FloorY(clamped) + 0.12f;
            return clamped == GameConstants.BaseFloor
                ? new Vector3(-4.5f, y, -3.5f)
                : new Vector3(0f, y, -4.5f);
        }

        public void RequestFloor(int floor, Action<FloorChunk> onReady)
        {
            int target = Mathf.Clamp(floor, GameConstants.MinFloor, GameConstants.MaxFloor);
            EnsureCityBackdrop();
            StartCoroutine(RequestFloorRoutine(target, onReady));
        }

        private IEnumerator BeginGameRoutine(int targetFloor)
        {
            IsBusy = true;

            // Unload any floors outside targetFloor ±1
            List<int> toRemove = new List<int>();
            foreach (KeyValuePair<int, FloorChunk> kv in _loadedChunks)
            {
                if (Mathf.Abs(kv.Key - targetFloor) > 1)
                {
                    toRemove.Add(kv.Key);
                }
            }
            for (int i = 0; i < toRemove.Count; i++)
            {
                yield return UnloadFloorRoutine(toRemove[i]);
            }

            // Ensure targetFloor and neighbors ±1 are loaded
            yield return LoadFloorRoutine(targetFloor, true);
            if (targetFloor - 1 >= GameConstants.MinFloor)
            {
                yield return LoadFloorRoutine(targetFloor - 1, false);
            }
            if (targetFloor + 1 <= GameConstants.MaxFloor)
            {
                yield return LoadFloorRoutine(targetFloor + 1, false);
            }

            IsBusy = false;
        }

        private IEnumerator RequestFloorRoutine(int targetFloor, Action<FloorChunk> onReady)
        {
            IsBusy = true;
            int oldFloor = CurrentFloor;
            CurrentFloor = targetFloor;

            yield return LoadFloorRoutine(targetFloor, true);
            FloorChunk targetChunk = GetChunk(targetFloor);
            if (targetChunk != null && !targetChunk.IsGenerated)
            {
                targetChunk.GenerateImmediate();
            }

            if (oldFloor != CurrentFloor)
            {
                GameEvents.RaiseFloorChanged(oldFloor, CurrentFloor);
            }

            IsBusy = false;
            onReady?.Invoke(targetChunk);

            // Stream neighbors and unload far floors in background
            yield return SyncSurroundingFloorsRoutine(targetFloor);
        }

        private IEnumerator SyncSurroundingFloorsRoutine(int centerFloor)
        {
            for (int offset = -1; offset <= 1; offset++)
            {
                int f = centerFloor + offset;
                if (f < GameConstants.MinFloor || f > GameConstants.MaxFloor) continue;
                if (!_loadedChunks.ContainsKey(f) && !_loadingFloors.Contains(f))
                {
                    yield return LoadFloorRoutine(f, offset == 0);
                }
            }

            // Unload chunks beyond ±2
            List<int> toUnload = new List<int>();
            foreach (KeyValuePair<int, FloorChunk> kv in _loadedChunks)
            {
                if (Mathf.Abs(kv.Key - centerFloor) > 2)
                {
                    toUnload.Add(kv.Key);
                }
            }

            for (int i = 0; i < toUnload.Count; i++)
            {
                yield return UnloadFloorRoutine(toUnload[i]);
            }
        }

        private IEnumerator LoadFloorRoutine(int floor, bool immediateGeneration)
        {
            if (_loadedChunks.TryGetValue(floor, out FloorChunk existing) && existing != null)
            {
                if (immediateGeneration && !existing.IsGenerated)
                {
                    existing.GenerateImmediate();
                }
                yield break;
            }

            if (_loadingFloors.Contains(floor))
            {
                while (_loadingFloors.Contains(floor))
                {
                    yield return null;
                }
                yield break;
            }

            _loadingFloors.Add(floor);
            string sceneName = GameConstants.FloorSceneName(floor);
            FloorChunk chunk = null;

            bool canLoadScene = Application.CanStreamedLevelBeLoaded(sceneName);
            if (canLoadScene)
            {
                Scene existingScene = SceneManager.GetSceneByName(sceneName);
                if (!existingScene.isLoaded)
                {
                    AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
                    if (op != null)
                    {
                        while (!op.isDone)
                        {
                            yield return null;
                        }
                    }
                }

                Scene loadedScene = SceneManager.GetSceneByName(sceneName);
                if (loadedScene.IsValid() && loadedScene.isLoaded)
                {
                    _sceneBackedFloors.Add(floor);
                    GameObject[] roots = loadedScene.GetRootGameObjects();
                    for (int i = 0; i < roots.Length; i++)
                    {
                        chunk = roots[i].GetComponentInChildren<FloorChunk>();
                        if (chunk != null) break;
                    }
                }
            }

            if (chunk == null)
            {
                chunk = CreateRuntimeChunk(floor);
            }

            chunk.floor = floor;
            _loadedChunks[floor] = chunk;
            _loadingFloors.Remove(floor);

            if (immediateGeneration && !chunk.IsGenerated)
            {
                chunk.GenerateImmediate();
            }
            else
            {
                while (chunk != null && !chunk.IsGenerated)
                {
                    yield return null;
                }
            }
        }

        private IEnumerator UnloadFloorRoutine(int floor)
        {
            if (!_loadedChunks.TryGetValue(floor, out FloorChunk chunk))
            {
                yield break;
            }

            _loadedChunks.Remove(floor);
            if (ZombieDirector.Instance != null)
            {
                ZombieDirector.Instance.UnregisterChunk(floor);
            }

            if (_sceneBackedFloors.Contains(floor))
            {
                _sceneBackedFloors.Remove(floor);
                string sceneName = GameConstants.FloorSceneName(floor);
                Scene sc = SceneManager.GetSceneByName(sceneName);
                if (sc.IsValid() && sc.isLoaded)
                {
                    AsyncOperation op = SceneManager.UnloadSceneAsync(sc);
                    if (op != null)
                    {
                        while (!op.isDone)
                        {
                            yield return null;
                        }
                    }
                }
            }
            else if (chunk != null)
            {
                Destroy(chunk.gameObject);
            }

            GameEvents.RaiseFloorUnloaded(floor);
        }

        private FloorChunk EnsureChunkImmediate(int floor)
        {
            if (_loadedChunks.TryGetValue(floor, out FloorChunk existing) && existing != null)
            {
                return existing;
            }

            FloorChunk chunk = CreateRuntimeChunk(floor);
            _loadedChunks[floor] = chunk;
            chunk.GenerateImmediate();
            return chunk;
        }

        private static FloorChunk CreateRuntimeChunk(int floor)
        {
            GameObject go = new GameObject(GameConstants.FloorSceneName(floor) + "_Chunk");
            go.transform.position = new Vector3(0f, GameConstants.FloorY(floor), 0f);
            FloorChunk chunk = go.AddComponent<FloorChunk>();
            chunk.floor = floor;
            return chunk;
        }

        private void EnsureCityBackdrop()
        {
            if (_cityBackdrop != null) return;
            GameObject go = new GameObject("CityBackdrop_Root");
            _cityBackdrop = go.AddComponent<CityBackdrop>();
            _cityBackdrop.BuildIfNeeded();
        }
    }
}
