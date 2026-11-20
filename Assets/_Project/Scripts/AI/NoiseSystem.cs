// 45: Layered Hell - Acoustic propagation bus for footsteps, gunfire, melee, doors, and explosions.
using System;
using UnityEngine;

namespace LayeredHell
{
    public class NoiseSystem : MonoBehaviour
    {
        public static NoiseSystem Instance { get; private set; }

        public static event Action<Vector3, float, GameObject> NoiseEmitted;

        public Vector3 LastNoisePosition { get; private set; }
        public float LastNoiseLoudness { get; private set; }
        public float LastNoiseTime { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public static void Emit(Vector3 pos, float loudness, GameObject source = null)
        {
            if (loudness <= 0.05f) return;

            if (Instance != null)
            {
                Instance.LastNoisePosition = pos;
                Instance.LastNoiseLoudness = loudness;
                Instance.LastNoiseTime = Time.time;
            }

            NoiseEmitted?.Invoke(pos, loudness, source);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            NoiseEmitted = null;
            Instance = null;
        }
    }
}
