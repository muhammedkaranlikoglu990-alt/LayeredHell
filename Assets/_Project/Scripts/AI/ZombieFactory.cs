// 45: Layered Hell - Runtime Zombie spawner with visual model, body capsule, headshot sphere, and NavMeshAgent.
using System;
using UnityEngine;
using UnityEngine.AI;

namespace LayeredHell
{
    public static class ZombieFactory
    {
        public static ZombieAI Spawn(string archetype, Vector3 pos, int floor, System.Random rng)
        {
            if (rng == null) rng = new System.Random();

            ZombieArchetype arch = ZombieArchetype.Get(archetype);
            string modelId = PickZombieModelId(arch.modelPrefix, rng);

            GameObject root = new GameObject("Zombie_" + arch.id + "_F" + floor);
            root.transform.position = pos;
            root.transform.rotation = Quaternion.Euler(0f, (float)rng.NextDouble() * 360f, 0f);

            int enemyLayer = LayerMask.NameToLayer(LayerNames.Enemy);
            if (enemyLayer >= 0) root.layer = enemyLayer;

            // Visual representation from ModelLibrary (or color-coded fallback)
            GameObject visual = ModelLibrary.InstantiateOrPlaceholder(modelId, root.transform, arch.fallbackSize, arch.fallbackColor);
            visual.name = "Visual_" + modelId;
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;

            // Remove any stray colliders on visual children so root + Head colliders control hit detection cleanly
            Collider[] childCols = visual.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < childCols.Length; i++)
            {
                UnityEngine.Object.Destroy(childCols[i]);
            }

            // Body CapsuleCollider on root
            CapsuleCollider bodyCol = root.AddComponent<CapsuleCollider>();
            bodyCol.radius = Mathf.Max(0.32f, arch.fallbackSize.x * 0.45f);
            bodyCol.height = Mathf.Max(1.5f, arch.fallbackSize.y);
            bodyCol.center = new Vector3(0f, bodyCol.height * 0.5f, 0f);

            // Child "Head" SphereCollider for WeaponController headshot detection (name contains "Head")
            GameObject headGo = new GameObject("Head_Collider");
            headGo.transform.SetParent(root.transform, false);
            headGo.transform.localPosition = new Vector3(0f, bodyCol.height * 0.86f, 0.05f);
            if (enemyLayer >= 0) headGo.layer = enemyLayer;

            SphereCollider headCol = headGo.AddComponent<SphereCollider>();
            headCol.radius = arch.id == "goliath" ? 0.42f : 0.24f;

            // NavMeshAgent
            NavMeshAgent agent = root.AddComponent<NavMeshAgent>();
            agent.radius = bodyCol.radius;
            agent.height = bodyCol.height;
            agent.speed = arch.speed;
            agent.angularSpeed = 320f;
            agent.acceleration = 14f;
            agent.stoppingDistance = Mathf.Min(1.35f, arch.attackRange * 0.75f);

            // ZombieAI component
            ZombieAI ai = root.AddComponent<ZombieAI>();
            ai.Agent = agent;
            ai.Animator = visual.GetComponentInChildren<Animator>();
            ai.Initialize(arch.id, floor);

            // Attach GoliathBoss controller if spawning Denek-0 boss
            if (string.Equals(arch.id, "goliath", StringComparison.OrdinalIgnoreCase))
            {
                root.AddComponent<GoliathBoss>();
            }

            return ai;
        }

        private static string PickZombieModelId(string prefix, System.Random rng)
        {
            if (ModelLibrary.Instance != null)
            {
                string id = ModelLibrary.Instance.GetRandomId(prefix, rng);
                if (!string.IsNullOrEmpty(id)) return id;
            }
            int variant = rng.Next(1, 6);
            return prefix + variant.ToString("00");
        }
    }
}
