// 45: Layered Hell - 3-Phase Denek-0 (Hasan Bozkurt / Goliath) Boss Controller on Floor B4.
using UnityEngine;

namespace LayeredHell
{
    public class GoliathBoss : MonoBehaviour
    {
        private ZombieAI _zombie;
        private int _phase = 1;
        private float _specialTimer = 5.5f;
        private float _stepTimer = 0.65f;
        private bool _defeatRaised;

        public int CurrentPhase { get { return _phase; } }

        private void Awake()
        {
            _zombie = GetComponent<ZombieAI>();
        }

        private void Start()
        {
            AudioManager.Play("zmb_goliath_roar_far", transform.position, 1.0f);
            GameEvents.RaiseToast("B4 Laboratuvarı: Denek-0 (Goliath) uyandı!");
        }

        private void Update()
        {
            if (_zombie == null) return;

            if (_zombie.IsDead)
            {
                if (!_defeatRaised)
                {
                    _defeatRaised = true;
                    OnBossDefeated();
                }
                return;
            }

            if (GameManager.Instance != null && !GameManager.Instance.IsPlaying) return;

            float hpRatio = _zombie.MaxHealth > 0f ? (_zombie.Health / _zombie.MaxHealth) : 1f;

            // Phase transitions at 65% and 30% HP
            if (_phase == 1 && hpRatio <= 0.65f)
            {
                EnterPhase(2);
            }
            else if (_phase == 2 && hpRatio <= 0.30f)
            {
                EnterPhase(3);
            }

            // Heavy footstep audio while moving
            if (_zombie.State == ZombieState.Chase || _zombie.State == ZombieState.Wander)
            {
                _stepTimer -= Time.deltaTime;
                if (_stepTimer <= 0f)
                {
                    _stepTimer = _phase == 3 ? 0.42f : 0.58f;
                    AudioManager.Play("zmb_goliath_step", transform.position, 0.95f);
                }
            }

            // Periodic boss special ability (Shockwave Slam / Acid Barrage / Charge)
            if (_zombie.State == ZombieState.Chase || _zombie.State == ZombieState.Attack)
            {
                _specialTimer -= Time.deltaTime;
                if (_specialTimer <= 0f)
                {
                    _specialTimer = _phase == 3 ? 4.5f : (_phase == 2 ? 6.0f : 7.5f);
                    ExecuteBossSpecial();
                }
            }
        }

        private void EnterPhase(int newPhase)
        {
            _phase = newPhase;
            AudioManager.Play("zmb_goliath_enrage", transform.position, 1.0f);
            _zombie.PlayAnim("Scream");

            if (newPhase == 2)
            {
                _zombie.SetSpeedMultiplier(1.22f);
                GameEvents.RaiseToast("Goliath öfkelendi! Laboratuvar kapsülleri kırılıyor!");
                SpawnMinionWave(2);
            }
            else if (newPhase == 3)
            {
                _zombie.SetSpeedMultiplier(1.42f);
                GameEvents.RaiseToast("Goliath son evrede! LZR-9 mutasyonu kontrolden çıktı!");
                SpawnMinionWave(3);
            }
        }

        private void ExecuteBossSpecial()
        {
            if (PlayerController.Instance == null) return;

            Vector3 playerPos = PlayerController.Instance.Position;
            float dist = Vector3.Distance(transform.position, playerPos);

            if (dist <= 4.8f)
            {
                // Ground Shockwave Slam
                _zombie.PlayAnim("Attack2");
                AudioManager.Play("zmb_goliath_attack", transform.position, 1.0f);
                NoiseSystem.Emit(transform.position, 45f, gameObject);

                if (PlayerController.Instance.Stats != null)
                {
                    float slamDmg = 24f * (1f - Mathf.Clamp01(dist / 5.5f) * 0.5f);
                    DamageInfo info = new DamageInfo(
                        slamDmg,
                        DamageType.Explosive,
                        gameObject,
                        playerPos,
                        (playerPos - transform.position).normalized,
                        false);
                    PlayerController.Instance.Stats.TakeDamage(info);
                }
            }
            else if (_phase >= 2 && dist <= 18f)
            {
                // Mutagen Acid Spit Burst in Phase 2 & 3
                _zombie.PlayAnim("Attack");
                Vector3 origin = transform.position + Vector3.up * 2.1f + transform.forward * 0.7f;
                AcidProjectile.Spawn(origin, playerPos + Vector3.up * 1.0f, 18f, gameObject);
            }
        }

        private void SpawnMinionWave(int count)
        {
            System.Random rng = new System.Random((int)(Time.time * 100f) + _phase * 31);
            for (int i = 0; i < count; i++)
            {
                float angle = (i / (float)count) * Mathf.PI * 2f;
                Vector3 offset = new Vector3(Mathf.Cos(angle) * 5.5f, 0f, Mathf.Sin(angle) * 5.5f);
                Vector3 spawnPos = transform.position + offset;
                string arch = (i % 2 == 0) ? "runner" : "spitter";
                ZombieAI minion = ZombieFactory.Spawn(arch, spawnPos, _zombie.floor, rng);
                if (minion != null)
                {
                    minion.ForceHordeAggro();
                }
            }
        }

        private void OnBossDefeated()
        {
            AudioManager.Play("zmb_goliath_death", transform.position, 1.0f);
            GameEvents.RaiseObjective("goliath_defeated");
            GameEvents.RaiseToast("Denek-0 (Goliath) etkisiz hale getirildi!");

            // Guaranteed high-tier rewards
            WorldPickup.Spawn("antidote", 2, transform.position + new Vector3(0.6f, 0.4f, 0.6f));
            WorldPickup.Spawn("medkit", 2, transform.position + new Vector3(-0.6f, 0.4f, 0.6f));
        }
    }
}
