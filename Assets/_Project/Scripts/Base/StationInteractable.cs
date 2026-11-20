// 45: Layered Hell — Interactive safe-house station on Floor 38 (38-B).
using UnityEngine;

namespace LayeredHell
{
    public class StationInteractable : MonoBehaviour, IInteractable
    {
        public string stationId = "workbench";
        public string displayName = "Çalışma Tezgâhı";

        public static StationInteractable Setup(GameObject go, string stationId, string displayName = "")
        {
            if (go == null) return null;

            if (go.GetComponentInChildren<Collider>() == null)
            {
                var box = go.AddComponent<BoxCollider>();
                box.center = new Vector3(0f, 0.6f, 0f);
                box.size = new Vector3(1.2f, 1.2f, 1.0f);
            }

            int interactLayer = LayerMask.NameToLayer(LayerNames.Interactable);
            if (interactLayer >= 0)
                go.layer = interactLayer;

            var st = go.GetComponent<StationInteractable>();
            if (st == null) st = go.AddComponent<StationInteractable>();
            st.stationId = stationId;
            st.displayName = string.IsNullOrEmpty(displayName) ? DefaultDisplayName(stationId) : displayName;
            return st;
        }

        public string GetPrompt()
        {
            bool built = BaseManager.Instance == null || BaseManager.Instance.HasStation(stationId);
            int level = BaseManager.Instance != null ? BaseManager.Instance.GetUpgradeLevel(stationId) : 1;

            switch (stationId)
            {
                case "bed":
                    return "[E] Yatak — Dinlen ve Oyunu Kaydet";
                case "storage":
                    return "[E] Erzak Sandığı — Depo & Envanter (Sv." + Mathf.Max(1, level) + ")";
                case "radio_station":
                    return built
                        ? "[E] 88.7 Kule Radyosu — Telsiz Kayıtları (Sv." + Mathf.Max(1, level) + ")"
                        : "[E] Kırık Telsiz Masası — Onarmak İçin Üs Menüsünü Aç";
                case "generator":
                    return built
                        ? "[E] Balkon Jeneratörü — Güç Aktif (Sv." + level + ") / Yükselt"
                        : "[E] Jeneratör Yuvası — İnşa Etmek İçin Üs Menüsünü Aç";
                default:
                    if (!built)
                        return "[E] " + displayName + " (Kurulu Değil — Üs Menüsünden İnşa Et)";
                    return "[E] " + displayName + " — Üretim Yap (Sv." + Mathf.Max(1, level) + ")";
            }
        }

        public bool CanInteract()
        {
            return true;
        }

        public void Interact(PlayerController player)
        {
            bool built = BaseManager.Instance == null || BaseManager.Instance.HasStation(stationId);

            if (stationId == "bed")
            {
                if (player != null && player.Stats != null)
                {
                    player.Stats.RestoreStamina(100f);
                    if (player.Stats.Hunger > 25f && player.Stats.Thirst > 25f)
                        player.Stats.Heal(20f);
                }
                if (SaveSystem.Instance != null)
                    SaveSystem.Instance.Save(0);

                AudioManager.Play2D("ui_save", 1f);
                GameEvents.RaiseToast("38-B Güvenli Ev: Dinlenildi ve ilerleme kaydedildi.");
                return;
            }

            if (stationId == "storage")
            {
                AudioManager.Play2D("ui_open", 0.9f);
                if (GameMenuUI.Instance != null)
                    GameMenuUI.Instance.Open("Envanter", "storage");
                return;
            }

            if (stationId == "generator")
            {
                AudioManager.Play2D("ui_open", 0.9f);
                if (GameMenuUI.Instance != null)
                    GameMenuUI.Instance.Open("Üs", "generator");
                return;
            }

            if (stationId == "radio_station")
            {
                if (!built)
                {
                    AudioManager.Play2D("radio_static", 0.8f);
                    GameEvents.RaiseToast("Telsiz henüz onarılmadı. Üs sekmesinden onarın veya Nehir'i kurtarın.");
                    if (GameMenuUI.Instance != null)
                        GameMenuUI.Instance.Open("Üs", "radio_station");
                    return;
                }

                AudioManager.Play2D("radio_tune", 0.9f);
                GameEvents.RaiseObjective("radio_repaired");
                if (GameMenuUI.Instance != null)
                    GameMenuUI.Instance.Open("Günlük", "radio");
                return;
            }

            if (!built)
            {
                AudioManager.Play2D("ui_denied", 0.85f);
                GameEvents.RaiseToast(displayName + " henüz kurulmadı. Üs sekmesinden kurabilirsiniz.");
                if (GameMenuUI.Instance != null)
                    GameMenuUI.Instance.Open("Üs", stationId);
                return;
            }

            AudioManager.Play2D("ui_open", 0.9f);
            if (GameMenuUI.Instance != null)
                GameMenuUI.Instance.Open("Üretim", stationId);
        }

        public static string DefaultDisplayName(string id)
        {
            switch (id)
            {
                case "workbench": return "Çalışma Tezgâhı";
                case "stove": return "Derme Çatma Ocak";
                case "purifier": return "Yağmur Suyu Arıtıcısı";
                case "generator": return "Daire Jeneratörü";
                case "radio_station": return "88.7 Kule Radyo İstasyonu";
                case "med_station": return "Tıbbi İstasyon (Dr. Selin)";
                case "weapon_bench": return "Silah Tezgâhı (Kemal Usta)";
                case "chem_station": return "Kimya İstasyonu (Elif)";
                case "storage": return "Erzak Sandığı";
                case "bed": return "Dinlenme Yatağı";
                default: return id;
            }
        }
    }
}
