// 45: Layered Hell - Spitter acid projectile dealing corrosive damage and LZR-9 infection.
using UnityEngine;

namespace LayeredHell
{
    public class AcidProjectile : MonoBehaviour
    {
        public Vector3 velocity;
        public float gravity = 6.5f;
        public float damage = 14f;
        public float infectionGain = 4.5f;
        public float splashRadius = 1.85f;
        public float maxLifetime = 4.5f;
        public GameObject shooter;

        private float _age;
        private bool _detonated;

        public static AcidProjectile Spawn(Vector3 origin, Vector3 targetPos, float damage, GameObject shooter)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.name = "AcidProjectile";
            go.transform.position = origin;
            go.transform.localScale = new Vector3(0.26f, 0.26f, 0.26f);

            Collider col = go.GetComponent<Collider>();
            if (col != null)
            {
                col.isTrigger = true;
            }

            Renderer rend = go.GetComponent<Renderer>();
            if (rend != null && rend.material != null)
            {
                rend.material.color = new Color(0.35f, 0.95f, 0.25f, 1f);
            }

            AcidProjectile proj = go.AddComponent<AcidProjectile>();
            proj.damage = damage;
            proj.shooter = shooter;

            Vector3 toTarget = targetPos - origin;
            float dist = Mathf.Max(1f, new Vector2(toTarget.x, toTarget.z).magnitude);
            float flightTime = Mathf.Clamp(dist / 14f, 0.25f, 1.35f);
            Vector3 v = toTarget / flightTime;
            v.y += 0.5f * proj.gravity * flightTime;
            proj.velocity = v;

            AudioManager.Play("zmb_spit", origin);
            return proj;
        }

        private void Update()
        {
            if (_detonated) return;

            float dt = Time.deltaTime;
            _age += dt;
            if (_age >= maxLifetime)
            {
                Detonate(transform.position);
                return;
            }

            Vector3 prev = transform.position;
            velocity.y -= gravity * dt;
            Vector3 next = prev + velocity * dt;
            Vector3 step = next - prev;
            float dist = step.magnitude;

            if (dist > 0.001f)
            {
                if (Physics.Raycast(prev, step / dist, out RaycastHit hit, dist, ~0, QueryTriggerInteraction.Ignore))
                {
                    if (shooter == null || !hit.transform.IsChildOf(shooter.transform))
                    {
                        Detonate(hit.point, hit.collider);
                        return;
                    }
                }
            }

            transform.position = next;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_detonated || other == null) return;
            if (shooter != null && other.transform.IsChildOf(shooter.transform)) return;
            Detonate(transform.position, other);
        }

        private void Detonate(Vector3 hitPos, Collider directHit = null)
        {
            if (_detonated) return;
            _detonated = true;

            AudioManager.Play("zmb_spit_hit", hitPos);

            bool hitPlayer = false;
            if (directHit != null && PlayerController.Instance != null &&
                directHit.transform.IsChildOf(PlayerController.Instance.transform))
            {
                ApplyPlayerAcidDamage(1.0f);
                hitPlayer = true;
            }

            if (!hitPlayer && PlayerController.Instance != null)
            {
                float dist = Vector3.Distance(hitPos, PlayerController.Instance.Position + Vector3.up * 0.9f);
                if (dist <= splashRadius)
                {
                    float falloff = Mathf.Clamp01(1f - (dist / splashRadius) * 0.5f);
                    ApplyPlayerAcidDamage(falloff);
                }
            }

            Destroy(gameObject);
        }

        private void ApplyPlayerAcidDamage(float scale)
        {
            if (PlayerController.Instance == null || PlayerController.Instance.Stats == null) return;
            DamageInfo info = new DamageInfo(
                damage * scale,
                DamageType.Acid,
                shooter,
                transform.position,
                velocity.normalized,
                false);
            PlayerController.Instance.Stats.TakeDamage(info);
            PlayerController.Instance.Stats.AddInfection(infectionGain * scale);
        }
    }
}
