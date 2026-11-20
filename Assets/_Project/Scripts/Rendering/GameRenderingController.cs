// Mobile URP quality tier selector, 60 FPS stabilizer, and dynamic render scale manager for Android 10-15+.
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace LayeredHell
{
    public sealed class GameRenderingController : MonoBehaviour
    {
        public static GameRenderingController Instance { get; private set; }

        public enum QualityTier
        {
            Low = 0,
            Medium = 1,
            High = 2
        }

        [SerializeField] private QualityTier currentTier = QualityTier.High;
        [SerializeField] private bool dynamicScaleEnabled = true;

        private float _fpsSampleTimer;
        private int _fpsFrameCount;
        private float _currentRenderScale = 1.0f;

        public QualityTier CurrentTier { get { return currentTier; } }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            ApplyFrameRateTarget();
            AutoDetectAndApplyQuality();
        }

        public static void ApplyFrameRateTarget()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        }

        public void AutoDetectAndApplyQuality()
        {
            int ramMb = SystemInfo.systemMemorySize;
            QualityTier tier = QualityTier.High;

            if (ramMb > 0 && ramMb < 4500)
                tier = QualityTier.Low;
            else if (ramMb >= 4500 && ramMb < 6500)
                tier = QualityTier.Medium;
            else
                tier = QualityTier.High;

            ApplyQualityTier(tier);
        }

        public void ApplyQualityTier(QualityTier tier)
        {
            currentTier = tier;
            int idx = (int)tier;
            if (QualitySettings.names != null && idx < QualitySettings.names.Length)
            {
                QualitySettings.SetQualityLevel(idx, true);
            }

            var urp = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (urp == null)
                urp = QualitySettings.renderPipeline as UniversalRenderPipelineAsset;

            switch (tier)
            {
                case QualityTier.Low:
                    _currentRenderScale = 0.78f;
                    QualitySettings.lodBias = 0.75f;
                    if (urp != null)
                    {
                        urp.renderScale = _currentRenderScale;
                        urp.shadowDistance = 18f;
                        urp.msaaSampleCount = 1;
                        urp.supportsHDR = false;
                    }
                    break;

                case QualityTier.Medium:
                    _currentRenderScale = 0.88f;
                    QualitySettings.lodBias = 1.0f;
                    if (urp != null)
                    {
                        urp.renderScale = _currentRenderScale;
                        urp.shadowDistance = 28f;
                        urp.msaaSampleCount = 2;
                        urp.supportsHDR = true;
                    }
                    break;

                default:
                    _currentRenderScale = 1.0f;
                    QualitySettings.lodBias = 1.2f;
                    if (urp != null)
                    {
                        urp.renderScale = _currentRenderScale;
                        urp.shadowDistance = 40f;
                        urp.msaaSampleCount = 2;
                        urp.supportsHDR = true;
                    }
                    break;
            }
        }

        private void Update()
        {
            if (!dynamicScaleEnabled) return;

            _fpsFrameCount++;
            _fpsSampleTimer += Time.unscaledDeltaTime;
            if (_fpsSampleTimer < 2.5f) return;

            float avgFps = _fpsFrameCount / _fpsSampleTimer;
            _fpsFrameCount = 0;
            _fpsSampleTimer = 0f;

            var urp = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
            if (urp == null) return;

            if (avgFps < 50f && _currentRenderScale > 0.70f)
            {
                _currentRenderScale = Mathf.Max(0.70f, _currentRenderScale - 0.05f);
                urp.renderScale = _currentRenderScale;
            }
            else if (avgFps > 58f && _currentRenderScale < 1.0f && currentTier == QualityTier.High)
            {
                _currentRenderScale = Mathf.Min(1.0f, _currentRenderScale + 0.04f);
                urp.renderScale = _currentRenderScale;
            }
        }
    }
}
