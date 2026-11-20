// 45: Layered Hell - Per-chunk emergency & fluorescent light flicker controller.
using UnityEngine;

namespace LayeredHell
{
    public class ChunkFlickerLight : MonoBehaviour
    {
        public Light targetLight;
        public float baseIntensity = 1.6f;
        public float minMultiplier = 0.25f;
        public float maxMultiplier = 1.15f;
        public float flickerSpeed = 7.5f;
        public bool sporadicBlackout = true;

        private float _seedOffset;
        private float _blackoutTimer;
        private bool _inBlackout;

        public static ChunkFlickerLight Create(Transform parent, Vector3 localPos, Color color, float intensity, float range, bool sporadic = true)
        {
            GameObject go = new GameObject("ChunkLight");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPos;

            Light lt = go.AddComponent<Light>();
            lt.type = LightType.Point;
            lt.color = color;
            lt.intensity = intensity;
            lt.range = range;
            lt.shadows = LightShadows.None;

            ChunkFlickerLight flicker = go.AddComponent<ChunkFlickerLight>();
            flicker.targetLight = lt;
            flicker.baseIntensity = intensity;
            flicker.sporadicBlackout = sporadic;
            return flicker;
        }

        private void Awake()
        {
            if (targetLight == null) targetLight = GetComponent<Light>();
            Vector3 p = transform.position;
            _seedOffset = Mathf.Abs(p.x * 13.7f + p.y * 7.3f + p.z * 19.1f) % 100f;
            _blackoutTimer = 2f + (_seedOffset % 6f);
        }

        private void Update()
        {
            if (targetLight == null) return;

            if (sporadicBlackout)
            {
                _blackoutTimer -= Time.deltaTime;
                if (_blackoutTimer <= 0f)
                {
                    if (_inBlackout)
                    {
                        _inBlackout = false;
                        _blackoutTimer = 3.5f + Mathf.PerlinNoise(Time.time, _seedOffset) * 8f;
                    }
                    else
                    {
                        _inBlackout = true;
                        _blackoutTimer = 0.08f + Mathf.PerlinNoise(_seedOffset, Time.time) * 0.35f;
                    }
                }

                if (_inBlackout)
                {
                    targetLight.intensity = baseIntensity * 0.05f;
                    return;
                }
            }

            float n = Mathf.PerlinNoise(Time.time * flickerSpeed, _seedOffset);
            float mult = Mathf.Lerp(minMultiplier, maxMultiplier, n);
            targetLight.intensity = baseIntensity * mult;
        }
    }
}
