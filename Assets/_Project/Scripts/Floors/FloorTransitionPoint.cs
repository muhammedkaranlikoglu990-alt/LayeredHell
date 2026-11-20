// 45: Layered Hell - Interactive stairwell & elevator transition points between floors.
using UnityEngine;

namespace LayeredHell
{
    public class FloorTransitionPoint : MonoBehaviour, IInteractable
    {
        public int targetFloor;
        public bool elevator;
        public bool requiresPower;

        private bool _isTransitioning;

        public static FloorTransitionPoint Setup(GameObject go, int targetFloor, bool elevator)
        {
            if (go == null) return null;

            int interactLayer = LayerMask.NameToLayer(LayerNames.Interactable);
            if (interactLayer >= 0) go.layer = interactLayer;

            Collider col = go.GetComponent<Collider>();
            if (col == null)
            {
                BoxCollider box = go.AddComponent<BoxCollider>();
                box.size = new Vector3(1.6f, 2.4f, 1.6f);
                box.center = new Vector3(0f, 1.2f, 0f);
            }

            FloorTransitionPoint tp = go.GetComponent<FloorTransitionPoint>();
            if (tp == null) tp = go.AddComponent<FloorTransitionPoint>();

            tp.targetFloor = Mathf.Clamp(targetFloor, GameConstants.MinFloor, GameConstants.MaxFloor);
            tp.elevator = elevator;
            tp.requiresPower = elevator;
            return tp;
        }

        public string GetPrompt()
        {
            string label = GameConstants.FloorLabel(targetFloor);
            FloorInfo info = FloorCatalog.Get(targetFloor);
            string title = info != null ? info.name : label;

            if (elevator)
            {
                if (!HasElevatorPower())
                {
                    return "Asansör [" + title + "] (Güç Yok - Jeneratör Gerekli)";
                }
                return "Asansörle Git: " + title;
            }

            if (targetFloor == GameConstants.MaxFloor && !IsRoofUnlocked())
            {
                return "Çatı Kapısı [" + title + "] (Kilitli - Çatı Anahtarı / Panel Gerekli)";
            }

            if (targetFloor <= -2 && !IsLabUnlocked())
            {
                return "Laboratuvar Merdiveni [" + title + "] (Kilitli - Laboratuvar Kapısı Açılmalı)";
            }

            string dir = (FloorStreamer.Instance != null && targetFloor > FloorStreamer.Instance.CurrentFloor)
                ? "Yukarı Çık"
                : "Aşağı İn";
            return dir + ": " + title;
        }

        public bool CanInteract()
        {
            if (_isTransitioning) return false;
            if (FloorStreamer.Instance != null && FloorStreamer.Instance.IsBusy) return false;
            return true;
        }

        public void Interact(PlayerController player)
        {
            if (_isTransitioning || player == null) return;

            if (elevator && !HasElevatorPower())
            {
                AudioManager.Play("door_locked", transform.position);
                GameEvents.RaiseToast("Asansör çalışmıyor. Jeneratör gücü gerekli.");
                return;
            }

            if (targetFloor == GameConstants.MaxFloor && !IsRoofUnlocked())
            {
                AudioManager.Play("door_locked", transform.position);
                GameEvents.RaiseToast("Çatı güvenlik kapısı kilitli. 44. kattaki panelden kilidi açmalısın.");
                return;
            }

            if (targetFloor <= -2 && !IsLabUnlocked())
            {
                AudioManager.Play("door_locked", transform.position);
                GameEvents.RaiseToast("Chronos Bio-Labs alt kat geçişi kilitli. B1 güvenlik kapısını açmalısın.");
                return;
            }

            if (FloorStreamer.Instance == null) return;

            _isTransitioning = true;
            player.SetInputLocked(true);

            if (elevator)
            {
                AudioManager.Play("elevator_door", transform.position);
                AudioManager.Play2D("elevator_move", 0.85f);
            }
            else
            {
                AudioManager.Play("step_metalstair_walk", transform.position);
            }

            int dest = Mathf.Clamp(targetFloor, GameConstants.MinFloor, GameConstants.MaxFloor);
            FloorStreamer.Instance.RequestFloor(dest, chunk =>
            {
                _isTransitioning = false;
                if (player != null)
                {
                    Vector3 spawnPos = FloorStreamer.Instance != null
                        ? FloorStreamer.Instance.GetPlayerSpawn(dest)
                        : new Vector3(0f, GameConstants.FloorY(dest) + 0.15f, 0f);
                    Quaternion spawnRot = chunk != null && chunk.PlayerSpawn != null
                        ? chunk.PlayerSpawn.rotation
                        : Quaternion.identity;

                    player.Teleport(spawnPos, spawnRot);
                    player.SetInputLocked(false);

                    if (elevator)
                    {
                        AudioManager.Play2D("elevator_ding", 0.9f);
                    }
                }
            });
        }

        private bool HasElevatorPower()
        {
            if (!requiresPower) return true;
            if (QuestManager.Instance != null && QuestManager.Instance.HasFlag("generator_online")) return true;
            if (BaseManager.Instance != null && BaseManager.Instance.GetUpgradeLevel("generator") > 0) return true;
            return false;
        }

        private static bool IsRoofUnlocked()
        {
            if (QuestManager.Instance != null && QuestManager.Instance.HasFlag("roof_unlocked")) return true;
            if (Inventory.Instance != null && Inventory.Instance.Has("key_roof", 1)) return true;
            return false;
        }

        private static bool IsLabUnlocked()
        {
            if (QuestManager.Instance != null && QuestManager.Instance.HasFlag("lab_unlocked")) return true;
            if (Inventory.Instance != null && Inventory.Instance.Has("key_lab", 1)) return true;
            return false;
        }
    }
}
