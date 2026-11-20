// First-person character controller handling movement, look, crouch, sprint, jump, head-bob, and quick actions.
using UnityEngine;

namespace LayeredHell
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerController : MonoBehaviour
    {
        public static PlayerController Instance { get; private set; }

        public Transform CameraTransform { get; set; }
        public PlayerStats Stats { get; set; }
        public WeaponController Weapons { get; set; }
        public Light Flashlight { get; set; }
        public Animator ArmsAnimator { get; set; }

        public Vector3 Position { get { return transform.position; } }
        public bool IsGrounded { get; private set; }
        public bool IsCrouching { get; private set; }
        public bool IsSprinting { get; private set; }
        public bool InputLocked { get; private set; }

        public SurfaceType CurrentSurface
        {
            get
            {
                if (_footsteps != null) return _footsteps.CurrentSurface;
                return SurfaceType.Concrete;
            }
        }

        private CharacterController _cc;
        private FootstepPlayer _footsteps;
        private Camera _cam;

        private const float WalkSpeed = 3.8f;
        private const float SprintSpeed = 6.2f;
        private const float CrouchSpeed = 2.15f;
        private const float JumpSpeed = 5.4f;
        private const float Gravity = -16.5f;
        private const float StandingHeight = 1.8f;
        private const float CrouchingHeight = 1.1f;
        private const float StandingEyeY = 1.62f;
        private const float CrouchingEyeY = 0.96f;
        private const float SprintStaminaPerSec = 11f;
        private const float JumpStaminaCost = 12f;

        private float _yaw;
        private float _pitch;
        private float _verticalVelocity;
        private float _currentHeight = StandingHeight;
        private float _currentEyeY = StandingEyeY;
        private float _bobPhase;
        private bool _wasGrounded = true;
        private float _fallStartY;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            _cc = GetComponent<CharacterController>();
            _footsteps = GetComponent<FootstepPlayer>();
            if (Stats == null) Stats = GetComponent<PlayerStats>();
            if (Weapons == null) Weapons = GetComponent<WeaponController>();

            _yaw = transform.eulerAngles.y;
            _fallStartY = transform.position.y;
        }

        private void Start()
        {
            if (CameraTransform != null)
            {
                _cam = CameraTransform.GetComponent<Camera>();
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void SetInputLocked(bool locked)
        {
            InputLocked = locked;
            if (locked)
            {
                IsSprinting = false;
            }
        }

        public void Teleport(Vector3 pos, Quaternion rot)
        {
            if (_cc == null) _cc = GetComponent<CharacterController>();
            bool wasEnabled = _cc != null && _cc.enabled;
            if (_cc != null) _cc.enabled = false;

            transform.SetPositionAndRotation(pos, Quaternion.Euler(0f, rot.eulerAngles.y, 0f));
            _yaw = rot.eulerAngles.y;
            _pitch = 0f;
            _verticalVelocity = 0f;
            _fallStartY = pos.y;

            if (CameraTransform != null)
            {
                CameraTransform.localRotation = Quaternion.identity;
            }

            if (_cc != null && wasEnabled) _cc.enabled = true;
        }

        private void Update()
        {
            if (GameManager.Instance != null && !GameManager.Instance.IsPlaying)
            {
                return;
            }

            if (Stats != null && Stats.IsDead)
            {
                return;
            }

            if (InputRouter.MenuPressed && GameManager.Instance == null && GameMenuUI.Instance != null)
            {
                GameMenuUI.Instance.Toggle();
            }

            if (InputLocked || (GameMenuUI.Instance != null && GameMenuUI.Instance.IsOpen))
            {
                ApplyGravityOnly();
                return;
            }

            HandleQuickActions();
            HandleLook();
            HandleStance();
            HandleMovement();
            HandleCameraEffects();
        }

        private void HandleQuickActions()
        {
            if (InputRouter.FlashlightPressed && Flashlight != null)
            {
                Flashlight.enabled = !Flashlight.enabled;
                AudioManager.Play2D("flashlight_click", 0.75f);
            }

            if (InputRouter.QuickHealPressed && Inventory.Instance != null && Stats != null)
            {
                if (Stats.Health < Stats.MaxHealth - 1f)
                {
                    if (!Inventory.Instance.Use("medkit"))
                    {
                        if (!Inventory.Instance.Use("bandage"))
                        {
                            Inventory.Instance.Use("painkiller");
                        }
                    }
                }
                else if (Stats.Infection > 5f)
                {
                    if (!Inventory.Instance.Use("antidote"))
                    {
                        Inventory.Instance.Use("antibiotic");
                    }
                }
            }

            if (InputRouter.NextWeaponPressed && Weapons != null)
            {
                Weapons.CycleWeapon();
            }
        }

        private void HandleLook()
        {
            Vector2 look = InputRouter.Look;
            _yaw += look.x;
            _pitch = Mathf.Clamp(_pitch - look.y, -85f, 85f);

            transform.rotation = Quaternion.Euler(0f, _yaw, 0f);
            if (CameraTransform != null)
            {
                CameraTransform.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
            }
        }

        private void HandleStance()
        {
            if (InputRouter.CrouchPressed)
            {
                if (IsCrouching)
                {
                    if (CanStandUp())
                    {
                        IsCrouching = false;
                    }
                }
                else
                {
                    IsCrouching = true;
                    IsSprinting = false;
                }
            }

            float targetHeight = IsCrouching ? CrouchingHeight : StandingHeight;
            float targetEyeY = IsCrouching ? CrouchingEyeY : StandingEyeY;

            _currentHeight = Mathf.Lerp(_currentHeight, targetHeight, Time.deltaTime * 12f);
            _currentEyeY = Mathf.Lerp(_currentEyeY, targetEyeY, Time.deltaTime * 12f);

            if (_cc != null)
            {
                _cc.height = _currentHeight;
                _cc.center = new Vector3(0f, _currentHeight * 0.5f, 0f);
            }
        }

        private bool CanStandUp()
        {
            Vector3 origin = transform.position + Vector3.up * CrouchingHeight;
            float checkDist = StandingHeight - CrouchingHeight + 0.1f;
            return !Physics.SphereCast(origin, 0.25f, Vector3.up, out _, checkDist, ~LayerNames.PlayerMask, QueryTriggerInteraction.Ignore);
        }

        private void HandleMovement()
        {
            if (_cc == null || !_cc.enabled) return;

            IsGrounded = _cc.isGrounded;

            if (IsGrounded && !_wasGrounded)
            {
                float fallDistance = _fallStartY - transform.position.y;
                if (fallDistance > 4.2f && Stats != null)
                {
                    float fallDamage = (fallDistance - 4.0f) * 14f;
                    Stats.TakeDamage(new DamageInfo(fallDamage, DamageType.Fall, gameObject, transform.position, Vector3.down));
                }
                if (fallDistance > 1.0f)
                {
                    AudioManager.Play("jump_land", transform.position, 0.7f);
                    NoiseSystem.Emit(transform.position, 8f, gameObject);
                }
            }
            else if (!IsGrounded && _wasGrounded)
            {
                _fallStartY = transform.position.y;
            }
            else if (!IsGrounded && transform.position.y > _fallStartY)
            {
                _fallStartY = transform.position.y;
            }
            _wasGrounded = IsGrounded;

            Vector2 moveInput = InputRouter.Move;
            bool wantsSprint = InputRouter.SprintHeld && moveInput.y > 0.25f && !InputRouter.AimHeld;

            if (wantsSprint && IsCrouching && CanStandUp())
            {
                IsCrouching = false;
            }

            if (wantsSprint && !IsCrouching && Stats != null && Stats.UseStamina(SprintStaminaPerSec * Time.deltaTime))
            {
                IsSprinting = true;
            }
            else
            {
                IsSprinting = false;
            }

            float speed = WalkSpeed;
            if (IsCrouching) speed = CrouchSpeed;
            else if (IsSprinting) speed = SprintSpeed;

            if (InputRouter.AimHeld)
            {
                speed *= 0.78f;
            }

            Vector3 desiredMove = (transform.right * moveInput.x + transform.forward * moveInput.y) * speed;

            if (IsGrounded)
            {
                if (_verticalVelocity < 0f)
                {
                    _verticalVelocity = -2f;
                }

                if (InputRouter.JumpPressed && !IsCrouching)
                {
                    if (Stats == null || Stats.UseStamina(JumpStaminaCost))
                    {
                        _verticalVelocity = JumpSpeed;
                        IsGrounded = false;
                    }
                }
            }
            else
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }

            desiredMove.y = _verticalVelocity;
            _cc.Move(desiredMove * Time.deltaTime);
        }

        private void ApplyGravityOnly()
        {
            if (_cc == null || !_cc.enabled) return;
            IsGrounded = _cc.isGrounded;
            if (IsGrounded && _verticalVelocity < 0f) _verticalVelocity = -2f;
            else _verticalVelocity += Gravity * Time.deltaTime;
            _cc.Move(new Vector3(0f, _verticalVelocity, 0f) * Time.deltaTime);
        }

        private void HandleCameraEffects()
        {
            if (CameraTransform == null) return;

            float bobOffsetY = 0f;
            float bobOffsetX = 0f;

            if (GameSettings.HeadBob && IsGrounded && _cc != null)
            {
                Vector3 hVel = _cc.velocity;
                hVel.y = 0f;
                float spd = hVel.magnitude;
                if (spd > 0.35f)
                {
                    float freq = IsSprinting ? 13.5f : (IsCrouching ? 6.5f : 9.5f);
                    float amp = IsSprinting ? 0.048f : (IsCrouching ? 0.018f : 0.03f);
                    _bobPhase += Time.deltaTime * freq;
                    bobOffsetY = Mathf.Sin(_bobPhase) * amp;
                    bobOffsetX = Mathf.Cos(_bobPhase * 0.5f) * amp * 0.6f;
                }
                else
                {
                    _bobPhase = 0f;
                }
            }

            CameraTransform.localPosition = new Vector3(bobOffsetX, _currentEyeY + bobOffsetY, 0f);

            if (_cam == null) _cam = CameraTransform.GetComponent<Camera>();
            if (_cam != null)
            {
                float baseFov = GameSettings.FieldOfView;
                float targetFov = InputRouter.AimHeld ? baseFov * 0.82f : (IsSprinting ? baseFov + 4f : baseFov);
                _cam.fieldOfView = Mathf.Lerp(_cam.fieldOfView, targetFov, Time.deltaTime * 10f);
            }
        }
    }
}
