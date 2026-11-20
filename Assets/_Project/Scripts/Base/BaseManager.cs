// 45: Layered Hell — BaseManager: Floor 38 Safe House (38-B) upgrades, stations, survivors, passive water/power, and barricades.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LayeredHell
{
    [Serializable]
    public class UpgradeDef
    {
        public string id;
        public string name;
        public string description;
        public int maxLevel;
        public string requiresSurvivor;
        public ItemStack[][] costsPerLevel;

        public UpgradeDef(
            string id,
            string name,
            string description,
            int maxLevel,
            ItemStack[][] costsPerLevel,
            string requiresSurvivor = "")
        {
            this.id = id;
            this.name = name;
            this.description = description;
            this.maxLevel = Mathf.Clamp(maxLevel, 1, 3);
            this.costsPerLevel = costsPerLevel ?? new ItemStack[0][];
            this.requiresSurvivor = requiresSurvivor ?? "";
        }

        public ItemStack[] GetCostForNextLevel(int currentLevel)
        {
            if (currentLevel < 0 || currentLevel >= maxLevel)
                return new ItemStack[0];
            if (costsPerLevel == null || currentLevel >= costsPerLevel.Length)
                return new ItemStack[0];
            return costsPerLevel[currentLevel] ?? new ItemStack[0];
        }
    }

    public class BaseManager : MonoBehaviour, ISaveable
    {
        public static BaseManager Instance { get; private set; }

        private readonly List<UpgradeDef> _upgrades = new List<UpgradeDef>();
        private readonly Dictionary<string, int> _upgradeLevels = new Dictionary<string, int>(StringComparer.Ordinal);
        private readonly HashSet<string> _rescuedSurvivors = new HashSet<string>(StringComparer.Ordinal);
        private readonly List<GameObject> _spawnedBaseObjects = new List<GameObject>();

        private float _productionTimer;
        private float _raidTimer;
        private const float ProductionIntervalSeconds = 75f;
        private const float RaidCheckIntervalSeconds = 210f;

        public IReadOnlyList<UpgradeDef> Upgrades => _upgrades;
        public string SaveKey => "base_manager";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            BuildUpgradeDefinitions();
            ResetForNewGame();
        }

        private void OnEnable()
        {
            GameEvents.FloorLoaded += OnFloorLoaded;
        }

        private void OnDisable()
        {
            GameEvents.FloorLoaded -= OnFloorLoaded;
        }

        private void Start()
        {
            if (SaveSystem.Instance != null)
                SaveSystem.Instance.Register(this);
        }

        private void OnDestroy()
        {
            if (SaveSystem.Instance != null)
                SaveSystem.Instance.Unregister(this);
            if (Instance == this)
                Instance = null;
        }

        private void Update()
        {
            if (GameManager.Instance != null && !GameManager.Instance.IsPlaying)
                return;

            // Passive production tick (purifier -> purified_water, stove/generator -> rations/tea into storage)
            _productionTimer += Time.deltaTime;
            if (_productionTimer >= ProductionIntervalSeconds)
            {
                _productionTimer = 0f;
                RunPassiveProductionTick();
            }

            // Optional night raid pressure against 38-B barricades
            _raidTimer += Time.deltaTime;
            if (_raidTimer >= RaidCheckIntervalSeconds)
            {
                _raidTimer = 0f;
                CheckBarricadeNightRaid();
            }
        }

        public bool HasStation(string id)
        {
            if (string.IsNullOrEmpty(id)) return true;

            switch (id)
            {
                case "workbench":
                case "stove":
                case "storage":
                case "bed":
                    return true;
                case "purifier":
                    return GetUpgradeLevel("purifier") >= 1;
                case "generator":
                    return GetUpgradeLevel("generator") >= 1;
                case "radio_station":
                    return IsRescued("nehir") || GetUpgradeLevel("radio_station") >= 1;
                case "med_station":
                    return IsRescued("selin") || GetUpgradeLevel("med_station") >= 1;
                case "weapon_bench":
                    return IsRescued("kemal") || GetUpgradeLevel("weapon_bench") >= 1;
                case "chem_station":
                    return IsRescued("elif") || GetUpgradeLevel("chem_station") >= 1;
                default:
                    return GetUpgradeLevel(id) >= 1;
            }
        }

        public bool IsRescued(string survivorId)
        {
            return !string.IsNullOrEmpty(survivorId) && _rescuedSurvivors.Contains(survivorId);
        }

        public int GetUpgradeLevel(string upgradeId)
        {
            if (string.IsNullOrEmpty(upgradeId)) return 0;
            return _upgradeLevels.TryGetValue(upgradeId, out int lvl) ? lvl : 0;
        }

        public UpgradeDef GetUpgradeDef(string upgradeId)
        {
            for (int i = 0; i < _upgrades.Count; i++)
            {
                if (_upgrades[i].id == upgradeId)
                    return _upgrades[i];
            }
            return null;
        }

        public bool CanUpgrade(string upgradeId, out string reason)
        {
            var def = GetUpgradeDef(upgradeId);
            if (def == null)
            {
                reason = "Bilinmeyen geliştirme.";
                return false;
            }

            int current = GetUpgradeLevel(upgradeId);
            if (current >= def.maxLevel)
            {
                reason = "Maksimum seviyeye ulaşıldı (Sv." + def.maxLevel + ").";
                return false;
            }

            if (!string.IsNullOrEmpty(def.requiresSurvivor) && !IsRescued(def.requiresSurvivor))
            {
                var surv = StoryDatabase.GetSurvivor(def.requiresSurvivor);
                string sName = surv != null ? surv.name : def.requiresSurvivor;
                reason = "Gerekli uzman kurtarılmadı: " + sName;
                return false;
            }

            if (Inventory.Instance == null)
            {
                reason = "Envanter hazır değil.";
                return false;
            }

            var costs = def.GetCostForNextLevel(current);
            for (int i = 0; i < costs.Length; i++)
            {
                int have = Inventory.Instance.Count(costs[i].id);
                if (have < costs[i].count)
                {
                    var itemDef = ItemDatabase.Get(costs[i].id);
                    reason = "Eksik: " + itemDef.name + " (" + have + "/" + costs[i].count + ")";
                    return false;
                }
            }

            reason = "Geliştirilebilir";
            return true;
        }

        public bool TryUpgrade(string upgradeId, out string reason)
        {
            if (!CanUpgrade(upgradeId, out reason))
            {
                AudioManager.Play2D("ui_denied", 0.9f);
                return false;
            }

            var def = GetUpgradeDef(upgradeId);
            int current = GetUpgradeLevel(upgradeId);
            var costs = def.GetCostForNextLevel(current);

            for (int i = 0; i < costs.Length; i++)
            {
                Inventory.Instance.Remove(costs[i].id, costs[i].count);
            }

            int nextLevel = current + 1;
            _upgradeLevels[upgradeId] = nextLevel;

            if (upgradeId == "backpack" && Inventory.Instance != null)
            {
                Inventory.Instance.SetExtraCapacity(nextLevel * 12f);
            }

            if (upgradeId == "barricade_door" || upgradeId == "barricade_windows")
            {
                AudioManager.Play2D("barricade_build", 0.95f);
                GameEvents.RaiseObjective("barricade_built");
            }
            else if (upgradeId == "purifier")
            {
                AudioManager.Play2D("craft", 0.9f);
                GameEvents.RaiseObjective("purifier_built");
            }
            else if (upgradeId == "generator")
            {
                AudioManager.Play2D("generator_start", 0.95f);
                GameEvents.RaiseObjective("generator_online");
            }
            else if (upgradeId == "radio_station")
            {
                AudioManager.Play2D("radio_tune", 0.95f);
                GameEvents.RaiseObjective("radio_repaired");
            }
            else
            {
                AudioManager.Play2D("hammer", 0.9f);
            }

            AudioManager.Play2D("ui_upgrade", 1f);
            GameEvents.RaiseBaseUpgraded(upgradeId);
            GameEvents.RaiseToast(def.name + " Seviye " + nextLevel + " yapıldı!");

            RefreshBaseFloorVisualsIfLoaded();
            return true;
        }

        public void Rescue(string survivorId)
        {
            if (string.IsNullOrEmpty(survivorId)) return;
            bool isNew = _rescuedSurvivors.Add(survivorId);

            // Auto-unlock associated station at Level 1 when the specialist arrives at 38-B
            if (survivorId == "selin" && GetUpgradeLevel("med_station") < 1)
                _upgradeLevels["med_station"] = 1;
            else if (survivorId == "kemal" && GetUpgradeLevel("weapon_bench") < 1)
                _upgradeLevels["weapon_bench"] = 1;
            else if (survivorId == "elif" && GetUpgradeLevel("chem_station") < 1)
                _upgradeLevels["chem_station"] = 1;
            else if (survivorId == "nehir" && GetUpgradeLevel("radio_station") < 1)
            {
                _upgradeLevels["radio_station"] = 1;
                GameEvents.RaiseObjective("radio_repaired");
            }

            if (isNew && Inventory.Instance != null)
            {
                switch (survivorId)
                {
                    case "murat":
                        if (!Inventory.Instance.Has("key_lab", 1))
                            Inventory.Instance.Add("key_lab", 1);
                        GameEvents.RaiseToast("Murat Şen 38-B'ye geçti ve Chronos Laboratuvar Anahtarı'nı verdi!");
                        break;
                    case "tarik":
                        if (!Inventory.Instance.Has("key_vehicle", 1))
                            Inventory.Instance.Add("key_vehicle", 1);
                        GameEvents.RaiseToast("Tarık Aydın 38-B ile irtibat kurdu ve Zırhlı Araç Kontak Anahtarı'nı verdi!");
                        break;
                    case "selin":
                        Inventory.Instance.Add("antidote", 1);
                        GameEvents.RaiseToast("Dr. Selin Arslan 38-B'de Tıbbi İstasyon'u kurdu (+1 LZR-9 Panzehiri).");
                        break;
                    case "kemal":
                        Inventory.Instance.Add("ammo_9mm", 18);
                        GameEvents.RaiseToast("Kemal Usta 38-B'de Silah Tezgâhı'nı kurdu (+18 9mm Mermi).");
                        break;
                    case "elif":
                        Inventory.Instance.Add("molotov", 2);
                        GameEvents.RaiseToast("Elif Yıldız 38-B'de Kimya İstasyonu'nu kurdu (+2 Molotof).");
                        break;
                    case "nehir":
                        GameEvents.RaiseToast("Nehir Özkan 38-B'de 88.7 Kule Radyosu'nu devreye aldı!");
                        break;
                }
            }

            AudioManager.Play2D("ui_confirm", 1f);
            GameEvents.RaiseSurvivorRescued(survivorId);
            GameEvents.RaiseObjective(survivorId + "_rescued");

            RefreshBaseFloorVisualsIfLoaded();
        }

        public void ResetForNewGame()
        {
            _rescuedSurvivors.Clear();
            _upgradeLevels.Clear();
            _productionTimer = 0f;
            _raidTimer = 0f;

            // Default starting station levels at apartment 38-B
            _upgradeLevels["workbench"] = 1;
            _upgradeLevels["stove"] = 1;
            _upgradeLevels["storage"] = 1;
            _upgradeLevels["barricade_door"] = 0;
            _upgradeLevels["barricade_windows"] = 0;
            _upgradeLevels["purifier"] = 0;
            _upgradeLevels["generator"] = 0;
            _upgradeLevels["radio_station"] = 0;
            _upgradeLevels["med_station"] = 0;
            _upgradeLevels["weapon_bench"] = 0;
            _upgradeLevels["chem_station"] = 0;
            _upgradeLevels["backpack"] = 0;

            if (Inventory.Instance != null)
                Inventory.Instance.SetExtraCapacity(0f);
        }

        private void RunPassiveProductionTick()
        {
            if (Inventory.Instance == null) return;

            int purifierLvl = GetUpgradeLevel("purifier");
            int genLvl = GetUpgradeLevel("generator");
            int stoveLvl = GetUpgradeLevel("stove");

            bool producedAny = false;
            if (purifierLvl > 0)
            {
                int waterCount = purifierLvl + (genLvl > 0 ? 1 : 0);
                Inventory.Instance.AddToStorageDirect("purified_water", waterCount);
                producedAny = true;
            }

            if (genLvl > 0 && stoveLvl >= 2 && _rescuedSurvivors.Count >= 2)
            {
                Inventory.Instance.AddToStorageDirect("herbal_tea", 1);
                producedAny = true;
            }

            if (IsRescued("selin") && GetUpgradeLevel("med_station") >= 2)
            {
                Inventory.Instance.AddToStorageDirect("bandage", 1);
                producedAny = true;
            }

            if (producedAny)
            {
                GameEvents.RaiseToast("38-B Güvenli Ev: Arıtılmış su ve erzak sandığa eklendi.");
            }
        }

        private void CheckBarricadeNightRaid()
        {
            int doorLvl = GetUpgradeLevel("barricade_door");
            int winLvl = GetUpgradeLevel("barricade_windows");
            int defense = doorLvl + winLvl;

            if (defense <= 0)
            {
                GameEvents.RaiseToast("Uyarı: 38. kat koridorunda enfekte sesleri artıyor. 38-B kapısına barikat kurun!");
            }
            else if (defense >= 3 && Inventory.Instance != null)
            {
                // Strong barricades occasionally trap a scavenging walker outside with ammo/scrap
                Inventory.Instance.AddToStorageDirect("scrap_metal", 2);
            }
        }

        private void OnFloorLoaded(int floor)
        {
            if (floor == GameConstants.BaseFloor)
            {
                RefreshBaseFloorVisualsIfLoaded();
            }
        }

        public void RefreshBaseFloorVisualsIfLoaded()
        {
            if (FloorStreamer.Instance == null) return;
            var chunk = FloorStreamer.Instance.GetChunk(GameConstants.BaseFloor);
            if (chunk == null) return;

            SpawnBaseObjectsOnChunk(chunk);
        }

        private void SpawnBaseObjectsOnChunk(FloorChunk chunk)
        {
            for (int i = _spawnedBaseObjects.Count - 1; i >= 0; i--)
            {
                if (_spawnedBaseObjects[i] != null)
                    Destroy(_spawnedBaseObjects[i]);
            }
            _spawnedBaseObjects.Clear();

            var anchors = chunk.Anchors;
            float baseY = GameConstants.FloorY(GameConstants.BaseFloor);

            // Spawn all 10 stations at their anchors (or fallback positions inside 38-B)
            SpawnStationAtAnchor(chunk, anchors, "workbench", "craft_workbench_01", new Vector3(-4.5f, baseY, 3.5f), new Color(0.55f, 0.38f, 0.22f));
            SpawnStationAtAnchor(chunk, anchors, "stove", "craft_stove_01", new Vector3(-2.5f, baseY, 4.5f), new Color(0.75f, 0.35f, 0.18f));
            SpawnStationAtAnchor(chunk, anchors, "storage", "craft_storage_01", new Vector3(-4.5f, baseY, 1.2f), new Color(0.45f, 0.48f, 0.52f));
            SpawnStationAtAnchor(chunk, anchors, "bed", "craft_bed_01", new Vector3(-4.5f, baseY, -2.5f), new Color(0.35f, 0.45f, 0.65f));
            SpawnStationAtAnchor(chunk, anchors, "purifier", "craft_purifier_01", new Vector3(4.8f, baseY, 4.8f), new Color(0.28f, 0.65f, 0.85f));
            SpawnStationAtAnchor(chunk, anchors, "generator", "craft_generator_01", new Vector3(4.8f, baseY, 2.5f), new Color(0.82f, 0.65f, 0.20f));
            SpawnStationAtAnchor(chunk, anchors, "radio_station", "craft_radio_01", new Vector3(1.5f, baseY, 4.2f), new Color(0.40f, 0.72f, 0.55f));
            SpawnStationAtAnchor(chunk, anchors, "med_station", "craft_medstation_01", new Vector3(3.5f, baseY, -1.5f), new Color(0.85f, 0.28f, 0.28f));
            SpawnStationAtAnchor(chunk, anchors, "weapon_bench", "craft_weaponbench_01", new Vector3(3.5f, baseY, 0.8f), new Color(0.58f, 0.42f, 0.28f));
            SpawnStationAtAnchor(chunk, anchors, "chem_station", "craft_chem_01", new Vector3(1.2f, baseY, -3.5f), new Color(0.35f, 0.78f, 0.40f));

            // Spawn Door & Window Barricades based on upgrade levels
            int doorLvl = GetUpgradeLevel("barricade_door");
            if (doorLvl > 0)
            {
                string doorModel = doorLvl >= 2 ? "bar_steelplate_01" : "bar_planks_01";
                SpawnBarricadeAtAnchor(chunk, anchors, "barricade_door", doorModel, new Vector3(0f, baseY, -5.2f));
            }

            int winLvl = GetUpgradeLevel("barricade_windows");
            if (winLvl > 0)
            {
                string winModel = winLvl >= 2 ? "bar_steelplate_01" : "bar_planks_01";
                SpawnBarricadeAtAnchor(chunk, anchors, "barricade_window_1", winModel, new Vector3(5.2f, baseY, 4.0f));
                if (winLvl >= 2)
                    SpawnBarricadeAtAnchor(chunk, anchors, "barricade_window_2", winModel, new Vector3(5.2f, baseY, 1.0f));
                if (winLvl >= 3)
                    SpawnBarricadeAtAnchor(chunk, anchors, "barricade_window_3", winModel, new Vector3(5.2f, baseY, -2.0f));
            }

            // Spawn rescued survivors inside 38-B near their stations
            SpawnRescuedSurvivorInBase(chunk, "selin", new Vector3(2.4f, baseY, -1.5f), Quaternion.Euler(0f, -45f, 0f));
            SpawnRescuedSurvivorInBase(chunk, "kemal", new Vector3(2.4f, baseY, 0.8f), Quaternion.Euler(0f, -90f, 0f));
            SpawnRescuedSurvivorInBase(chunk, "elif", new Vector3(1.2f, baseY, -2.4f), Quaternion.Euler(0f, 0f, 0f));
            SpawnRescuedSurvivorInBase(chunk, "nehir", new Vector3(1.5f, baseY, 3.1f), Quaternion.Euler(0f, 180f, 0f));
            SpawnRescuedSurvivorInBase(chunk, "murat", new Vector3(-1.2f, baseY, -3.5f), Quaternion.Euler(0f, 25f, 0f));
            SpawnRescuedSurvivorInBase(chunk, "tarik", new Vector3(-2.4f, baseY, 1.5f), Quaternion.Euler(0f, 90f, 0f));
        }

        private void SpawnStationAtAnchor(
            FloorChunk chunk,
            IReadOnlyDictionary<string, Transform> anchors,
            string stationId,
            string modelId,
            Vector3 fallbackPos,
            Color fallbackColor)
        {
            Vector3 pos = fallbackPos;
            Quaternion rot = Quaternion.identity;
            if (anchors != null && anchors.TryGetValue(stationId, out var anchor) && anchor != null)
            {
                pos = anchor.position;
                rot = anchor.rotation;
            }

            bool unlocked = HasStation(stationId);
            Color tint = unlocked ? fallbackColor : new Color(0.25f, 0.25f, 0.28f, 0.7f);

            var holder = new GameObject("BaseStation_" + stationId);
            holder.transform.SetParent(chunk.transform, true);
            holder.transform.SetPositionAndRotation(pos, rot);

            var model = ModelLibrary.InstantiateOrPlaceholder(modelId, holder.transform, new Vector3(1.1f, 0.95f, 0.8f), tint);
            if (model != null)
            {
                model.transform.localPosition = Vector3.zero;
                model.transform.localRotation = Quaternion.identity;
            }

            StationInteractable.Setup(holder, stationId, StationInteractable.DefaultDisplayName(stationId));
            _spawnedBaseObjects.Add(holder);
        }

        private void SpawnBarricadeAtAnchor(
            FloorChunk chunk,
            IReadOnlyDictionary<string, Transform> anchors,
            string anchorKey,
            string modelId,
            Vector3 fallbackPos)
        {
            Vector3 pos = fallbackPos;
            Quaternion rot = Quaternion.identity;
            if (anchors != null && anchors.TryGetValue(anchorKey, out var anchor) && anchor != null)
            {
                pos = anchor.position;
                rot = anchor.rotation;
            }

            var holder = new GameObject("BaseBarricade_" + anchorKey);
            holder.transform.SetParent(chunk.transform, true);
            holder.transform.SetPositionAndRotation(pos, rot);

            ModelLibrary.InstantiateOrPlaceholder(modelId, holder.transform, new Vector3(1.4f, 1.8f, 0.25f), new Color(0.48f, 0.36f, 0.24f));
            _spawnedBaseObjects.Add(holder);
        }

        private void SpawnRescuedSurvivorInBase(FloorChunk chunk, string survivorId, Vector3 pos, Quaternion rot)
        {
            if (!IsRescued(survivorId)) return;
            var npc = SurvivorNPC.Create(survivorId, pos, rot);
            if (npc != null)
            {
                npc.inBaseMode = true;
                npc.transform.SetParent(chunk.transform, true);
                _spawnedBaseObjects.Add(npc.gameObject);
            }
        }

        private void BuildUpgradeDefinitions()
        {
            _upgrades.Clear();

            // All 12 mandatory CONTRACT upgrade IDs, each <= 3 levels:
            // barricade_door, barricade_windows, purifier, generator, stove, workbench, storage, radio_station, med_station, weapon_bench, chem_station, backpack

            _upgrades.Add(new UpgradeDef(
                "barricade_door",
                "Çelik Kapı Barikatı",
                "38-B daire giriş kapısını kalaslar, sürgüler ve çelik plakalarla koridor sürülerine karşı güçlendirir.",
                3,
                new[]
                {
                    new[] { new ItemStack("wood_plank", 3), new ItemStack("nails", 4) },
                    new[] { new ItemStack("wood_plank", 4), new ItemStack("scrap_metal", 4), new ItemStack("nails", 6) },
                    new[] { new ItemStack("steel_plate", 3), new ItemStack("scrap_metal", 5), new ItemStack("adhesive_epoxy", 2) }
                }));

            _upgrades.Add(new UpgradeDef(
                "barricade_windows",
                "Balkon & Pencere Siperleri",
                "Pencereleri ve balkon korkuluklarını fırtınaya ve tırmanan mutantlara karşı zırhlandırır.",
                3,
                new[]
                {
                    new[] { new ItemStack("wood_plank", 2), new ItemStack("duct_tape", 2) },
                    new[] { new ItemStack("wood_plank", 3), new ItemStack("scrap_metal", 3), new ItemStack("nails", 4) },
                    new[] { new ItemStack("steel_plate", 2), new ItemStack("scrap_metal", 4), new ItemStack("wire", 3) }
                }));

            _upgrades.Add(new UpgradeDef(
                "purifier",
                "Yağmur Suyu Arıtıcısı",
                "Balkona kurulan filtreli su toplama düzeneği. Düzenli olarak sandığa Arıtılmış Yağmur Suyu üretir.",
                3,
                new[]
                {
                    new[] { new ItemStack("scrap_metal", 2), new ItemStack("cloth_rag", 2), new ItemStack("duct_tape", 1) },
                    new[] { new ItemStack("rubber_hose", 2), new ItemStack("charcoal_filter", 1), new ItemStack("plastic_casing", 2) },
                    new[] { new ItemStack("charcoal_filter", 2), new ItemStack("copper_coil", 1), new ItemStack("chemicals", 2) }
                }));

            _upgrades.Add(new UpgradeDef(
                "generator",
                "Daire Jeneratörü",
                "38-B'ye kesintisiz elektrik sağlar, istasyon verimini artırır ve telsiz menzilini güçlendirir.",
                3,
                new[]
                {
                    new[] { new ItemStack("scrap_metal", 3), new ItemStack("wire", 3), new ItemStack("fuel", 1) },
                    new[] { new ItemStack("battery", 1), new ItemStack("copper_coil", 1), new ItemStack("spark_plug", 1) },
                    new[] { new ItemStack("battery", 2), new ItemStack("circuit", 2), new ItemStack("solar_cell", 2) }
                }));

            _upgrades.Add(new UpgradeDef(
                "stove",
                "Derme Çatma Ocak",
                "Konserveleri yüksek besin değerli sıcak yemeklere ve şifalı çaylara dönüştüren kamp ocağı.",
                3,
                new[]
                {
                    new[] { new ItemStack("scrap_metal", 2), new ItemStack("fuel", 1) },
                    new[] { new ItemStack("scrap_metal", 4), new ItemStack("copper_coil", 1), new ItemStack("fuel", 1) },
                    new[] { new ItemStack("steel_plate", 2), new ItemStack("rubber_hose", 2), new ItemStack("fuel", 2) }
                }));

            _upgrades.Add(new UpgradeDef(
                "workbench",
                "Çalışma Tezgâhı",
                "Aras'ın temel üretim, zırh onarımı, maymuncuk ve barikat kiti hazırlama tezgâhı.",
                3,
                new[]
                {
                    new[] { new ItemStack("wood_plank", 2), new ItemStack("scrap_metal", 2) },
                    new[] { new ItemStack("scrap_metal", 4), new ItemStack("nails", 4), new ItemStack("duct_tape", 2) },
                    new[] { new ItemStack("steel_plate", 2), new ItemStack("adhesive_epoxy", 2), new ItemStack("circuit", 1) }
                }));

            _upgrades.Add(new UpgradeDef(
                "storage",
                "Erzak Sandığı",
                "38-B'deki güvenli malzeme ve cephane deposu. Pasif üretilen sular burada birikir.",
                3,
                new[]
                {
                    new[] { new ItemStack("wood_plank", 2), new ItemStack("nails", 2) },
                    new[] { new ItemStack("wood_plank", 4), new ItemStack("scrap_metal", 3), new ItemStack("nails", 4) },
                    new[] { new ItemStack("steel_plate", 2), new ItemStack("scrap_metal", 4), new ItemStack("lockpick", 1) }
                }));

            _upgrades.Add(new UpgradeDef(
                "radio_station",
                "88.7 Kule Radyo İstasyonu",
                "Kule içi acil durum frekanslarını ve dış dünyadaki askeri karantina yayınlarını yakalayan telsiz.",
                3,
                new[]
                {
                    new[] { new ItemStack("wire", 2), new ItemStack("circuit", 1), new ItemStack("battery", 1) },
                    new[] { new ItemStack("circuit", 2), new ItemStack("copper_coil", 1), new ItemStack("wire", 3) },
                    new[] { new ItemStack("radio_transistor", 1), new ItemStack("circuit", 2), new ItemStack("battery", 1) }
                }));

            _upgrades.Add(new UpgradeDef(
                "med_station",
                "Tıbbi İstasyon (Dr. Selin)",
                "Dr. Selin Arslan yönetiminde ilk yardım çantası, antibiyotik, adrenalin ve LZR-9 Panzehiri üretir.",
                3,
                new[]
                {
                    new[] { new ItemStack("cloth_rag", 3), new ItemStack("alcohol_pure", 1), new ItemStack("chemicals", 1) },
                    new[] { new ItemStack("chemicals", 3), new ItemStack("antiseptic", 2), new ItemStack("optic_lens", 1) },
                    new[] { new ItemStack("surgical_kit", 1), new ItemStack("mutagen_sample", 2), new ItemStack("circuit", 1) }
                },
                "selin"));

            _upgrades.Add(new UpgradeDef(
                "weapon_bench",
                "Silah Tezgâhı (Kemal Usta)",
                "Kemal Usta yönetiminde her kalibrede mermi dolumu, pala/balta ve silah modifikasyon kiti üretir.",
                3,
                new[]
                {
                    new[] { new ItemStack("scrap_metal", 4), new ItemStack("duct_tape", 2) },
                    new[] { new ItemStack("steel_plate", 2), new ItemStack("weapon_parts", 1), new ItemStack("adhesive_epoxy", 1) },
                    new[] { new ItemStack("weapon_parts", 3), new ItemStack("steel_plate", 3), new ItemStack("copper_coil", 1) }
                },
                "kemal"));

            _upgrades.Add(new UpgradeDef(
                "chem_station",
                "Kimya İstasyonu (Elif Yıldız)",
                "Elif yönetiminde barut sentezi, molotof, el bombası ve asit/biyo-tehlike zırhları üretir.",
                3,
                new[]
                {
                    new[] { new ItemStack("chemicals", 2), new ItemStack("empty_bottle", 2), new ItemStack("rubber_hose", 1) },
                    new[] { new ItemStack("chemicals", 4), new ItemStack("sulfur_powder", 2), new ItemStack("charcoal_filter", 1) },
                    new[] { new ItemStack("acid_gland", 2), new ItemStack("chemicals", 4), new ItemStack("circuit", 2) }
                },
                "elif"));

            _upgrades.Add(new UpgradeDef(
                "backpack",
                "Taktik Sırt Çantası Takviyesi",
                "Aras'ın taşıma kapasitesini her seviyede +12 kg artırır (Maksimum 76 kg).",
                3,
                new[]
                {
                    new[] { new ItemStack("cloth_rag", 4), new ItemStack("leather_strip", 2), new ItemStack("duct_tape", 2) },
                    new[] { new ItemStack("leather_strip", 4), new ItemStack("cloth_rag", 4), new ItemStack("climbing_rope", 1) },
                    new[] { new ItemStack("leather_strip", 5), new ItemStack("steel_plate", 1), new ItemStack("adhesive_epoxy", 2) }
                }));
        }

        [Serializable]
        private class UpgradeEntry
        {
            public string id;
            public int level;
        }

        [Serializable]
        private class SaveData
        {
            public List<UpgradeEntry> upgrades = new List<UpgradeEntry>();
            public List<string> rescued = new List<string>();
        }

        public string CaptureState()
        {
            var data = new SaveData();
            foreach (var kv in _upgradeLevels)
            {
                data.upgrades.Add(new UpgradeEntry { id = kv.Key, level = kv.Value });
            }
            foreach (var s in _rescuedSurvivors)
            {
                data.rescued.Add(s);
            }
            return JsonUtility.ToJson(data);
        }

        public void RestoreState(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            var data = JsonUtility.FromJson<SaveData>(json);
            if (data == null) return;

            _upgradeLevels.Clear();
            if (data.upgrades != null)
            {
                for (int i = 0; i < data.upgrades.Count; i++)
                {
                    var e = data.upgrades[i];
                    if (!string.IsNullOrEmpty(e.id))
                        _upgradeLevels[e.id] = e.level;
                }
            }

            _rescuedSurvivors.Clear();
            if (data.rescued != null)
            {
                for (int i = 0; i < data.rescued.Count; i++)
                {
                    if (!string.IsNullOrEmpty(data.rescued[i]))
                        _rescuedSurvivors.Add(data.rescued[i]);
                }
            }

            if (Inventory.Instance != null)
            {
                Inventory.Instance.SetExtraCapacity(GetUpgradeLevel("backpack") * 12f);
            }

            RefreshBaseFloorVisualsIfLoaded();
        }
    }
}
