// Player survival stats: health, stamina, hunger, thirst, infection, armor, death, and persistence.
using System;
using UnityEngine;

namespace LayeredHell
{
    public sealed class PlayerStats : MonoBehaviour, IDamageable, ISaveable
    {
        public float Health { get; private set; } = 100f;
        public float MaxHealth { get; private set; } = 100f;
        public float Stamina { get; private set; } = 100f;
        public float MaxStamina { get; private set; } = 100f;
        public float Hunger { get; private set; } = 100f;
        public float Thirst { get; private set; } = 100f;
        public float Infection { get; private set; } = 0f;

        public float ArmorReduction
        {
            get
            {
                if (Inventory.Instance != null)
                {
                    return Mathf.Clamp(Inventory.Instance.ArmorReduction, 0f, 0.8f);
                }
                return 0f;
            }
        }

        public bool IsDead { get; private set; }
        public event Action Died;

        // Survival decay rates (units per second)
        private const float HungerDecayPerSec = 0.045f;
        private const float ThirstDecayPerSec = 0.065f;
        private const float StaminaRegenPerSec = 18f;
        private const float StaminaRegenDelay = 0.9f;
        private const float StarvationDamagePerSec = 0.8f;
        private const float InfectionAdvancePerSec = 0.02f;
        private const float InfectionDamagePerSec = 1.4f;

        private float _lastStaminaUseTime;
        private float _coughTimer = 14f;

        [Serializable]
        private struct SaveData
        {
            public float health;
            public float maxHealth;
            public float stamina;
            public float maxStamina;
            public float hunger;
            public float thirst;
            public float infection;
            public float posX;
            public float posY;
            public float posZ;
            public float yaw;
        }

        public string SaveKey { get { return "player_stats"; } }

        private void OnEnable()
        {
            if (SaveSystem.Instance != null)
            {
                SaveSystem.Instance.Register(this);
            }
        }

        private void Start()
        {
            if (SaveSystem.Instance != null)
            {
                SaveSystem.Instance.Register(this);
            }
        }

        private void OnDisable()
        {
            if (SaveSystem.Instance != null)
            {
                SaveSystem.Instance.Unregister(this);
            }
        }

        private void Update()
        {
            if (IsDead) return;
            if (GameManager.Instance != null && !GameManager.Instance.IsPlaying) return;

            float dt = Time.deltaTime;
            if (dt <= 0f) return;

            // Gradual hunger & thirst decay
            Hunger = Mathf.Clamp(Hunger - HungerDecayPerSec * dt, 0f, 100f);
            Thirst = Mathf.Clamp(Thirst - ThirstDecayPerSec * dt, 0f, 100f);

            // Stamina regeneration scales with hydration and nourishment
            if (Time.time - _lastStaminaUseTime >= StaminaRegenDelay && Stamina < MaxStamina)
            {
                float vitalsFactor = 0.45f + 0.55f * ((Hunger + Thirst) / 200f);
                Stamina = Mathf.Min(MaxStamina, Stamina + StaminaRegenPerSec * vitalsFactor * dt);
            }

            // Starvation / dehydration drain
            if (Hunger <= 0.01f || Thirst <= 0.01f)
            {
                float drain = StarvationDamagePerSec * ((Hunger <= 0.01f ? 0.5f : 0f) + (Thirst <= 0.01f ? 0.75f : 0f)) * dt;
                ApplyRawDamage(drain, false);
            }

            // Infection progression once infected
            if (Infection > 0.01f)
            {
                Infection = Mathf.Clamp(Infection + InfectionAdvancePerSec * dt, 0f, 100f);

                if (Infection >= 60f)
                {
                    _coughTimer -= dt;
                    if (_coughTimer <= 0f)
                    {
                        _coughTimer = UnityEngine.Random.Range(10f, 22f);
                        AudioManager.Play("cough", transform.position, 0.75f);
                        NoiseSystem.Emit(transform.position, 7f, gameObject);
                    }
                }

                if (Infection >= 99.9f)
                {
                    ApplyRawDamage(InfectionDamagePerSec * dt, false);
                }
            }
        }

        public void TakeDamage(DamageInfo info)
        {
            if (IsDead) return;
            if (info.amount <= 0f) return;

            float dmg = info.amount;

            // Armor reduction for physical / acid attacks
            if (info.type == DamageType.Bullet || info.type == DamageType.Melee ||
                info.type == DamageType.Explosive || info.type == DamageType.Acid)
            {
                dmg *= (1f - ArmorReduction);
            }

            // Melee / acid hits from infected carry infection buildup
            if (info.type == DamageType.Melee)
            {
                AddInfection(UnityEngine.Random.Range(2.5f, 5.5f));
            }
            else if (info.type == DamageType.Acid)
            {
                AddInfection(UnityEngine.Random.Range(3.5f, 7f));
            }
            else if (info.type == DamageType.Infection)
            {
                AddInfection(info.amount * 0.5f);
            }

            ApplyRawDamage(dmg, true);
        }

        private void ApplyRawDamage(float dmg, bool notifyEvent)
        {
            if (IsDead || dmg <= 0f) return;

            Health = Mathf.Max(0f, Health - dmg);
            if (notifyEvent)
            {
                GameEvents.RaisePlayerDamaged(dmg);
            }

            if (Health <= 0.001f)
            {
                Health = 0f;
                Die();
            }
        }

        private void Die()
        {
            if (IsDead) return;
            IsDead = true;

            try
            {
                Died?.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetState(GameState.Dead);
            }
        }

        public void Heal(float amount)
        {
            if (IsDead || amount <= 0f) return;
            Health = Mathf.Clamp(Health + amount, 0f, MaxHealth);
        }

        public void Eat(float amount)
        {
            if (IsDead || amount <= 0f) return;
            Hunger = Mathf.Clamp(Hunger + amount, 0f, 100f);
        }

        public void Drink(float amount)
        {
            if (IsDead || amount <= 0f) return;
            Thirst = Mathf.Clamp(Thirst + amount, 0f, 100f);
        }

        public void AddInfection(float amount)
        {
            if (IsDead || amount <= 0f) return;
            Infection = Mathf.Clamp(Infection + amount, 0f, 100f);
        }

        public void ReduceInfection(float amount)
        {
            if (amount <= 0f) return;
            Infection = Mathf.Clamp(Infection - amount, 0f, 100f);
        }

        public void RestoreStamina(float amount)
        {
            if (IsDead || amount <= 0f) return;
            Stamina = Mathf.Clamp(Stamina + amount, 0f, MaxStamina);
        }

        public bool UseStamina(float amount)
        {
            if (IsDead) return false;
            if (amount <= 0f) return true;
            if (Stamina < amount) return false;

            Stamina = Mathf.Max(0f, Stamina - amount);
            _lastStaminaUseTime = Time.time;
            return true;
        }

        public void Respawn()
        {
            IsDead = false;
            Health = MaxHealth;
            Stamina = MaxStamina;
            Hunger = Mathf.Max(Hunger, 75f);
            Thirst = Mathf.Max(Thirst, 75f);
            Infection = Mathf.Min(Infection, 25f);

            int targetFloor = GameConstants.BaseFloor;
            if (FloorStreamer.Instance != null)
            {
                targetFloor = FloorStreamer.Instance.CurrentFloor;
                if (targetFloor < GameConstants.MinFloor || targetFloor > GameConstants.MaxFloor)
                {
                    targetFloor = GameConstants.BaseFloor;
                }
                Vector3 spawnPos = FloorStreamer.Instance.GetPlayerSpawn(targetFloor);
                if (PlayerController.Instance != null)
                {
                    PlayerController.Instance.Teleport(spawnPos, Quaternion.identity);
                }
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetState(GameState.Playing);
            }
        }

        public string CaptureState()
        {
            Vector3 pos = transform.position;
            SaveData data = new SaveData
            {
                health = Health,
                maxHealth = MaxHealth,
                stamina = Stamina,
                maxStamina = MaxStamina,
                hunger = Hunger,
                thirst = Thirst,
                infection = Infection,
                posX = pos.x,
                posY = pos.y,
                posZ = pos.z,
                yaw = transform.eulerAngles.y
            };
            return JsonUtility.ToJson(data);
        }

        public void RestoreState(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            MaxHealth = data.maxHealth > 1f ? data.maxHealth : 100f;
            MaxStamina = data.maxStamina > 1f ? data.maxStamina : 100f;
            Health = Mathf.Clamp(data.health > 0f ? data.health : MaxHealth, 1f, MaxHealth);
            Stamina = Mathf.Clamp(data.stamina, 0f, MaxStamina);
            Hunger = Mathf.Clamp(data.hunger, 0f, 100f);
            Thirst = Mathf.Clamp(data.thirst, 0f, 100f);
            Infection = Mathf.Clamp(data.infection, 0f, 100f);
            IsDead = false;

            if (PlayerController.Instance != null && (Mathf.Abs(data.posX) > 0.001f || Mathf.Abs(data.posY) > 0.001f || Mathf.Abs(data.posZ) > 0.001f))
            {
                PlayerController.Instance.Teleport(
                    new Vector3(data.posX, data.posY, data.posZ),
                    Quaternion.Euler(0f, data.yaw, 0f));
            }
        }
    }
}
