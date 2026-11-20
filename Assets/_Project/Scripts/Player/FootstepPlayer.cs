// Emits surface-appropriate footstep audio and AI noise pulses based on player movement speed and stance.
using UnityEngine;

namespace LayeredHell
{
    public sealed class FootstepPlayer : MonoBehaviour
    {
        private PlayerController _controller;
        private CharacterController _cc;
        private float _stepTimer;
        private SurfaceType _detectedSurface = SurfaceType.Concrete;

        private const float WalkStepInterval = 0.52f;
        private const float RunStepInterval = 0.34f;
        private const float CrouchStepInterval = 0.72f;

        private const float WalkNoiseRadius = 6f;
        private const float RunNoiseRadius = 12f;
        private const float CrouchNoiseRadius = 2f;

        public SurfaceType CurrentSurface { get { return _detectedSurface; } }

        private void Awake()
        {
            _controller = GetComponent<PlayerController>();
            _cc = GetComponent<CharacterController>();
        }

        private void Update()
        {
            if (_controller == null) _controller = GetComponent<PlayerController>();
            if (_cc == null) _cc = GetComponent<CharacterController>();
            if (_controller == null || _cc == null) return;
            if (GameManager.Instance != null && !GameManager.Instance.IsPlaying) return;

            UpdateSurfaceDetection();

            if (!_controller.IsGrounded)
            {
                return;
            }

            Vector3 horizVel = _cc.velocity;
            horizVel.y = 0f;
            float speed = horizVel.magnitude;

            if (speed < 0.35f && InputRouter.Move.sqrMagnitude < 0.04f)
            {
                _stepTimer = 0.12f;
                return;
            }

            bool sprinting = _controller.IsSprinting;
            bool crouching = _controller.IsCrouching;

            float interval = WalkStepInterval;
            if (sprinting) interval = RunStepInterval;
            else if (crouching) interval = CrouchStepInterval;

            _stepTimer -= Time.deltaTime;
            if (_stepTimer <= 0f)
            {
                _stepTimer = interval;
                EmitFootstep(sprinting, crouching);
            }
        }

        public SurfaceType ProbeSurface()
        {
            UpdateSurfaceDetection();
            return _detectedSurface;
        }

        private void UpdateSurfaceDetection()
        {
            Vector3 origin = transform.position + Vector3.up * 0.35f;
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 1.2f, ~0, QueryTriggerInteraction.Ignore))
            {
                SurfaceTag tag = hit.collider.GetComponentInParent<SurfaceTag>();
                if (tag != null)
                {
                    _detectedSurface = tag.surface;
                    return;
                }
            }
            _detectedSurface = SurfaceType.Concrete;
        }

        private void EmitFootstep(bool sprinting, bool crouching)
        {
            string surfaceName = GetSurfaceString(_detectedSurface);
            string mode = sprinting ? "run" : (crouching ? "crouch" : "walk");
            float volume = sprinting ? 0.95f : (crouching ? 0.42f : 0.72f);
            float radius = sprinting ? RunNoiseRadius : (crouching ? CrouchNoiseRadius : WalkNoiseRadius);

            string clipBase = "step_" + surfaceName + "_" + mode;
            AudioManager.Play(clipBase, transform.position, volume, 0.08f);
            NoiseSystem.Emit(transform.position, radius, gameObject);
        }

        private static string GetSurfaceString(SurfaceType surface)
        {
            switch (surface)
            {
                case SurfaceType.Parquet: return "parquet";
                case SurfaceType.Tile: return "tile";
                case SurfaceType.Water: return "water";
                case SurfaceType.Glass: return "glass";
                case SurfaceType.MetalStair: return "metalstair";
                default: return "concrete";
            }
        }
    }
}
