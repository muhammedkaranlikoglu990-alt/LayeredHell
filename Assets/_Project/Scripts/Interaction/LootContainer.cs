// Searchable loot container rolling deterministic items from ItemDatabase and dropping overflow as WorldPickups.
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LayeredHell
{
    public sealed class LootContainer : MonoBehaviour, IInteractable, ISaveable
    {
        public string lootTable = "residential";
        public int seed;
        public string containerName = "Dolap";
        public bool opened;

        [Serializable]
        private struct SaveData
        {
            public bool opened;
        }

        public string SaveKey
        {
            get
            {
                Vector3 p = transform.position;
                int hx = Mathf.RoundToInt(p.x * 10f);
                int hy = Mathf.RoundToInt(p.y * 10f);
                int hz = Mathf.RoundToInt(p.z * 10f);
                return "loot_" + seed + "_" + hx + "_" + hy + "_" + hz;
            }
        }

        public static LootContainer Setup(GameObject go, string table, int seed, string name = "Dolap")
        {
            if (go == null) return null;

            if (go.GetComponentInChildren<Collider>() == null)
            {
                BoxCollider box = go.AddComponent<BoxCollider>();
                box.center = new Vector3(0f, 0.55f, 0f);
                box.size = new Vector3(0.9f, 1.1f, 0.6f);
            }

            LootContainer container = go.GetComponent<LootContainer>();
            if (container == null) container = go.AddComponent<LootContainer>();
            container.lootTable = string.IsNullOrEmpty(table) ? "residential" : table;
            container.seed = seed;
            container.containerName = string.IsNullOrEmpty(name) ? "Dolap" : name;
            return container;
        }

        private void OnEnable()
        {
            if (SaveSystem.Instance != null)
            {
                SaveSystem.Instance.Register(this);
            }
        }

        private void Start()
        {
            if (SaveSystem.Instance != null)
            {
                SaveSystem.Instance.Register(this);
            }
        }

        private void OnDisable()
        {
            if (SaveSystem.Instance != null)
            {
                SaveSystem.Instance.Unregister(this);
            }
        }

        public bool CanInteract()
        {
            return !opened;
        }

        public string GetPrompt()
        {
            return opened ? (containerName + " (Boş)") : ("Ara: " + containerName);
        }

        public void Interact(PlayerController player)
        {
            if (opened) return;
            opened = true;

            AudioManager.Play("door_creak", transform.position, 0.65f);

            List<ItemStack> items = ItemDatabase.RollLoot(lootTable, new System.Random(seed));
            if (items == null || items.Count == 0)
            {
                GameEvents.RaiseToast(containerName + " boş çıktı.");
                return;
            }

            StringBuilder summary = new StringBuilder();
            int dropIndex = 0;

            for (int i = 0; i < items.Count; i++)
            {
                ItemStack stack = items[i];
                if (string.IsNullOrEmpty(stack.id) || stack.count <= 0) continue;

                int added = Inventory.Instance != null ? Inventory.Instance.Add(stack.id, stack.count) : 0;
                int overflow = stack.count - added;

                if (overflow > 0)
                {
                    Vector3 offset = new Vector3((dropIndex % 3 - 1) * 0.22f, 0.65f, 0.35f + (dropIndex / 3) * 0.2f);
                    WorldPickup.Spawn(stack.id, overflow, transform.position + transform.TransformDirection(offset));
                    dropIndex++;
                }

                ItemDef def = ItemDatabase.Get(stack.id);
                string itemName = def != null ? def.name : stack.id;
                if (summary.Length > 0) summary.Append(", ");
                summary.Append(itemName).Append(" x").Append(stack.count);
            }

            AudioManager.Play2D("ui_open", 0.7f);
            if (summary.Length > 0)
            {
                GameEvents.RaiseToast("Bulundu: " + summary);
            }
        }

        public string CaptureState()
        {
            return JsonUtility.ToJson(new SaveData { opened = opened });
        }

        public void RestoreState(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            opened = data.opened;
        }
    }
}
