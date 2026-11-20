// 45: Layered Hell — ItemDatabase: 115+ items, 12 weapons, and 13 deterministic loot tables.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LayeredHell
{
    public enum ItemType
    {
        Weapon,
        Ammo,
        Food,
        Drink,
        Medical,
        Material,
        Armor,
        Key,
        Quest,
        Throwable,
        Misc
    }

    [Serializable]
    public class ItemDef
    {
        public string id;
        public string name;
        public string description;
        public ItemType type;
        public float weight;
        public int maxStack;
        public int value;
        public float heal;
        public float food;
        public float water;
        public float infectionReduce;
        public float staminaRestore;
        public float armorReduction;
        public string iconColorHex;

        public ItemDef() { }

        public ItemDef(
            string id, string name, string description, ItemType type,
            float weight, int maxStack, int value,
            float heal = 0f, float food = 0f, float water = 0f,
            float infectionReduce = 0f, float staminaRestore = 0f,
            float armorReduction = 0f, string iconColorHex = "#CCCCCC")
        {
            this.id = id;
            this.name = name;
            this.description = description;
            this.type = type;
            this.weight = weight;
            this.maxStack = Mathf.Max(1, maxStack);
            this.value = value;
            this.heal = heal;
            this.food = food;
            this.water = water;
            this.infectionReduce = infectionReduce;
            this.staminaRestore = staminaRestore;
            this.armorReduction = armorReduction;
            this.iconColorHex = iconColorHex;
        }
    }

    [Serializable]
    public class WeaponDef
    {
        public string id;
        public string displayName;
        public bool melee;
        public bool thrown;
        public bool automatic;
        public float damage;
        public float fireRate;
        public float range;
        public float spread;
        public float recoil;
        public float reloadTime;
        public float noise;
        public int magSize;
        public int pellets;
        public string ammoId;
        public string modelId;
        public float headshotMultiplier;
        public int upgradeLevel;

        public WeaponDef() { }

        public WeaponDef(
            string id, string displayName, bool melee, bool thrown, bool automatic,
            float damage, float fireRate, float range, float spread, float recoil,
            float reloadTime, float noise, int magSize, int pellets, string ammoId,
            float headshotMultiplier = 2.0f, int upgradeLevel = 0)
        {
            this.id = id;
            this.displayName = displayName;
            this.melee = melee;
            this.thrown = thrown;
            this.automatic = automatic;
            this.damage = damage;
            this.fireRate = fireRate;
            this.range = range;
            this.spread = spread;
            this.recoil = recoil;
            this.reloadTime = reloadTime;
            this.noise = noise;
            this.magSize = magSize;
            this.pellets = Mathf.Max(1, pellets);
            this.ammoId = ammoId ?? "";
            this.modelId = "wpn_" + id + "_01";
            this.headshotMultiplier = headshotMultiplier;
            this.upgradeLevel = upgradeLevel;
        }
    }

    public static class ItemDatabase
    {
        private struct LootEntry
        {
            public string itemId;
            public int weight;
            public int minCount;
            public int maxCount;

            public LootEntry(string itemId, int weight, int minCount = 1, int maxCount = 1)
            {
                this.itemId = itemId;
                this.weight = weight;
                this.minCount = minCount;
                this.maxCount = maxCount;
            }
        }

        private static readonly Dictionary<string, ItemDef> _items = new Dictionary<string, ItemDef>(StringComparer.Ordinal);
        private static readonly List<ItemDef> _allItems = new List<ItemDef>();
        private static readonly Dictionary<string, WeaponDef> _weapons = new Dictionary<string, WeaponDef>(StringComparer.Ordinal);
        private static readonly Dictionary<string, LootEntry[]> _lootTables = new Dictionary<string, LootEntry[]>(StringComparer.Ordinal);
        private static bool _initialized;

        public static IEnumerable<ItemDef> All
        {
            get
            {
                EnsureInitialized();
                return _allItems;
            }
        }

        public static bool Exists(string id)
        {
            EnsureInitialized();
            return !string.IsNullOrEmpty(id) && _items.ContainsKey(id);
        }

        public static ItemDef Get(string id)
        {
            EnsureInitialized();
            if (!string.IsNullOrEmpty(id) && _items.TryGetValue(id, out var def))
                return def;

            string safeId = string.IsNullOrEmpty(id) ? "unknown_item" : id;
            return new ItemDef(
                safeId,
                "Bilinmeyen Eşya (" + safeId + ")",
                "Kule enkazından çıkarılmış tanımlanamayan nesne.",
                ItemType.Misc,
                0.2f, 10, 5, 0f, 0f, 0f, 0f, 0f, 0f, "#888888");
        }

        public static WeaponDef GetWeapon(string id)
        {
            EnsureInitialized();
            if (!string.IsNullOrEmpty(id) && _weapons.TryGetValue(id, out var w))
                return w;
            return null;
        }

        public static void UpgradeWeaponStat(string weaponId, float damageMultiplier = 1.18f, float recoilFactor = 0.88f)
        {
            EnsureInitialized();
            if (!string.IsNullOrEmpty(weaponId) && _weapons.TryGetValue(weaponId, out var w))
            {
                if (w.upgradeLevel < 3)
                {
                    w.upgradeLevel++;
                    w.damage = Mathf.Round(w.damage * damageMultiplier * 10f) / 10f;
                    w.recoil = Mathf.Max(0.1f, w.recoil * recoilFactor);
                }
            }
        }

        public static List<ItemStack> RollLoot(string tableId, System.Random rng)
        {
            EnsureInitialized();
            if (rng == null) rng = new System.Random();

            var result = new List<ItemStack>();
            string key = string.IsNullOrEmpty(tableId) ? "residential" : tableId;
            if (!_lootTables.TryGetValue(key, out var pool) || pool == null || pool.Length == 0)
            {
                pool = _lootTables["residential"];
            }

            int rolls = rng.Next(1, 4); // 1 to 3 items per container
            if (key == "safe" || key == "military" || key == "lab")
                rolls = rng.Next(2, 5);

            int totalWeight = 0;
            for (int i = 0; i < pool.Length; i++)
                totalWeight += pool[i].weight;

            if (totalWeight <= 0) return result;

            for (int r = 0; r < rolls; r++)
            {
                int pick = rng.Next(0, totalWeight);
                int accum = 0;
                for (int i = 0; i < pool.Length; i++)
                {
                    accum += pool[i].weight;
                    if (pick < accum)
                    {
                        var e = pool[i];
                        int count = e.maxCount > e.minCount ? rng.Next(e.minCount, e.maxCount + 1) : e.minCount;
                        if (count > 0)
                        {
                            MergeIntoList(result, e.itemId, count);
                        }
                        break;
                    }
                }
            }

            return result;
        }

        private static void MergeIntoList(List<ItemStack> list, string itemId, int count)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].id == itemId)
                {
                    list[i] = new ItemStack(itemId, list[i].count + count);
                    return;
                }
            }
            list.Add(new ItemStack(itemId, count));
        }

        private static void Reg(ItemDef def)
        {
            _items[def.id] = def;
            _allItems.Add(def);
        }

        private static void RegWeapon(WeaponDef w, string name, string desc, float weight, int value, ItemType type = ItemType.Weapon, int maxStack = 1, string color = "#D9822B")
        {
            _weapons[w.id] = w;
            Reg(new ItemDef(w.id, name, desc, type, weight, maxStack, value, 0f, 0f, 0f, 0f, 0f, 0f, color));
        }

        private static void EnsureInitialized()
        {
            if (_initialized) return;
            _initialized = true;
            _items.Clear();
            _allItems.Clear();
            _weapons.Clear();
            _lootTables.Clear();

            BuildWeapons();
            BuildAmmo();
            BuildFoodAndDrinks();
            BuildMedical();
            BuildMaterials();
            BuildArmor();
            BuildKeysAndQuestItems();
            BuildSurvivalAndMisc();
            BuildLootTables();
        }

        private static void BuildWeapons()
        {
            // 6 Ranged weapons
            RegWeapon(
                new WeaponDef("pistol", "Yavuz 9mm Tabanca", false, false, false, 24f, 4.2f, 45f, 1.4f, 1.2f, 1.6f, 55f, 12, 1, "ammo_9mm", 2.2f, 0),
                "Yavuz 9mm Tabanca", "Vanguard özel güvenlik personelinin standart yarı otomatik 9mm tabancası. Dengeli ve güvenilir.", 1.2f, 120);

            RegWeapon(
                new WeaponDef("revolver", "Kobra .357 Toplu Tabanca", false, false, false, 52f, 1.6f, 55f, 0.9f, 2.8f, 2.4f, 65f, 6, 1, "ammo_357", 2.6f, 0),
                "Kobra .357 Toplu Tabanca", "Ağır .357 Magnum fişek kullanan yüksek durdurucu güce sahip altıpatlar.", 1.4f, 210);

            RegWeapon(
                new WeaponDef("shotgun", "Atmaca 12G Pompalı Tüfek", false, false, false, 14f, 1.1f, 22f, 6.5f, 3.6f, 2.8f, 70f, 6, 8, "ammo_12g", 1.8f, 0),
                "Atmaca 12G Pompalı Tüfek", "Dar koridorlarda tek atışla enfekte sürülerini dağıtan 12 kalibre taktik pompalı tüfek.", 3.2f, 280);

            RegWeapon(
                new WeaponDef("smg", "Fırtına MP-9 Hafif Makineli", false, false, true, 19f, 10.5f, 35f, 2.8f, 1.1f, 1.9f, 58f, 30, 1, "ammo_9mm", 2.0f, 0),
                "Fırtına MP-9 Hafif Makineli", "9mm mermi yakan yüksek atış hızlı kompakt makineli tabanca. Koşucu sürülerine karşı ideal.", 2.3f, 260);

            RegWeapon(
                new WeaponDef("rifle", "Bora 5.56 Saldırı Tüfeği", false, false, true, 36f, 7.5f, 75f, 1.6f, 1.6f, 2.2f, 66f, 30, 1, "ammo_556", 2.4f, 0),
                "Bora 5.56 Saldırı Tüfeği", "Vanguard ağır müdahale timlerinin kullandığı 5.56mm tam otomatik piyade tüfeği.", 3.6f, 380);

            RegWeapon(
                new WeaponDef("sniper", "Karayel 7.62 Keskin Nişancı", false, false, false, 95f, 0.85f, 120f, 0.25f, 3.8f, 2.9f, 72f, 5, 1, "ammo_762", 3.2f, 0),
                "Karayel 7.62 Keskin Nişancı", "Zırhlı hedefleri ve ağır mutantları tek kafa vuruşuyla deviren 7.62mm sürgülü tüfek.", 4.2f, 450);

            // 4 Melee weapons
            RegWeapon(
                new WeaponDef("pipe", "Ağır Tesisat Borusu", true, false, false, 22f, 1.35f, 2.3f, 0f, 0.4f, 0f, 8f, 0, 1, "", 1.7f, 0),
                "Ağır Tesisat Borusu", "38-B dairesinin tesisatından sökülmüş kalın çelik su borusu. Sessiz ve dayanıklı.", 1.5f, 35, ItemType.Weapon, 1, "#8A959E");

            RegWeapon(
                new WeaponDef("bat", "Çivili Beyzbol Sopası", true, false, false, 30f, 1.25f, 2.5f, 0f, 0.5f, 0f, 8f, 0, 1, "", 1.85f, 0),
                "Çivili Beyzbol Sopası", "Ucu sanayi çivileriyle güçlendirilmiş gürgen beyzbol sopası.", 1.7f, 65, ItemType.Weapon, 1, "#B07C4F");

            RegWeapon(
                new WeaponDef("axe", "Yangın Baltası", true, false, false, 46f, 0.95f, 2.6f, 0f, 0.7f, 0f, 9f, 0, 1, "", 2.1f, 0),
                "Yangın Baltası", "Kule yangın dolaplarından alınmış ağır çelik başlıklı acil durum baltası.", 2.5f, 110, ItemType.Weapon, 1, "#C93B2B");

            RegWeapon(
                new WeaponDef("machete", "Karbon Çelik Pala", true, false, false, 38f, 1.65f, 2.4f, 0f, 0.4f, 0f, 7f, 0, 1, "", 2.2f, 0),
                "Karbon Çelik Pala", "Kemal Usta'nın tezgâhında bilenmiş, hızlı ve ölümcül keskinlikte ağır pala.", 1.3f, 150, ItemType.Weapon, 1, "#A8B5C2");

            // 2 Thrown weapons
            RegWeapon(
                new WeaponDef("grenade", "M67 Parça Tesirli El Bombası", false, true, false, 140f, 0.8f, 18f, 0f, 0.5f, 0.5f, 90f, 1, 1, "grenade", 1.0f, 0),
                "M67 Parça Tesirli El Bombası", "Dar odalardaki zombi gruplarını parçalayan askeri tip el bombası. Çok yüksek ses çıkarır.", 0.4f, 95, ItemType.Throwable, 5, "#5B7043");

            RegWeapon(
                new WeaponDef("molotov", "Molotof Kokteyli", false, true, false, 95f, 0.8f, 16f, 0f, 0.4f, 0.5f, 75f, 1, 1, "molotov", 1.0f, 0),
                "Molotof Kokteyli", "Yakıt ve bez parçasıyla hazırlanmış yangın bombası. Koridorları alevle kapatır.", 0.5f, 60, ItemType.Throwable, 5, "#E06A28");
        }

        private static void BuildAmmo()
        {
            Reg(new ItemDef("ammo_9mm", "9x19mm Parabellum Mermi", "Tabanca ve hafif makineli tüfekler için standart 9mm fişek.", ItemType.Ammo, 0.02f, 120, 3, 0, 0, 0, 0, 0, 0, "#D4AF37"));
            Reg(new ItemDef("ammo_357", ".357 Magnum Mermi", "Toplu tabancalar için yüksek basınçlı ağır çekirdekli fişek.", ItemType.Ammo, 0.03f, 60, 6, 0, 0, 0, 0, 0, 0, "#E5C158"));
            Reg(new ItemDef("ammo_12g", "12 Kalibre Av Fişeği", "Pompalı tüfekler için kalın şevrotin saçmalı fişek.", ItemType.Ammo, 0.05f, 48, 7, 0, 0, 0, 0, 0, 0, "#C83E31"));
            Reg(new ItemDef("ammo_556", "5.56x45mm NATO Mermi", "Saldırı tüfekleri için yüksek hızlı zırh delici piyade mermisi.", ItemType.Ammo, 0.03f, 120, 5, 0, 0, 0, 0, 0, 0, "#B89B3C"));
            Reg(new ItemDef("ammo_762", "7.62x51mm Keskin Nişancı Mermisi", "Uzun menzilli tüfekler için ağır tam metal gömlekli fişek.", ItemType.Ammo, 0.04f, 40, 10, 0, 0, 0, 0, 0, 0, "#9E7E28"));
        }

        private static void BuildFoodAndDrinks()
        {
            // Mandatory CONTRACT food & drinks
            Reg(new ItemDef("canned_beans", "Konserve Kuru Fasulye", "Soğuk da yenebilen doyurucu barbunya ve kuru fasulye konservesi.", ItemType.Food, 0.4f, 10, 18, 4f, 32f, 4f, 0f, 5f, 0f, "#C96A3B"));
            Reg(new ItemDef("canned_tomato", "Konserve Domates Çorbası", "Hem açlığı hem susuzluğu hafifleten yoğun domates konservesi.", ItemType.Food, 0.4f, 10, 16, 3f, 22f, 16f, 0f, 5f, 0f, "#D13B2E"));
            Reg(new ItemDef("canned_tuna", "Konserve Ton Balığı", "Yüksek proteinli zeytinyağlı ton balığı konservesi.", ItemType.Food, 0.25f, 12, 22, 5f, 30f, 0f, 0f, 10f, 0f, "#3F82B8"));
            Reg(new ItemDef("water_bottle", "Pet Şişe Su (500ml)", "Kapalı ambalajda içme suyu. Kulede şebeke kesildiğinden beri altın değerinde.", ItemType.Drink, 0.5f, 10, 20, 0f, 0f, 38f, 0f, 12f, 0f, "#3A9AD9"));
            Reg(new ItemDef("purified_water", "Arıtılmış Yağmur Suyu", "38-B balkonundaki filtre sisteminden geçirilip kaynatılmış temiz su.", ItemType.Drink, 0.5f, 10, 25, 2f, 0f, 48f, 0f, 18f, 0f, "#52B8F2"));
            Reg(new ItemDef("soda", "Kutu Gazoz", "AVM otomatlarından kalma şekerli gazlı içecek. Hızlı enerji verir.", ItemType.Drink, 0.35f, 10, 14, 0f, 5f, 26f, 0f, 25f, 0f, "#E04F5F"));
            Reg(new ItemDef("snack", "Protein Barı", "Spor salonu ve ofis çekmecelerinde bulunan yoğun kalorili tahıl barı.", ItemType.Food, 0.15f, 15, 12, 2f, 20f, -2f, 0f, 15f, 0f, "#B58342"));
            Reg(new ItemDef("cooked_meal", "Sıcak Güveç Yemeği", "Ocakta konserveler ve baharatlarla pişirilmiş doyurucu sıcak yemek.", ItemType.Food, 0.6f, 6, 45, 18f, 58f, 15f, 0f, 30f, 0f, "#E68A2E"));

            // Additional Turkish foods & drinks
            Reg(new ItemDef("dried_meat", "Kurutulmuş Pastırma", "Bozulmadan uzun süre dayanan baharatlı kuru et dilimleri.", ItemType.Food, 0.2f, 12, 28, 6f, 36f, -5f, 0f, 15f, 0f, "#8F2D24"));
            Reg(new ItemDef("military_ration", "Askerî Kumanya Paketi (MRE)", "Karantina ekiplerinin bıraktığı yüksek kalorili vakumlu saha kumanyası.", ItemType.Food, 0.65f, 6, 55, 15f, 65f, 20f, 0f, 35f, 0f, "#5C6B46"));
            Reg(new ItemDef("chocolate_bar", "Bitter Çikolata", "Moral ve enerji veren fındıklı bitter çikolata paketi.", ItemType.Food, 0.12f, 15, 14, 3f, 18f, -2f, 0f, 22f, 0f, "#593A28"));
            Reg(new ItemDef("crackers", "Tuzlu Galeta Paketi", "Kuru ama mideyi bastıran kepekli galeta.", ItemType.Food, 0.15f, 15, 10, 1f, 16f, -4f, 0f, 6f, 0f, "#D9B675"));
            Reg(new ItemDef("canned_corn", "Konserve Tatlı Mısır", "Haşlanmış mısır konservesi, hem sulu hem besleyici.", ItemType.Food, 0.35f, 10, 15, 2f, 24f, 10f, 0f, 8f, 0f, "#F2C94C"));
            Reg(new ItemDef("canned_olives", "Teneke Zeytin", "Yağlı siyah gemlik zeytini, yüksek kalorili erzak.", ItemType.Food, 0.35f, 10, 17, 3f, 26f, 0f, 0f, 8f, 0f, "#3B3C36"));
            Reg(new ItemDef("instant_noodles", "Hazır Erişte Paketi", "Sıcak suyla dakikalar içinde hazırlanan baharatlı erişte.", ItemType.Food, 0.15f, 12, 12, 2f, 22f, -3f, 0f, 10f, 0f, "#E5A93B"));
            Reg(new ItemDef("honey_jar", "Süzme Çiçek Balı", "Doğal antibakteriyel ve enerji deposu küçük kavanoz bal.", ItemType.Food, 0.35f, 8, 30, 8f, 28f, 0f, 2f, 28f, 0f, "#F2994A"));
            Reg(new ItemDef("roasted_nuts", "Karışık Kuruyemiş", "Kavrulmuş fındık, badem ve ceviz paketi.", ItemType.Food, 0.2f, 12, 18, 3f, 25f, -3f, 0f, 18f, 0f, "#9E6B3B"));
            Reg(new ItemDef("baby_food", "Meyveli Bebek Maması", "Hızlı sindirilen vitaminli cam kavanoz püre.", ItemType.Food, 0.2f, 10, 14, 4f, 18f, 12f, 0f, 10f, 0f, "#F2A679"));
            Reg(new ItemDef("energy_drink", "Volt Enerji İçecek", "Kafein ve taurin yüklü kutu içecek. Yorgunluğu anında keser.", ItemType.Drink, 0.35f, 10, 24, 0f, 4f, 28f, 0f, 55f, 0f, "#27AE60"));
            Reg(new ItemDef("herbal_tea", "Demlenmiş Ihlamur Çayı", "Termosta sıcak tutulmuş şifalı bitki çayı. Enfeksiyon direncini destekler.", ItemType.Drink, 0.4f, 8, 26, 10f, 4f, 36f, 5f, 20f, 0f, "#B59B42"));
            Reg(new ItemDef("fruit_juice", "Meyve Nektarı Kutusu", "Şeftali ve portakal karışımı vitaminli meyve suyu.", ItemType.Drink, 0.35f, 10, 16, 3f, 8f, 32f, 0f, 16f, 0f, "#F2994A"));
            Reg(new ItemDef("cold_coffee", "Soğuk Kahve Kutusu", "Gece nöbetlerinde uyanık tutan sert sütlü kahve.", ItemType.Drink, 0.3f, 10, 18, 0f, 5f, 24f, 0f, 42f, 0f, "#6E472B"));
            Reg(new ItemDef("isotonic_drink", "İzotonik Sporcu İçeceği", "34. kattaki spor salonundan alınmış elektrolit destekli içecek.", ItemType.Drink, 0.5f, 10, 22, 4f, 4f, 42f, 0f, 35f, 0f, "#2D9CDB"));
            Reg(new ItemDef("dirty_water", "Bulanık Şebeke Suyu", "Boru hattında kalmış paslı su. İçilirse enfeksiyon riski taşır, arıtılmalıdır.", ItemType.Drink, 0.5f, 10, 4, -5f, 0f, 22f, -8f, 0f, 0f, "#6E735F"));
        }

        private static void BuildMedical()
        {
            // Mandatory CONTRACT medical items
            Reg(new ItemDef("bandage", "Steril Sargı Bezi", "Kanamayı durduran ve hafif yaraları kapatan tıbbi gazlı bez.", ItemType.Medical, 0.1f, 15, 20, 25f, 0f, 0f, 0f, 0f, 0f, "#E8EEF2"));
            Reg(new ItemDef("medkit", "İlk Yardım Çantası", "Cerrahi dikiş seti, antiseptik ve pıhtılaştırıcı içeren kapsamlı sağlık çantası.", ItemType.Medical, 0.6f, 6, 75, 70f, 0f, 0f, 8f, 10f, 0f, "#D93838"));
            Reg(new ItemDef("antibiotic", "Geniş Spektrumlu Antibiyotik", "İkincil bakteriyel enfeksiyonları ve LZR-9 semptomlarını baskılayan hap.", ItemType.Medical, 0.1f, 10, 55, 10f, 0f, 0f, 28f, 0f, 0f, "#4A90E2"));
            Reg(new ItemDef("antidote", "LZR-9 Panzehir Serumu", "Dr. Selin Arslan'ın geliştirdiği, kandaki mutajenik yıkımı temizleyen mor oto-enjektör.", ItemType.Medical, 0.2f, 6, 140, 25f, 0f, 0f, 75f, 20f, 0f, "#9B51E0"));
            Reg(new ItemDef("painkiller", "Kuvvetli Ağrı Kesici", "Travma ağrısını dindirip nefesi açan acil durum tableti.", ItemType.Medical, 0.08f, 15, 30, 18f, 0f, 0f, 0f, 25f, 0f, "#F2994A"));
            Reg(new ItemDef("adrenaline", "Adrenalin Oto-Enjektörü", "Kalp ritmini ve kas gücünü patlama noktasına çıkaran acil müdahale şırıngası.", ItemType.Medical, 0.15f, 8, 65, 20f, 0f, 0f, 0f, 95f, 0f, "#F2C94C"));

            // Extra medical items
            Reg(new ItemDef("antiseptic", "Antiseptik Solüsyon", "Isırık ve çizik yaralarını dezenfekte etmek için kullanılan iyotlu çözelti.", ItemType.Medical, 0.25f, 10, 28, 12f, 0f, 0f, 15f, 0f, 0f, "#B85D19"));
            Reg(new ItemDef("surgical_kit", "Sahra Cerrahi Seti", "31. kat Arslan Polikliniği'nden alınmış steril pens, neşter ve dikiş kiti.", ItemType.Medical, 0.45f, 5, 90, 85f, 0f, 0f, 12f, 0f, 0f, "#C0392B"));
            Reg(new ItemDef("splint", "Ortopedik Atel", "Burkulma ve kırıkları sabitleyerek hareket kabiliyetini geri kazandıran atel.", ItemType.Medical, 0.3f, 8, 35, 32f, 0f, 0f, 0f, 30f, 0f, "#7F8C8D"));
            Reg(new ItemDef("herbal_poultice", "BitkiselYara Lapası", "Temiz bez ve şifalı özlerle hazırlanmış geleneksel yara sargısı.", ItemType.Medical, 0.15f, 12, 24, 30f, 0f, 0f, 6f, 5f, 0f, "#6AA84F"));
            Reg(new ItemDef("charcoal_tablets", "Aktif Karbon Tabletleri", "Solunan Sis gazının ve bozuk suyun toksinlerini emen tıbbi karbon.", ItemType.Medical, 0.1f, 12, 32, 5f, 0f, 0f, 20f, 0f, 0f, "#333333"));
            Reg(new ItemDef("burn_gel", "Yanık ve Asit Jeli", "Tükürücü zombilerin asit yanıklarını nötralize eden soğutucu jel.", ItemType.Medical, 0.2f, 10, 42, 38f, 0f, 0f, 10f, 10f, 0f, "#56CCF2"));
            Reg(new ItemDef("vitamin_pack", "Multivitamin Kompleksi", "Bağışıklık sistemini destekleyen yoğun vitamin kapsülleri.", ItemType.Medical, 0.1f, 15, 22, 8f, 5f, 0f, 10f, 20f, 0f, "#F2994A"));
            Reg(new ItemDef("morphine_ampoule", "Morfin Ampulü", "Ağır yaralanmalarda şoku önleyen kontrollü klinik analjezik.", ItemType.Medical, 0.12f, 8, 60, 45f, 0f, 0f, 0f, 15f, 0f, "#BB6BD9"));
        }

        private static void BuildMaterials()
        {
            // Mandatory CONTRACT materials
            Reg(new ItemDef("scrap_metal", "Hurda Metal", "Sökülmüş havalandırma sacları ve kırık metal parçaları. Üretimin temel taşı.", ItemType.Material, 0.35f, 30, 8, 0, 0, 0, 0, 0, 0, "#7F8C8D"));
            Reg(new ItemDef("wood_plank", "Ahşap Kalas", "Mobilya ve kapı kasalarından sökülmüş sağlam kereste parçası.", ItemType.Material, 0.5f, 25, 7, 0, 0, 0, 0, 0, 0, "#A67B5B"));
            Reg(new ItemDef("cloth_rag", "Kumaş Parçası", "Perde ve kıyafetlerden yırtılmış temizlenebilir pamuklu bez.", ItemType.Material, 0.08f, 40, 4, 0, 0, 0, 0, 0, 0, "#D2B48C"));
            Reg(new ItemDef("duct_tape", "Sanayi Tipi Koli Bandı", "Lif takviyeli su geçirmez gri tamir bandı.", ItemType.Material, 0.15f, 25, 12, 0, 0, 0, 0, 0, 0, "#95A5A6"));
            Reg(new ItemDef("wire", "Bakır Kablo", "Duvar buatlarından çekilmiş iletken bakır elektrik teli.", ItemType.Material, 0.15f, 30, 10, 0, 0, 0, 0, 0, 0, "#D35400"));
            Reg(new ItemDef("chemicals", "Kimyasal Çözücü", "Laboratuvar ve temizlik depolarından toplanmış reaktif kimyasal şişesi.", ItemType.Material, 0.25f, 25, 16, 0, 0, 0, 0, 0, 0, "#2ECC71"));
            Reg(new ItemDef("gunpowder", "Dumansız Barut", "Mühimmat ve patlayıcı üretiminde kullanılan yüksek enerjili barut.", ItemType.Material, 0.15f, 30, 18, 0, 0, 0, 0, 0, 0, "#34495E"));
            Reg(new ItemDef("circuit", "Elektronik Devre Kartı", "Terminallerden ve ofis cihazlarından sökülmüş mikrodenetleyici kart.", ItemType.Material, 0.15f, 20, 24, 0, 0, 0, 0, 0, 0, "#27AE60"));
            Reg(new ItemDef("battery", "Yüksek Kapasiteli Akü", "Jeneratör, telsiz ve ağır araç sistemlerine güç veren kurşun-asit akü hücresi.", ItemType.Material, 0.9f, 10, 40, 0, 0, 0, 0, 0, 0, "#2980B9"));
            Reg(new ItemDef("fuel", "Bidon Yakıt", "Jeneratör, molotof ve zırhlı araç için arıtılmış dizel/benzin karışımı.", ItemType.Material, 0.8f, 12, 35, 0, 0, 0, 0, 0, 0, "#E67E22"));
            Reg(new ItemDef("nails", "Çelik Çivi Kutusu", "Barikat kurulumu ve yakın dövüş silah modifikasyonu için sanayi çivisi.", ItemType.Material, 0.2f, 40, 8, 0, 0, 0, 0, 0, 0, "#BDC3C7"));

            // Additional crafting / scavenging materials
            Reg(new ItemDef("empty_can", "Boş Konserve Kutusu", "Ses tuzağı veya derme çatma filtre yapımında kullanılan teneke kutu.", ItemType.Material, 0.08f, 25, 2, 0, 0, 0, 0, 0, 0, "#95A5A6"));
            Reg(new ItemDef("empty_bottle", "Boş Cam Şişe", "Su doldurmak veya molotof kokteyli hazırlamak için kalın cam şişe.", ItemType.Material, 0.15f, 20, 4, 0, 0, 0, 0, 0, 0, "#76D7C4"));
            Reg(new ItemDef("leather_strip", "Kalın Deri Kayış", "Koltuk döşemelerinden ve kemerlerden kesilmiş sağlam deri şerit.", ItemType.Material, 0.15f, 25, 11, 0, 0, 0, 0, 0, 0, "#6E3B1F"));
            Reg(new ItemDef("rubber_hose", "Kauçuk Hortum", "Su arıtıcı ve kimya istasyonu bağlantılarında kullanılan esnek hortum.", ItemType.Material, 0.25f, 20, 12, 0, 0, 0, 0, 0, 0, "#2C3E50"));
            Reg(new ItemDef("steel_plate", "Sertleştirilmiş Çelik Plaka", "Zırh ve ağır kapı barikatı yapımında kullanılan balistik çelik levha.", ItemType.Material, 0.9f, 15, 28, 0, 0, 0, 0, 0, 0, "#5D6D7E"));
            Reg(new ItemDef("brass_casings", "Boş Pirinç Kovan", "Kemal Usta'nın silah tezgâhında yeniden doldurulmaya uygun boş mermi kovanları.", ItemType.Material, 0.1f, 50, 6, 0, 0, 0, 0, 0, 0, "#D4AC0D"));
            Reg(new ItemDef("lead_scrap", "Kurşun Parçaları", "Mermi çekirdeği dökmek için kullanılan ağır kurşun levha artıkları.", ItemType.Material, 0.3f, 30, 9, 0, 0, 0, 0, 0, 0, "#566573"));
            Reg(new ItemDef("alcohol_pure", "Saf Tıbbi Alkol", "Dezenfektan, ilaç ve yanıcı karışım üretiminde kullanılan etil alkol.", ItemType.Material, 0.25f, 20, 19, 0, 0, 0, 0, 0, 0, "#85C1E9"));
            Reg(new ItemDef("charcoal_filter", "Karbon Filtre Kartuşu", "Yağmur suyu arıtıcısı ve gaz maskeleri için aktif karbon filtre.", ItemType.Material, 0.25f, 15, 22, 0, 0, 0, 0, 0, 0, "#424949"));
            Reg(new ItemDef("adhesive_epoxy", "ÇiftBileşenli Epoksi", "Metali metale kaynak gibi yapıştıran endüstriyel reçine.", ItemType.Material, 0.18f, 20, 16, 0, 0, 0, 0, 0, 0, "#F39C12"));
            Reg(new ItemDef("weapon_parts", "Hassas Silah Parçaları", "Yay, iğne, namlu bileziği ve tetik mekanizması yedek parçaları.", ItemType.Material, 0.35f, 15, 38, 0, 0, 0, 0, 0, 0, "#7F8C8D"));
            Reg(new ItemDef("optic_lens", "Optik Mercek", "Güvenlik kameralarından ve mikroskoplardan sökülmüş kristal mercek.", ItemType.Material, 0.15f, 15, 25, 0, 0, 0, 0, 0, 0, "#5DADE2"));
            Reg(new ItemDef("sulfur_powder", "Kükürt Tozu", "19. kat kimya laboratuvarından çıkmış sarı reaktif kükürt.", ItemType.Material, 0.15f, 25, 17, 0, 0, 0, 0, 0, 0, "#F4D03F"));
            Reg(new ItemDef("mutagen_sample", "Dondurulmuş LZR-9 Örneği", "Enfekte dokudan veya laboratuvar tüplerinden alınan biyo-örnek. Panzehir için şart.", ItemType.Material, 0.2f, 15, 45, 0, 0, 0, 0, 0, 0, "#8E44AD"));
            Reg(new ItemDef("acid_gland", "Asit Bezi Kesesi", "Tükürücü mutantlardan çıkarılan aşındırıcı salgı kesesi. Elif'in araştırmalarında kullanılır.", ItemType.Material, 0.25f, 15, 40, 0, 0, 0, 0, 0, 0, "#58D68D"));
            Reg(new ItemDef("solar_cell", "Kırık Güneş Paneli Hücresi", "Çatı ve balkon aydınlatmalarından sökülmüş fotovoltaik plaka.", ItemType.Material, 0.4f, 12, 30, 0, 0, 0, 0, 0, 0, "#2E86C1"));
            Reg(new ItemDef("copper_coil", "Bakır Bobin Sargısı", "Asansör motorları ve trafolardan sökülmüş elektromanyetik bobin.", ItemType.Material, 0.45f, 15, 26, 0, 0, 0, 0, 0, 0, "#CA6F1E"));
            Reg(new ItemDef("plastic_casing", "Sert Polimer Gövde", "Ofis kasaları ve ekipman kutularından kesilmiş dayanıklı plastik.", ItemType.Material, 0.2f, 25, 8, 0, 0, 0, 0, 0, 0, "#909497"));
            Reg(new ItemDef("spark_plug", "Ateşleme Bujisi", "Jeneratör ve motor onarımında kullanılan sağlam iridyum buji.", ItemType.Material, 0.15f, 15, 22, 0, 0, 0, 0, 0, 0, "#CACFD2"));
        }

        private static void BuildArmor()
        {
            // Mandatory CONTRACT armors
            Reg(new ItemDef("armor_scrap", "Hurda Plaka Yelek", "Deri kayışlar, koli bandı ve dövülmüş çelik saclarla birleştirilmiş göğüs zırhı.", ItemType.Armor, 3.2f, 1, 140, 0f, 0f, 0f, 0f, 0f, 0.28f, "#7F8C8D"));
            Reg(new ItemDef("armor_acid", "Kimyasal & Asit Korumalı Zırh", "Elif Yıldız'ın polimer kaplamayla güçlendirdiği, asit ve ısırıklara dayanıklı yelek.", ItemType.Armor, 3.6f, 1, 240, 0f, 0f, 0f, 0f, 0f, 0.45f, "#27AE60"));

            // Additional armors
            Reg(new ItemDef("armor_leather", "Güçlendirilmiş Deri Ceket", "Omuz ve kolları kalın deri şeritlerle kaplanmış hafif koruyucu mont.", ItemType.Armor, 2.0f, 1, 85, 0f, 0f, 0f, 0f, 0f, 0.16f, "#6E3B1F"));
            Reg(new ItemDef("armor_tactical", "Vanguard Balistik Yelek", "Vanguard ağır güvenlik timlerinin kevlar ve seramik plakalı taktik yeleği.", ItemType.Armor, 4.5f, 1, 340, 0f, 0f, 0f, 0f, 0f, 0.62f, "#2C3E50"));
            Reg(new ItemDef("armor_hazmat", "Zırhlı Biyo-Tehlike Tulumu", "Chronos B3 karantina katı için üretilmiş kurşun ve asit takviyeli ağır koruma tulumu.", ItemType.Armor, 4.8f, 1, 420, 0f, 0f, 0f, 0f, 0f, 0.72f, "#F39C12"));
        }

        private static void BuildKeysAndQuestItems()
        {
            // Mandatory CONTRACT keys & keycards
            Reg(new ItemDef("keycard_exec_1", "Yönetici Kartı 1 (Bronz)", "39. kattaki yönetici kasasından alınan 1. kademe Aethel-Vanguard yetki kartı.", ItemType.Key, 0.05f, 1, 100, 0, 0, 0, 0, 0, 0, "#CD7F32"));
            Reg(new ItemDef("keycard_exec_2", "Yönetici Kartı 2 (Gümüş)", "41. kattaki Chronos yönetici kasasından alınan 2. kademe yüksek güvenlik kartı.", ItemType.Key, 0.05f, 1, 150, 0, 0, 0, 0, 0, 0, "#BDC3C7"));
            Reg(new ItemDef("keycard_exec_3", "Yönetici Kartı 3 (Altın)", "44. kattaki Volkan Demirci süitinden alınan tam yetkili çatı geçiş kartı.", ItemType.Key, 0.05f, 1, 200, 0, 0, 0, 0, 0, 0, "#F1C40F"));
            Reg(new ItemDef("key_roof", "Çatı Helipad Anahtarı", "45. kat çatı kapısının elektromekanik kilidini açan ağır çelik anahtar.", ItemType.Key, 0.08f, 1, 150, 0, 0, 0, 0, 0, 0, "#E74C3C"));
            Reg(new ItemDef("key_lab", "Chronos Laboratuvar Anahtarı", "8. kattaki Murat Şen'in güvenlik dolabından alınan B1-B4 biyo-laboratuvar erişim anahtarı.", ItemType.Key, 0.08f, 1, 150, 0, 0, 0, 0, 0, 0, "#8E44AD"));
            Reg(new ItemDef("key_vehicle", "Zırhlı Araç Kontak Anahtarı", "B1 otoparkındaki Vanguard Zırhlı Müdahale Aracı'nı çalıştıran çipli anahtar.", ItemType.Key, 0.08f, 1, 150, 0, 0, 0, 0, 0, 0, "#2980B9"));

            // Mandatory CONTRACT quest items
            Reg(new ItemDef("evidence_1", "Chronos Kanıt Dosyası #1: Sevk Kayıtları", "B1 güvenlik terminalinden indirilen, kaçak tıbbi ekipman ve 'gönüllü' denek nakil kayıtları.", ItemType.Quest, 0.1f, 1, 200, 0, 0, 0, 0, 0, 0, "#9B59B6"));
            Reg(new ItemDef("evidence_2", "Chronos Kanıt Dosyası #2: Havalandırma Raporu", "B2 jeneratör arşivinden alınan, LZR-9 gazının kasıtlı olarak üst şaftlara yönlendirildiğini gösteren şema.", ItemType.Quest, 0.1f, 1, 200, 0, 0, 0, 0, 0, 0, "#9B59B6"));
            Reg(new ItemDef("evidence_3", "Chronos Kanıt Dosyası #3: Denek-0 Günlüğü", "B3 karantina kayıtlarından çıkan, Hasan Bozkurt'a verilen 9 doz LZR-9 serumunun klinik raporu.", ItemType.Quest, 0.1f, 1, 200, 0, 0, 0, 0, 0, 0, "#9B59B6"));
            Reg(new ItemDef("evidence_4", "Chronos Kanıt Dosyası #4: Dr. Leyla Aksoy'un Diski", "B3'te hayatını kaybeden muhbir Dr. Leyla Aksoy'un şifreli USB belleği ve ses kayıtları.", ItemType.Quest, 0.1f, 1, 200, 0, 0, 0, 0, 0, 0, "#9B59B6"));
            Reg(new ItemDef("evidence_5", "Chronos Kanıt Dosyası #5: Project Lazarus Ana Verisi", "B4 ana sunucusundan kopyalanan, Orhan Vural, Baran Tuncel ve KAPAK Harekâtı arasındaki gizli protokol.", ItemType.Quest, 0.1f, 1, 250, 0, 0, 0, 0, 0, 0, "#9B59B6"));
            Reg(new ItemDef("engine_part", "Ağır Vasıta Turbo Enjektör Bloğu", "Zırhlı müdahale aracının hasarlı dizel motorunu onarmak için gereken yedek motor parçası.", ItemType.Quest, 2.2f, 2, 180, 0, 0, 0, 0, 0, 0, "#E67E22"));
            Reg(new ItemDef("radar_code", "Hava Savunma Uplink Kodu (UPLINK-7)", "43. kat balo salonu arşivinden alınan çatı frekans doğrulama şifresi.", ItemType.Quest, 0.05f, 1, 180, 0, 0, 0, 0, 0, 0, "#3498DB"));
            Reg(new ItemDef("hospital_tag", "Arslan Polikliniği Hasta Bilekliği", "31. kattaki ilk vakalardan birine ait, üzerinde kan grubu ve LZR-9 semptom kodu yazılı bileklik.", ItemType.Quest, 0.02f, 5, 25, 0, 0, 0, 0, 0, 0, "#E74C3C"));

            // Additional keys
            Reg(new ItemDef("key_pharmacy", "Eczane Depo Anahtarı", "31. kat poliklinik ilaç dolaplarını ve kilitli revir kapılarını açan anahtar.", ItemType.Key, 0.05f, 3, 45, 0, 0, 0, 0, 0, 0, "#5DADE2"));
            Reg(new ItemDef("key_gunsmith", "Silah Atölyesi Kasa Anahtarı", "24. kattaki Kemal Usta'nın yedek mühimmat dolaplarını açan anahtar.", ItemType.Key, 0.05f, 3, 50, 0, 0, 0, 0, 0, 0, "#D35400"));
            Reg(new ItemDef("key_service", "Servis Şaftı Bakım Anahtarı", "B2 ve ofis katlarındaki teknik bakım kapılarını açan üçgen uçlu anahtar.", ItemType.Key, 0.06f, 3, 40, 0, 0, 0, 0, 0, 0, "#F39C12"));
            Reg(new ItemDef("key_apartment", "Konut Katı Yedek Anahtarı", "25-37. katlar arasındaki kilitli daire kapılarında kullanılabilen paspartu anahtar.", ItemType.Key, 0.05f, 5, 35, 0, 0, 0, 0, 0, 0, "#BDC3C7"));
            Reg(new ItemDef("key_mall_office", "AVM Yönetim Anahtarı", "1-11. katlardaki mağaza kepenklerini ve kasa odalarını açan anahtar.", ItemType.Key, 0.05f, 3, 45, 0, 0, 0, 0, 0, 0, "#2ECC71"));
        }

        private static void BuildSurvivalAndMisc()
        {
            // Mandatory CONTRACT utility items
            Reg(new ItemDef("flashlight_battery", "Lityum Fener Pili", "El feneri ve optik cihazlar için yüksek ömürlü şarjlı pil.", ItemType.Misc, 0.1f, 15, 15, 0f, 0f, 0f, 0f, 10f, 0f, "#F1C40F"));
            Reg(new ItemDef("lockpick", "Çelik Maymuncuk Seti", "Kilitli kapıları ve dolapları sessizce açmaya yarayan ince yay çeliği takım.", ItemType.Misc, 0.06f, 20, 22, 0f, 0f, 0f, 0f, 0f, 0f, "#BDC3C7"));

            // Extra tools, bombs, upgrade kits, lore items to exceed 110 items comfortably
            Reg(new ItemDef("noise_maker", "Kurmalı Ses Tuzağı", "Boş konserve ve devre kartıyla yapılmış, koşucu zombileri başka yöne çeken ses aparatı.", ItemType.Misc, 0.25f, 10, 30, 0, 0, 0, 0, 0, 0, "#E67E22"));
            Reg(new ItemDef("flare_stick", "Kırmızı İşaret Fişeği", "Karanlık katları aydınlatan ve zombilerin dikkatini dağıtan magnezyum meşalesi.", ItemType.Misc, 0.2f, 10, 20, 0, 0, 0, 0, 0, 0, "#E74C3C"));
            Reg(new ItemDef("water_purification_tablet", "Klor Arıtma Tableti", "Bulanık suyu içilebilir hale getirmeye yardımcı dezenfeksiyon tableti.", ItemType.Misc, 0.05f, 20, 14, 0, 0, 10f, 4f, 0, 0, "#5DADE2"));
            Reg(new ItemDef("weapon_upgrade_kit", "Silah Modifikasyon Kiti", "Kemal Usta'nın tezgâhında silahların hasar ve geri tepmesini iyileştiren namlu/yay seti.", ItemType.Misc, 0.6f, 5, 130, 0, 0, 0, 0, 0, 0, "#E67E22"));
            Reg(new ItemDef("barricade_kit", "Hazır Barikat Takviye Kiti", "38-B güvenli ev kapı ve pencerelerini hızla sağlamlaştıran kalas ve köşebent paketi.", ItemType.Material, 1.2f, 6, 65, 0, 0, 0, 0, 0, 0, "#A67B5B"));
            Reg(new ItemDef("gas_mask_filter", "Biyo-Filtre Kartuşu", "Yoğun LZR-9 Sis bölgelerinde solunum yollarını koruyan yedek süzgeç.", ItemType.Misc, 0.25f, 8, 48, 0, 0, 0, 15f, 0, 0, "#27AE60"));
            Reg(new ItemDef("repair_wrench", "Ağır İngiliz Anahtarı", "Mekanik onarım ve söküm işlerinde kullanılan dövme çelik anahtar.", ItemType.Misc, 0.8f, 3, 35, 0, 0, 0, 0, 0, 0, "#7F8C8D"));
            Reg(new ItemDef("soldering_iron", "Gazlı Havya", "Devre kartı ve telsiz onarımında kullanılan taşınabilir lehim aleti.", ItemType.Misc, 0.35f, 3, 40, 0, 0, 0, 0, 0, 0, "#D35400"));
            Reg(new ItemDef("multimeter", "Dijital Multimetre", "Asansör ve jeneratör panolarındaki voltaj kaçaklarını ölçen cihaz.", ItemType.Misc, 0.3f, 3, 38, 0, 0, 0, 0, 0, 0, "#F1C40F"));
            Reg(new ItemDef("crowbar_tool", "Çelik Levye", "Mühürlü kasaları ve sıkışmış kapakları kanırtmak için kullanılan alet.", ItemType.Misc, 1.1f, 2, 45, 0, 0, 0, 0, 0, 0, "#C0392B"));
            Reg(new ItemDef("matches_box", "Fırtına Kibriti", "Nemli havada bile yanan parafin kaplı kibrit kutusu.", ItemType.Misc, 0.05f, 20, 6, 0, 0, 0, 0, 0, 0, "#E74C3C"));
            Reg(new ItemDef("lighter_zippo", "Benzinli Çakmak", "Ocak ve molotof ateşlemek için güvenilir metal çakmak.", ItemType.Misc, 0.08f, 5, 18, 0, 0, 0, 0, 0, 0, "#D4AF37"));
            Reg(new ItemDef("candle_pack", "Kalın Parafin Mum", "Elektrik kesintisinde güvenli evi aydınlatan uzun ömürlü mum.", ItemType.Misc, 0.2f, 15, 8, 0, 0, 0, 0, 0, 0, "#F9E79F"));
            Reg(new ItemDef("soap_bar", "Antiseptik Sabun", "Enfeksiyon bulaşını azaltan kükürtlü temizlik sabunu.", ItemType.Misc, 0.15f, 15, 10, 2f, 0, 0, 4f, 0, 0, "#AED6F1"));
            Reg(new ItemDef("bleach_bottle", "Çamaşır Suyu", "Yüzey dezenfeksiyonu ve kimyasal sentezde kullanılan sodyum hipoklorit.", ItemType.Material, 0.6f, 8, 14, 0, 0, 0, 0, 0, 0, "#85C1E9"));
            Reg(new ItemDef("fertilizer_bag", "Amonyum Nitrat Gübresi", "Saksı ve teras depolarından bulunan, patlayıcı sentezine uygun kimyasal.", ItemType.Material, 0.7f, 10, 22, 0, 0, 0, 0, 0, 0, "#D5D8DC"));
            Reg(new ItemDef("radio_transistor", "Yedek Frekans Transistörü", "Nehir'in 88.7 Kule Radyosu vericisini güçlendiren yüksek frekans parçası.", ItemType.Material, 0.12f, 10, 32, 0, 0, 0, 0, 0, 0, "#5DADE2"));
            Reg(new ItemDef("watch_broken", "Durmuş Kol Saati", "Camı çatlamış ve 00:00'da, patlama anında durmuş metal kol saati.", ItemType.Misc, 0.08f, 10, 15, 0, 0, 0, 0, 0, 0, "#BDC3C7"));
            Reg(new ItemDef("family_photo", "Solmuş Aile Fotoğrafı", "Arkada 'Kule 45'te yeni hayatımız - Ekim 2026' yazan yanık kenarlı fotoğraf.", ItemType.Misc, 0.02f, 10, 5, 0, 0, 0, 0, 0, 0, "#D7BDE2"));
            Reg(new ItemDef("security_badge", "Vanguard Kimlik Kartı", "Kan lekeli personel yaka kartı. Arkasında acil durum toplanma protokolü yazılı.", ItemType.Misc, 0.03f, 10, 12, 0, 0, 0, 0, 0, 0, "#34495E"));
            Reg(new ItemDef("chronos_vial_empty", "Boş Chronos Numune Tüpü", "Üzerinde 'LZR-9 / BİYOLOJİK TEHLİKE' etiketi bulunan kırılmaz cam tüp.", ItemType.Material, 0.08f, 20, 14, 0, 0, 0, 0, 0, 0, "#9B59B6"));
            Reg(new ItemDef("old_book", "Teknik Mühendislik El Kitabı", "Aras'ın notlar aldığı yapı statiği ve havalandırma şemaları kitabı.", ItemType.Misc, 0.4f, 5, 16, 0, 0, 0, 0, 0, 0, "#A9CCE3"));
            Reg(new ItemDef("cash_bundle", "Değersiz Banknot Destesi", "Karantina altındaki bu kulede artık yalnızca tutuşturucu olarak işe yarayan para destesi.", ItemType.Misc, 0.05f, 30, 8, 0, 0, 0, 0, 0, 0, "#82E0AA"));
            Reg(new ItemDef("gold_ring", "Altın Alyans", "Terk edilmiş dairelerden birinde kalmış işlemeli yüzük.", ItemType.Misc, 0.02f, 10, 60, 0, 0, 0, 0, 0, 0, "#F1C40F"));
            Reg(new ItemDef("climbing_rope", "Statik Dağcı İpi (10m)", "Aras'ın eski arama-kurtarma çantasından kalma yüksek mukavemetli ip.", ItemType.Material, 0.7f, 6, 35, 0, 0, 0, 0, 0, 0, "#E74C3C"));
            Reg(new ItemDef("thermal_blanket", "Acil Durum Termal Battaniyesi", "Isı kaybını önleyen alüminyum folyo kurtarma örtüsü.", ItemType.Misc, 0.1f, 10, 18, 5f, 0, 0, 0, 10f, 0, "#F7DC6F"));
        }

        private static void BuildLootTables()
        {
            // All 13 mandatory loot tables:
            // residential, office, mall, medical, gym, barricade_crate, lab, executive, military, basement, safe, kitchen, tools
            _lootTables["residential"] = new[]
            {
                new LootEntry("canned_beans", 18, 1, 2),
                new LootEntry("canned_tomato", 16, 1, 2),
                new LootEntry("water_bottle", 20, 1, 2),
                new LootEntry("cloth_rag", 24, 1, 3),
                new LootEntry("duct_tape", 14, 1, 2),
                new LootEntry("bandage", 18, 1, 2),
                new LootEntry("painkiller", 12, 1, 2),
                new LootEntry("ammo_9mm", 14, 4, 10),
                new LootEntry("flashlight_battery", 12, 1, 2),
                new LootEntry("wood_plank", 15, 1, 2),
                new LootEntry("scrap_metal", 15, 1, 2),
                new LootEntry("lockpick", 8, 1, 2),
                new LootEntry("crackers", 12, 1, 2),
                new LootEntry("family_photo", 4, 1, 1),
                new LootEntry("candle_pack", 10, 1, 2)
            };

            _lootTables["office"] = new[]
            {
                new LootEntry("snack", 22, 1, 2),
                new LootEntry("soda", 20, 1, 2),
                new LootEntry("cold_coffee", 16, 1, 2),
                new LootEntry("wire", 22, 1, 3),
                new LootEntry("circuit", 18, 1, 2),
                new LootEntry("duct_tape", 16, 1, 2),
                new LootEntry("battery", 10, 1, 1),
                new LootEntry("flashlight_battery", 16, 1, 2),
                new LootEntry("painkiller", 14, 1, 2),
                new LootEntry("bandage", 14, 1, 2),
                new LootEntry("ammo_9mm", 12, 5, 12),
                new LootEntry("plastic_casing", 16, 1, 2),
                new LootEntry("lockpick", 10, 1, 2),
                new LootEntry("security_badge", 6, 1, 1)
            };

            _lootTables["mall"] = new[]
            {
                new LootEntry("canned_tuna", 16, 1, 2),
                new LootEntry("soda", 20, 1, 2),
                new LootEntry("water_bottle", 18, 1, 2),
                new LootEntry("chocolate_bar", 16, 1, 2),
                new LootEntry("cloth_rag", 20, 2, 4),
                new LootEntry("leather_strip", 14, 1, 2),
                new LootEntry("duct_tape", 16, 1, 2),
                new LootEntry("bandage", 14, 1, 2),
                new LootEntry("ammo_9mm", 14, 6, 14),
                new LootEntry("ammo_12g", 10, 3, 6),
                new LootEntry("battery", 10, 1, 1),
                new LootEntry("empty_bottle", 14, 1, 2),
                new LootEntry("armor_leather", 4, 1, 1)
            };

            _lootTables["medical"] = new[]
            {
                new LootEntry("bandage", 28, 2, 4),
                new LootEntry("medkit", 14, 1, 1),
                new LootEntry("antibiotic", 18, 1, 2),
                new LootEntry("painkiller", 22, 1, 3),
                new LootEntry("antiseptic", 20, 1, 2),
                new LootEntry("alcohol_pure", 18, 1, 2),
                new LootEntry("adrenaline", 10, 1, 1),
                new LootEntry("burn_gel", 12, 1, 2),
                new LootEntry("charcoal_tablets", 14, 1, 2),
                new LootEntry("chemicals", 16, 1, 2),
                new LootEntry("surgical_kit", 6, 1, 1),
                new LootEntry("antidote", 4, 1, 1),
                new LootEntry("hospital_tag", 8, 1, 1)
            };

            _lootTables["gym"] = new[]
            {
                new LootEntry("isotonic_drink", 24, 1, 2),
                new LootEntry("energy_drink", 22, 1, 2),
                new LootEntry("snack", 24, 1, 3),
                new LootEntry("water_bottle", 20, 1, 2),
                new LootEntry("bandage", 18, 1, 2),
                new LootEntry("painkiller", 16, 1, 2),
                new LootEntry("adrenaline", 10, 1, 1),
                new LootEntry("vitamin_pack", 16, 1, 2),
                new LootEntry("cloth_rag", 18, 1, 3),
                new LootEntry("leather_strip", 14, 1, 2),
                new LootEntry("bat", 5, 1, 1)
            };

            _lootTables["barricade_crate"] = new[]
            {
                new LootEntry("wood_plank", 26, 2, 4),
                new LootEntry("scrap_metal", 26, 2, 4),
                new LootEntry("nails", 24, 2, 5),
                new LootEntry("duct_tape", 20, 1, 3),
                new LootEntry("wire", 18, 1, 3),
                new LootEntry("steel_plate", 12, 1, 2),
                new LootEntry("fuel", 12, 1, 2),
                new LootEntry("ammo_9mm", 16, 6, 15),
                new LootEntry("ammo_12g", 12, 4, 8),
                new LootEntry("bandage", 14, 1, 2),
                new LootEntry("lockpick", 10, 1, 2)
            };

            _lootTables["lab"] = new[]
            {
                new LootEntry("chemicals", 26, 2, 4),
                new LootEntry("alcohol_pure", 20, 1, 3),
                new LootEntry("mutagen_sample", 18, 1, 2),
                new LootEntry("circuit", 20, 1, 2),
                new LootEntry("battery", 14, 1, 2),
                new LootEntry("antibiotic", 18, 1, 2),
                new LootEntry("antidote", 10, 1, 1),
                new LootEntry("medkit", 12, 1, 1),
                new LootEntry("acid_gland", 12, 1, 2),
                new LootEntry("charcoal_filter", 14, 1, 2),
                new LootEntry("optic_lens", 12, 1, 2),
                new LootEntry("ammo_556", 12, 8, 16)
            };

            _lootTables["executive"] = new[]
            {
                new LootEntry("ammo_357", 20, 6, 12),
                new LootEntry("ammo_9mm", 18, 10, 20),
                new LootEntry("ammo_556", 14, 10, 20),
                new LootEntry("medkit", 16, 1, 1),
                new LootEntry("painkiller", 18, 1, 2),
                new LootEntry("circuit", 18, 1, 2),
                new LootEntry("battery", 14, 1, 2),
                new LootEntry("dried_meat", 14, 1, 2),
                new LootEntry("cold_coffee", 14, 1, 2),
                new LootEntry("gold_ring", 10, 1, 1),
                new LootEntry("lockpick", 14, 1, 2),
                new LootEntry("revolver", 4, 1, 1)
            };

            _lootTables["military"] = new[]
            {
                new LootEntry("ammo_556", 26, 12, 24),
                new LootEntry("ammo_9mm", 22, 12, 24),
                new LootEntry("ammo_12g", 20, 6, 12),
                new LootEntry("ammo_762", 16, 5, 10),
                new LootEntry("ammo_357", 14, 6, 12),
                new LootEntry("gunpowder", 20, 2, 4),
                new LootEntry("military_ration", 18, 1, 2),
                new LootEntry("medkit", 16, 1, 2),
                new LootEntry("adrenaline", 14, 1, 2),
                new LootEntry("grenade", 12, 1, 2),
                new LootEntry("steel_plate", 14, 1, 2),
                new LootEntry("weapon_parts", 14, 1, 2),
                new LootEntry("armor_tactical", 4, 1, 1)
            };

            _lootTables["basement"] = new[]
            {
                new LootEntry("scrap_metal", 24, 1, 3),
                new LootEntry("wire", 22, 1, 3),
                new LootEntry("fuel", 18, 1, 2),
                new LootEntry("battery", 16, 1, 2),
                new LootEntry("chemicals", 18, 1, 2),
                new LootEntry("gunpowder", 16, 1, 3),
                new LootEntry("duct_tape", 18, 1, 2),
                new LootEntry("ammo_12g", 14, 4, 8),
                new LootEntry("ammo_556", 14, 8, 16),
                new LootEntry("rubber_hose", 14, 1, 2),
                new LootEntry("copper_coil", 12, 1, 2),
                new LootEntry("spark_plug", 10, 1, 1)
            };

            _lootTables["safe"] = new[]
            {
                new LootEntry("ammo_357", 22, 8, 16),
                new LootEntry("ammo_556", 20, 12, 24),
                new LootEntry("ammo_762", 18, 6, 12),
                new LootEntry("medkit", 20, 1, 2),
                new LootEntry("antidote", 12, 1, 1),
                new LootEntry("adrenaline", 16, 1, 2),
                new LootEntry("circuit", 18, 1, 3),
                new LootEntry("weapon_parts", 16, 1, 2),
                new LootEntry("weapon_upgrade_kit", 10, 1, 1),
                new LootEntry("grenade", 12, 1, 2),
                new LootEntry("gold_ring", 14, 1, 2)
            };

            _lootTables["kitchen"] = new[]
            {
                new LootEntry("canned_beans", 24, 1, 2),
                new LootEntry("canned_tomato", 24, 1, 2),
                new LootEntry("canned_tuna", 20, 1, 2),
                new LootEntry("canned_corn", 18, 1, 2),
                new LootEntry("canned_olives", 16, 1, 2),
                new LootEntry("water_bottle", 24, 1, 2),
                new LootEntry("instant_noodles", 18, 1, 2),
                new LootEntry("honey_jar", 12, 1, 1),
                new LootEntry("roasted_nuts", 14, 1, 2),
                new LootEntry("empty_bottle", 16, 1, 2),
                new LootEntry("empty_can", 14, 1, 2),
                new LootEntry("matches_box", 12, 1, 2)
            };

            _lootTables["tools"] = new[]
            {
                new LootEntry("scrap_metal", 26, 2, 4),
                new LootEntry("wood_plank", 22, 2, 3),
                new LootEntry("nails", 24, 2, 5),
                new LootEntry("duct_tape", 22, 1, 3),
                new LootEntry("wire", 22, 1, 3),
                new LootEntry("circuit", 16, 1, 2),
                new LootEntry("battery", 14, 1, 2),
                new LootEntry("fuel", 16, 1, 2),
                new LootEntry("gunpowder", 16, 1, 3),
                new LootEntry("steel_plate", 14, 1, 2),
                new LootEntry("weapon_parts", 14, 1, 2),
                new LootEntry("lockpick", 16, 1, 3),
                new LootEntry("adhesive_epoxy", 14, 1, 2)
            };
        }
    }
}
