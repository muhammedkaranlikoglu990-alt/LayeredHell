// 45: Layered Hell — SurvivorNPC: rescuable specialists on tower floors & idle companions at Floor 38 Safe House.
using UnityEngine;

namespace LayeredHell
{
    public class SurvivorNPC : MonoBehaviour, IInteractable
    {
        public string survivorId = "selin";
        public bool inBaseMode;

        private int _dialogueIndex;
        private Animator _animator;

        public static SurvivorNPC Create(string survivorId, Vector3 pos, Quaternion rot)
        {
            bool alreadyRescued = BaseManager.Instance != null && BaseManager.Instance.IsRescued(survivorId);
            int floor = Mathf.RoundToInt(pos.y / GameConstants.FloorHeight);

            var go = new GameObject("Survivor_" + survivorId);
            go.transform.SetPositionAndRotation(pos, rot);

            int interactLayer = LayerMask.NameToLayer(LayerNames.Interactable);
            if (interactLayer >= 0)
                go.layer = interactLayer;

            var col = go.AddComponent<CapsuleCollider>();
            col.center = new Vector3(0f, 0.9f, 0f);
            col.radius = 0.4f;
            col.height = 1.8f;

            string modelId = "chr_" + survivorId + "_01";
            Color fallbackColor = GetSurvivorColor(survivorId);
            var visual = ModelLibrary.InstantiateOrPlaceholder(modelId, go.transform, new Vector3(0.55f, 1.75f, 0.45f), fallbackColor);
            if (visual != null)
            {
                visual.transform.localPosition = Vector3.zero;
                visual.transform.localRotation = Quaternion.identity;
            }

            var npc = go.AddComponent<SurvivorNPC>();
            npc.survivorId = survivorId;
            npc.inBaseMode = (floor == GameConstants.BaseFloor);
            npc._animator = go.GetComponentInChildren<Animator>();

            if (npc._animator != null)
            {
                npc._animator.CrossFadeInFixedTime("Idle", 0.15f);
            }

            // If this spawn is on a field floor (not floor 38) and the survivor was already rescued, hide it.
            if (alreadyRescued && floor != GameConstants.BaseFloor)
            {
                go.SetActive(false);
            }

            return npc;
        }

        private void OnEnable()
        {
            if (_animator == null)
                _animator = GetComponentInChildren<Animator>();
            if (_animator != null)
                _animator.CrossFadeInFixedTime("Idle", 0.15f);
        }

        public string GetPrompt()
        {
            var def = StoryDatabase.GetSurvivor(survivorId);
            string displayName = def != null ? def.name : survivorId;
            string profession = def != null ? def.profession : "Hayatta Kalan";

            bool rescued = BaseManager.Instance != null && BaseManager.Instance.IsRescued(survivorId);
            if (!rescued && !inBaseMode)
                return "[E] " + displayName + " (" + profession + ") — Konuş ve 38-B'ye Kurtar";

            return "[E] " + displayName + " (" + profession + ") — Konuş";
        }

        public bool CanInteract()
        {
            return true;
        }

        public void Interact(PlayerController player)
        {
            var def = StoryDatabase.GetSurvivor(survivorId);
            string speaker = def != null ? def.name : survivorId;
            bool rescued = BaseManager.Instance != null && BaseManager.Instance.IsRescued(survivorId);

            if (!rescued && !inBaseMode)
            {
                string intro = "Tanrım, sonunda yaşayan biri! 38. katta güvenli bir dairen mi var? Hemen oraya çıkalım!";
                if (def != null && def.introLines != null && def.introLines.Length > 0)
                {
                    intro = def.introLines[_dialogueIndex % def.introLines.Length];
                }
                _dialogueIndex++;

                AudioManager.Play2D("radio_voice", 0.95f);
                GameEvents.RaiseRadio(speaker, intro);

                if (BaseManager.Instance != null)
                {
                    BaseManager.Instance.Rescue(survivorId);
                }

                // Transition away from field floor to Floor 38 Safe House
                int currentFloor = Mathf.RoundToInt(transform.position.y / GameConstants.FloorHeight);
                if (currentFloor != GameConstants.BaseFloor)
                {
                    inBaseMode = true;
                    gameObject.SetActive(false);
                }
                return;
            }

            // Already rescued / talking at Floor 38 Safe House
            string line = "38-B barikatları dayandığı sürece buradayız, Aras. Tezgâhı istediğin zaman kullanabilirsin.";
            if (def != null && def.rescuedLines != null && def.rescuedLines.Length > 0)
            {
                line = def.rescuedLines[_dialogueIndex % def.rescuedLines.Length];
            }
            _dialogueIndex++;

            AudioManager.Play2D("ui_click", 0.85f);
            GameEvents.RaiseRadio(speaker, line);

            string station = GetAssociatedStation(survivorId);
            if (!string.IsNullOrEmpty(station) && GameMenuUI.Instance != null && _dialogueIndex > 1)
            {
                if (station == "radio_station")
                    GameMenuUI.Instance.Open("Günlük", "radio");
                else
                    GameMenuUI.Instance.Open("Üretim", station);
            }
        }

        private static string GetAssociatedStation(string id)
        {
            switch (id)
            {
                case "selin": return "med_station";
                case "kemal": return "weapon_bench";
                case "elif": return "chem_station";
                case "nehir": return "radio_station";
                default: return "";
            }
        }

        private static Color GetSurvivorColor(string id)
        {
            switch (id)
            {
                case "selin": return new Color(0.25f, 0.72f, 0.82f);
                case "kemal": return new Color(0.75f, 0.45f, 0.22f);
                case "elif": return new Color(0.35f, 0.82f, 0.45f);
                case "nehir": return new Color(0.65f, 0.45f, 0.85f);
                case "murat": return new Color(0.28f, 0.40f, 0.65f);
                case "tarik": return new Color(0.78f, 0.62f, 0.24f);
                default: return new Color(0.6f, 0.6f, 0.65f);
            }
        }
    }
}
