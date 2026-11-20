// 45: Layered Hell - Zombie archetype definitions (walker, runner, spitter, armored, goliath).
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LayeredHell
{
    [Serializable]
    public class ZombieArchetype
    {
        public string id;
        public string displayName;
        public float hp;
        public float speed;
        public float damage;
        public float hearingMultiplier;
        public float sightRange;
        public float fov;
        public float attackRange;
        public float attackCooldown;
        public float armor;
        public string special;
        public string modelPrefix;
        public Color fallbackColor;
        public Vector3 fallbackSize;

        public ZombieArchetype(string id, string displayName, float hp, float speed, float damage,
            float hearingMultiplier, float sightRange, float fov, float attackRange, float attackCooldown,
            float armor, string special, string modelPrefix, Color fallbackColor, Vector3 fallbackSize)
        {
            this.id = id;
            this.displayName = displayName;
            this.hp = hp;
            this.speed = speed;
            this.damage = damage;
            this.hearingMultiplier = hearingMultiplier;
            this.sightRange = sightRange;
            this.fov = fov;
            this.attackRange = attackRange;
            this.attackCooldown = attackCooldown;
            this.armor = Mathf.Clamp01(armor);
            this.special = special;
            this.modelPrefix = modelPrefix;
            this.fallbackColor = fallbackColor;
            this.fallbackSize = fallbackSize;
        }

        private static readonly Dictionary<string, ZombieArchetype> Map =
            new Dictionary<string, ZombieArchetype>(StringComparer.OrdinalIgnoreCase);

        private static readonly List<ZombieArchetype> AllList = new List<ZombieArchetype>();

        public static IReadOnlyList<ZombieArchetype> All
        {
            get
            {
                EnsureInitialized();
                return AllList;
            }
        }

        public static ZombieArchetype Get(string id)
        {
            EnsureInitialized();
            if (!string.IsNullOrEmpty(id) && Map.TryGetValue(id, out ZombieArchetype arch))
            {
                return arch;
            }
            return Map["walker"];
        }

        private static void EnsureInitialized()
        {
            if (AllList.Count > 0) return;

            Register(new ZombieArchetype(
                "walker",
                "Enfekte Sakin",
                65f,
                2.2f,
                14f,
                1.0f,
                14f,
                110f,
                1.75f,
                1.25f,
                0.0f,
                "grab_bite",
                "zmb_walker_",
                new Color(0.38f, 0.44f, 0.36f),
                new Vector3(0.75f, 1.80f, 0.55f)));

            Register(new ZombieArchetype(
                "runner",
                "Ses-Duyarlı Koşucu",
                52f,
                5.3f,
                16f,
                2.1f,
                18f,
                135f,
                1.70f,
                0.95f,
                0.0f,
                "sprint_hearing",
                "zmb_runner_",
                new Color(0.55f, 0.26f, 0.24f),
                new Vector3(0.70f, 1.78f, 0.52f)));

            Register(new ZombieArchetype(
                "spitter",
                "Mutajen Tüküren",
                58f,
                2.8f,
                12f,
                1.25f,
                20f,
                125f,
                9.5f,
                2.20f,
                0.05f,
                "acid_spit",
                "zmb_spitter_",
                new Color(0.30f, 0.65f, 0.28f),
                new Vector3(0.76f, 1.75f, 0.58f)));

            Register(new ZombieArchetype(
                "armored",
                "Zırhlı Vanguard Güvenliği",
                145f,
                2.6f,
                22f,
                1.1f,
                16f,
                115f,
                1.85f,
                1.35f,
                0.55f,
                "ballistic_armor",
                "zmb_armored_",
                new Color(0.22f, 0.26f, 0.34f),
                new Vector3(0.88f, 1.90f, 0.65f)));

            Register(new ZombieArchetype(
                "goliath",
                "Goliath (Denek-0 Hasan Bozkurt)",
                950f,
                3.6f,
                34f,
                2.5f,
                28f,
                165f,
                2.85f,
                1.55f,
                0.40f,
                "boss_phases",
                "zmb_goliath_",
                new Color(0.45f, 0.16f, 0.22f),
                new Vector3(1.45f, 2.75f, 1.15f)));
        }

        private static void Register(ZombieArchetype arch)
        {
            Map[arch.id] = arch;
            AllList.Add(arch);
        }
    }
}
