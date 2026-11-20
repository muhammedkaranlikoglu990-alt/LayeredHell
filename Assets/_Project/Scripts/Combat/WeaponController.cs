// Handles equipped weapon state, hitscan firing, shotgun pellets, melee arcs, throwables, reload coroutines, and persistence.
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LayeredHell
{
    public sealed class WeaponController : MonoBehaviour, ISaveable
    {
        private static readonly string[] AllWeaponOrder = new string[]
        {
            "pistol", "revolver", "shotgun", "smg", "rifle", "sniper",
            "pipe", "bat", "axe", "machete", "grenade", "molotov"
        };

        public string CurrentWeaponId { get; private set; } = string.Empty;
        public int AmmoInMag { get; private set; }
        public int ReserveAmmo { get; private set; }
        public bool IsReloading { get; private set; }

        public event Action AmmoChanged;

        private readonly Dictionary<string, int> _magCounts = new Dictionary<string, int>();
        private PlayerController _player;
        private Coroutine _reloadRoutine;
        private GameObject _heldWeaponVisual;
        private float _nextFireTime;
        private bool _subscribedInventory;

        [Serializable]
        private struct MagEntry
        {
            public string weaponId;
            public int count;
        }

        [Serializable]
        private struct SaveData
        {
            public string currentWeaponId;
            public List<MagEntry> mags;
        }

        public string SaveKey { get { return "weapon_controller"; } }

        private void Awake()
        {
            _player = GetComponent<PlayerController>();
        }

        private void OnEnable()
        {
            if (SaveSystem.Instance != null)
            {
                SaveSystem.Instance.Register(this);
            }
            TrySubscribeInventory();
        }

        private void Start()
        {
            if (SaveSystem.Instance != null)
            {
                SaveSystem.Instance.Register(this);
            }
            TrySubscribeInventory();

            if (string.IsNullOrEmpty(CurrentWeaponId) && Inventory.Instance != null)
            {
                if (!string.IsNullOrEmpty(Inventory.Instance.EquippedWeaponId))
                {
                    Equip(Inventory.Instance.EquippedWeaponId);
                }
                else
                {
                    CycleWeapon();
                }
            }
        }

        private void OnDisable()
        {
            if (SaveSystem.Instance != null)
            {
                SaveSystem.Instance.Unregister(this);
            }
            UnsubscribeInventory();
        }

        private void TrySubscribeInventory()
        {
            if (_subscribedInventory || Inventory.Instance == null) return;
            _subscribedInventory = true;
            Inventory.Instance.WeaponEquipped += OnInventoryWeaponEquipped;
            Inventory.Instance.Changed += OnInventoryChanged;
        }

        private void UnsubscribeInventory()
        {
            if (!_subscribedInventory || Inventory.Instance == null) return;
            _subscribedInventory = false;
            Inventory.Instance.WeaponEquipped -= OnInventoryWeaponEquipped;
            Inventory.Instance.Changed -= OnInventoryChanged;
        }

        private void OnInventoryWeaponEquipped(string weaponId)
        {
            if (string.IsNullOrEmpty(weaponId))
            {
                Unequip();
            }
            else if (CurrentWeaponId != weaponId)
            {
                Equip(weaponId);
            }
        }

        private void OnInventoryChanged()
        {
            RefreshAmmoCounts();
        }

        public void Equip(string weaponId)
        {
            if (string.IsNullOrEmpty(weaponId))
            {
                Unequip();
                return;
            }

            WeaponDef def = ItemDatabase.GetWeapon(weaponId);
            if (def == null) return;

            CancelReload();

            if (!string.IsNullOrEmpty(CurrentWeaponId))
            {
                _magCounts[CurrentWeaponId] = AmmoInMag;
            }

            CurrentWeaponId = weaponId;

            if (Inventory.Instance != null && Inventory.Instance.EquippedWeaponId != weaponId)
            {
                Inventory.Instance.EquipWeapon(weaponId);
            }

            if (def.melee)
            {
                AmmoInMag = 0;
                ReserveAmmo = 0;
            }
            else if (def.thrown)
            {
                int owned = Inventory.Instance != null ? Inventory.Instance.Count(weaponId) : 1;
                AmmoInMag = owned > 0 ? 1 : 0;
                ReserveAmmo = Mathf.Max(0, owned - AmmoInMag);
            }
            else
            {
                if (!_magCounts.TryGetValue(weaponId, out int savedMag))
                {
                    savedMag = def.magSize;
                    _magCounts[weaponId] = savedMag;
                }
                AmmoInMag = Mathf.Clamp(savedMag, 0, def.magSize);
                ReserveAmmo = (Inventory.Instance != null && !string.IsNullOrEmpty(def.ammoId))
                    ? Inventory.Instance.Count(def.ammoId)
                    : 0;
            }

            SpawnHeldVisual(def);
            PlayArmsState("FPS_Draw", 0.08f);

            if (!def.melee && !def.thrown)
            {
                AudioManager.Play("wpn_" + def.id + "_draw", transform.position, 0.75f);
            }
            else
            {
                AudioManager.Play2D("ui_equip", 0.7f);
            }

            _nextFireTime = Time.time + 0.2f;
            RaiseAmmoChanged();
        }

        public void Unequip()
        {
            CancelReload();
            if (!string.IsNullOrEmpty(CurrentWeaponId))
            {
                _magCounts[CurrentWeaponId] = AmmoInMag;
            }
            CurrentWeaponId = string.Empty;
            AmmoInMag = 0;
            ReserveAmmo = 0;

            if (_heldWeaponVisual != null)
            {
                Destroy(_heldWeaponVisual);
                _heldWeaponVisual = null;
            }

            RaiseAmmoChanged();
        }

        public void CycleWeapon()
        {
            if (Inventory.Instance == null) return;

            int startIdx = -1;
            for (int i = 0; i < AllWeaponOrder.Length; i++)
            {
                if (AllWeaponOrder[i] == CurrentWeaponId)
                {
                    startIdx = i;
                    break;
                }
            }

            for (int step = 1; step <= AllWeaponOrder.Length; step++)
            {
                int idx = (startIdx + step) % AllWeaponOrder.Length;
                string candidate = AllWeaponOrder[idx];
                if (Inventory.Instance.Has(candidate, 1))
                {
                    Equip(candidate);
                    return;
                }
            }
        }

        private void Update()
        {
            if (!_subscribedInventory) TrySubscribeInventory();

            if (GameManager.Instance != null && !GameManager.Instance.IsPlaying) return;
            if (_player == null) _player = GetComponent<PlayerController>();
            if (_player != null && (_player.InputLocked || (_player.Stats != null && _player.Stats.IsDead))) return;
            if (GameMenuUI.Instance != null && GameMenuUI.Instance.IsOpen) return;

            UpdateMovementArmsAnimation();

            if (string.IsNullOrEmpty(CurrentWeaponId)) return;

            WeaponDef def = ItemDatabase.GetWeapon(CurrentWeaponId);
            if (def == null) return;

            if (InputRouter.ReloadPressed && !IsReloading && !def.melee && !def.thrown)
            {
                TryStartReload(def);
                return;
            }

            bool trigger = def.automatic ? InputRouter.FireHeld : InputRouter.FirePressed;
            if (trigger && !IsReloading && Time.time >= _nextFireTime)
            {
                float interval = 1f / Mathf.Max(0.2f, def.fireRate);
                _nextFireTime = Time.time + interval;

                if (def.melee)
                {
                    PerformMeleeAttack(def);
                }
                else if (def.thrown)
                {
                    PerformThrowAttack(def);
                }
                else
                {
                    PerformRangedAttack(def, InputRouter.FirePressed);
                }
            }
        }

        private void PerformMeleeAttack(WeaponDef def)
        {
            float dmgMultiplier = 1f;
            if (_player != null && _player.Stats != null)
            {
                if (!_player.Stats.UseStamina(8f))
                {
                    dmgMultiplier = 0.55f;
                }
            }

            PlayArmsState("FPS_Melee", 0.05f);
            AudioManager.Play("melee_swing_" + def.id, transform.position, 0.9f);
            NoiseSystem.Emit(transform.position, def.noise > 0f ? def.noise : 8f, gameObject);

            Transform cam = GetCameraTransform();
            Vector3 origin = cam.position;
            Vector3 forward = cam.forward;
            float range = Mathf.Max(1.85f, def.range);

            int mask = ~LayerNames.PlayerMask;
            if (Physics.SphereCast(origin, 0.28f, forward, out RaycastHit hit, range, mask, QueryTriggerInteraction.Collide))
            {
                IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();
                bool isHead = IsHeadCollider(hit.collider);

                if (damageable != null)
                {
                    float headMult = isHead ? Mathf.Max(1.5f, def.headshotMultiplier) : 1f;
                    float finalDmg = def.damage * dmgMultiplier * headMult;
                    damageable.TakeDamage(new DamageInfo(finalDmg, DamageType.Melee, gameObject, hit.point, forward, isHead));
                    AudioManager.Play(isHead ? "melee_hit_bone" : "melee_hit_flesh", hit.point, 0.95f);
                }
                else
                {
                    AudioManager.Play("melee_hit_metal", hit.point, 0.75f);
                }
            }
        }

        private void PerformThrowAttack(WeaponDef def)
        {
            if (Inventory.Instance != null && !Inventory.Instance.Remove(def.id, 1))
            {
                CycleWeapon();
                return;
            }

            PlayArmsState("FPS_Fire", 0.05f);
            Transform cam = GetCameraTransform();
            Vector3 launchOrigin = cam.position + cam.forward * 0.45f + Vector3.down * 0.08f;
            Vector3 launchVel = cam.forward * 15.5f + Vector3.up * 2.4f;

            ThrowableProjectile.Launch(def.id, launchOrigin, launchVel, def.damage, def.noise, gameObject);

            RefreshAmmoCounts();
            if (Inventory.Instance != null && !Inventory.Instance.Has(def.id, 1))
            {
                CycleWeapon();
            }
        }

        private void PerformRangedAttack(WeaponDef def, bool justPressed)
        {
            if (AmmoInMag <= 0)
            {
                if (justPressed)
                {
                    AudioManager.Play("wpn_" + def.id + "_empty", transform.position, 0.85f);
                    if (ReserveAmmo > 0)
                    {
                        TryStartReload(def);
                    }
                }
                return;
            }

            AmmoInMag--;
            _magCounts[CurrentWeaponId] = AmmoInMag;
            RaiseAmmoChanged();

            PlayArmsState("FPS_Fire", 0.04f);
            AudioManager.Play("wpn_" + def.id + "_fire", transform.position, 1.0f);
            AudioManager.Play("wpn_" + def.id + "_echo", transform.position, 0.55f);

            string shellSurf = "concrete";
            if (_player != null && _player.CurrentSurface == SurfaceType.MetalStair) shellSurf = "metal";
            else if (_player != null && _player.CurrentSurface == SurfaceType.Parquet) shellSurf = "wood";
            AudioManager.Play("shell_" + shellSurf, transform.position, 0.4f);

            NoiseSystem.Emit(transform.position, def.noise > 0f ? def.noise : 60f, gameObject);

            Transform cam = GetCameraTransform();
            Vector3 origin = cam.position;
            float spreadDeg = def.spread * (InputRouter.AimHeld ? 0.45f : 1.0f);
            int pellets = Mathf.Max(1, def.pellets);
            float range = Mathf.Max(20f, def.range);
            int mask = ~LayerNames.PlayerMask;

            for (int p = 0; p < pellets; p++)
            {
                Vector3 dir = ApplySpread(cam.forward, spreadDeg);
                if (Physics.Raycast(origin, dir, out RaycastHit hit, range, mask, QueryTriggerInteraction.Collide))
                {
                    IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();
                    bool isHead = IsHeadCollider(hit.collider);

                    if (damageable != null)
                    {
                        float headMult = isHead ? Mathf.Max(1.5f, def.headshotMultiplier) : 1f;
                        float dmg = def.damage * headMult;
                        damageable.TakeDamage(new DamageInfo(dmg, DamageType.Bullet, gameObject, hit.point, dir, isHead));
                        AudioManager.Play(isHead ? "melee_hit_bone" : "melee_hit_flesh", hit.point, 0.85f);
                    }
                    else
                    {
                        SurfaceTag surf = hit.collider.GetComponentInParent<SurfaceTag>();
                        string imp = "impact_concrete";
                        if (surf != null)
                        {
                            if (surf.surface == SurfaceType.Glass) imp = "impact_glass";
                            else if (surf.surface == SurfaceType.MetalStair) imp = "impact_metal";
                        }
                        AudioManager.Play(imp, hit.point, 0.75f);
                    }
                }
            }

            if (AmmoInMag <= 0 && ReserveAmmo > 0)
            {
                TryStartReload(def);
            }
        }

        private void TryStartReload(WeaponDef def)
        {
            if (IsReloading || def == null || def.melee || def.thrown) return;
            if (AmmoInMag >= def.magSize) return;
            RefreshAmmoCounts();
            if (ReserveAmmo <= 0)
            {
                AudioManager.Play("wpn_" + def.id + "_empty", transform.position, 0.8f);
                return;
            }

            _reloadRoutine = StartCoroutine(ReloadRoutine(def));
        }

        private IEnumerator ReloadRoutine(WeaponDef def)
        {
            IsReloading = true;
            RaiseAmmoChanged();

            PlayArmsState("FPS_Reload", 0.08f);
            AudioManager.Play("wpn_" + def.id + "_reload_out", transform.position, 0.85f);

            float duration = Mathf.Max(0.5f, def.reloadTime);
            yield return new WaitForSeconds(duration * 0.65f);

            AudioManager.Play("wpn_" + def.id + "_reload_in", transform.position, 0.85f);
            yield return new WaitForSeconds(duration * 0.35f);

            int needed = def.magSize - AmmoInMag;
            if (needed > 0 && Inventory.Instance != null && !string.IsNullOrEmpty(def.ammoId))
            {
                int available = Inventory.Instance.Count(def.ammoId);
                int take = Mathf.Min(needed, available);
                if (take > 0 && Inventory.Instance.Remove(def.ammoId, take))
                {
                    AmmoInMag += take;
                    _magCounts[CurrentWeaponId] = AmmoInMag;
                }
            }

            IsReloading = false;
            _reloadRoutine = null;
            RefreshAmmoCounts();
        }

        private void CancelReload()
        {
            if (_reloadRoutine != null)
            {
                StopCoroutine(_reloadRoutine);
                _reloadRoutine = null;
            }
            IsReloading = false;
        }

        private void RefreshAmmoCounts()
        {
            if (string.IsNullOrEmpty(CurrentWeaponId))
            {
                ReserveAmmo = 0;
                RaiseAmmoChanged();
                return;
            }

            WeaponDef def = ItemDatabase.GetWeapon(CurrentWeaponId);
            if (def == null || def.melee)
            {
                AmmoInMag = 0;
                ReserveAmmo = 0;
            }
            else if (def.thrown)
            {
                int total = Inventory.Instance != null ? Inventory.Instance.Count(CurrentWeaponId) : 0;
                AmmoInMag = total > 0 ? 1 : 0;
                ReserveAmmo = Mathf.Max(0, total - AmmoInMag);
            }
            else
            {
                ReserveAmmo = (Inventory.Instance != null && !string.IsNullOrEmpty(def.ammoId))
                    ? Inventory.Instance.Count(def.ammoId)
                    : 0;
            }

            RaiseAmmoChanged();
        }

        private void SpawnHeldVisual(WeaponDef def)
        {
            if (_heldWeaponVisual != null)
            {
                Destroy(_heldWeaponVisual);
                _heldWeaponVisual = null;
            }

            Transform cam = GetCameraTransform();
            if (cam == null || def == null) return;

            string modelId = !string.IsNullOrEmpty(def.modelId) ? def.modelId : ("wpn_" + def.id + "_01");
            if (ModelLibrary.Instance != null && ModelLibrary.Instance.Has(modelId))
            {
                GameObject prefab = ModelLibrary.Instance.Get(modelId);
                if (prefab != null)
                {
                    _heldWeaponVisual = Instantiate(prefab, cam, false);
                }
            }

            if (_heldWeaponVisual == null)
            {
                _heldWeaponVisual = ModelLibrary.InstantiateOrPlaceholder(
                    modelId,
                    cam,
                    def.melee ? new Vector3(0.05f, 0.38f, 0.05f) : new Vector3(0.06f, 0.1f, 0.28f),
                    new Color(0.24f, 0.25f, 0.27f));
            }

            if (_heldWeaponVisual != null)
            {
                _heldWeaponVisual.name = "Held_" + def.id;
                _heldWeaponVisual.transform.localPosition = new Vector3(0.22f, -0.20f, 0.42f);
                _heldWeaponVisual.transform.localRotation = Quaternion.identity;

                Collider[] cols = _heldWeaponVisual.GetComponentsInChildren<Collider>();
                for (int i = 0; i < cols.Length; i++)
                {
                    cols[i].enabled = false;
                }
            }
        }

        private string _lastMoveAnim = string.Empty;

        private void UpdateMovementArmsAnimation()
        {
            if (IsReloading || Time.time < _nextFireTime) return;
            if (_player == null || _player.ArmsAnimator == null) return;

            float moveMag = InputRouter.Move.sqrMagnitude;
            string desired = "FPS_Idle";
            if (moveMag > 0.05f && _player.IsGrounded)
            {
                desired = _player.IsSprinting ? "FPS_Run" : "FPS_Walk";
            }

            if (_lastMoveAnim != desired)
            {
                _lastMoveAnim = desired;
                _player.ArmsAnimator.CrossFadeInFixedTime(desired, 0.15f);
            }
        }

        private void PlayArmsState(string clipName, float fade)
        {
            _lastMoveAnim = clipName;
            if (_player != null && _player.ArmsAnimator != null)
            {
                _player.ArmsAnimator.CrossFadeInFixedTime(clipName, fade);
            }
        }

        private Transform GetCameraTransform()
        {
            if (_player != null && _player.CameraTransform != null) return _player.CameraTransform;
            if (Camera.main != null) return Camera.main.transform;
            return transform;
        }

        private static bool IsHeadCollider(Collider col)
        {
            if (col == null) return false;
            if (col.name.IndexOf("Head", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            if (col.transform.parent != null && col.transform.parent.name.IndexOf("Head", StringComparison.OrdinalIgnoreCase) >= 0) return true;
            return false;
        }

        private static Vector3 ApplySpread(Vector3 forward, float spreadDegrees)
        {
            if (spreadDegrees <= 0.01f) return forward;
            float rad = spreadDegrees * Mathf.Deg2Rad;
            Vector2 rnd = UnityEngine.Random.insideUnitCircle * Mathf.Tan(rad);
            Quaternion rot = Quaternion.LookRotation(forward);
            return (forward + rot * new Vector3(rnd.x, rnd.y, 0f)).normalized;
        }

        private void RaiseAmmoChanged()
        {
            try
            {
                AmmoChanged?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public string CaptureState()
        {
            if (!string.IsNullOrEmpty(CurrentWeaponId))
            {
                _magCounts[CurrentWeaponId] = AmmoInMag;
            }

            SaveData data = new SaveData
            {
                currentWeaponId = CurrentWeaponId,
                mags = new List<MagEntry>()
            };

            foreach (KeyValuePair<string, int> kv in _magCounts)
            {
                data.mags.Add(new MagEntry { weaponId = kv.Key, count = kv.Value });
            }

            return JsonUtility.ToJson(data);
        }

        public void RestoreState(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            _magCounts.Clear();

            if (data.mags != null)
            {
                for (int i = 0; i < data.mags.Count; i++)
                {
                    MagEntry entry = data.mags[i];
                    if (!string.IsNullOrEmpty(entry.weaponId))
                    {
                        _magCounts[entry.weaponId] = entry.count;
                    }
                }
            }

            if (!string.IsNullOrEmpty(data.currentWeaponId))
            {
                Equip(data.currentWeaponId);
            }
        }
    }
}
