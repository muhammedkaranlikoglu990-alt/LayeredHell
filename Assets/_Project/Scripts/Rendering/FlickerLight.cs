// Reusable emergency light flicker component for damaged corridors and labs.
using UnityEngine;

namespace LayeredHell
{
    [RequireComponent(typeof(Light))]
    public sealed class FlickerLight : MonoBehaviour
    {
        public float minIntensity = 0.15f;
        public float maxIntensity = 1.15f;
        public float flickerSpeed = 9.0f;
        public float blackoutChance = 0.08f;

        private Light _light;
        private float _seed;
        private float _blackoutTimer;

        private void Awake()
        {
            _light = GetComponent<Light>();
            _seed = Random.Range(0f, 100f);
        }

        private void Update()
        {
            if (_light == null) return;

            if (_blackoutTimer > 0f)
            {
                _blackoutTimer -= Time.deltaTime;
                _light.intensity = minIntensity * 0.2f;
                return;
            }

            if (Random.value < blackoutChance * Time.deltaTime)
            {
                _blackoutTimer = Random.Range(0.08f, 0.35f);
                return;
            }

            float n = Mathf.PerlinNoise(_seed, Time.time * flickerSpeed);
            _light.intensity = Mathf.Lerp(minIntensity, maxIntensity, n);
        }
    }
}
