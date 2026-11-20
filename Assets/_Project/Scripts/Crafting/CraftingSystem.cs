// 45: Layered Hell — CraftingSystem with 50 recipes across 6 safe-house stations.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LayeredHell
{
    [Serializable]
    public class RecipeDef
    {
        public string id;
        public string resultId;
        public int resultCount;
        public string station;
        public ItemStack[] ingredients;
        public string requiresSurvivor;
        public int requiresUpgradeLevel;

        public RecipeDef() { }

        public RecipeDef(
            string id,
            string resultId,
            int resultCount,
            string station,
            ItemStack[] ingredients,
            string requiresSurvivor = "",
            int requiresUpgradeLevel = 0)
        {
            this.id = id;
            this.resultId = resultId;
            this.resultCount = Mathf.Max(1, resultCount);
            this.station = station ?? "workbench";
            this.ingredients = ingredients ?? new ItemStack[0];
            this.requiresSurvivor = requiresSurvivor ?? "";
            this.requiresUpgradeLevel = Mathf.Max(0, requiresUpgradeLevel);
        }
    }

    public class CraftingSystem : MonoBehaviour
    {
        public static CraftingSystem Instance { get; private set; }

        private readonly List<RecipeDef> _recipes = new List<RecipeDef>();

        public IReadOnlyList<RecipeDef> Recipes => _recipes;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            BuildRecipes();
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public IEnumerable<RecipeDef> GetRecipes(string station)
        {
            if (string.IsNullOrEmpty(station) || station == "all")
                return _recipes;

            var list = new List<RecipeDef>();
            for (int i = 0; i < _recipes.Count; i++)
            {
                if (string.Equals(_recipes[i].station, station, StringComparison.OrdinalIgnoreCase))
                    list.Add(_recipes[i]);
            }
            return list;
        }

        public bool CanCraft(RecipeDef r, out string reason)
        {
            if (r == null)
            {
                reason = "Geçersiz tarif.";
                return false;
            }

            if (BaseManager.Instance != null)
            {
                if (!string.IsNullOrEmpty(r.station) && !BaseManager.Instance.HasStation(r.station))
                {
                    reason = "Gerekli istasyon kurulu değil: " + StationDisplayName(r.station);
                    return false;
                }

                if (!string.IsNullOrEmpty(r.requiresSurvivor) && !BaseManager.Instance.IsRescued(r.requiresSurvivor))
                {
                    reason = "Uzman kurtarılmadı: " + SurvivorDisplayName(r.requiresSurvivor);
                    return false;
                }

                if (r.requiresUpgradeLevel > 0 && BaseManager.Instance.GetUpgradeLevel(r.station) < r.requiresUpgradeLevel)
                {
                    reason = StationDisplayName(r.station) + " Seviye " + r.requiresUpgradeLevel + " gerekli.";
                    return false;
                }
            }

            if (Inventory.Instance == null)
            {
                reason = "Envanter hazır değil.";
                return false;
            }

            if (r.ingredients != null)
            {
                for (int i = 0; i < r.ingredients.Length; i++)
                {
                    var ing = r.ingredients[i];
                    int have = Inventory.Instance.Count(ing.id);
                    if (have < ing.count)
                    {
                        var def = ItemDatabase.Get(ing.id);
                        reason = "Eksik malzeme: " + def.name + " (" + have + "/" + ing.count + ")";
                        return false;
                    }
                }
            }

            reason = "Üretilebilir";
            return true;
        }

        public bool Craft(RecipeDef r)
        {
            if (!CanCraft(r, out string reason))
            {
                AudioManager.Play2D("ui_denied", 0.9f);
                GameEvents.RaiseToast(reason);
                return false;
            }

            for (int i = 0; i < r.ingredients.Length; i++)
            {
                Inventory.Instance.Remove(r.ingredients[i].id, r.ingredients[i].count);
            }

            int added = Inventory.Instance.Add(r.resultId, r.resultCount);
            int leftover = r.resultCount - added;
            if (leftover > 0)
            {
                if (PlayerController.Instance != null)
                {
                    WorldPickup.Spawn(r.resultId, leftover, PlayerController.Instance.Position + Vector3.up * 0.5f);
                }
                else
                {
                    Inventory.Instance.AddToStorageDirect(r.resultId, leftover);
                }
            }

            AudioManager.Play2D("ui_craft", 1f);
            AudioManager.Play2D("craft", 0.85f);
            GameEvents.RaiseItemCrafted(r.resultId, r.resultCount);

            var resultDef = ItemDatabase.Get(r.resultId);
            GameEvents.RaiseToast("Üretildi: " + resultDef.name + " x" + r.resultCount);
            return true;
        }

        public static string StationDisplayName(string station)
        {
            switch (station)
            {
                case "workbench": return "Çalışma Tezgâhı";
                case "stove": return "Derme Çatma Ocak";
                case "purifier": return "Yağmur Suyu Arıtıcısı";
                case "med_station": return "Tıbbi İstasyon (Dr. Selin)";
                case "weapon_bench": return "Silah Tezgâhı (Kemal Usta)";
                case "chem_station": return "Kimya İstasyonu (Elif)";
                default: return station;
            }
        }

        private static string SurvivorDisplayName(string id)
        {
            switch (id)
            {
                case "selin": return "Dr. Selin Arslan (31. Kat)";
                case "kemal": return "Kemal Usta (24. Kat)";
                case "elif": return "Elif Yıldız (19. Kat)";
                case "nehir": return "Nehir Özkan (27. Kat)";
                case "murat": return "Murat Şen (8. Kat)";
                case "tarik": return "Tarık Aydın (B1 Otopark)";
                default: return id;
            }
        }

        private void BuildRecipes()
        {
            _recipes.Clear();

            // =====================================================================
            // 1. WORKBENCH (Çalışma Tezgâhı - Başlangıçta açık, 12 tarif)
            // =====================================================================
            _recipes.Add(new RecipeDef("rcp_bandage_basic", "bandage", 2, "workbench",
                new[] { new ItemStack("cloth_rag", 2), new ItemStack("alcohol_pure", 1) }));

            _recipes.Add(new RecipeDef("rcp_bandage_rag", "bandage", 1, "workbench",
                new[] { new ItemStack("cloth_rag", 3) }));

            _recipes.Add(new RecipeDef("rcp_lockpick", "lockpick", 2, "workbench",
                new[] { new ItemStack("scrap_metal", 1), new ItemStack("wire", 1) }));

            _recipes.Add(new RecipeDef("rcp_nails", "nails", 6, "workbench",
                new[] { new ItemStack("scrap_metal", 2) }));

            _recipes.Add(new RecipeDef("rcp_pipe", "pipe", 1, "workbench",
                new[] { new ItemStack("scrap_metal", 3), new ItemStack("duct_tape", 1) }));

            _recipes.Add(new RecipeDef("rcp_bat_spiked", "bat", 1, "workbench",
                new[] { new ItemStack("wood_plank", 2), new ItemStack("nails", 4), new ItemStack("duct_tape", 1) }));

            _recipes.Add(new RecipeDef("rcp_armor_leather", "armor_leather", 1, "workbench",
                new[] { new ItemStack("leather_strip", 4), new ItemStack("cloth_rag", 3), new ItemStack("duct_tape", 2) }));

            _recipes.Add(new RecipeDef("rcp_armor_scrap", "armor_scrap", 1, "workbench",
                new[] { new ItemStack("scrap_metal", 5), new ItemStack("leather_strip", 3), new ItemStack("duct_tape", 3) }, "", 1));

            _recipes.Add(new RecipeDef("rcp_barricade_kit", "barricade_kit", 1, "workbench",
                new[] { new ItemStack("wood_plank", 3), new ItemStack("nails", 4), new ItemStack("scrap_metal", 2) }));

            _recipes.Add(new RecipeDef("rcp_noise_maker", "noise_maker", 1, "workbench",
                new[] { new ItemStack("empty_can", 1), new ItemStack("wire", 1), new ItemStack("nails", 2) }));

            _recipes.Add(new RecipeDef("rcp_flashlight_battery", "flashlight_battery", 2, "workbench",
                new[] { new ItemStack("battery", 1), new ItemStack("wire", 1) }));

            _recipes.Add(new RecipeDef("rcp_splint", "splint", 1, "workbench",
                new[] { new ItemStack("wood_plank", 1), new ItemStack("cloth_rag", 2), new ItemStack("duct_tape", 1) }));

            _recipes.Add(new RecipeDef("rcp_steel_plate", "steel_plate", 1, "workbench",
                new[] { new ItemStack("scrap_metal", 4), new ItemStack("adhesive_epoxy", 1) }, "", 2));

            _recipes.Add(new RecipeDef("rcp_climbing_rope", "climbing_rope", 1, "workbench",
                new[] { new ItemStack("cloth_rag", 6), new ItemStack("leather_strip", 2), new ItemStack("duct_tape", 2) }, "", 2));

            // =====================================================================
            // 2. STOVE (Derme Çatma Ocak - Başlangıçta açık, 7 tarif)
            // =====================================================================
            _recipes.Add(new RecipeDef("rcp_cooked_beans", "cooked_meal", 1, "stove",
                new[] { new ItemStack("canned_beans", 1), new ItemStack("canned_tomato", 1) }));

            _recipes.Add(new RecipeDef("rcp_cooked_tuna_stew", "cooked_meal", 1, "stove",
                new[] { new ItemStack("canned_tuna", 1), new ItemStack("canned_corn", 1) }));

            _recipes.Add(new RecipeDef("rcp_cooked_noodles", "cooked_meal", 1, "stove",
                new[] { new ItemStack("instant_noodles", 2), new ItemStack("purified_water", 1) }));

            _recipes.Add(new RecipeDef("rcp_herbal_tea", "herbal_tea", 2, "stove",
                new[] { new ItemStack("purified_water", 1), new ItemStack("honey_jar", 1) }));

            _recipes.Add(new RecipeDef("rcp_boil_dirty_water", "purified_water", 1, "stove",
                new[] { new ItemStack("dirty_water", 1) }));

            _recipes.Add(new RecipeDef("rcp_dried_meat_feast", "cooked_meal", 2, "stove",
                new[] { new ItemStack("dried_meat", 1), new ItemStack("canned_beans", 1), new ItemStack("purified_water", 1) }, "", 1));

            _recipes.Add(new RecipeDef("rcp_cold_coffee_brew", "cold_coffee", 2, "stove",
                new[] { new ItemStack("purified_water", 1), new ItemStack("snack", 1) }));

            // =====================================================================
            // 3. PURIFIER (Yağmur Suyu Arıtıcısı - Balkon istasyonu, 5 tarif)
            // =====================================================================
            _recipes.Add(new RecipeDef("rcp_purify_water_bottle", "purified_water", 2, "purifier",
                new[] { new ItemStack("water_bottle", 1), new ItemStack("charcoal_filter", 1) }));

            _recipes.Add(new RecipeDef("rcp_purify_dirty_tablet", "purified_water", 1, "purifier",
                new[] { new ItemStack("dirty_water", 1), new ItemStack("water_purification_tablet", 1) }));

            _recipes.Add(new RecipeDef("rcp_purify_rain_bottles", "purified_water", 2, "purifier",
                new[] { new ItemStack("empty_bottle", 2), new ItemStack("cloth_rag", 1) }));

            _recipes.Add(new RecipeDef("rcp_charcoal_filter", "charcoal_filter", 1, "purifier",
                new[] { new ItemStack("empty_can", 1), new ItemStack("charcoal_tablets", 2), new ItemStack("cloth_rag", 1) }));

            _recipes.Add(new RecipeDef("rcp_isotonic_mix", "isotonic_drink", 2, "purifier",
                new[] { new ItemStack("purified_water", 1), new ItemStack("vitamin_pack", 1) }, "", 2));

            // =====================================================================
            // 4. MED_STATION (Tıbbi İstasyon - Dr. Selin Arslan, 8 tarif)
            // =====================================================================
            _recipes.Add(new RecipeDef("rcp_medkit", "medkit", 1, "med_station",
                new[] { new ItemStack("bandage", 2), new ItemStack("antiseptic", 1), new ItemStack("painkiller", 1) }, "selin", 0));

            _recipes.Add(new RecipeDef("rcp_antibiotic", "antibiotic", 2, "med_station",
                new[] { new ItemStack("chemicals", 2), new ItemStack("alcohol_pure", 1), new ItemStack("honey_jar", 1) }, "selin", 0));

            _recipes.Add(new RecipeDef("rcp_antidote_lzr9", "antidote", 1, "med_station",
                new[] { new ItemStack("mutagen_sample", 1), new ItemStack("antibiotic", 1), new ItemStack("chemicals", 2) }, "selin", 0));

            _recipes.Add(new RecipeDef("rcp_adrenaline", "adrenaline", 1, "med_station",
                new[] { new ItemStack("chemicals", 2), new ItemStack("painkiller", 1), new ItemStack("energy_drink", 1) }, "selin", 1));

            _recipes.Add(new RecipeDef("rcp_burn_gel", "burn_gel", 2, "med_station",
                new[] { new ItemStack("antiseptic", 1), new ItemStack("purified_water", 1), new ItemStack("chemicals", 1) }, "selin", 0));

            _recipes.Add(new RecipeDef("rcp_surgical_kit", "surgical_kit", 1, "med_station",
                new[] { new ItemStack("medkit", 1), new ItemStack("alcohol_pure", 2), new ItemStack("scrap_metal", 1) }, "selin", 2));

            _recipes.Add(new RecipeDef("rcp_herbal_poultice", "herbal_poultice", 2, "med_station",
                new[] { new ItemStack("cloth_rag", 2), new ItemStack("herbal_tea", 1) }, "selin", 0));

            _recipes.Add(new RecipeDef("rcp_morphine", "morphine_ampoule", 1, "med_station",
                new[] { new ItemStack("painkiller", 2), new ItemStack("alcohol_pure", 1), new ItemStack("chronos_vial_empty", 1) }, "selin", 2));

            // =====================================================================
            // 5. WEAPON_BENCH (Silah Tezgâhı - Kemal Usta, 10 tarif)
            // =====================================================================
            _recipes.Add(new RecipeDef("rcp_ammo_9mm", "ammo_9mm", 18, "weapon_bench",
                new[] { new ItemStack("gunpowder", 2), new ItemStack("scrap_metal", 2) }, "kemal", 0));

            _recipes.Add(new RecipeDef("rcp_ammo_357", "ammo_357", 10, "weapon_bench",
                new[] { new ItemStack("gunpowder", 2), new ItemStack("brass_casings", 2), new ItemStack("lead_scrap", 1) }, "kemal", 0));

            _recipes.Add(new RecipeDef("rcp_ammo_12g", "ammo_12g", 10, "weapon_bench",
                new[] { new ItemStack("gunpowder", 2), new ItemStack("nails", 3), new ItemStack("plastic_casing", 1) }, "kemal", 0));

            _recipes.Add(new RecipeDef("rcp_ammo_556", "ammo_556", 20, "weapon_bench",
                new[] { new ItemStack("gunpowder", 3), new ItemStack("brass_casings", 2), new ItemStack("scrap_metal", 2) }, "kemal", 1));

            _recipes.Add(new RecipeDef("rcp_ammo_762", "ammo_762", 8, "weapon_bench",
                new[] { new ItemStack("gunpowder", 3), new ItemStack("brass_casings", 2), new ItemStack("lead_scrap", 2) }, "kemal", 2));

            _recipes.Add(new RecipeDef("rcp_machete", "machete", 1, "weapon_bench",
                new[] { new ItemStack("steel_plate", 2), new ItemStack("leather_strip", 2), new ItemStack("adhesive_epoxy", 1) }, "kemal", 0));

            _recipes.Add(new RecipeDef("rcp_axe", "axe", 1, "weapon_bench",
                new[] { new ItemStack("steel_plate", 2), new ItemStack("wood_plank", 2), new ItemStack("duct_tape", 2) }, "kemal", 0));

            _recipes.Add(new RecipeDef("rcp_weapon_upgrade_kit", "weapon_upgrade_kit", 1, "weapon_bench",
                new[] { new ItemStack("weapon_parts", 2), new ItemStack("steel_plate", 1), new ItemStack("adhesive_epoxy", 1) }, "kemal", 1));

            _recipes.Add(new RecipeDef("rcp_armor_tactical", "armor_tactical", 1, "weapon_bench",
                new[] { new ItemStack("armor_scrap", 1), new ItemStack("steel_plate", 3), new ItemStack("leather_strip", 3) }, "kemal", 2));

            _recipes.Add(new RecipeDef("rcp_revolver_build", "revolver", 1, "weapon_bench",
                new[] { new ItemStack("weapon_parts", 3), new ItemStack("steel_plate", 2), new ItemStack("wood_plank", 1) }, "kemal", 2));

            // =====================================================================
            // 6. CHEM_STATION (Kimya İstasyonu - Elif Yıldız, 8 tarif)
            // =====================================================================
            _recipes.Add(new RecipeDef("rcp_molotov", "molotov", 2, "chem_station",
                new[] { new ItemStack("fuel", 1), new ItemStack("cloth_rag", 2), new ItemStack("empty_bottle", 2) }, "elif", 0));

            _recipes.Add(new RecipeDef("rcp_grenade", "grenade", 1, "chem_station",
                new[] { new ItemStack("gunpowder", 3), new ItemStack("scrap_metal", 2), new ItemStack("nails", 3) }, "elif", 1));

            _recipes.Add(new RecipeDef("rcp_gunpowder_synth", "gunpowder", 4, "chem_station",
                new[] { new ItemStack("chemicals", 2), new ItemStack("sulfur_powder", 1), new ItemStack("charcoal_tablets", 1) }, "elif", 0));

            _recipes.Add(new RecipeDef("rcp_gunpowder_fertilizer", "gunpowder", 5, "chem_station",
                new[] { new ItemStack("fertilizer_bag", 1), new ItemStack("fuel", 1), new ItemStack("chemicals", 1) }, "elif", 1));

            _recipes.Add(new RecipeDef("rcp_armor_acid", "armor_acid", 1, "chem_station",
                new[] { new ItemStack("armor_scrap", 1), new ItemStack("acid_gland", 2), new ItemStack("rubber_hose", 2), new ItemStack("chemicals", 3) }, "elif", 0));

            _recipes.Add(new RecipeDef("rcp_armor_hazmat", "armor_hazmat", 1, "chem_station",
                new[] { new ItemStack("armor_acid", 1), new ItemStack("steel_plate", 2), new ItemStack("charcoal_filter", 2), new ItemStack("adhesive_epoxy", 2) }, "elif", 2));

            _recipes.Add(new RecipeDef("rcp_battery_recharge", "battery", 1, "chem_station",
                new[] { new ItemStack("chemicals", 2), new ItemStack("lead_scrap", 2), new ItemStack("wire", 2) }, "elif", 0));

            _recipes.Add(new RecipeDef("rcp_gas_mask_filter", "gas_mask_filter", 2, "chem_station",
                new[] { new ItemStack("charcoal_filter", 1), new ItemStack("chemicals", 1), new ItemStack("cloth_rag", 2) }, "elif", 0));
        }
    }
}
