// 45: Layered Hell — StoryInteractables: JournalPickup, WallText, and ObjectiveInteractable.
using System;
using UnityEngine;

namespace LayeredHell
{
    public class JournalPickup : MonoBehaviour, IInteractable
    {
        public string journalId = "";

        public static JournalPickup Create(string journalId, Vector3 pos)
        {
            return Create(journalId, pos, Quaternion.identity);
        }

        public static JournalPickup Create(string journalId, Vector3 pos, Quaternion rot)
        {
            var go = new GameObject("JournalPickup_" + journalId);
            go.transform.SetPositionAndRotation(pos, rot);

            int interactLayer = LayerMask.NameToLayer(LayerNames.Interactable);
            if (interactLayer >= 0)
                go.layer = interactLayer;

            var col = go.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, 0.08f, 0f);
            col.size = new Vector3(0.45f, 0.25f, 0.45f);

            var visual = ModelLibrary.InstantiateOrPlaceholder(
                "loot_journal_01",
                go.transform,
                new Vector3(0.28f, 0.05f, 0.36f),
                new Color(0.88f, 0.72f, 0.28f));

            if (visual != null)
            {
                visual.transform.localPosition = new Vector3(0f, 0.03f, 0f);
            }

            var jp = go.AddComponent<JournalPickup>();
            jp.journalId = journalId;

            if (JournalSystem.Instance != null && JournalSystem.Instance.IsCollected(journalId))
            {
                go.SetActive(false);
            }

            return jp;
        }

        public string GetPrompt()
        {
            var entry = StoryDatabase.GetJournal(journalId);
            if (entry != null)
                return "[E] Oku ve Al: " + entry.title + " (" + entry.author + ")";
            return "[E] Notu Oku";
        }

        public bool CanInteract()
        {
            return true;
        }

        public void Interact(PlayerController player)
        {
            if (JournalSystem.Instance != null)
            {
                JournalSystem.Instance.Collect(journalId);
            }

            AudioManager.Play2D("pickup_paper", 0.95f);
            AudioManager.Play2D("ui_journal", 0.9f);
            gameObject.SetActive(false);
        }
    }

    public class WallText : MonoBehaviour, IInteractable
    {
        public string text = "";

        public static WallText Create(string text, Vector3 pos, Quaternion rot)
        {
            var go = new GameObject("WallText");
            go.transform.SetPositionAndRotation(pos, rot);

            int interactLayer = LayerMask.NameToLayer(LayerNames.Interactable);
            if (interactLayer >= 0)
                go.layer = interactLayer;

            var col = go.AddComponent<BoxCollider>();
            col.center = Vector3.zero;
            col.size = new Vector3(1.8f, 0.8f, 0.15f);

            // Subtle dark graffiti backing plate
            var backQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            backQuad.name = "GraffitiBacking";
            backQuad.transform.SetParent(go.transform, false);
            backQuad.transform.localPosition = new Vector3(0f, 0f, -0.01f);
            backQuad.transform.localScale = new Vector3(1.75f, 0.72f, 1f);
            var qCol = backQuad.GetComponent<Collider>();
            if (qCol != null) Destroy(qCol);
            var qRend = backQuad.GetComponent<Renderer>();
            if (qRend != null && qRend.material != null)
            {
                qRend.material.color = new Color(0.06f, 0.03f, 0.03f, 0.85f);
            }

            // World-space 3D TextMesh readable up close
            var textGo = new GameObject("TextMesh");
            textGo.transform.SetParent(go.transform, false);
            textGo.transform.localPosition = new Vector3(0f, 0f, -0.02f);
            textGo.transform.localScale = new Vector3(0.045f, 0.045f, 0.045f);

            var tm = textGo.AddComponent<TextMesh>();
            tm.text = WrapWallText(text, 34);
            tm.fontSize = 36;
            tm.characterSize = 1f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = new Color(0.86f, 0.18f, 0.14f, 0.96f);

            var wt = go.AddComponent<WallText>();
            wt.text = text;
            return wt;
        }

        public string GetPrompt()
        {
            return "Duvar Yazısı: “" + text + "”";
        }

        public bool CanInteract()
        {
            return true;
        }

        public void Interact(PlayerController player)
        {
            GameEvents.RaiseToast("Duvar Yazısı: “" + text + "”");
        }

        private static string WrapWallText(string raw, int maxCharsPerLine)
        {
            if (string.IsNullOrEmpty(raw) || raw.Length <= maxCharsPerLine)
                return raw ?? "";

            string[] words = raw.Split(' ');
            var sb = new System.Text.StringBuilder();
            int lineLen = 0;

            for (int i = 0; i < words.Length; i++)
            {
                string w = words[i];
                if (lineLen > 0 && lineLen + 1 + w.Length > maxCharsPerLine)
                {
                    sb.Append('\n');
                    lineLen = 0;
                }
                else if (lineLen > 0)
                {
                    sb.Append(' ');
                    lineLen++;
                }
                sb.Append(w);
                lineLen += w.Length;
            }
            return sb.ToString();
        }
    }

    public class ObjectiveInteractable : MonoBehaviour, IInteractable, ISaveable
    {
        public ObjectivePlacement placement;
        public bool completed;

        public string SaveKey => "obj_" + (placement != null && !string.IsNullOrEmpty(placement.key) ? placement.key : name);

        public static ObjectiveInteractable Create(ObjectivePlacement p, Vector3 pos, Quaternion rot)
        {
            if (p == null) return null;

            var go = new GameObject("Objective_" + p.key);
            go.transform.SetPositionAndRotation(pos, rot);

            int interactLayer = LayerMask.NameToLayer(LayerNames.Interactable);
            if (interactLayer >= 0)
                go.layer = interactLayer;

            var col = go.AddComponent<BoxCollider>();
            col.center = new Vector3(0f, 0.65f, 0f);
            col.size = p.anchorHint == "vehicle" ? new Vector3(2.4f, 1.8f, 4.2f) : new Vector3(1.1f, 1.3f, 1.1f);

            string modelId = !string.IsNullOrEmpty(p.modelId) ? p.modelId : GetDefaultModelForHint(p.anchorHint);
            Vector3 fallbackSize = p.anchorHint == "vehicle" ? new Vector3(2.2f, 1.7f, 4.0f) : new Vector3(0.85f, 1.1f, 0.65f);
            Color fallbackColor = GetColorForHint(p.anchorHint);

            var visual = ModelLibrary.InstantiateOrPlaceholder(modelId, go.transform, fallbackSize, fallbackColor);
            if (visual != null)
            {
                visual.transform.localPosition = Vector3.zero;
                visual.transform.localRotation = Quaternion.identity;
            }

            var obj = go.AddComponent<ObjectiveInteractable>();
            obj.placement = p;

            if (QuestManager.Instance != null && QuestManager.Instance.HasFlag(p.key))
            {
                obj.completed = true;
            }

            return obj;
        }

        private void Start()
        {
            if (SaveSystem.Instance != null)
                SaveSystem.Instance.Register(this);

            if (!completed && placement != null && QuestManager.Instance != null && QuestManager.Instance.HasFlag(placement.key))
                completed = true;
        }

        private void OnDestroy()
        {
            if (SaveSystem.Instance != null)
                SaveSystem.Instance.Unregister(this);
        }

        public string GetPrompt()
        {
            if (placement == null) return "";
            if (completed || (QuestManager.Instance != null && QuestManager.Instance.HasFlag(placement.key)))
                return "[Tamamlandı] " + placement.prompt;

            return "[E] " + placement.prompt;
        }

        public bool CanInteract()
        {
            if (placement == null) return false;
            if (completed) return false;
            if (QuestManager.Instance != null && QuestManager.Instance.HasFlag(placement.key))
            {
                completed = true;
                return false;
            }
            return true;
        }

        public void Interact(PlayerController player)
        {
            if (!CanInteract()) return;

            // 1. Check required objective flags
            if (placement.requiresFlags != null && QuestManager.Instance != null)
            {
                for (int i = 0; i < placement.requiresFlags.Length; i++)
                {
                    string flag = placement.requiresFlags[i];
                    if (!string.IsNullOrEmpty(flag) && !QuestManager.Instance.HasFlag(flag))
                    {
                        AudioManager.Play2D("ui_denied", 0.9f);
                        GameEvents.RaiseToast("Önce şu adımı tamamlamalısınız: " + DescribeFlag(flag));
                        return;
                    }
                }
            }

            // 2. Check required inventory items
            if (placement.requires != null && placement.requires.Length > 0)
            {
                if (Inventory.Instance == null) return;
                for (int i = 0; i < placement.requires.Length; i++)
                {
                    var req = placement.requires[i];
                    if (!Inventory.Instance.Has(req.id, req.count))
                    {
                        var def = ItemDatabase.Get(req.id);
                        int have = Inventory.Instance.Count(req.id);
                        AudioManager.Play2D("ui_denied", 0.9f);
                        GameEvents.RaiseToast("Gerekli eşya eksik: " + def.name + " (" + have + "/" + req.count + ")");
                        return;
                    }
                }

                if (placement.consumeItems)
                {
                    for (int i = 0; i < placement.requires.Length; i++)
                    {
                        var req = placement.requires[i];
                        Inventory.Instance.Remove(req.id, req.count);
                    }
                }
            }

            // 3. Grant reward/quest items
            if (placement.grants != null && Inventory.Instance != null)
            {
                for (int i = 0; i < placement.grants.Length; i++)
                {
                    var g = placement.grants[i];
                    if (!string.IsNullOrEmpty(g.id) && g.count > 0)
                    {
                        int added = Inventory.Instance.Add(g.id, g.count);
                        if (added < g.count && player != null)
                        {
                            WorldPickup.Spawn(g.id, g.count - added, player.Position + Vector3.up * 0.5f);
                        }
                    }
                }
            }

            // 4. Mark complete, notify audio/toast, and raise objective event
            completed = true;
            AudioManager.Play2D("ui_confirm", 1f);

            if (!string.IsNullOrEmpty(placement.code))
            {
                AudioManager.Play2D("radio_beep", 0.9f);
            }

            if (!string.IsNullOrEmpty(placement.revealsMessage))
            {
                GameEvents.RaiseToast(placement.revealsMessage);
            }

            if (!string.IsNullOrEmpty(placement.key))
            {
                GameEvents.RaiseObjective(placement.key);
            }
        }

        private static string DescribeFlag(string flag)
        {
            switch (flag)
            {
                case "radar_disabled": return "45. Kat Hava Savunma Radarını Devre Dışı Bırak";
                case "exec_keycard_1": return "39. Kat Yönetici Kartı 1'i Al (1704)";
                case "exec_keycard_2": return "41. Kat Yönetici Kartı 2'yi Al (2611)";
                case "exec_keycard_3": return "44. Kat Yönetici Kartı 3'ü Al (4538)";
                case "murat_rescued": return "8. Kattaki Güvenlik Görevlisi Murat Şen ile Konuş";
                case "tarik_rescued": return "B1 Otoparkındaki Tamirci Tarık Aydın ile Konuş";
                case "lab_unlocked": return "B1 Chronos Laboratuvar Kapısını Aç";
                case "vehicle_repaired": return "B1 Otoparkındaki Zırhlı Müdahale Aracını Onar";
                case "goliath_defeated": return "B4 Katındaki Denek-0 Goliath'ı Etkisiz Hale Getir";
                default: return flag;
            }
        }

        private static string GetDefaultModelForHint(string hint)
        {
            switch (hint)
            {
                case "safe": return "exe_safe_01";
                case "terminal": return "off_monitor_01";
                case "console": return "exe_radar_01";
                case "locker": return "off_cabinet_01";
                case "vehicle": return "veh_armored_response_01";
                case "vent": return "arch_vent_01";
                case "desk": return "off_desk_01";
                default: return "off_monitor_01";
            }
        }

        private static Color GetColorForHint(string hint)
        {
            switch (hint)
            {
                case "safe": return new Color(0.75f, 0.62f, 0.22f);
                case "terminal": return new Color(0.22f, 0.65f, 0.85f);
                case "console": return new Color(0.85f, 0.35f, 0.20f);
                case "vehicle": return new Color(0.25f, 0.32f, 0.28f);
                default: return new Color(0.55f, 0.58f, 0.62f);
            }
        }

        [Serializable]
        private class SaveData
        {
            public bool completed;
        }

        public string CaptureState()
        {
            return JsonUtility.ToJson(new SaveData { completed = completed });
        }

        public void RestoreState(string json)
        {
            if (string.IsNullOrEmpty(json)) return;
            var data = JsonUtility.FromJson<SaveData>(json);
            if (data != null)
                completed = data.completed;
        }
    }
}
