// 45: Layered Hell - Procedural deterministic 48x48m floor generator & runtime NavMesh builder.
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

namespace LayeredHell
{
    public class FloorChunk : MonoBehaviour
    {
        public int floor = GameConstants.BaseFloor;

        private readonly Dictionary<string, Transform> _anchors = new Dictionary<string, Transform>(StringComparer.OrdinalIgnoreCase);
        private readonly List<Transform> _zombieSpawns = new List<Transform>();
        private bool _generating;

        public Transform PlayerSpawn { get; private set; }
        public IReadOnlyDictionary<string, Transform> Anchors { get { return _anchors; } }
        public bool IsGenerated { get; private set; }

        public event Action<FloorChunk> Generated;

        private void Start()
        {
            if (!IsGenerated && !_generating)
            {
                StartCoroutine(GenerateRoutine(true));
            }
        }

        private void OnDestroy()
        {
            if (ZombieDirector.Instance != null)
            {
                ZombieDirector.Instance.UnregisterChunk(floor);
            }
        }

        public void GenerateImmediate()
        {
            if (IsGenerated) return;
            StopAllCoroutines();
            _generating = true;
            IEnumerator routine = GenerateRoutine(false);
            while (routine.MoveNext()) { }
        }

        private IEnumerator GenerateRoutine(bool yieldBetweenSteps)
        {
            if (IsGenerated) yield break;
            _generating = true;

            float baseY = GameConstants.FloorY(floor);
            transform.position = new Vector3(0f, baseY, 0f);
            transform.rotation = Quaternion.identity;

            int seed = floor * 7919 + 17;
            System.Random rng = new System.Random(seed);
            FloorInfo info = FloorCatalog.Get(floor);

            _anchors.Clear();
            _zombieSpawns.Clear();

            // 1. Structural shell (48x48m floor slab, ceiling, outer perimeter walls & windows, pillars)
            BuildStructuralShell(info, rng);
            if (yieldBetweenSteps) yield return null;

            // 2. Central service core (stairwells Up/Down, elevator shaft, player spawn, core corridors)
            BuildCoreAndTransitions(info, rng);
            if (yieldBetweenSteps) yield return null;

            // 3. Interior room partitions, doors, and theme-specific modular rooms
            BuildInteriorRoomsAndAnchors(info, rng);
            if (yieldBetweenSteps) yield return null;

            // 4. Theme props, furniture, lighting
            BuildThemePropsAndLights(info, rng);
            if (yieldBetweenSteps) yield return null;

            // 5. Runtime NavMesh bake on floor geometry
            BuildRuntimeNavMesh();
            if (yieldBetweenSteps) yield return null;

            // 6. Gameplay entities: loot containers, world pickups, wall texts, journals, survivors, objectives
            PopulateStoryAndLoot(info, rng);

            // 7. Register zombie spawn points with ZombieDirector
            RegisterZombies(info);

            IsGenerated = true;
            _generating = false;

            Generated?.Invoke(this);
            GameEvents.RaiseFloorLoaded(floor);
        }

        private void BuildStructuralShell(FloorInfo info, System.Random rng)
        {
            Transform shellRoot = new GameObject("StructureShell").transform;
            shellRoot.SetParent(transform, false);

            Color floorColor = GetThemeFloorColor(info.theme);
            Color wallColor = GetThemeWallColor(info.theme);

            // Solid 48x48m floor slab
            GameObject slab = new GameObject("FloorSlab_48x48");
            slab.transform.SetParent(shellRoot, false);
            slab.transform.localPosition = new Vector3(0f, -0.15f, 0f);
            BoxCollider slabCol = slab.AddComponent<BoxCollider>();
            slabCol.size = new Vector3(48f, 0.3f, 48f);
            SurfaceTag sTag = slab.AddComponent<SurfaceTag>();
            sTag.surface = GetThemeSurface(info.theme);

            // Visual floor tiles (6x6 grid of 8x8m tiles)
            for (int gx = -3; gx < 3; gx++)
            {
                for (int gz = -3; gz < 3; gz++)
                {
                    string tileId = PickModel("arch_floor_", "arch_floor_01", rng);
                    Vector3 pos = new Vector3((gx + 0.5f) * 8f, 0f, (gz + 0.5f) * 8f);
                    GameObject tile = ModelLibrary.InstantiateOrPlaceholder(tileId, shellRoot, new Vector3(8f, 0.08f, 8f), floorColor);
                    tile.transform.localPosition = pos;
                }
            }

            // Ceiling (omitted on Roof floor 45 so sky/helipad is open)
            if (info.theme != FloorTheme.Roof)
            {
                GameObject ceiling = ModelLibrary.InstantiateOrPlaceholder("arch_ceiling_01", shellRoot,
                    new Vector3(48f, 0.2f, 48f), wallColor * 0.75f);
                ceiling.name = "CeilingSlab";
                ceiling.transform.localPosition = new Vector3(0f, GameConstants.FloorHeight - 0.1f, 0f);
                EnsureBoxCollider(ceiling, new Vector3(48f, 0.2f, 48f));
            }

            // Outer walls (4 sides, 48m length, height 3.4m; windows on upper floors, concrete on basement, waist parapet on roof)
            float wallH = info.theme == FloorTheme.Roof ? 1.3f : GameConstants.FloorHeight;
            CreateSolidWall(shellRoot, "OuterWall_North", new Vector3(0f, 0f, 23.8f), new Vector3(48f, wallH, 0.4f), wallColor, rng, floor > 0 && info.theme != FloorTheme.Roof);
            CreateSolidWall(shellRoot, "OuterWall_South", new Vector3(0f, 0f, -23.8f), new Vector3(48f, wallH, 0.4f), wallColor, rng, floor > 0 && info.theme != FloorTheme.Roof);
            CreateSolidWall(shellRoot, "OuterWall_East", new Vector3(23.8f, 0f, 0f), new Vector3(0.4f, wallH, 48f), wallColor, rng, floor > 0 && info.theme != FloorTheme.Roof);
            CreateSolidWall(shellRoot, "OuterWall_West", new Vector3(-23.8f, 0f, 0f), new Vector3(0.4f, wallH, 48f), wallColor, rng, floor > 0 && info.theme != FloorTheme.Roof);

            // Structural support pillars
            float[] pillarCoords = { -16f, -8f, 8f, 16f };
            for (int ix = 0; ix < pillarCoords.Length; ix++)
            {
                for (int iz = 0; iz < pillarCoords.Length; iz++)
                {
                    if (info.theme == FloorTheme.Roof && Mathf.Abs(pillarCoords[ix]) < 12f && Mathf.Abs(pillarCoords[iz]) < 12f)
                        continue;

                    string pilId = PickModel("arch_pillar_", "arch_pillar_01", rng);
                    GameObject pil = ModelLibrary.InstantiateOrPlaceholder(pilId, shellRoot,
                        new Vector3(0.9f, wallH, 0.9f), wallColor * 0.85f);
                    pil.transform.localPosition = new Vector3(pillarCoords[ix], 0f, pillarCoords[iz]);
                    EnsureBoxCollider(pil, new Vector3(0.9f, wallH, 0.9f));
                }
            }
        }

        private void BuildCoreAndTransitions(FloorInfo info, System.Random rng)
        {
            Transform coreRoot = new GameObject("ServiceCore").transform;
            coreRoot.SetParent(transform, false);

            // Player Spawn point near central stairwell landing
            GameObject spawnGo = new GameObject("PlayerSpawn");
            spawnGo.transform.SetParent(coreRoot, false);
            spawnGo.transform.localPosition = floor == GameConstants.BaseFloor
                ? new Vector3(-4.5f, 0.12f, -3.5f)
                : new Vector3(0f, 0.12f, -4.5f);
            spawnGo.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            PlayerSpawn = spawnGo.transform;

            // Up stairwell transition (if floor < MaxFloor)
            if (floor < GameConstants.MaxFloor)
            {
                string stairUpId = PickModel("arch_stair_", "arch_stair_01", rng);
                GameObject stairUp = ModelLibrary.InstantiateOrPlaceholder(stairUpId, coreRoot,
                    new Vector3(2.4f, 2.6f, 3.6f), new Color(0.35f, 0.38f, 0.42f));
                stairUp.name = "Stairwell_Up_To_" + (floor + 1);
                stairUp.transform.localPosition = new Vector3(-3.8f, 0f, -7.5f);
                FloorTransitionPoint.Setup(stairUp, floor + 1, false);
            }

            // Down stairwell transition (if floor > MinFloor)
            if (floor > GameConstants.MinFloor)
            {
                string stairDownId = PickModel("arch_stair_", "arch_stair_01", rng);
                GameObject stairDown = ModelLibrary.InstantiateOrPlaceholder(stairDownId, coreRoot,
                    new Vector3(2.4f, 2.6f, 3.6f), new Color(0.30f, 0.33f, 0.37f));
                stairDown.name = "Stairwell_Down_To_" + (floor - 1);
                stairDown.transform.localPosition = new Vector3(3.8f, 0f, -7.5f);
                FloorTransitionPoint.Setup(stairDown, floor - 1, false);
            }

            // Express Elevator transitions (connects Base 38, Lobby 0, Basement -1, Executive 39, Clinics 31, Gunsmith 24, Chemist 19, FoodCourt 8)
            int elevatorTarget = ChooseElevatorTarget(floor);
            string elevId = PickModel("arch_elevator_", "arch_elevator_01", rng);
            GameObject elevGo = ModelLibrary.InstantiateOrPlaceholder(elevId, coreRoot,
                new Vector3(2.4f, 2.8f, 1.2f), new Color(0.45f, 0.47f, 0.52f));
            elevGo.name = "Elevator_To_" + elevatorTarget;
            elevGo.transform.localPosition = new Vector3(0f, 0f, -8.5f);
            FloorTransitionPoint.Setup(elevGo, elevatorTarget, true);
        }

        private static int ChooseElevatorTarget(int currentFloor)
        {
            if (currentFloor == GameConstants.BaseFloor) return 0;
            return GameConstants.BaseFloor;
        }

        private void BuildInteriorRoomsAndAnchors(FloorInfo info, System.Random rng)
        {
            Transform roomsRoot = new GameObject("InteriorRooms").transform;
            roomsRoot.SetParent(transform, false);

            Color wallColor = GetThemeWallColor(info.theme);
            float h = info.theme == FloorTheme.Roof ? 1.4f : GameConstants.FloorHeight;

            if (info.theme != FloorTheme.Roof && info.theme != FloorTheme.Ballroom)
            {
                // North wing partition wall with doorway gaps
                CreateSolidWall(roomsRoot, "Part_North_L", new Vector3(-13f, 0f, 8f), new Vector3(18f, h, 0.35f), wallColor, rng, false);
                CreateSolidWall(roomsRoot, "Part_North_R", new Vector3(13f, 0f, 8f), new Vector3(18f, h, 0.35f), wallColor, rng, false);

                // South wing partition wall with doorway gaps
                CreateSolidWall(roomsRoot, "Part_South_L", new Vector3(-13f, 0f, -10f), new Vector3(18f, h, 0.35f), wallColor, rng, false);
                CreateSolidWall(roomsRoot, "Part_South_R", new Vector3(13f, 0f, -10f), new Vector3(18f, h, 0.35f), wallColor, rng, false);

                // Side room dividers
                CreateSolidWall(roomsRoot, "Part_Mid_W", new Vector3(-10f, 0f, 16f), new Vector3(0.35f, h, 14f), wallColor, rng, false);
                CreateSolidWall(roomsRoot, "Part_Mid_E", new Vector3(10f, 0f, 16f), new Vector3(0.35f, h, 14f), wallColor, rng, false);

                // Interactive doors at partition openings
                PlaceDoor(roomsRoot, rng, "door_F" + floor + "_N", new Vector3(0f, 0f, 8f), Quaternion.identity);
                PlaceDoor(roomsRoot, rng, "door_F" + floor + "_NW", new Vector3(-3.2f, 0f, 8f), Quaternion.identity);
                PlaceDoor(roomsRoot, rng, "door_F" + floor + "_NE", new Vector3(3.2f, 0f, 8f), Quaternion.identity);
            }

            // Always register standard objective anchor hints so StoryDatabase objectives find exact placements
            RegisterAnchor("terminal", new Vector3(6.5f, 0f, 14.5f), Quaternion.Euler(0f, 180f, 0f));
            RegisterAnchor("safe", new Vector3(-14.5f, 0f, 18.0f), Quaternion.Euler(0f, 180f, 0f));
            RegisterAnchor("console", new Vector3(0f, 0f, 15.5f), Quaternion.Euler(0f, 180f, 0f));
            RegisterAnchor("locker", new Vector3(14.5f, 0f, 17.5f), Quaternion.Euler(0f, -90f, 0f));
            RegisterAnchor("vehicle", new Vector3(0f, 0f, 4.5f), Quaternion.Euler(0f, 0f, 0f));
            RegisterAnchor("vent", new Vector3(-18.5f, 0f, -4.0f), Quaternion.Euler(0f, 90f, 0f));
            RegisterAnchor("desk", new Vector3(5.5f, 0f, 4.5f), Quaternion.Euler(0f, 180f, 0f));

            // Floor 38 Safe House required base anchors (CONTRACT §4 C2a)
            if (floor == GameConstants.BaseFloor)
            {
                RegisterAnchor("workbench", new Vector3(-8.5f, 0f, 4.5f), Quaternion.Euler(0f, 90f, 0f));
                RegisterAnchor("weapon_bench", new Vector3(-8.5f, 0f, 1.0f), Quaternion.Euler(0f, 90f, 0f));
                RegisterAnchor("chem_station", new Vector3(-8.5f, 0f, -2.5f), Quaternion.Euler(0f, 90f, 0f));
                RegisterAnchor("med_station", new Vector3(8.5f, 0f, 4.5f), Quaternion.Euler(0f, -90f, 0f));
                RegisterAnchor("purifier", new Vector3(-5.0f, 0f, 19.5f), Quaternion.Euler(0f, 180f, 0f));
                RegisterAnchor("stove", new Vector3(4.5f, 0f, 6.5f), Quaternion.Euler(0f, 180f, 0f));
                RegisterAnchor("generator", new Vector3(8.5f, 0f, -5.5f), Quaternion.Euler(0f, -90f, 0f));
                RegisterAnchor("radio_station", new Vector3(0f, 0f, 6.5f), Quaternion.Euler(0f, 180f, 0f));
                RegisterAnchor("storage", new Vector3(-4.0f, 0f, 6.5f), Quaternion.Euler(0f, 180f, 0f));
                RegisterAnchor("bed", new Vector3(8.5f, 0f, 1.0f), Quaternion.Euler(0f, -90f, 0f));
                RegisterAnchor("barricade_door", new Vector3(0f, 0f, -6.0f), Quaternion.Euler(0f, 0f, 0f));
                RegisterAnchor("barricade_window_1", new Vector3(-12.0f, 0f, 23.2f), Quaternion.Euler(0f, 180f, 0f));
                RegisterAnchor("barricade_window_2", new Vector3(0f, 0f, 23.2f), Quaternion.Euler(0f, 180f, 0f));
                RegisterAnchor("barricade_window_3", new Vector3(12.0f, 0f, 23.2f), Quaternion.Euler(0f, 180f, 0f));
            }
        }

        private void BuildThemePropsAndLights(FloorInfo info, System.Random rng)
        {
            Transform propsRoot = new GameObject("ThemeProps").transform;
            propsRoot.SetParent(transform, false);

            string[] prefixes = GetThemePropPrefixes(info.theme);
            int propCount = info.theme == FloorTheme.Roof ? 10 : 24;

            for (int i = 0; i < propCount; i++)
            {
                string prefix = prefixes[i % prefixes.Length];
                string modelId = PickModel(prefix, prefix + "01", rng);

                float px = Mathf.Lerp(-20f, 20f, (float)rng.NextDouble());
                float pz = Mathf.Lerp(-19f, 20f, (float)rng.NextDouble());

                // Keep central spawn and stairwell zone clear
                if (Mathf.Abs(px) < 6.5f && pz < -1.5f && pz > -10.5f)
                {
                    px += (px >= 0f ? 8f : -8f);
                }

                Color propColor = GetThemePropColor(info.theme, rng);
                GameObject prop = ModelLibrary.InstantiateOrPlaceholder(modelId, propsRoot,
                    new Vector3(1.4f, 1.2f, 1.4f), propColor);
                prop.transform.localPosition = new Vector3(px, 0f, pz);
                prop.transform.localRotation = Quaternion.Euler(0f, (rng.Next(0, 4) * 90f), 0f);
                EnsureBoxCollider(prop, new Vector3(1.3f, 1.2f, 1.3f));
            }

            // Special landmark models per story floor
            if (floor == -1)
            {
                // Vanguard Armored Response Vehicle on B1 Parking Garage
                GameObject apc = ModelLibrary.InstantiateOrPlaceholder("veh_armored_response_01", propsRoot,
                    new Vector3(2.8f, 2.4f, 6.2f), new Color(0.18f, 0.22f, 0.20f));
                apc.name = "Vanguard_Armored_Vehicle";
                apc.transform.localPosition = new Vector3(0f, 0f, 3.0f);
                EnsureBoxCollider(apc, new Vector3(2.8f, 2.4f, 6.2f));
            }
            else if (floor == 0)
            {
                // Welded steel quarantine plates sealing the ground floor exit
                for (int i = -2; i <= 2; i++)
                {
                    GameObject plate = ModelLibrary.InstantiateOrPlaceholder("bar_steelplate_01", propsRoot,
                        new Vector3(3.2f, 3.0f, 0.4f), new Color(0.28f, 0.25f, 0.24f));
                    plate.name = "QuarantineSteelPlate_" + i;
                    plate.transform.localPosition = new Vector3(i * 3.4f, 0f, 22.8f);
                    EnsureBoxCollider(plate, new Vector3(3.2f, 3.0f, 0.4f));
                }
            }
            else if (floor == GameConstants.MaxFloor)
            {
                // Helipad & Vanguard Air-Defense Radar on Floor 45 Roof
                GameObject pad = ModelLibrary.InstantiateOrPlaceholder("exe_helipad_01", propsRoot,
                    new Vector3(14f, 0.15f, 14f), new Color(0.22f, 0.24f, 0.26f));
                pad.name = "Roof_Helipad";
                pad.transform.localPosition = new Vector3(0f, 0.02f, 2.0f);

                GameObject radar = ModelLibrary.InstantiateOrPlaceholder("exe_radar_01", propsRoot,
                    new Vector3(3.2f, 4.5f, 3.2f), new Color(0.35f, 0.38f, 0.42f));
                radar.name = "Vanguard_Radar_Dish";
                radar.transform.localPosition = new Vector3(0f, 0f, 17.5f);
                EnsureBoxCollider(radar, new Vector3(3.2f, 4.5f, 3.2f));
            }

            // Atmospheric ceiling / emergency lights
            Color lightCol = GetThemeLightColor(info.theme);
            float[] lx = { -14f, 0f, 14f };
            float[] lz = { -14f, 0f, 14f };
            bool flicker = floor != GameConstants.BaseFloor;

            for (int ix = 0; ix < lx.Length; ix++)
            {
                for (int iz = 0; iz < lz.Length; iz++)
                {
                    ChunkFlickerLight.Create(propsRoot, new Vector3(lx[ix], 2.95f, lz[iz]),
                        lightCol, floor == GameConstants.BaseFloor ? 2.1f : 1.45f, 14f, flicker && ((ix + iz) % 2 == 1));
                }
            }
        }

        private void BuildRuntimeNavMesh()
        {
            try
            {
                NavMeshSurface surface = gameObject.GetComponent<NavMeshSurface>();
                if (surface == null)
                {
                    surface = gameObject.AddComponent<NavMeshSurface>();
                }
                surface.collectObjects = CollectObjects.Children;
                surface.BuildNavMesh();
            }
            catch (Exception)
            {
                // Graceful fallback if NavMesh settings are unavailable in stripped environment
            }
        }

        private void PopulateStoryAndLoot(FloorInfo info, System.Random rng)
        {
            Transform entityRoot = new GameObject("Entities").transform;
            entityRoot.SetParent(transform, false);

            // 1. Loot containers (4 to 7 per floor)
            int containerCount = floor == GameConstants.BaseFloor ? 3 : 5 + (info.danger % 3);
            string[] containerNames = GetContainerNames(info.theme);
            for (int i = 0; i < containerCount; i++)
            {
                Vector3 pos = GetOpenFloorPosition(rng, i, containerCount);
                string modelId = PickModel("off_cabinet_", "off_cabinet_01", rng);
                GameObject cgo = ModelLibrary.InstantiateOrPlaceholder(modelId, entityRoot,
                    new Vector3(1.0f, 1.4f, 0.7f), new Color(0.32f, 0.34f, 0.36f));
                cgo.name = "LootContainer_" + floor + "_" + i;
                cgo.transform.localPosition = pos;
                EnsureBoxCollider(cgo, new Vector3(1.0f, 1.4f, 0.7f));

                int containerSeed = (floor + 100) * 1000 + i * 37;
                string cName = containerNames[i % containerNames.Length];
                LootContainer.Setup(cgo, info.lootTable, containerSeed, cName);
            }

            // 2. Loose WorldPickups (2-4 items rolled from floor's loot table)
            List<ItemStack> looseLoot = ItemDatabase.RollLoot(info.lootTable, rng);
            for (int i = 0; i < looseLoot.Count; i++)
            {
                ItemStack stack = looseLoot[i];
                if (string.IsNullOrEmpty(stack.id) || stack.count <= 0) continue;
                Vector3 worldPos = transform.TransformPoint(GetOpenFloorPosition(rng, i + 10, looseLoot.Count + 10) + new Vector3(0f, 0.35f, 0f));
                WorldPickup pickup = WorldPickup.Spawn(stack.id, stack.count, worldPos);
                if (pickup != null)
                {
                    pickup.transform.SetParent(entityRoot, true);
                }
            }

            // 3. Wall graffiti / Turkish survivor messages (StoryDatabase.GetWallTexts)
            List<string> wallTexts = StoryDatabase.GetWallTexts(floor);
            if (wallTexts != null)
            {
                Vector3[] wallPositions =
                {
                    new Vector3(-6.5f, 1.65f, 7.75f),
                    new Vector3(6.5f, 1.65f, -9.75f),
                    new Vector3(-23.4f, 1.65f, 3.0f),
                    new Vector3(23.4f, 1.65f, -3.0f)
                };
                Quaternion[] wallRots =
                {
                    Quaternion.Euler(0f, 0f, 0f),
                    Quaternion.Euler(0f, 180f, 0f),
                    Quaternion.Euler(0f, -90f, 0f),
                    Quaternion.Euler(0f, 90f, 0f)
                };

                for (int i = 0; i < wallTexts.Count && i < wallPositions.Length; i++)
                {
                    if (string.IsNullOrEmpty(wallTexts[i])) continue;
                    Vector3 wPos = transform.TransformPoint(wallPositions[i]);
                    WallText wt = WallText.Create(wallTexts[i], wPos, wallRots[i]);
                    if (wt != null) wt.transform.SetParent(entityRoot, true);
                }
            }

            // 4. Journals on this floor (StoryDatabase.GetJournalIdsForFloor)
            List<string> journalIds = StoryDatabase.GetJournalIdsForFloor(floor);
            if (journalIds != null)
            {
                for (int i = 0; i < journalIds.Count; i++)
                {
                    string jId = journalIds[i];
                    if (string.IsNullOrEmpty(jId)) continue;
                    if (JournalSystem.Instance != null && JournalSystem.Instance.IsCollected(jId)) continue;

                    Vector3 localPos = new Vector3(-3.5f + i * 3.5f, 0.45f, 2.5f + (i % 2) * 3.0f);
                    JournalPickup jp = JournalPickup.Create(jId, transform.TransformPoint(localPos));
                    if (jp != null) jp.transform.SetParent(entityRoot, true);
                }
            }

            // 5. Survivors on this floor (StoryDatabase.GetSurvivorsOnFloor)
            List<SurvivorDef> survivors = StoryDatabase.GetSurvivorsOnFloor(floor);
            if (survivors != null)
            {
                for (int i = 0; i < survivors.Count; i++)
                {
                    SurvivorDef sDef = survivors[i];
                    if (sDef == null || string.IsNullOrEmpty(sDef.id)) continue;
                    if (BaseManager.Instance != null && BaseManager.Instance.IsRescued(sDef.id)) continue;

                    Vector3 localPos = new Vector3(-2.5f + i * 4f, 0.05f, 11.5f);
                    SurvivorNPC npc = SurvivorNPC.Create(sDef.id, transform.TransformPoint(localPos), Quaternion.Euler(0f, 180f, 0f));
                    if (npc != null) npc.transform.SetParent(entityRoot, true);
                }
            }

            // 6. Interactive Story Objectives (StoryDatabase.GetObjectivesForFloor)
            List<ObjectivePlacement> objectives = StoryDatabase.GetObjectivesForFloor(floor);
            if (objectives != null)
            {
                for (int i = 0; i < objectives.Count; i++)
                {
                    ObjectivePlacement obj = objectives[i];
                    if (obj == null) continue;

                    Vector3 objWorldPos;
                    Quaternion objWorldRot = Quaternion.Euler(0f, 180f, 0f);
                    if (!string.IsNullOrEmpty(obj.anchorHint) && _anchors.TryGetValue(obj.anchorHint, out Transform anchorTr) && anchorTr != null)
                    {
                        objWorldPos = anchorTr.position + new Vector3(i * 1.35f, 0f, 0f);
                        objWorldRot = anchorTr.rotation;
                    }
                    else
                    {
                        objWorldPos = transform.TransformPoint(new Vector3(4.0f + i * 2.5f, 0f, 13.0f));
                    }

                    ObjectiveInteractable oi = ObjectiveInteractable.Create(obj, objWorldPos, objWorldRot);
                    if (oi != null) oi.transform.SetParent(entityRoot, true);
                }
            }
        }

        private void RegisterZombies(FloorInfo info)
        {
            if (floor == GameConstants.BaseFloor) return;

            Transform spawnRoot = new GameObject("ZombieSpawns").transform;
            spawnRoot.SetParent(transform, false);

            Vector3[] localSpawns =
            {
                new Vector3(-16f, 0.1f, -15f),
                new Vector3(16f, 0.1f, -15f),
                new Vector3(-17f, 0.1f, 3f),
                new Vector3(17f, 0.1f, 3f),
                new Vector3(-15f, 0.1f, 17f),
                new Vector3(15f, 0.1f, 17f),
                new Vector3(0f, 0.1f, 18f),
                new Vector3(-7f, 0.1f, 12f),
                new Vector3(7f, 0.1f, 12f)
            };

            for (int i = 0; i < localSpawns.Length; i++)
            {
                GameObject sp = new GameObject("ZSpawn_" + i);
                sp.transform.SetParent(spawnRoot, false);
                sp.transform.localPosition = localSpawns[i];
                _zombieSpawns.Add(sp.transform);
            }

            if (ZombieDirector.Instance != null)
            {
                ZombieDirector.Instance.RegisterChunk(this, _zombieSpawns);
                if (floor == GameConstants.MinFloor)
                {
                    bool goliathDead = QuestManager.Instance != null && QuestManager.Instance.HasFlag("goliath_defeated");
                    if (!goliathDead)
                    {
                        ZombieDirector.Instance.SpawnBoss();
                    }
                }
            }
        }

        private void RegisterAnchor(string key, Vector3 localPos, Quaternion localRot)
        {
            GameObject go = new GameObject("Anchor_" + key);
            go.transform.SetParent(transform, false);
            go.transform.localPosition = localPos;
            go.transform.localRotation = localRot;
            _anchors[key] = go.transform;
        }

        private void PlaceDoor(Transform parent, System.Random rng, string doorId, Vector3 localPos, Quaternion localRot)
        {
            string modelId = PickModel("arch_door_", "arch_door_01", rng);
            GameObject dgo = ModelLibrary.InstantiateOrPlaceholder(modelId, parent,
                new Vector3(1.2f, 2.3f, 0.15f), new Color(0.38f, 0.28f, 0.20f));
            dgo.name = "Door_" + doorId;
            dgo.transform.localPosition = localPos;
            dgo.transform.localRotation = localRot;
            bool locked = (floor != GameConstants.BaseFloor) && (rng.NextDouble() < 0.18);
            Door.Setup(dgo, doorId, "", locked);
        }

        private static void CreateSolidWall(Transform parent, string name, Vector3 localPos, Vector3 size,
            Color color, System.Random rng, bool withWindowVisual)
        {
            string modelId = withWindowVisual ? PickModel("arch_window_", "arch_window_01", rng) : PickModel("arch_wall_", "arch_wall_01", rng);
            GameObject wall = ModelLibrary.InstantiateOrPlaceholder(modelId, parent, size, color);
            wall.name = name;
            wall.transform.localPosition = new Vector3(localPos.x, localPos.y + size.y * 0.5f, localPos.z);
            wall.transform.localScale = size;
            EnsureBoxCollider(wall, Vector3.one);
        }

        private static void EnsureBoxCollider(GameObject go, Vector3 fallbackSize)
        {
            if (go == null) return;
            Collider col = go.GetComponent<Collider>();
            if (col == null)
            {
                BoxCollider box = go.AddComponent<BoxCollider>();
                box.size = fallbackSize;
            }
        }

        private static string PickModel(string prefix, string fallback, System.Random rng)
        {
            if (ModelLibrary.Instance != null)
            {
                string id = ModelLibrary.Instance.GetRandomId(prefix, rng);
                if (!string.IsNullOrEmpty(id)) return id;
            }
            return fallback;
        }

        private static Vector3 GetOpenFloorPosition(System.Random rng, int index, int total)
        {
            float angle = (index / (float)Mathf.Max(1, total)) * Mathf.PI * 2f + (float)rng.NextDouble() * 0.5f;
            float radius = 11f + (float)rng.NextDouble() * 9f;
            return new Vector3(Mathf.Cos(angle) * radius, 0f, Mathf.Sin(angle) * radius);
        }

        private static SurfaceType GetThemeSurface(FloorTheme theme)
        {
            switch (theme)
            {
                case FloorTheme.Residential:
                case FloorTheme.Base:
                case FloorTheme.Executive:
                case FloorTheme.Suite:
                    return SurfaceType.Parquet;
                case FloorTheme.Mall:
                case FloorTheme.FoodCourt:
                case FloorTheme.Lobby:
                case FloorTheme.Clinic:
                case FloorTheme.Chemist:
                case FloorTheme.Ballroom:
                    return SurfaceType.Tile;
                case FloorTheme.Tunnels:
                    return SurfaceType.Water;
                default:
                    return SurfaceType.Concrete;
            }
        }

        private static Color GetThemeFloorColor(FloorTheme theme)
        {
            switch (theme)
            {
                case FloorTheme.Lab:
                case FloorTheme.LabStorage:
                    return new Color(0.18f, 0.24f, 0.26f);
                case FloorTheme.Tunnels:
                case FloorTheme.Parking:
                    return new Color(0.16f, 0.16f, 0.18f);
                case FloorTheme.Lobby:
                case FloorTheme.Mall:
                case FloorTheme.FoodCourt:
                    return new Color(0.28f, 0.27f, 0.25f);
                case FloorTheme.Office:
                case FloorTheme.Chemist:
                case FloorTheme.Gunsmith:
                    return new Color(0.22f, 0.24f, 0.28f);
                case FloorTheme.Residential:
                case FloorTheme.Base:
                    return new Color(0.32f, 0.24f, 0.18f);
                case FloorTheme.Clinic:
                    return new Color(0.28f, 0.33f, 0.34f);
                case FloorTheme.Gym:
                    return new Color(0.20f, 0.22f, 0.26f);
                case FloorTheme.Executive:
                case FloorTheme.Ballroom:
                case FloorTheme.Suite:
                    return new Color(0.30f, 0.20f, 0.18f);
                default:
                    return new Color(0.18f, 0.19f, 0.21f);
            }
        }

        private static Color GetThemeWallColor(FloorTheme theme)
        {
            return GetThemeFloorColor(theme) * 1.15f;
        }

        private static Color GetThemePropColor(FloorTheme theme, System.Random rng)
        {
            Color baseCol = GetThemeFloorColor(theme);
            float jitter = 0.85f + (float)rng.NextDouble() * 0.35f;
            return new Color(
                Mathf.Clamp01(baseCol.r * jitter + 0.08f),
                Mathf.Clamp01(baseCol.g * jitter + 0.08f),
                Mathf.Clamp01(baseCol.b * jitter + 0.08f));
        }

        private static Color GetThemeLightColor(FloorTheme theme)
        {
            switch (theme)
            {
                case FloorTheme.Lab:
                case FloorTheme.LabStorage:
                    return new Color(0.45f, 0.90f, 0.85f);
                case FloorTheme.Tunnels:
                case FloorTheme.Parking:
                    return new Color(0.95f, 0.45f, 0.20f);
                case FloorTheme.Base:
                    return new Color(1.0f, 0.82f, 0.58f);
                case FloorTheme.Clinic:
                    return new Color(0.80f, 0.95f, 1.0f);
                case FloorTheme.Executive:
                case FloorTheme.Ballroom:
                case FloorTheme.Suite:
                    return new Color(1.0f, 0.85f, 0.62f);
                default:
                    return new Color(0.85f, 0.90f, 0.95f);
            }
        }

        private static string[] GetThemePropPrefixes(FloorTheme theme)
        {
            switch (theme)
            {
                case FloorTheme.Lab:
                case FloorTheme.LabStorage:
                    return new[] { "lab_pod_", "lab_tank_", "lab_server_", "lab_cage_", "lab_pipe_", "med_lab_" };
                case FloorTheme.Tunnels:
                    return new[] { "lab_pipe_", "bar_crate_", "bar_debris_", "arch_vent_" };
                case FloorTheme.Parking:
                    return new[] { "veh_car_", "bar_concrete_", "bar_crate_", "bar_wreck_" };
                case FloorTheme.Lobby:
                    return new[] { "bar_steelplate_", "bar_sandbag_", "bar_crate_", "mall_kiosk_" };
                case FloorTheme.Mall:
                case FloorTheme.FoodCourt:
                    return new[] { "mall_shelf_", "mall_counter_", "mall_mannequin_", "mall_cart_", "mall_kiosk_", "mall_atm_" };
                case FloorTheme.Office:
                    return new[] { "off_desk_", "off_chair_", "off_partition_", "off_cabinet_", "off_printer_", "off_whiteboard_", "off_cooler_" };
                case FloorTheme.Chemist:
                    return new[] { "med_lab_", "med_cabinet_", "off_desk_", "lab_tank_" };
                case FloorTheme.Gunsmith:
                    return new[] { "bar_crate_", "off_cabinet_", "off_desk_", "bar_steelplate_" };
                case FloorTheme.Clinic:
                    return new[] { "med_bed_", "med_ivstand_", "med_monitor_", "med_cabinet_", "med_wheelchair_" };
                case FloorTheme.Gym:
                    return new[] { "gym_bench_", "gym_rack_", "gym_treadmill_", "gym_weights_" };
                case FloorTheme.Executive:
                case FloorTheme.Suite:
                    return new[] { "exe_desk_", "exe_safe_", "res_sofa_", "off_cabinet_" };
                case FloorTheme.Ballroom:
                    return new[] { "exe_chandelier_", "exe_banquet_", "res_chair_", "res_table_" };
                case FloorTheme.Roof:
                    return new[] { "bar_sandbag_", "bar_crate_", "arch_vent_" };
                default:
                    return new[] { "res_bed_", "res_sofa_", "res_table_", "res_chair_", "res_wardrobe_", "res_shelf_", "res_fridge_" };
            }
        }

        private static string[] GetContainerNames(FloorTheme theme)
        {
            switch (theme)
            {
                case FloorTheme.Lab:
                case FloorTheme.LabStorage:
                case FloorTheme.Chemist:
                    return new[] { "Numune Dolabı", "Kimyasal Kasası", "Laboratuvar Çekmecesi" };
                case FloorTheme.Clinic:
                    return new[] { "İlaç Dolabı", "İlk Yardım Kutusu", "Tıbbi Malzeme Kasası" };
                case FloorTheme.Parking:
                case FloorTheme.Tunnels:
                case FloorTheme.Gunsmith:
                    return new[] { "Takım Sandığı", "Mühimmat Kasası", "Bakım Dolabı" };
                case FloorTheme.Mall:
                case FloorTheme.FoodCourt:
                    return new[] { "Mağaza Rafı", "Kasa Çekmecesi", "Erzak Dolabı" };
                case FloorTheme.Office:
                case FloorTheme.Executive:
                case FloorTheme.Suite:
                    return new[] { "Evrak Dolabı", "Çelik Çekmece", "Ofis Dolabı" };
                default:
                    return new[] { "Dolap", "Komodin", "Mutfak Dolabı", "Erzak Kutusu" };
            }
        }
    }
}
