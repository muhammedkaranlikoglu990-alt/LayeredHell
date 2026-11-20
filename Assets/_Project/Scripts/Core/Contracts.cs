// 45: Layered Hell - Shared contracts. EVERY system depends only on what is declared here
// (plus the public APIs listed in Docs/CONTRACT.md). Do not rename members without updating the doc.
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LayeredHell
{
    public enum GameState { Boot, MainMenu, Playing, Paused, Dead, Ending }

    public enum DamageType { Bullet, Melee, Explosive, Fire, Acid, Fall, Infection, Starvation }

    public enum SurfaceType { Concrete, Parquet, Tile, Water, Glass, MetalStair }

    [Serializable]
    public struct DamageInfo
    {
        public float amount;
        public DamageType type;
        public Vector3 point;
        public Vector3 direction;
        public GameObject source;
        public bool headshot;

        public DamageInfo(float amount, DamageType type, GameObject source = null, Vector3 point = default, Vector3 direction = default, bool headshot = false)
        {
            this.amount = amount; this.type = type; this.source = source;
            this.point = point; this.direction = direction; this.headshot = headshot;
        }
    }

    [Serializable]
    public struct ItemStack
    {
        public string id;
        public int count;
        public ItemStack(string id, int count) { this.id = id; this.count = count; }
        public override string ToString() { return id + " x" + count; }
    }

    public interface IDamageable
    {
        bool IsDead { get; }
        void TakeDamage(DamageInfo info);
    }

    /// <summary>Anything the player can use with the interact button.</summary>
    public interface IInteractable
    {
        string GetPrompt();
        bool CanInteract();
        void Interact(PlayerController player);
    }

    /// <summary>Persisted by SaveSystem. CaptureState/RestoreState exchange JsonUtility strings.</summary>
    public interface ISaveable
    {
        string SaveKey { get; }
        string CaptureState();
        void RestoreState(string json);
    }

    /// <summary>Put on a floor/ground collider (or its parent) so footsteps know the surface.</summary>
    public sealed class SurfaceTag : MonoBehaviour
    {
        public SurfaceType surface = SurfaceType.Concrete;
    }

    public static class GameConstants
    {
        public const int MinFloor = -4;       // deepest lab level
        public const int MaxFloor = 45;       // roof / helipad
        public const int GroundFloor = 0;     // sealed lobby
        public const int BaseFloor = 38;      // Aras' apartment 38-B (Safe House)
        public const int TotalFloors = 50;    // -4..45 inclusive
        public const float FloorHeight = 3.4f;
        public const string BuildName = "45: Layered Hell";
        public const int SaveVersion = 1;

        public static string FloorLabel(int floor)
        {
            if (floor < 0) return "B" + (-floor);
            if (floor == 0) return "Zemin";
            return floor + ". Kat";
        }

        /// <summary>Scene name of a floor chunk, e.g. Floor_P38, Floor_M04, Floor_P00.</summary>
        public static string FloorSceneName(int floor)
        {
            return floor < 0 ? "Floor_M" + (-floor).ToString("00") : "Floor_P" + floor.ToString("00");
        }

        /// <summary>World-space Y of the floor slab. Floors are stacked in ONE world coordinate space.</summary>
        public static float FloorY(int floor) { return floor * FloorHeight; }
    }

    /// <summary>
    /// Decoupling bus. Producers call Raise*, consumers subscribe. All events are cleared on play-mode start
    /// so disabled domain reload is safe.
    /// </summary>
    public static class GameEvents
    {
        public static event Action<GameState> StateChanged;
        public static event Action<int, int> FloorChanged;            // old, new
        public static event Action<int> FloorLoaded;                  // floor chunk finished generating
        public static event Action<int> FloorUnloaded;
        public static event Action<string, int> ItemAdded;            // itemId, count
        public static event Action<string, int> ItemRemoved;
        public static event Action<string, int> ItemCrafted;
        public static event Action<string, int> ZombieKilled;         // archetypeId (walker/runner/spitter/armored/goliath), floor
        public static event Action<string> SurvivorRescued;           // survivor id (selin/kemal/elif/...)
        public static event Action<string> JournalCollected;          // journal entry id
        public static event Action<string> ObjectiveTriggered;        // free-form keys, see CONTRACT.md
        public static event Action<string> QuestStarted;
        public static event Action<string> QuestCompleted;
        public static event Action<string> Toast;                     // short UI message
        public static event Action<string, string> RadioMessage;      // speaker, text
        public static event Action<string> EndingTriggered;           // "helicopter" | "armored_vehicle"
        public static event Action<string> BaseUpgraded;              // upgrade id
        public static event Action<float> PlayerDamaged;              // amount (for screen effects)

        public static void RaiseStateChanged(GameState s) { StateChanged?.Invoke(s); }
        public static void RaiseFloorChanged(int o, int n) { FloorChanged?.Invoke(o, n); }
        public static void RaiseFloorLoaded(int f) { FloorLoaded?.Invoke(f); }
        public static void RaiseFloorUnloaded(int f) { FloorUnloaded?.Invoke(f); }
        public static void RaiseItemAdded(string id, int n) { ItemAdded?.Invoke(id, n); }
        public static void RaiseItemRemoved(string id, int n) { ItemRemoved?.Invoke(id, n); }
        public static void RaiseItemCrafted(string id, int n) { ItemCrafted?.Invoke(id, n); }
        public static void RaiseZombieKilled(string archetype, int floor) { ZombieKilled?.Invoke(archetype, floor); }
        public static void RaiseSurvivorRescued(string id) { SurvivorRescued?.Invoke(id); }
        public static void RaiseJournalCollected(string id) { JournalCollected?.Invoke(id); }
        public static void RaiseObjective(string key) { ObjectiveTriggered?.Invoke(key); }
        public static void RaiseQuestStarted(string id) { QuestStarted?.Invoke(id); }
        public static void RaiseQuestCompleted(string id) { QuestCompleted?.Invoke(id); }
        public static void RaiseToast(string msg) { Toast?.Invoke(msg); }
        public static void RaiseRadio(string speaker, string text) { RadioMessage?.Invoke(speaker, text); }
        public static void RaiseEnding(string ending) { EndingTriggered?.Invoke(ending); }
        public static void RaiseBaseUpgraded(string id) { BaseUpgraded?.Invoke(id); }
        public static void RaisePlayerDamaged(float amount) { PlayerDamaged?.Invoke(amount); }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            StateChanged = null; FloorChanged = null; FloorLoaded = null; FloorUnloaded = null;
            ItemAdded = null; ItemRemoved = null; ItemCrafted = null; ZombieKilled = null;
            SurvivorRescued = null; JournalCollected = null; ObjectiveTriggered = null;
            QuestStarted = null; QuestCompleted = null; Toast = null; RadioMessage = null;
            EndingTriggered = null; BaseUpgraded = null; PlayerDamaged = null;
        }
    }

    public static class LayerNames
    {
        public const string Player = "Player";
        public const string Enemy = "Enemy";
        public const string Interactable = "Interactable";
        public const string World = "Default";
        public static int PlayerMask { get { return LayerMask.GetMask(Player); } }
    }
}
