// World item pickup spawned on floors or dropped when inventory overflows.
using UnityEngine;

namespace LayeredHell
{
    public sealed class WorldPickup : MonoBehaviour, IInteractable
    {
        public string itemId = string.Empty;
        public int count = 1;

        public static WorldPickup Spawn(string itemId, int count, Vector3 pos)
        {
            if (string.IsNullOrEmpty(itemId) || count <= 0) return null;

            ItemDef def = ItemDatabase.Get(itemId);
            float weight = def != null ? Mathf.Clamp(def.weight, 0.1f, 4f) : 0.3f;
            float side = Mathf.Clamp(0.16f + weight * 0.06f, 0.16f, 0.42f);
            Vector3 fallbackSize = new Vector3(side, side * 0.75f, side);

            Color fallbackColor = new Color(0.72f, 0.58f, 0.32f);
            if (def != null && !string.IsNullOrEmpty(def.iconColorHex))
            {
                if (ColorUtility.TryParseHtmlString(def.iconColorHex, out Color parsed))
                {
                    fallbackColor = parsed;
                }
            }

            string modelId = "loot_" + itemId;
            GameObject go = null;

            if (ModelLibrary.Instance != null && ModelLibrary.Instance.Has(modelId))
            {
                GameObject prefab = ModelLibrary.Instance.Get(modelId);
                if (prefab != null)
                {
                    go = Instantiate(prefab, pos, Quaternion.identity);
                }
            }

            if (go == null)
            {
                go = ModelLibrary.InstantiateOrPlaceholder(modelId, null, fallbackSize, fallbackColor);
                if (go != null)
                {
                    go.transform.position = pos;
                }
            }

            if (go == null)
            {
                go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.transform.position = pos;
                go.transform.localScale = fallbackSize;
            }

            go.name = "Pickup_" + itemId;

            if (go.GetComponentInChildren<Collider>() == null)
            {
                BoxCollider col = go.AddComponent<BoxCollider>();
                col.size = fallbackSize;
            }

            WorldPickup pickup = go.GetComponent<WorldPickup>();
            if (pickup == null) pickup = go.AddComponent<WorldPickup>();
            pickup.itemId = itemId;
            pickup.count = count;

            return pickup;
        }

        public bool CanInteract()
        {
            return !string.IsNullOrEmpty(itemId) && count > 0;
        }

        public string GetPrompt()
        {
            ItemDef def = ItemDatabase.Get(itemId);
            string displayName = def != null ? def.name : itemId;
            if (count > 1)
            {
                return "Al: " + displayName + " x" + count;
            }
            return "Al: " + displayName;
        }

        public void Interact(PlayerController player)
        {
            if (string.IsNullOrEmpty(itemId) || count <= 0) return;

            if (Inventory.Instance == null)
            {
                Destroy(gameObject);
                return;
            }

            int added = Inventory.Instance.Add(itemId, count);
            if (added > 0)
            {
                PlayPickupAudio(itemId);
                count -= added;
                if (count <= 0)
                {
                    Destroy(gameObject);
                }
                else
                {
                    GameEvents.RaiseToast("Envanter dolu! Kalan: " + count);
                }
            }
            else
            {
                AudioManager.Play2D("ui_denied", 0.8f);
                GameEvents.RaiseToast("Envanter çok ağır!");
            }
        }

        private void PlayPickupAudio(string id)
        {
            ItemDef def = ItemDatabase.Get(id);
            string sound = "pickup_metal";
            if (def != null)
            {
                switch (def.type)
                {
                    case ItemType.Weapon: sound = "pickup_weapon"; break;
                    case ItemType.Ammo: sound = "pickup_ammo"; break;
                    case ItemType.Food: sound = id.Contains("canned") ? "pickup_can" : "pickup_food"; break;
                    case ItemType.Drink: sound = "pickup_bottle"; break;
                    case ItemType.Medical: sound = "pickup_med"; break;
                    case ItemType.Key: sound = "pickup_key"; break;
                    case ItemType.Quest: sound = "pickup_paper"; break;
                    default:
                        if (id.Contains("battery")) sound = "pickup_battery";
                        else if (id.Contains("cloth")) sound = "pickup_cloth";
                        else if (id.Contains("wood")) sound = "pickup_wood";
                        else if (id.Contains("chem")) sound = "pickup_chem";
                        break;
                }
            }
            AudioManager.Play(sound, transform.position, 0.85f);
        }
    }
}
