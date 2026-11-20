// Controls dark autumn night atmosphere, exponential fog, and subtle lightning flashes per floor theme.
using UnityEngine;
using UnityEngine.Rendering;

namespace LayeredHell
{
    public sealed class ProceduralSkyDarkener : MonoBehaviour
    {
        [SerializeField] private Light directionalMoonLight;
        private float _nextThunderTimer = 18f;
        private float _flashIntensity;

        private void OnEnable()
        {
            GameEvents.FloorChanged += OnFloorChanged;
            ApplyAtmosphereForFloor(GameConstants.BaseFloor);
        }

        private void OnDisable()
        {
            GameEvents.FloorChanged -= OnFloorChanged;
        }

        private void Start()
        {
            EnsureMoonLight();
            if (FloorStreamer.Instance != null)
                ApplyAtmosphereForFloor(FloorStreamer.Instance.CurrentFloor);
        }

        private void EnsureMoonLight()
        {
            if (directionalMoonLight != null) return;
            var lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
            for (int i = 0; i < lights.Length; i++)
            {
                if (lights[i].type == LightType.Directional)
                {
                    directionalMoonLight = lights[i];
                    break;
                }
            }
            if (directionalMoonLight == null)
            {
                var go = new GameObject("StormMoonLight");
                go.transform.SetParent(transform, false);
                go.transform.rotation = Quaternion.Euler(52f, -34f, 0f);
                directionalMoonLight = go.AddComponent<Light>();
                directionalMoonLight.type = LightType.Directional;
                directionalMoonLight.color = new Color(0.32f, 0.40f, 0.55f);
                directionalMoonLight.intensity = 0.22f;
                directionalMoonLight.shadows = LightShadows.Soft;
            }
        }

        private void OnFloorChanged(int oldFloor, int newFloor)
        {
            ApplyAtmosphereForFloor(newFloor);
        }

        public void ApplyAtmosphereForFloor(int floor)
        {
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;

            if (floor < 0)
            {
                // Underground Chronos Bio-Labs & service tunnels: greenish mutagen mist
                RenderSettings.ambientLight = new Color(0.04f, 0.06f, 0.05f);
                RenderSettings.fogColor = new Color(0.03f, 0.07f, 0.05f);
                RenderSettings.fogDensity = 0.042f;
                if (directionalMoonLight != null) directionalMoonLight.intensity = 0.04f;
            }
            else if (floor == 38)
            {
                // Floor 38 Safe House: warm dim shelter lighting
                RenderSettings.ambientLight = new Color(0.11f, 0.09f, 0.07f);
                RenderSettings.fogColor = new Color(0.05f, 0.05f, 0.06f);
                RenderSettings.fogDensity = 0.020f;
                if (directionalMoonLight != null) directionalMoonLight.intensity = 0.18f;
            }
            else if (floor == 45)
            {
                // Roof helipad: rainy night storm exposure
                RenderSettings.ambientLight = new Color(0.08f, 0.10f, 0.14f);
                RenderSettings.fogColor = new Color(0.04f, 0.06f, 0.09f);
                RenderSettings.fogDensity = 0.014f;
                if (directionalMoonLight != null) directionalMoonLight.intensity = 0.35f;
            }
            else
            {
                // Interior tower floors
                RenderSettings.ambientLight = new Color(0.06f, 0.07f, 0.09f);
                RenderSettings.fogColor = new Color(0.03f, 0.04f, 0.05f);
                RenderSettings.fogDensity = 0.030f;
                if (directionalMoonLight != null) directionalMoonLight.intensity = 0.16f;
            }
        }

        private void Update()
        {
            if (_flashIntensity > 0.001f)
            {
                _flashIntensity = Mathf.MoveTowards(_flashIntensity, 0f, Time.deltaTime * 2.8f);
                if (directionalMoonLight != null)
                    directionalMoonLight.intensity = 0.18f + _flashIntensity;
            }

            _nextThunderTimer -= Time.deltaTime;
            if (_nextThunderTimer <= 0f)
            {
                _nextThunderTimer = Random.Range(24f, 52f);
                int curFloor = FloorStreamer.Instance != null ? FloorStreamer.Instance.CurrentFloor : 38;
                if (curFloor >= 1)
                {
                    _flashIntensity = curFloor == 45 ? 1.1f : 0.45f;
                    AudioManager.Play2D("thunder", curFloor == 45 ? 0.65f : 0.25f);
                }
            }
        }
    }
}
