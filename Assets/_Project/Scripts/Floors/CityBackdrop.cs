// 45: Layered Hell - Always-visible low-poly metropolitan skyline, streets, lamps, trees, and cars around Tower 45.
using System;
using UnityEngine;

namespace LayeredHell
{
    public class CityBackdrop : MonoBehaviour
    {
        private bool _built;

        public void BuildIfNeeded()
        {
            if (_built) return;
            _built = true;

            System.Random rng = new System.Random(452026);
            Transform root = transform;
            root.position = Vector3.zero;

            // Ground plane surrounding the 48x48m tower footprint
            CreateGroundApron(root);

            // Inner street ring (radius 60-95m): street lamps, trees, abandoned cars, quarantine barricades
            BuildStreetRing(root, rng);

            // Mid skyline ring (radius 95-210m): medium & tall city buildings
            BuildBuildingRing(root, rng, 95f, 200f, 36, 28f, 115f);

            // Outer skyline ring (radius 215-390m): distant monolith skyscrapers
            BuildBuildingRing(root, rng, 215f, 385f, 44, 55f, 175f);

            // Distant quarantine searchlights & moody city glow lights
            BuildAtmosphericLights(root);
        }

        private static void CreateGroundApron(Transform root)
        {
            Color asphalt = new Color(0.09f, 0.10f, 0.12f, 1f);
            // 4 slabs around the tower (leaving 48x48 centre free for basement levels)
            CreateGroundSlab(root, "CityGround_North", new Vector3(0f, -0.25f, 224f), new Vector3(800f, 0.5f, 400f), asphalt);
            CreateGroundSlab(root, "CityGround_South", new Vector3(0f, -0.25f, -224f), new Vector3(800f, 0.5f, 400f), asphalt);
            CreateGroundSlab(root, "CityGround_East", new Vector3(224f, -0.25f, 0f), new Vector3(400f, 0.5f, 48f), asphalt);
            CreateGroundSlab(root, "CityGround_West", new Vector3(-224f, -0.25f, 0f), new Vector3(400f, 0.5f, 48f), asphalt);
        }

        private static void CreateGroundSlab(Transform parent, string name, Vector3 pos, Vector3 size, Color color)
        {
            GameObject slab = ModelLibrary.InstantiateOrPlaceholder("arch_floor_01", parent, size, color);
            slab.name = name;
            slab.transform.localPosition = pos;
            slab.transform.localScale = size;
            Collider col = slab.GetComponent<Collider>();
            if (col != null) Destroy(col);
        }

        private static void BuildStreetRing(Transform root, System.Random rng)
        {
            Transform streetRoot = new GameObject("StreetRing").transform;
            streetRoot.SetParent(root, false);

            int lampCount = 28;
            for (int i = 0; i < lampCount; i++)
            {
                float angle = (i / (float)lampCount) * Mathf.PI * 2f;
                float radius = 58f + (i % 2) * 14f;
                Vector3 pos = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);

                string lampId = GetModelIdOrFallback("city_lamp_", "city_lamp_01", rng);
                GameObject lamp = ModelLibrary.InstantiateOrPlaceholder(lampId, streetRoot, new Vector3(0.4f, 6.5f, 0.4f), new Color(0.25f, 0.27f, 0.30f));
                lamp.transform.localPosition = pos;
                lamp.transform.localRotation = Quaternion.Euler(0f, -angle * Mathf.Rad2Deg + 90f, 0f);
                StripColliders(lamp);

                string treeId = GetModelIdOrFallback("city_tree_", "city_tree_01", rng);
                Vector3 treePos = pos + new Vector3(Mathf.Sin(angle) * 4.5f, 0f, -Mathf.Cos(angle) * 4.5f);
                GameObject tree = ModelLibrary.InstantiateOrPlaceholder(treeId, streetRoot, new Vector3(2.8f, 5.5f, 2.8f), new Color(0.12f, 0.20f, 0.14f));
                tree.transform.localPosition = treePos;
                tree.transform.localRotation = Quaternion.Euler(0f, (float)rng.NextDouble() * 360f, 0f);
                StripColliders(tree);
            }

            int carCount = 24;
            for (int i = 0; i < carCount; i++)
            {
                float angle = ((float)rng.NextDouble()) * Mathf.PI * 2f;
                float radius = 52f + (float)rng.NextDouble() * 34f;
                Vector3 pos = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);

                string carId = GetModelIdOrFallback("veh_car_", "veh_car_01", rng);
                Color carColor = new Color(
                    0.15f + (float)rng.NextDouble() * 0.25f,
                    0.15f + (float)rng.NextDouble() * 0.25f,
                    0.18f + (float)rng.NextDouble() * 0.30f);

                GameObject car = ModelLibrary.InstantiateOrPlaceholder(carId, streetRoot, new Vector3(2.1f, 1.5f, 4.4f), carColor);
                car.transform.localPosition = pos;
                car.transform.localRotation = Quaternion.Euler(0f, (float)rng.NextDouble() * 360f, 0f);
                StripColliders(car);
            }
        }

        private static void BuildBuildingRing(Transform root, System.Random rng, float minRadius, float maxRadius,
            int count, float minHeight, float maxHeight)
        {
            Transform ringRoot = new GameObject("Buildings_" + Mathf.RoundToInt(minRadius)).transform;
            ringRoot.SetParent(root, false);

            for (int i = 0; i < count; i++)
            {
                float baseAngle = (i / (float)count) * Mathf.PI * 2f;
                float jitterAngle = ((float)rng.NextDouble() - 0.5f) * (Mathf.PI * 2f / count) * 0.65f;
                float angle = baseAngle + jitterAngle;
                float radius = Mathf.Lerp(minRadius, maxRadius, (float)rng.NextDouble());

                float width = 18f + (float)rng.NextDouble() * 24f;
                float depth = 18f + (float)rng.NextDouble() * 24f;
                float height = Mathf.Lerp(minHeight, maxHeight, (float)rng.NextDouble());

                Vector3 pos = new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
                string bldId = GetModelIdOrFallback("city_building_", "city_building_01", rng);
                float shade = 0.10f + (float)rng.NextDouble() * 0.12f;
                Color bldColor = new Color(shade, shade * 1.05f, shade * 1.18f, 1f);

                GameObject bld = ModelLibrary.InstantiateOrPlaceholder(bldId, ringRoot, new Vector3(width, height, depth), bldColor);
                bld.name = "CityTower_" + i;
                bld.transform.localPosition = pos;
                bld.transform.localRotation = Quaternion.Euler(0f, (i * 37) % 360, 0f);

                // If using a placeholder or unit-sized building mesh, scale gracefully
                if (bld.transform.localScale.y < 2f)
                {
                    bld.transform.localScale = new Vector3(width / 16f, height / 40f, depth / 16f);
                }

                StripColliders(bld);
            }
        }

        private static void BuildAtmosphericLights(Transform root)
        {
            Transform lightsRoot = new GameObject("CityAtmosphereLights").transform;
            lightsRoot.SetParent(root, false);

            Color[] palette =
            {
                new Color(1.0f, 0.55f, 0.22f), // sodium street orange
                new Color(0.35f, 0.65f, 1.0f), // cold quarantine searchlight blue
                new Color(0.95f, 0.25f, 0.20f) // distant warning beacon red
            };

            for (int i = 0; i < 8; i++)
            {
                float angle = (i / 8f) * Mathf.PI * 2f;
                Vector3 pos = new Vector3(Mathf.Cos(angle) * 85f, 22f + (i % 3) * 35f, Mathf.Sin(angle) * 85f);
                GameObject lgo = new GameObject("CityGlow_" + i);
                lgo.transform.SetParent(lightsRoot, false);
                lgo.transform.localPosition = pos;

                Light lt = lgo.AddComponent<Light>();
                lt.type = LightType.Point;
                lt.color = palette[i % palette.Length];
                lt.intensity = 2.2f;
                lt.range = 65f;
                lt.shadows = LightShadows.None;
            }
        }

        private static string GetModelIdOrFallback(string prefix, string fallback, System.Random rng)
        {
            if (ModelLibrary.Instance != null)
            {
                string id = ModelLibrary.Instance.GetRandomId(prefix, rng);
                if (!string.IsNullOrEmpty(id)) return id;
            }
            return fallback;
        }

        private static void StripColliders(GameObject go)
        {
            if (go == null) return;
            Collider[] cols = go.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < cols.Length; i++)
            {
                Destroy(cols[i]);
            }
        }
    }
}
