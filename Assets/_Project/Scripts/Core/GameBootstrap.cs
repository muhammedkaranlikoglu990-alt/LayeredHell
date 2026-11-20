// Bootstraps the persistent [Systems] GameObject and attaches all singleton managers in deterministic order.
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LayeredHell
{
    public sealed class GameBootstrap : MonoBehaviour
    {
        private static bool _bootstrapped;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _bootstrapped = false;
        }

        private void Awake()
        {
            GameSettings.EnsureLoaded();

            if (_bootstrapped || GameManager.Instance != null)
            {
                TransitionFromBootIfNeeded();
                return;
            }

            _bootstrapped = true;

            var systems = new GameObject("[Systems]");
            DontDestroyOnLoad(systems);

            // Order specified by CONTRACT.md §4 (C1):
            systems.AddComponent<GameManager>();
            systems.AddComponent<SaveSystem>();
            systems.AddComponent<AudioManager>();
            systems.AddComponent<NoiseSystem>();
            systems.AddComponent<Inventory>();
            systems.AddComponent<CraftingSystem>();
            systems.AddComponent<QuestManager>();
            systems.AddComponent<JournalSystem>();
            systems.AddComponent<RadioSystem>();
            systems.AddComponent<BaseManager>();
            systems.AddComponent<FloorStreamer>();
            systems.AddComponent<ZombieDirector>();
            systems.AddComponent<EndingManager>();
            systems.AddComponent<HUD>();
            systems.AddComponent<GameMenuUI>();
            systems.AddComponent<MainMenuUI>();
            systems.AddComponent<MobileControls>();
            systems.AddComponent<InputRouterBehaviour>();

            TransitionFromBootIfNeeded();
        }

        private static void TransitionFromBootIfNeeded()
        {
            if (GameManager.Instance != null && GameManager.Instance.State == GameState.Boot)
            {
                GameManager.Instance.SetState(GameState.MainMenu);
            }

            Scene active = SceneManager.GetActiveScene();
            if (active.name != "MainMenu" && Application.CanStreamedLevelBeLoaded("MainMenu"))
            {
                SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
            }
        }
    }
}
