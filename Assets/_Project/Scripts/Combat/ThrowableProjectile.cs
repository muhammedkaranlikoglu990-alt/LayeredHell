// Thrown grenade and molotov projectile physics, detonation, area damage, fire hazard pool, and AI noise emission.
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LayeredHell
{
    public sealed class ThrowableProjectile : MonoBehaviour
    {
        public string weaponId = "grenade";
        public float damage = 110f;
        public float radius = 5.5f;
        public float noiseRadius = 90f;
        public float fuseSeconds = 2.6f;
        public bool isMolotov;
        public GameObject owner;

        private bool _detonated;
        private float _spawnTime;

        public static ThrowableProjectile Launch(string weaponId, Vector3 origin, Vector3 velocity, float damage, float noiseRadius, GameObject owner)
        {
            bool molotov = string.Equals(weaponId, "molotov", System.StringComparison.OrdinalIgnoreCase);
            string modelId = "wpn_" + weaponId + "_01";

            GameObject go = null;
            if (ModelLibrary.Instance != null && ModelLibrary.Instance.Has(modelId))
            {
                GameObject prefab = ModelLibrary.Instance.Get(modelId);
                if (prefab != null)
                {
                    go = Instantiate(prefab, origin, Quaternion.identity);
                }
            }

            if (go == null)
            {
                go = GameObject.CreatePrimitive(molotov ? PrimitiveType.Cylinder : PrimitiveType.Sphere);
                go.transform.position = origin;
                go.transform.localScale = molotov ? new Vector3(0.12f, 0.18f, 0.12f) : new Vector3(0.16f, 0.16f, 0.16f);
            }

            go.name = "Projectile_" + weaponId;

            if (go.GetComponentInChildren<Collider>() == null)
            {
                SphereCollider col = go.AddComponent<SphereCollider>();
                col.radius = 0.1f;
            }

            Rigidbody rb = go.GetComponent<Rigidbody>();
            if (rb == null) rb = go.AddComponent<Rigidbody>();
            rb.mass = 0.45f;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            rb.linearVelocity = velocity;
            rb.angularDamping = 0.05f;
            rb.angularVelocity = new Vector3(6f, 3f, 4f);

            ThrowableProjectile proj = go.GetComponent<ThrowableProjectile>();
            if (proj == null) proj = go.AddComponent<ThrowableProjectile>();
            proj.weaponId = weaponId;
            proj.isMolotov = molotov;
            proj.damage = damage > 0f ? damage : (molotov ? 45f : 120f);
            proj.radius = molotov ? 3.8f : 5.5f;
            proj.noiseRadius = noiseRadius > 0f ? noiseRadius : (molotov ? 40f : 90f);
            proj.fuseSeconds = molotov ? 3.5f : 2.6f;
            proj.owner = owner;

            return proj;
        }

        private void Awake()
        {
            _spawnTime = Time.time;
        }

        private void Update()
        {
            if (_detonated) return;
            if (Time.time - _spawnTime >= fuseSeconds)
            {
                Detonate();
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (_detonated) return;
            if (owner != null && collision.transform.IsChildOf(owner.transform)) return;

            if (isMolotov)
            {
                Detonate();
            }
            else if (collision.relativeVelocity.sqrMagnitude > 4f)
            {
                AudioManager.Play("shell_concrete", transform.position, 0.5f);
            }
        }

        private void Detonate()
        {
            if (_detonated) return;
            _detonated = true;

            Vector3 pos = transform.position;

            if (isMolotov)
            {
                AudioManager.Play("molotov", pos, 1.0f);
                AudioManager.Play("impact_glass", pos, 0.9f);
                NoiseSystem.Emit(pos, noiseRadius, owner);
                ApplyAreaDamage(pos, radius, damage, DamageType.Fire);
                StartCoroutine(FirePoolRoutine(pos));
            }
            else
            {
                AudioManager.Play("explosion", pos, 1.0f);
                NoiseSystem.Emit(pos, noiseRadius, owner);
                ApplyAreaDamage(pos, radius, damage, DamageType.Explosive);
                StartCoroutine(ExplosionFlashRoutine(pos));
            }
        }

        private void ApplyAreaDamage(Vector3 center, float rad, float maxDamage, DamageType dmgType)
        {
            Collider[] hits = Physics.OverlapSphere(center, rad, ~0, QueryTriggerInteraction.Collide);
            HashSet<IDamageable> damaged = new HashSet<IDamageable>();

            for (int i = 0; i < hits.Length; i++)
            {
                Collider col = hits[i];
                if (col == null) continue;

                IDamageable target = col.GetComponentInParent<IDamageable>();
                if (target == null || target.IsDead || damaged.Contains(target)) continue;
                damaged.Add(target);

                MonoBehaviour mb = target as MonoBehaviour;
                Vector3 targetPos = mb != null ? mb.transform.position + Vector3.up * 0.9f : col.bounds.center;
                float dist = Vector3.Distance(center, targetPos);
                float falloff = Mathf.Clamp01(1f - (dist / (rad * 1.15f)));
                float finalDmg = Mathf.Max(maxDamage * 0.25f, maxDamage * falloff);
                Vector3 dir = (targetPos - center).normalized;

                target.TakeDamage(new DamageInfo(finalDmg, dmgType, owner != null ? owner : gameObject, targetPos, dir, false));
            }
        }

        private IEnumerator ExplosionFlashRoutine(Vector3 pos)
        {
            HideRenderersAndPhysics();

            GameObject lightGo = new GameObject("ExplosionFlash");
            lightGo.transform.position = pos + Vector3.up * 0.25f;
            Light l = lightGo.AddComponent<Light>();
            l.type = LightType.Point;
            l.range = 14f;
            l.intensity = 6f;
            l.color = new Color(1f, 0.65f, 0.2f);

            float t = 0f;
            while (t < 0.25f)
            {
                t += Time.deltaTime;
                l.intensity = Mathf.Lerp(6f, 0f, t / 0.25f);
                yield return null;
            }

            Destroy(lightGo);
            Destroy(gameObject);
        }

        private IEnumerator FirePoolRoutine(Vector3 pos)
        {
            HideRenderersAndPhysics();

            GameObject fireGo = new GameObject("MolotovFirePool");
            fireGo.transform.position = pos + Vector3.up * 0.2f;
            Light l = fireGo.AddComponent<Light>();
            l.type = LightType.Point;
            l.range = 7f;
            l.intensity = 2.8f;
            l.color = new Color(1f, 0.45f, 0.1f);

            float duration = 6.0f;
            float tickTimer = 0.5f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float dt = Time.deltaTime;
                elapsed += dt;
                tickTimer -= dt;

                l.intensity = 2.4f + Mathf.Sin(elapsed * 16f) * 0.5f;

                if (tickTimer <= 0f)
                {
                    tickTimer = 0.5f;
                    ApplyAreaDamage(pos, radius, damage * 0.35f, DamageType.Fire);
                }

                yield return null;
            }

            Destroy(fireGo);
            Destroy(gameObject);
        }

        private void HideRenderersAndPhysics()
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.isKinematic = true;
            }

            Collider[] cols = GetComponentsInChildren<Collider>();
            for (int i = 0; i < cols.Length; i++) cols[i].enabled = false;

            Renderer[] rends = GetComponentsInChildren<Renderer>();
            for (int i = 0; i < rends.Length; i++) rends[i].enabled = false;
        }
    }
}
