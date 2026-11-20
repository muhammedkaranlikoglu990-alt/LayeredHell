// 45: Layered Hell — Inventory & Storage Chest management (MonoBehaviour, ISaveable).
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LayeredHell
{
    public class Inventory : MonoBehaviour, ISaveable
    {
        public static Inventory Instance { get; private set; }

        public const float BaseMaxWeight = 40f;

        private readonly List<ItemStack> _items = new List<ItemStack>();
        private readonly List<ItemStack> _storage = new List<ItemStack>();
        private float _extraCapacity;
        private bool _suppressEvents;

        public IReadOnlyList<ItemStack> Items => _items;
        public IReadOnlyList<ItemStack> Storage => _storage;

        public string EquippedWeaponId { get; private set; } = "";
        public string EquippedArmorId { get; private set; } = "";

        public event Action Changed;
        public event Action<string> WeaponEquipped;

        public float Weight
        {
            get
            {
                float total = 0f;
                for (int i = 0; i < _items.Count; i++)
                {
                    var def = ItemDatabase.Get(_items[i].id);
                    total += def.weight * _items[i].count;
                }
                return Mathf.Round(total * 100f) / 100f;
            }
        }

        public float MaxWeight => BaseMaxWeight + _extraCapacity;

        public float ArmorReduction
        {
            get
            {
                if (string.IsNullOrEmpty(EquippedArmorId)) return 0f;
                if (!Has(EquippedArmorId, 1)) return 0f;
                var def = ItemDatabase.Get(EquippedArmorId);
                return Mathf.Clamp(def.armorReduction, 0f, 0.8f);
            }
        }

        public string SaveKey => "inventory";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            if (SaveSystem.Instance != null)
                SaveSystem.Instance.Register(this);

            if (_items.Count == 0 && string.IsNullOrEmpty(EquippedWeaponId))
                ResetForNewGame();
        }

        private void OnDestroy()
        {
            if (SaveSystem.Instance != null)
                SaveSystem.Instance.Unregister(this);
            if (Instance == this)
                Instance = null;
        }

        public void SetExtraCapacity(float extraKg)
        {
            _extraCapacity = Mathf.Max(0f, extraKg);
            Changed?.Invoke();
        }

        public int Count(string id)
        {
            if (string.IsNullOrEmpty(id)) return 0;
            int total = 0;
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i].id == id)
                    total += _items[i].count;
            }
            return total;
        }

        public int StorageCount(string id)
        {
            if (string.IsNullOrEmpty(id)) return 0;
            int total = 0;
            for (int i = 0; i < _storage.Count; i++)
            {
                if (_storage[i].id == id)
                    total += _storage[i].count;
            }
            return total;
        }

        public bool Has(string id, int n = 1)
        {
            if (n <= 0) return true;
            return Count(id) >= n;
        }

        public int Add(string id, int n = 1)
        {
            if (string.IsNullOrEmpty(id) || n <= 0) return 0;

            var def = ItemDatabase.Get(id);
            int canAdd = n;

            // Key & Quest items always fit to prevent quest soft-locks; other items respect MaxWeight.
            if (def.type != ItemType.Key && def.type != ItemType.Quest && def.weight > 0.0001f)
            {
                float remainingWeight = MaxWeight - Weight;
                if (remainingWeight <= 0.0001f)
                {
                    if (!_suppressEvents)
                        GameEvents.RaiseToast("Envanter dolu! Ağırlık sınırına ulaşıldı.");
                    return 0;
                }
                int maxByWeight = Mathf.FloorToInt((remainingWeight + 0.001f) / def.weight);
                canAdd = Mathf.Clamp(n, 0, maxByWeight);
                if (canAdd <= 0)
                {
                    if (!_suppressEvents)
                        GameEvents.RaiseToast("Çok ağır: " + def.name);
                    return 0;
                }
            }

            AddToList(_items, id, canAdd);

            if (def.type == ItemType.Armor && string.IsNullOrEmpty(EquippedArmorId))
            {
                EquippedArmorId = id;
            }
            else if (ItemDatabase.GetWeapon(id) != null && string.IsNullOrEmpty(EquippedWeaponId))
            {
                EquipWeapon(id);
            }

            if (!_suppressEvents)
            {
                GameEvents.RaiseItemAdded(id, canAdd);
                Changed?.Invoke();
            }

            return canAdd;
        }

        public bool Remove(string id, int n = 1)
        {
            if (string.IsNullOrEmpty(id) || n <= 0) return false;
            if (Count(id) < n) return false;

            RemoveFromList(_items, id, n);

            if (EquippedArmorId == id && Count(id) <= 0)
            {
                EquippedArmorId = "";
            }

            if (EquippedWeaponId == id && Count(id) <= 0)
            {
                string fallback = FindFirstAvailableWeapon();
                EquipWeapon(fallback);
            }

            if (!_suppressEvents)
            {
                GameEvents.RaiseItemRemoved(id, n);
                Changed?.Invoke();
            }

            return true;
        }

        public bool Use(string id)
        {
            if (string.IsNullOrEmpty(id) || !Has(id, 1)) return false;

            var def = ItemDatabase.Get(id);
            var weaponDef = ItemDatabase.GetWeapon(id);

            if (weaponDef != null || def.type == ItemType.Weapon || def.type == ItemType.Throwable)
            {
                EquipWeapon(id);
                AudioManager.Play2D("ui_equip", 0.9f);
                return true;
            }

            if (def.type == ItemType.Armor)
            {
                if (EquippedArmorId == id)
                {
                    EquippedArmorId = "";
                    GameEvents.RaiseToast(def.name + " çıkarıldı.");
                }
                else
                {
                    EquippedArmorId = id;
                    GameEvents.RaiseToast(def.name + " kuşanıldı (%" + Mathf.RoundToInt(def.armorReduction * 100f) + " koruma).");
                }
                AudioManager.Play2D("ui_equip", 0.9f);
                Changed?.Invoke();
                return true;
            }

            if (id == "weapon_upgrade_kit")
            {
                if (string.IsNullOrEmpty(EquippedWeaponId))
                {
                    GameEvents.RaiseToast("Önce geliştirilecek bir silah kuşanın.");
                    return false;
                }
                var eqWpn = ItemDatabase.GetWeapon(EquippedWeaponId);
                if (eqWpn == null || eqWpn.upgradeLevel >= 3)
                {
                    GameEvents.RaiseToast("Bu silah zaten maksimum seviyede.");
                    return false;
                }
                Remove(id, 1);
                ItemDatabase.UpgradeWeaponStat(EquippedWeaponId);
                AudioManager.Play2D("ui_upgrade", 1f);
                GameEvents.RaiseToast(eqWpn.displayName + " geliştirildi (Seviye " + eqWpn.upgradeLevel + ").");
                Changed?.Invoke();
                return true;
            }

            bool hasConsumableEffect =
                def.type == ItemType.Food ||
                def.type == ItemType.Drink ||
                def.type == ItemType.Medical ||
                Mathf.Abs(def.heal) > 0.01f ||
                Mathf.Abs(def.food) > 0.01f ||
                Mathf.Abs(def.water) > 0.01f ||
                Mathf.Abs(def.infectionReduce) > 0.01f ||
                Mathf.Abs(def.staminaRestore) > 0.01f;

            if (!hasConsumableEffect)
            {
                GameEvents.RaiseToast(def.name + " doğrudan kullanılamaz.");
                return false;
            }

            var stats = PlayerController.Instance != null ? PlayerController.Instance.Stats : null;
            if (stats != null)
            {
                if (def.heal > 0f) stats.Heal(def.heal);
                else if (def.heal < 0f)
                    stats.TakeDamage(new DamageInfo(-def.heal, DamageType.Infection, gameObject));

                if (def.food > 0f) stats.Eat(def.food);
                if (def.water > 0f) stats.Drink(def.water);

                if (def.infectionReduce > 0f) stats.ReduceInfection(def.infectionReduce);
                else if (def.infectionReduce < 0f) stats.AddInfection(-def.infectionReduce);

                if (def.staminaRestore > 0f) stats.RestoreStamina(def.staminaRestore);
            }

            Remove(id, 1);

            if (def.type == ItemType.Drink)
                AudioManager.Play2D("drink", 0.9f);
            else if (def.type == ItemType.Food)
                AudioManager.Play2D("eat", 0.9f);
            else if (id == "antidote" || id == "adrenaline" || id == "morphine_ampoule")
                AudioManager.Play2D("inject", 0.95f);
            else
                AudioManager.Play2D("bandage", 0.9f);

            GameEvents.RaiseToast(def.name + " kullanıldı.");
            return true;
        }

        public void EquipWeapon(string id)
        {
            if (!string.IsNullOrEmpty(id) && !Has(id, 1))
                return;

            EquippedWeaponId = id ?? "";
            if (PlayerController.Instance != null && PlayerController.Instance.Weapons != null)
            {
                if (string.IsNullOrEmpty(EquippedWeaponId))
                    PlayerController.Instance.Weapons.Unequip();
                else
                    PlayerController.Instance.Weapons.Equip(EquippedWeaponId);
            }

            WeaponEquipped?.Invoke(EquippedWeaponId);
            Changed?.Invoke();
        }

        public bool MoveToStorage(string id, int n = 1)
        {
            if (string.IsNullOrEmpty(id) || n <= 0) return false;
            int available = Count(id);
            int toMove = Mathf.Min(n, available);
            if (toMove <= 0) return false;

            if (!Remove(id, toMove)) return false;
            AddToList(_storage, id, toMove);
            Changed?.Invoke();
            return true;
        }

        public bool MoveFromStorage(string id, int n = 1)
        {
            if (string.IsNullOrEmpty(id) || n <= 0) return false;
            int inStorage = StorageCount(id);
            int toMove = Mathf.Min(n, inStorage);
            if (toMove <= 0) return false;

            int added = Add(id, toMove);
            if (added <= 0) return false;

            RemoveFromList(_storage, id, added);
            Changed?.Invoke();
            return true;
        }

        public void AddToStorageDirect(string id, int count)
        {
            if (string.IsNullOrEmpty(id) || count <= 0) return;
            AddToList(_storage, id, count);
            Changed?.Invoke();
        }

        public void ResetForNewGame()
        {
            _suppressEvents = true;
            _items.Clear();
            _storage.Clear();
            _extraCapacity = 0f;
            EquippedWeaponId = "";
            EquippedArmorId = "";

            // Starting kit per CONTRACT §4
            Add("pipe", 1);
            Add("pistol", 1);
            Add("ammo_9mm", 24);
            Add("bandage", 2);
            Add("canned_beans", 1);
            Add("water_bottle", 1);

            _suppressEvents = false;
            EquipWeapon("pistol");
            Changed?.Invoke();
        }

        private string FindFirstAvailableWeapon()
        {
            for (int i = 0; i < _items.Count; i++)
            {
                if (ItemDatabase.GetWeapon(_items[i].id) != null && _items[i].count > 0)
                    return _items[i].id;
            }
            return "";
        }

        private static void AddToList(List<ItemStack> list, string id, int count)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].id == id)
                {
                    list[i] = new ItemStack(id, list[i].count + count);
                    return;
                }
            }
            list.Add(new ItemStack(id, count));
        }

        private static void RemoveFromList(List<ItemStack> list, string id, int count)
        {
            int remaining = count;
            for (int i = list.Count - 1; i >= 0 && remaining > 0; i--)
            {
                if (list[i].id != id) continue;
                if (list[i].count <= remaining)
                {
                    remaining -= list[i].count;
                    list.RemoveAt(i);
                }
                else
                {
                    list[i] = new ItemStack(id, list[i].count - remaining);
                    remaining = 0;
                }
            }
        }

        [Serializable]
        private class SaveData
        {
            public List<ItemStack> items = new List<ItemStack>();
            public List<ItemStack> storage = new List<ItemStack>();
            public string equippedWeaponId = "";
            public string equippedArmorId = "";
            public float extraCapacity = 0f;
        }

        public string CaptureState()
        {
            var data = new SaveData
            {
                items = new List<ItemStack>(_items),
                storage = new List<ItemStack>(_storage),
                equippedWeaponId = EquippedWeaponId,
                equippedArmorId = EquippedArmorId,
                extraCapacity = _extraCapacity
            };
            return JsonUtility.ToJson(data);
        }

        public void RestoreState(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            var data = JsonUtility.FromJson<SaveData>(json);
            if (data == null) return;

            _items.Clear();
            if (data.items != null) _items.AddRange(data.items);

            _storage.Clear();
            if (data.storage != null) _storage.AddRange(data.storage);

            _extraCapacity = data.extraCapacity;
            EquippedArmorId = data.equippedArmorId ?? "";
            EquipWeapon(data.equippedWeaponId ?? "");
            Changed?.Invoke();
        }
    }
}
